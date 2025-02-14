using System.Collections.ObjectModel;
using Dynamsoft.CVR;
using Dynamsoft.DBR;
using Dynamsoft.License;
using Twain.Wia.Sane.Scanner;

namespace documentbarcode;

public partial class MainPage : ContentPage
{
	private static string licenseKey = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==";
	private static ScannerController scannerController = new ScannerController();
	private static List<Dictionary<string, object>> devices = new List<Dictionary<string, object>>();
	private static string host = "http://127.0.0.1:18622";

	private List<byte[]> _streams = new List<byte[]>();
	public ObservableCollection<string> Items { get; set; }
	private int selectedIndex = -1;

	private CaptureVisionRouter cvr;
	public MainPage()
	{
		InitializeComponent();
		InitializeDevices();
		InitializeCVR();

		Items = new ObservableCollection<string>
		{
		};

		BindingContext = this;
		ColorPicker.SelectedIndex = 0;
		ResolutionPicker.SelectedIndex = 0;
	}

	private void InitializeCVR()
	{
		string errorMsg;
		int errorCode = LicenseManager.InitLicense("DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==", out errorMsg);
		if (errorCode != (int)Dynamsoft.Core.EnumErrorCode.EC_OK)
			Console.WriteLine("License initialization error: " + errorMsg);

		cvr = new CaptureVisionRouter();
	}

	private async void InitializeDevices()
	{

		var scanners = await scannerController.GetDevices(host, ScannerType.TWAINX64SCANNER | ScannerType.ESCLSCANNER);
		if (scanners.Count == 0)
		{
			await DisplayAlert("Error", "No scanner found", "OK");
			return;
		}
		for (int i = 0; i < scanners.Count; i++)
		{
			var scanner = scanners[i];
			devices.Add(scanner);
			var name = scanner["name"];
			if (name != null)
			{
				Items.Add(name.ToString());
			}
		}

		DevicePicker.SelectedIndex = 0;
	}

	private async void OnLoadImageClicked(object sender, System.EventArgs e)
	{
		if (DevicePicker.SelectedIndex < 0) return;
		var parameters = new Dictionary<string, object>
				{
					{"license", licenseKey},
					{"device", devices[DevicePicker.SelectedIndex]["device"]}
				};

		parameters["config"] = new Dictionary<string, object>
				{
					{"IfShowUI", showUICheckbox.IsChecked},
					{"PixelType", ColorPicker.SelectedIndex},
					{"Resolution", (int)ResolutionPicker.SelectedItem},
					{"IfFeederEnabled", adfCheckbox.IsChecked},
					{"IfDuplexEnabled", duplexCheckbox.IsChecked},
				};

		var data = await scannerController.ScanDocument(host, parameters);
		string jobId = "";
		if (data.ContainsKey(ScannerController.SCAN_SUCCESS))
		{
			jobId = data[ScannerController.SCAN_SUCCESS];
		}

		string error = "";
		if (data.ContainsKey(ScannerController.SCAN_ERROR))
		{
			error = data[ScannerController.SCAN_ERROR];
		}

		if (!string.IsNullOrEmpty(jobId))
		{
			var images = await scannerController.GetImageStreams(host, jobId);
			int start = _streams.Count;
			for (int i = 0; i < images.Count; i++)
			{
				MemoryStream stream = new MemoryStream(images[i]);
				_streams.Add(images[i]);
				ImageSource imageStream = ImageSource.FromStream(() => stream);
				Image image = new Image
				{
					WidthRequest = 200,
					Aspect = Aspect.AspectFit,
					Source = imageStream,
					BindingContext = i + start
				};

				var tapGestureRecognizer = new TapGestureRecognizer();
				tapGestureRecognizer.Tapped += OnImageTapped;
				image.GestureRecognizers.Add(tapGestureRecognizer);

				ImageContainer.Children.Add(image);

			}

			ScrollToLatestImage();
			ShowLargeImage(_streams[_streams.Count - 1]);
		}
		else if (!string.IsNullOrEmpty(error))
		{
			await DisplayAlert("Error", error, "OK");
		}
	}

	private void ShowLargeImage(byte[] bytes)
	{
		MemoryStream stream = new MemoryStream(bytes);
		ImageSource imageStream = ImageSource.FromStream(() => stream);
		LargeImage.Source = imageStream;
	}


	private void OnImageTapped(object? sender, TappedEventArgs e)
	{
		if (sender is Image image && image.BindingContext is int index)
		{
			byte[] imageData = _streams[index];
			ShowLargeImage(imageData);
			selectedIndex = index;
		}
	}

	private void ScrollToLatestImage()
	{
		Dispatcher.Dispatch(() =>
		{
			if (ImageContainer.Children.Count > 0)
			{
				selectedIndex = _streams.Count - 1;
				var lastImage = ImageContainer.Children.Last();
				ImageScrollView.ScrollToAsync((Image)lastImage, ScrollToPosition.Start, true);
			}
		});
	}

	private void OnScanBarcodeClicked(object sender, System.EventArgs e)
	{
		if (_streams.Count == 0)
		{
			DisplayAlert("Error", "Please load an image first.", "OK");
			return;
		}

		BarcodeResultContent.Text = "";
		CapturedResult result = cvr.Capture(_streams[selectedIndex], PresetTemplate.PT_READ_BARCODES);
		if (result != null)
		{
			DecodedBarcodesResult barcodesResult = result.GetDecodedBarcodesResult();
			if (barcodesResult != null)
			{
				BarcodeResultItem[] items = barcodesResult.GetItems();
				foreach (BarcodeResultItem barcodeItem in items)
				{
					BarcodeResultContent.Text += "Result " + (Array.IndexOf(items, barcodeItem) + 1) + Environment.NewLine;
					BarcodeResultContent.Text += "Barcode Format: " + barcodeItem.GetFormatString() + Environment.NewLine;
					BarcodeResultContent.Text += "Barcode Text: " + barcodeItem.GetText() + Environment.NewLine;
				}

				BarcodeResultContent.Text += "Total barcode(s) found: " + items.Length + Environment.NewLine + Environment.NewLine;
			}
		}
	}

	private void OnDeleteAllClicked(object sender, EventArgs e)
	{
		ImageContainer.Clear();
		_streams.Clear();
		LargeImage.Source = null;
		selectedIndex = -1;
	}

	private async void OnSaveClicked(object sender, EventArgs e)
	{

		if (_streams.Count == 0) return;

		var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
		if (status != PermissionStatus.Granted)
		{
			return;
		}

		var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GenerateFilename());

		// Save the image to the specified file path
		File.WriteAllBytes(filePath, _streams[selectedIndex]);

		await DisplayAlert("Success", "Image saved to " + filePath, "OK");

	}

	private static string GenerateFilename()
	{
		DateTime now = DateTime.Now;
		string timestamp = now.ToString("yyyyMMdd_HHmmss");
		return $"image_{timestamp}.png";
	}
}

