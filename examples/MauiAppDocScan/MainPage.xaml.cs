using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.ObjectModel;
using Twain.Wia.Sane.Scanner;
using Microsoft.Maui.Graphics.Platform;

using IImage = Microsoft.Maui.Graphics.IImage;
using Microsoft.Maui.Controls;

namespace MauiAppDocScan
{
    public partial class MainPage : ContentPage
    {
        private static string licenseKey = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==";
        private static ScannerController scannerController = new ScannerController();
        private static List<Dictionary<string, object>> devices = new List<Dictionary<string, object>>();
        private static string host = "http://127.0.0.1:18622"; // Change this to your server IP address. e.g., http://127.0.0.1:18622//DWTAPI/Scanners
        private List<byte[]> _streams = new List<byte[]>();
        private SKBitmap bitmap;
        private int selectedIndex = -1;
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

            var text = await scannerController.CreateJob(host, parameters);
            string jobId = "";
            if (text.ContainsKey(ScannerController.SCAN_SUCCESS))
            {
                jobId = text[ScannerController.SCAN_SUCCESS];
            }

            string error = "";
            if (text.ContainsKey(ScannerController.SCAN_ERROR))
            {
                error = text[ScannerController.SCAN_ERROR];
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
                        HeightRequest = 200,
                        Aspect = Aspect.AspectFit,
                        VerticalOptions = LayoutOptions.CenterAndExpand,
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
                        Source = imageStream,
                        BindingContext = i + start
                    };

                    // Add the TapGestureRecognizer
                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += OnImageTapped;
                    image.GestureRecognizers.Add(tapGestureRecognizer);

                    ImageContainer.Children.Add(image);
                }

                if (ImageContainer.Children.Count > 0)
                {
                    selectedIndex = _streams.Count - 1;
                    var lastImage = ImageContainer.Children.Last();
                    DrawImage(_streams[_streams.Count - 1]);
                    await ImageScrollView.ScrollToAsync((Image)lastImage, ScrollToPosition.MakeVisible, true);
                }
            }
            else if (!string.IsNullOrEmpty(error))
            {
                DisplayAlert("Error", error, "OK");
            }
        }

        private void DrawImage(byte[] buffer)
        {
            try
            {
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }

                if (_streams.Count > 0)
                {
                    bitmap = SKBitmap.Decode(buffer);
                    skiaView.InvalidateSurface();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async void OnImageTapped(object sender, EventArgs e)
        {
            if (sender is Image image && image.BindingContext is int index)
            {
                byte[] imageData = _streams[index];
                DrawImage(imageData);
                selectedIndex = index;
            }
        }

        private void OnDeleteAllClicked(object sender, EventArgs e)
        {
            ImageContainer.Clear();
            _streams.Clear();
            if (bitmap != null)
            {
                bitmap.Dispose(); bitmap = null;
                skiaView.InvalidateSurface();
            }
            selectedIndex = -1;
        }

        private void OnRotateLeftClicked(object sender, EventArgs e)
        {
            if (ImageContainer.Children.Count == 0) return;

            Image image = (Image)ImageContainer.Children[selectedIndex];
            image.RotateTo(image.Rotation - 90);

            SKBitmap rotatedBitmap = RotateBitmap(bitmap, -90);
            bitmap.Dispose();
            bitmap = rotatedBitmap;
            skiaView.InvalidateSurface();
        }

        private void OnRotateRightClicked(object sender, EventArgs e)
        {
            if (ImageContainer.Children.Count == 0) return;

            Image image = (Image)ImageContainer.Children[selectedIndex];
            image.RotateTo(image.Rotation + 90);

            SKBitmap rotatedBitmap = RotateBitmap(bitmap, 90);
            bitmap.Dispose();
            bitmap = rotatedBitmap;
            skiaView.InvalidateSurface();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {

            if (_streams.Count == 0) return;

            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                // Handle the case where the user denies permission
                return;
            }

            if (bitmap != null)
            {
                //// Define the path where you want to save the images
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GenerateFilename());

                using SKImage image = SKImage.FromBitmap(bitmap);
                using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 100);

                using FileStream stream = File.OpenWrite(filePath);
                data.SaveTo(stream);


                DisplayAlert("Success", "Image saved to " + filePath, "OK");
            }

        }

        public IImage ConvertToIImage(Microsoft.Maui.Controls.Image mauiImage)
        {
            if (mauiImage.Source is FileImageSource fileImageSource)
            {
                var filePath = fileImageSource.File;
                using var stream = File.OpenRead(filePath);
                return StreamToIImage(stream);
            }

            if (mauiImage.Source is StreamImageSource streamImageSource)
            {
                var stream = streamImageSource.Stream.Invoke(default).Result;
                return StreamToIImage(stream);
            }

            throw new NotSupportedException("Unsupported image source type.");
        }

        private IImage StreamToIImage(Stream stream)
        {
            stream.Position = 0; // Reset the position to the beginning

            return PlatformImage.FromStream(stream);
        }

        private void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;

            canvas.Clear(SKColors.White);

            if (bitmap != null)
            {
                // Calculate the aspect ratio
                float bitmapWidth = bitmap.Width;
                float bitmapHeight = bitmap.Height;
                float canvasWidth = e.Info.Width;
                float canvasHeight = e.Info.Height;

                float scale = Math.Min(canvasWidth / bitmapWidth, canvasHeight / bitmapHeight);

                float newWidth = scale * bitmapWidth;
                float newHeight = scale * bitmapHeight;

                float left = (canvasWidth - newWidth) / 2;
                float top = (canvasHeight - newHeight) / 2;

                SKRect destRect = new SKRect(left, top, left + newWidth, top + newHeight);

                canvas.DrawBitmap(bitmap, destRect);
            }
        }

        public static string GenerateFilename()
        {
            DateTime now = DateTime.Now;
            string timestamp = now.ToString("yyyyMMdd_HHmmss");
            return $"image_{timestamp}.png";
        }

        public SKBitmap RotateBitmap(SKBitmap originalBitmap, float angle)
        {
            // Calculate the new width and height after rotation
            double radians = Math.PI * angle / 180.0;
            double cos = Math.Abs(Math.Cos(radians));
            double sin = Math.Abs(Math.Sin(radians));
            int newWidth = (int)(originalBitmap.Width * cos + originalBitmap.Height * sin);
            int newHeight = (int)(originalBitmap.Width * sin + originalBitmap.Height * cos);

            // Create a new bitmap to hold the rotated image
            SKBitmap rotatedBitmap = new SKBitmap(newWidth, newHeight);

            // Create a canvas to draw on the new bitmap
            using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
            {
                // Clear the canvas
                canvas.Clear(SKColors.Transparent);

                // Apply the rotation transformation
                canvas.Translate(newWidth / 2, newHeight / 2); // Move the origin to the center of the canvas
                canvas.RotateDegrees(angle); // Rotate the canvas
                canvas.Translate(-originalBitmap.Width / 2, -originalBitmap.Height / 2); // Move the origin back to the top-left corner

                // Draw the original bitmap onto the rotated canvas
                canvas.DrawBitmap(originalBitmap, 0, 0);
            }

            return rotatedBitmap;
        }
    }
}