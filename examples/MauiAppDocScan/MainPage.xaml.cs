using System.Collections.ObjectModel;
using Twain.Wia.Sane.Scanner;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;


namespace MauiAppDocScan
{
    public partial class MainPage : ContentPage
    {
        private static string licenseKey = "LICENSE-KEY";
        private static ScannerController scannerController = new ScannerController();
        private static List<Dictionary<string, object>> devices = new List<Dictionary<string, object>>();
        private static string host = "http://127.0.0.1:18622"; // Change this to your server IP address
        public ObservableCollection<string> Items { get; set; }

        public MainPage()
        {
            InitializeComponent();

            Items = new ObservableCollection<string>
            {
            };

            BindingContext = this;
            ColorPicker.SelectedIndex = 0;
            ResolutionPicker.SelectedIndex = 0;
        }

        private async void OnGetDeviceClicked(object sender, EventArgs e)
        {

            var scanners = await scannerController.GetDevices(host, ScannerType.TWAINSCANNER | ScannerType.TWAINX64SCANNER);
            devices.Clear();
            Items.Clear();
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
                Items.Add(name.ToString());
            }

            DevicePicker.SelectedIndex = 0;
        }

        private async void OnScanClicked(object sender, EventArgs e)
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
                    {"IfDuplexEnabled", duplexCheckbox.IsChecked}
                };

            string jobId = await scannerController.ScanDocument(host, parameters);

            if (!string.IsNullOrEmpty(jobId))
            {
                var images = await scannerController.GetImageStreams(host, jobId);
                for (int i = 0; i < images.Count; i++)
                {
                    MemoryStream stream = new MemoryStream(images[i]);
                    ImageSource imageStream = ImageSource.FromStream(() => stream);
                    Image image = new Image();
                    image.WidthRequest = 800;
                    image.HeightRequest = 800;
                    image.Aspect = Aspect.AspectFit;
                    image.VerticalOptions = LayoutOptions.CenterAndExpand;
                    image.HorizontalOptions = LayoutOptions.CenterAndExpand;
                    image.Source = imageStream;
                    ImageContainer.Children.Insert(0, image);
                }
            }
        }

        private void OnDeleteAllClicked(object sender, EventArgs e)
        {
            ImageContainer.Clear();
        }
    }
}