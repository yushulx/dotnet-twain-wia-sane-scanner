using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Twain.Wia.Sane.Scanner;
using Windows.Media.Ocr;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Diagnostics;
using System.Linq;

namespace WinFormsDocScan
{
    public partial class Form1 : Form
    {
        private static string licenseKey = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==";
        private static ScannerController scannerController = new ScannerController();
        private static List<Dictionary<string, object>> devices = new List<Dictionary<string, object>>();
        private static string host = "http://127.0.0.1:18622";
        private List<Image> scannedImages = new List<Image>();
        private Image? selectedImage = null;
        private int selectedImageIndex = -1;

        public ObservableCollection<string> Items { get; set; }
        public ObservableCollection<string> OcrLanguages { get; set; }

        public Form1()
        {
            InitializeComponent();

            // Maximize the window by default for better user experience
            this.WindowState = FormWindowState.Maximized;

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            Items = new ObservableCollection<string>();
            OcrLanguages = new ObservableCollection<string>();

            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.Padding = new Padding(15, 20, 15, 15); // Add padding: left, top, right, bottom

            // Initialize OCR button as disabled
            ocrButton.Enabled = false;

            // Add resize handler for responsive image sizing
            imagePanel.SizeChanged += ImagePanel_SizeChanged;
            flowLayoutPanel1.SizeChanged += FlowLayoutPanel1_SizeChanged;

            InitializeOcrLanguages();
        }

