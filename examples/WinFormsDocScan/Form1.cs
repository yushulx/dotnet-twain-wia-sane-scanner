using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Twain.Wia.Sane.Scanner;

namespace WinFormsDocScan
{
    public partial class Form1 : Form
    {
        private static string licenseKey = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==";
        private static ScannerController scannerController = new ScannerController();
        private static List<Dictionary<string, object>> devices = new List<Dictionary<string, object>>();
        private static string host = "http://127.0.0.1:18622";
        public ObservableCollection<string> Items { get; set; }
        public Form1()
        {
            InitializeComponent();

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            Items = new ObservableCollection<string>
            {
            };

            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel1.AutoScroll = true;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var scannerInfo = await scannerController.GetDevices(host, ScannerType.TWAINSCANNER | ScannerType.TWAINX64SCANNER);
            devices.Clear();
            Items.Clear();
            var scanners = new List<Dictionary<string, object>>();
            try
            {
                scanners = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(scannerInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            if (scanners.Count == 0)
            {
                MessageBox.Show("No scanner found");
                return;
            }
            for (int i = 0; i < scanners.Count; i++)
            {
                var scanner = scanners[i];
                devices.Add(scanner);
                var name = scanner["name"];
                Items.Add(name.ToString() ?? "N/A");
            }

            comboBox1.DataSource = Items;
            comboBox1.SelectedIndex = 0;
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var parameters = new Dictionary<string, object>
                {
                    {"license", licenseKey},
                    {"device", devices[comboBox1.SelectedIndex]["device"]}
                };

            parameters["config"] = new Dictionary<string, object>
                {
                    {"IfShowUI", false},
                    {"PixelType", 2},
                    {"Resolution", 200},
                    {"IfFeederEnabled", false},
                    {"IfDuplexEnabled", false}
                };

            var jobInfo = await scannerController.CreateJob(host, parameters);
            string jobId = "";
            try
            {
                var job = JsonConvert.DeserializeObject<Dictionary<string, object>>(jobInfo);
                jobId = (string)job["jobuid"];

                if (string.IsNullOrEmpty(jobId))
                {
                    Console.WriteLine("Failed to create job.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return;
            }

            if (!string.IsNullOrEmpty(jobId))
            {
                var images = await scannerController.GetImageStreams(host, jobId);
                for (int i = 0; i < images.Count; i++)
                {
                    MemoryStream stream = new MemoryStream(images[i]);
                    Image image = Image.FromStream(stream);

                    PictureBox pictureBox = new PictureBox()
                    {
                        Size = new Size(600, 600),
                        SizeMode = PictureBoxSizeMode.Zoom,
                    };
                    pictureBox.Image = image;
                    flowLayoutPanel1.Controls.Add(pictureBox);
                    flowLayoutPanel1.Controls.SetChildIndex(pictureBox, 0);
                }
            }
        }
    }
}