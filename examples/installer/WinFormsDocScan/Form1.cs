using System.Collections.ObjectModel;
using System.Windows.Forms;
using Twain.Wia.Sane.Scanner;

namespace WinFormsDocScan
{
    public partial class Form1 : Form
    {
        private static string licenseKey = "LICENSE-KEY";
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
            var scanners = await scannerController.GetDevices(host, ScannerType.TWAINSCANNER | ScannerType.TWAINX64SCANNER);
            devices.Clear();
            Items.Clear();
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

            string jobId = await scannerController.ScanDocument(host, parameters);

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