        private void InitializeOcrLanguages()
        {
            try
            {
                var supportedLanguages = OcrEngine.AvailableRecognizerLanguages;
                foreach (var language in supportedLanguages)
                {
                    OcrLanguages.Add($"{language.DisplayName} ({language.LanguageTag})");
                }

                languageComboBox.DataSource = OcrLanguages;
                if (OcrLanguages.Count > 0)
                {
                    // Try to select English as default
                    var englishIndex = OcrLanguages.ToList().FindIndex(lang => lang.Contains("English"));
                    languageComboBox.SelectedIndex = englishIndex >= 0 ? englishIndex : 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing OCR languages: {ex.Message}", "OCR Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var scannerInfo = await scannerController.GetDevices(host, ScannerType.TWAINSCANNER | ScannerType.TWAINX64SCANNER);
            devices.Clear();
            Items.Clear();
            var scanners = new List<Dictionary<string, object>>();
            try
            {
                scanners = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(scannerInfo) ?? new List<Dictionary<string, object>>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
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
            if (comboBox1.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a scanner device first.", "No Device Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

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
                    {"IfFeederEnabled", true},
                    {"IfDuplexEnabled", false}
                };

            var jobInfo = await scannerController.CreateJob(host, parameters);
            string jobId = "";
            try
            {
                var job = JsonConvert.DeserializeObject<Dictionary<string, object>>(jobInfo);
                jobId = job?["jobuid"]?.ToString() ?? "";

                if (string.IsNullOrEmpty(jobId))
                {
                    Debug.WriteLine("Failed to create job.");
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                return;
            }

            if (!string.IsNullOrEmpty(jobId))
            {
                var images = await scannerController.GetImageStreams(host, jobId);
                for (int i = 0; i < images.Count; i++)
                {
                    MemoryStream stream = new MemoryStream(images[i]);
                    Image image = Image.FromStream(stream);

                    // Store the image for OCR
                    scannedImages.Add(image);

                    var pictureBox = CreateImagePictureBox(image, scannedImages.Count - 1);
                    flowLayoutPanel1.Controls.Add(pictureBox);
                    flowLayoutPanel1.Controls.SetChildIndex(pictureBox, 0);

                    // Debug output to verify image is added
                    Debug.WriteLine($"Added image {i + 1} to flowLayoutPanel1. Total controls: {flowLayoutPanel1.Controls.Count}");
                }
            }
            await scannerController.DeleteJob(host, jobId);
        }

        private PictureBox CreateImagePictureBox(Image image, int index)
        {
            // Calculate sizing optimized for document scanning (portrait orientation)
            int panelWidth = Math.Max(flowLayoutPanel1.Width, 400);
            int pictureBoxWidth = Math.Max(280, panelWidth - 60); // Slightly narrower for document format

            // For documents, typically height > width, so use a taller aspect ratio
            // Calculate height based on image aspect ratio, but favor taller display
            double aspectRatio = (double)image.Width / image.Height;
            int pictureBoxHeight;

            if (aspectRatio > 1.0) // Landscape image
            {
                pictureBoxHeight = Math.Min(350, (int)(pictureBoxWidth / aspectRatio));
            }
            else // Portrait image (typical for documents)
            {
                pictureBoxHeight = Math.Min(500, (int)(pictureBoxWidth / aspectRatio)); // Allow taller for documents
            }

            // Ensure reasonable minimum height for visibility
            pictureBoxHeight = Math.Max(300, pictureBoxHeight);

            PictureBox pictureBox = new PictureBox()
            {
                Size = new Size(pictureBoxWidth, pictureBoxHeight),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10), // Good spacing between images
                Cursor = Cursors.Hand,
                Tag = index,
                Visible = true,
                Name = $"PictureBox_{index}",
                BackColor = Color.White // Default background color for unselected images
            };

            pictureBox.Image = image;
            pictureBox.Click += PictureBox_Click;

            // Debug output to verify sizing
            Debug.WriteLine($"Created PictureBox_{index}: Size={pictureBox.Size}, Panel Width={panelWidth}, Aspect Ratio={aspectRatio:F2}");

            return pictureBox;
        }
        private void PictureBox_Click(object? sender, EventArgs e)
        {
            if (sender is PictureBox pictureBox && pictureBox.Tag is int index)
            {
                // Highlight selected image with both border and background color
                foreach (Control control in flowLayoutPanel1.Controls)
                {
                    if (control is PictureBox pb)
                    {
                        if (pb == pictureBox)
                        {
                            // Selected image styling
                            pb.BorderStyle = BorderStyle.Fixed3D;
                            pb.BackColor = Color.LightBlue; // Highlight background color
                        }
                        else
                        {
                            // Unselected image styling
                            pb.BorderStyle = BorderStyle.FixedSingle;
                            pb.BackColor = Color.White; // Default background color
                        }
                    }
                }

                selectedImage = scannedImages[index];
                selectedImageIndex = index;
                ocrButton.Enabled = true;
                deleteImageButton.Enabled = true;
            }
        }

        private void deleteImageButton_Click(object sender, EventArgs e)
        {
            if (selectedImageIndex >= 0 && selectedImageIndex < scannedImages.Count)
            {
                // Remove the image from the list
                scannedImages.RemoveAt(selectedImageIndex);

                // Remove the corresponding PictureBox from the UI
                var pictureBoxToRemove = flowLayoutPanel1.Controls
                    .OfType<PictureBox>()
                    .FirstOrDefault(pb => pb.Tag is int index && index == selectedImageIndex);

                if (pictureBoxToRemove != null)
                {
                    flowLayoutPanel1.Controls.Remove(pictureBoxToRemove);
                    pictureBoxToRemove.Dispose();
                }

                // Update indices for remaining PictureBoxes
                UpdatePictureBoxIndices();

                // Clear selection
                selectedImage = null;
                selectedImageIndex = -1;
                ocrButton.Enabled = false;
                deleteImageButton.Enabled = false;

                Debug.WriteLine($"Deleted image. Remaining images: {scannedImages.Count}");
            }
        }

        private void deleteAllButton_Click(object sender, EventArgs e)
        {
            if (scannedImages.Count > 0)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete all {scannedImages.Count} images?",
                    "Delete All Images",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Clear all images
                    scannedImages.Clear();

                    // Remove all PictureBoxes from UI
                    var pictureBoxesToRemove = flowLayoutPanel1.Controls.OfType<PictureBox>().ToList();
                    foreach (var pb in pictureBoxesToRemove)
                    {
                        flowLayoutPanel1.Controls.Remove(pb);
                        pb.Dispose();
                    }

                    // Clear selection
                    selectedImage = null;
                    selectedImageIndex = -1;
                    ocrButton.Enabled = false;
                    deleteImageButton.Enabled = false;

                    Debug.WriteLine("All images deleted");
                }
            }
        }

        private void UpdatePictureBoxIndices()
        {
            var pictureBoxes = flowLayoutPanel1.Controls.OfType<PictureBox>().ToList();
            for (int i = 0; i < pictureBoxes.Count; i++)
            {
                var pb = pictureBoxes[i];
                if (pb.Tag is int oldIndex && oldIndex > selectedImageIndex)
                {
                    pb.Tag = oldIndex - 1; // Decrease index by 1
                    pb.Name = $"PictureBox_{oldIndex - 1}";
                }
            }
        }

        private async void ocrButton_Click(object sender, EventArgs e)
        {
            if (selectedImage == null)
            {
                MessageBox.Show("Please select an image first by clicking on it.", "No Image Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (languageComboBox.SelectedIndex < 0)
            {
                MessageBox.Show("Please select an OCR language.", "No Language Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                ocrButton.Enabled = false;
                ocrButton.Text = "Processing...";

                // Get selected language
                var selectedLanguageText = languageComboBox.SelectedItem?.ToString() ?? "";
                var languageTag = ExtractLanguageTag(selectedLanguageText);

                // Create OCR engine for the selected language
                var language = new Windows.Globalization.Language(languageTag);
                var ocrEngine = OcrEngine.TryCreateFromLanguage(language);

                if (ocrEngine == null)
                {
                    MessageBox.Show($"OCR engine could not be created for language: {languageTag}", "OCR Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Convert image to SoftwareBitmap
                var softwareBitmap = await ConvertImageToSoftwareBitmap(selectedImage);

                // Perform OCR
                var ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);

                // Display results
                var recognizedText = ocrResult.Text;
                if (string.IsNullOrWhiteSpace(recognizedText))
                {
                    ocrTextBox.Text = "No text was recognized in the selected image.";
                }
                else
                {
                    ocrTextBox.Text = recognizedText;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"OCR processing failed: {ex.Message}", "OCR Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ocrButton.Enabled = true;
                ocrButton.Text = "Run OCR";
            }
        }

        private void clearTextButton_Click(object sender, EventArgs e)
        {
            ocrTextBox.Clear();
        }

        private void loadImageButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Image Files";
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.tif|" +
                                      "JPEG files|*.jpg;*.jpeg|" +
                                      "PNG files|*.png|" +
                                      "Bitmap files|*.bmp|" +
                                      "GIF files|*.gif|" +
                                      "TIFF files|*.tiff;*.tif|" +
                                      "All files|*.*";
                openFileDialog.Multiselect = true;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Debug.WriteLine($"Selected {openFileDialog.FileNames.Length} files for loading");

                    foreach (string fileName in openFileDialog.FileNames)
                    {
                        try
                        {
                            // Load the image from file
                            Image image = Image.FromFile(fileName);
                            Debug.WriteLine($"Successfully loaded image: {Path.GetFileName(fileName)} - Size: {image.Size}");

                            // Store the image for OCR
                            scannedImages.Add(image);

                            var pictureBox = CreateImagePictureBox(image, scannedImages.Count - 1);
                            flowLayoutPanel1.Controls.Add(pictureBox);
                            flowLayoutPanel1.Controls.SetChildIndex(pictureBox, 0);

                            // Debug output to verify image is added
                            Debug.WriteLine($"Added PictureBox to flowLayoutPanel1. Total controls: {flowLayoutPanel1.Controls.Count}");
                            Debug.WriteLine($"FlowLayoutPanel1 properties - Size: {flowLayoutPanel1.Size}, Visible: {flowLayoutPanel1.Visible}, Location: {flowLayoutPanel1.Location}");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error loading image file '{Path.GetFileName(fileName)}': {ex.Message}",
                                          "Image Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }

                    // Force layout refresh after adding all images
                    flowLayoutPanel1.Invalidate();
                    flowLayoutPanel1.Update();
                    imagePanel.Invalidate();
                    imagePanel.Update();
                    Debug.WriteLine($"Layout refresh completed. FlowLayoutPanel1 has {flowLayoutPanel1.Controls.Count} controls");
                }
            }
        }

        private string ExtractLanguageTag(string languageText)
        {
            // Extract language tag from format "Language Name (tag)"
            var startIndex = languageText.LastIndexOf('(');
            var endIndex = languageText.LastIndexOf(')');

            if (startIndex >= 0 && endIndex > startIndex)
            {
                return languageText.Substring(startIndex + 1, endIndex - startIndex - 1);
            }

            return "en"; // Default to English
        }

        private async Task<SoftwareBitmap> ConvertImageToSoftwareBitmap(Image image)
        {
            using (var memoryStream = new MemoryStream())
            {
                image.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                var randomAccessStream = memoryStream.AsRandomAccessStream();
                var decoder = await BitmapDecoder.CreateAsync(randomAccessStream);
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

                return softwareBitmap;
            }
        }

        private void ImagePanel_SizeChanged(object? sender, EventArgs e)
        {
            // Resize all picture boxes when the panel is resized
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Image != null)
                {
                    int newWidth = Math.Max(280, flowLayoutPanel1.Width - 60); // Document-oriented width
                    pictureBox.Width = newWidth;

                    // Maintain aspect ratio with document-oriented height calculation
                    double aspectRatio = (double)pictureBox.Image.Width / pictureBox.Image.Height;
                    int newHeight;

                    if (aspectRatio > 1.0) // Landscape
                    {
                        newHeight = Math.Min(350, (int)(newWidth / aspectRatio));
                    }
                    else // Portrait (typical for documents)
                    {
                        newHeight = Math.Min(500, (int)(newWidth / aspectRatio));
                    }

                    newHeight = Math.Max(300, newHeight); // Minimum height
                    pictureBox.Height = newHeight;
                }
            }
        }

        private void FlowLayoutPanel1_SizeChanged(object? sender, EventArgs e)
        {
            // Resize all picture boxes when the flowLayoutPanel is resized
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is PictureBox pictureBox && pictureBox.Image != null)
                {
                    int newWidth = Math.Max(280, flowLayoutPanel1.Width - 60); // Document-oriented width
                    pictureBox.Width = newWidth;

                    // Maintain aspect ratio with document-oriented height calculation
                    double aspectRatio = (double)pictureBox.Image.Width / pictureBox.Image.Height;
                    int newHeight;

                    if (aspectRatio > 1.0) // Landscape
                    {
                        newHeight = Math.Min(350, (int)(newWidth / aspectRatio));
                    }
                    else // Portrait (typical for documents)
                    {
                        newHeight = Math.Min(500, (int)(newWidth / aspectRatio));
                    }

                    newHeight = Math.Max(300, newHeight); // Minimum height
                    pictureBox.Height = newHeight;
                }
            }
        }
    }
}