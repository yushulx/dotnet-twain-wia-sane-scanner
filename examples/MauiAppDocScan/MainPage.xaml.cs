using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using Twain.Wia.Sane.Scanner;

namespace MauiAppDocScan
{
    public partial class MainPage : ContentPage
    {
        private static string licenseKey = "LICENSE-KEY";
        private static ScannerController scannerController = new ScannerController();
        private static List<Dictionary<string, object>> devices = new List<Dictionary<string, object>>();
        private static string host = "http://192.168.8.72:18622";

        public ObservableCollection<string> Items { get; set; }
        public ObservableCollection<ImageItem> Images { get; set; }

        public MainPage()
        {
            InitializeComponent();

            Items = new ObservableCollection<string>
            {
            };

            Images = new ObservableCollection<ImageItem>
            {
            };

            BindingContext = this;
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
            var parameters = new Dictionary<string, object>
                {
                    {"license", licenseKey},
                    {"device", devices[DevicePicker.SelectedIndex]["device"]}
                };

            parameters["config"] = new Dictionary<string, object>
                {
                    {"IfShowUI", false},
                    {"PixelType", 2},
                    {"Resolution", 200},
                    {"IfFeederEnabled", false},
                    {"IfDuplexEnabled", false}
                };

            string jobId = await scannerController.ScanDocument(host, parameters);

            if (!string.IsNullOrEmpty(jobId))
            {
                var images = await scannerController.GetImageStreams(host, jobId);
                for (int i = 0; i < images.Count; i++)
                {
                    MemoryStream stream = new MemoryStream(images[i]);
                    ImageSource imageStream = ImageSource.FromStream(() => stream);
                    ImageItem item = new ImageItem();
                    item.ImageStream = imageStream;
                    Images.Insert(0, item);
                }

                scannerController.DeleteJob(host, jobId);
            }
        }
    }
}