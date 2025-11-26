using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using Twain.Wia.Sane.Scanner;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PDFtoImage;
using SkiaSharp.Views.Desktop;

namespace WinFormsDocScan
{
    public partial class Form1 : Form
    {
        private static string licenseKey = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==";
        private static ScannerController scannerController = new ScannerController();
        private static List<Dictionary<string, object>> devices = new List<Dictionary<string, object>>();
        private static string host = "http://127.0.0.1:18622";
        public ObservableCollection<string> Items { get; set; }

        private VideoCapture? videoCapture;
        private Form? webcamForm;
        private PictureBox? selectedPictureBox;
        private System.Windows.Forms.Timer? webcamTimer;

        // Drag and drop fields
        private Panel? draggedPanel;
        private System.Drawing.Point dragStartPoint;

        public Form1()
        {
            InitializeComponent();

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            Items = new ObservableCollection<string>
            {
            };

            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;
            flowLayoutPanel1.AutoScroll = true;

            // Enable drag and drop for file loading
            this.AllowDrop = true;
            flowLayoutPanel1.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;
            flowLayoutPanel1.DragEnter += Form1_DragEnter;
            flowLayoutPanel1.DragDrop += Form1_DragDrop;
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

        private void Form1_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null)
                {
                    foreach (string file in files)
                    {
                        string ext = Path.GetExtension(file).ToLower();
                        try
                        {
                            if (ext == ".pdf")
                            {
                                LoadPdfFile(file);
                            }
                            else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" ||
                                     ext == ".bmp" || ext == ".gif" || ext == ".tiff")
                            {
                                System.Drawing.Image image = System.Drawing.Image.FromFile(file);
                                AddImageToPanel(image);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error loading file {Path.GetFileName(file)}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
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
                    {"IfFeederEnabled", true},
                    {"IfDuplexEnabled", false}
                };

            var jobInfo = await scannerController.CreateJob(host, parameters);
            string jobId = "";
            try
            {
                var job = JsonConvert.DeserializeObject<Dictionary<string, object>>(jobInfo);
                if (job != null)
                {
                    jobId = (string)job["jobuid"];
                }

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

                progressBar1.Visible = true;
                progressBar1.Minimum = 0;
                progressBar1.Maximum = images.Count;
                progressBar1.Value = 0;

                for (int i = 0; i < images.Count; i++)
                {
                    MemoryStream stream = new MemoryStream(images[i]);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                    AddImageToPanel(image);

                    progressBar1.Value = i + 1;
                    Application.DoEvents();
                }
                progressBar1.Visible = false;
            }
            await scannerController.DeleteJob(host, jobId);
        }

        private void AddImageToPanel(System.Drawing.Image image)
        {
            Panel containerPanel = new Panel()
            {
                Size = new System.Drawing.Size(220, 280),
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            PictureBox pictureBox = new PictureBox()
            {
                Size = new System.Drawing.Size(200, 240),
                Location = new System.Drawing.Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = image,
                Tag = image
            };

            Label lblIndex = new Label()
            {
                Text = $"Image {flowLayoutPanel1.Controls.Count + 1}",
                Location = new System.Drawing.Point(10, 255),
                Size = new System.Drawing.Size(200, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular)
            };

            containerPanel.Tag = pictureBox;
            containerPanel.Click += ContainerPanel_Click;
            pictureBox.Click += ContainerPanel_Click;

            // Enable drag-and-drop for reordering
            containerPanel.MouseDown += ContainerPanel_MouseDown;
            containerPanel.MouseMove += ContainerPanel_MouseMove;
            containerPanel.MouseUp += ContainerPanel_MouseUp;
            containerPanel.DragEnter += ContainerPanel_DragEnter;
            containerPanel.DragDrop += ContainerPanel_DragDrop;
            containerPanel.DragOver += ContainerPanel_DragOver;
            containerPanel.DragLeave += ContainerPanel_DragLeave;
            containerPanel.AllowDrop = true;

            // Also attach drag events to PictureBox so dragging the image works
            pictureBox.MouseDown += ContainerPanel_MouseDown;
            pictureBox.MouseMove += ContainerPanel_MouseMove;
            pictureBox.MouseUp += ContainerPanel_MouseUp;
            pictureBox.DragEnter += ContainerPanel_DragEnter;
            pictureBox.DragDrop += ContainerPanel_DragDrop;
            pictureBox.DragOver += ContainerPanel_DragOver;
            pictureBox.DragLeave += ContainerPanel_DragLeave;
            pictureBox.AllowDrop = true;

            containerPanel.Controls.Add(pictureBox);
            containerPanel.Controls.Add(lblIndex);
            flowLayoutPanel1.Controls.Add(containerPanel);

            // Select the new image
            if (selectedPictureBox != null && selectedPictureBox.Parent is Panel oldPanel)
            {
                oldPanel.BackColor = Color.White;
                oldPanel.BorderStyle = BorderStyle.FixedSingle;
            }

            containerPanel.BackColor = Color.FromArgb(230, 240, 255);
            containerPanel.BorderStyle = BorderStyle.FixedSingle;
            selectedPictureBox = pictureBox;

            // Scroll to the new image
            flowLayoutPanel1.ScrollControlIntoView(containerPanel);
        }

        private void ContainerPanel_Click(object? sender, EventArgs e)
        {
            Control? clicked = sender as Control;
            Panel? containerPanel = null;

            if (clicked is PictureBox)
            {
                containerPanel = clicked.Parent as Panel;
            }
            else if (clicked is Panel)
            {
                containerPanel = clicked as Panel;
            }

            if (containerPanel != null)
            {
                // Remove highlight from previously selected panel
                if (selectedPictureBox != null && selectedPictureBox.Parent is Panel oldPanel)
                {
                    oldPanel.BackColor = Color.White;
                    oldPanel.BorderStyle = BorderStyle.FixedSingle;
                }

                // Highlight the selected panel
                containerPanel.BackColor = Color.FromArgb(230, 240, 255);
                containerPanel.BorderStyle = BorderStyle.FixedSingle;

                selectedPictureBox = containerPanel.Tag as PictureBox;
            }
        }

        private void ContainerPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (sender is Panel panel)
                {
                    draggedPanel = panel;
                }
                else if (sender is PictureBox pb && pb.Parent is Panel parent)
                {
                    draggedPanel = parent;
                }

                if (draggedPanel != null)
                {
                    // Select the dragged panel immediately
                    if (selectedPictureBox != null && selectedPictureBox.Parent is Panel oldPanel && oldPanel != draggedPanel)
                    {
                        oldPanel.BackColor = Color.White;
                        oldPanel.BorderStyle = BorderStyle.FixedSingle;
                    }

                    draggedPanel.BackColor = Color.FromArgb(230, 240, 255);
                    draggedPanel.BorderStyle = BorderStyle.FixedSingle;
                    selectedPictureBox = draggedPanel.Tag as PictureBox;

                    dragStartPoint = e.Location;
                    // Give visual feedback that item is ready to drag
                    draggedPanel.Cursor = Cursors.SizeAll;
                }
            }
        }

        private void ContainerPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            Panel? sourcePanel = null;
            if (sender is Panel p) sourcePanel = p;
            else if (sender is PictureBox pb) sourcePanel = pb.Parent as Panel;

            if (draggedPanel != null && sourcePanel != null && draggedPanel == sourcePanel)
            {
                // Check if mouse moved enough to start drag operation
                if (Math.Abs(e.X - dragStartPoint.X) > 10 || Math.Abs(e.Y - dragStartPoint.Y) > 10)
                {
                    // Visual feedback during drag
                    // Capture draggedPanel in a local variable because DoDragDrop is blocking
                    // and draggedPanel might be set to null in the DragDrop event handler
                    Panel panelToDrag = draggedPanel;
                    var originalBorderStyle = panelToDrag.BorderStyle;
                    panelToDrag.BorderStyle = BorderStyle.Fixed3D;

                    DragDropEffects effect = panelToDrag.DoDragDrop(panelToDrag, DragDropEffects.Move);

                    // Reset appearance and cursor after drag
                    if (!panelToDrag.IsDisposed)
                    {
                        panelToDrag.BorderStyle = originalBorderStyle;
                        panelToDrag.Cursor = Cursors.Hand;
                    }
                    draggedPanel = null;
                }
            }
        }

        private void ContainerPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            // Reset drag state if mouse released without dragging
            if (draggedPanel != null)
            {
                draggedPanel.Cursor = Cursors.Hand;
                draggedPanel = null;
            }
        }

        private void ContainerPanel_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(Panel)))
            {
                e.Effect = DragDropEffects.Move;
            }
            else if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ContainerPanel_DragOver(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(Panel)))
            {
                e.Effect = DragDropEffects.Move;

                // Visual feedback: highlight the target panel
                Panel? targetPanel = null;
                if (sender is Panel p) targetPanel = p;
                else if (sender is PictureBox pb) targetPanel = pb.Parent as Panel;

                if (targetPanel != null)
                {
                    targetPanel.BackColor = Color.FromArgb(220, 235, 255);
                }
            }
            else if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void ContainerPanel_DragLeave(object? sender, EventArgs e)
        {
            Panel? targetPanel = null;
            if (sender is Panel p) targetPanel = p;
            else if (sender is PictureBox pb) targetPanel = pb.Parent as Panel;

            if (targetPanel != null)
            {
                bool isSelected = (selectedPictureBox != null && selectedPictureBox.Parent == targetPanel);
                targetPanel.BackColor = isSelected ? Color.FromArgb(230, 240, 255) : Color.White;
            }
        }

        private void ContainerPanel_DragDrop(object? sender, DragEventArgs e)
        {
            Panel? targetPanel = null;
            if (sender is Panel p) targetPanel = p;
            else if (sender is PictureBox pb) targetPanel = pb.Parent as Panel;

            if (targetPanel != null && e.Data != null && e.Data.GetData(typeof(Panel)) is Panel sourcePanel)
            {
                // Restore target panel color
                bool isSelected = (selectedPictureBox != null && selectedPictureBox.Parent == targetPanel);
                targetPanel.BackColor = isSelected ? Color.FromArgb(230, 240, 255) : Color.White;

                if (sourcePanel != targetPanel)
                {
                    int sourceIndex = flowLayoutPanel1.Controls.GetChildIndex(sourcePanel);
                    int targetIndex = flowLayoutPanel1.Controls.GetChildIndex(targetPanel);

                    // Move the source panel to target position
                    flowLayoutPanel1.Controls.SetChildIndex(sourcePanel, targetIndex);

                    // Update labels after reordering
                    UpdateImageLabels();
                }
            }
            else if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Form1_DragDrop(sender, e);
            }
            draggedPanel = null;
        }

        private void btnLoadFiles_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image and PDF files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.pdf|Image files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|PDF files|*.pdf";
                openFileDialog.Multiselect = true;
                openFileDialog.Title = "Select Images or PDF Files";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string filename in openFileDialog.FileNames)
                    {
                        try
                        {
                            if (filename.ToLower().EndsWith(".pdf"))
                            {
                                LoadPdfFile(filename);
                            }
                            else
                            {
                                System.Drawing.Image image = System.Drawing.Image.FromFile(filename);
                                AddImageToPanel(image);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error loading file {Path.GetFileName(filename)}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void LoadPdfFile(string pdfPath)
        {
            try
            {
                // Read PDF file as byte array to avoid base64 issues
                byte[] pdfBytes = File.ReadAllBytes(pdfPath);

                // Convert PDF pages to images using PDFtoImage with byte array
                using (var pdfStream = new MemoryStream(pdfBytes))
                {
                    var skBitmaps = PDFtoImage.Conversion.ToImages(pdfStream);

                    foreach (var skBitmap in skBitmaps)
                    {
                        // Convert SkiaSharp.SKBitmap to System.Drawing.Bitmap
                        using (var image = skBitmap.ToBitmap())
                        {
                            System.Drawing.Image clonedImage = new System.Drawing.Bitmap(image);
                            AddImageToPanel(clonedImage);
                        }
                        skBitmap.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading PDF: {ex.Message}\n\nPlease ensure the PDF file is valid and not password-protected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnWebcam_Click(object sender, EventArgs e)
        {
            try
            {
                videoCapture = new VideoCapture(0); // 0 for default camera

                if (!videoCapture.IsOpened())
                {
                    MessageBox.Show("No webcam detected or unable to open camera!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    videoCapture?.Dispose();
                    videoCapture = null;
                    return;
                }

                webcamForm = new Form()
                {
                    Text = "Webcam Capture",
                    Size = new System.Drawing.Size(800, 650),
                    StartPosition = FormStartPosition.CenterScreen,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false
                };

                PictureBox webcamPictureBox = new PictureBox()
                {
                    Size = new System.Drawing.Size(760, 520),
                    Location = new System.Drawing.Point(20, 20),
                    BorderStyle = BorderStyle.FixedSingle,
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                Button btnCapture = new Button()
                {
                    Text = "📸 Capture",
                    Size = new System.Drawing.Size(120, 35),
                    Location = new System.Drawing.Point(280, 560),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                Button btnClose = new Button()
                {
                    Text = "❌ Close",
                    Size = new System.Drawing.Size(120, 35),
                    Location = new System.Drawing.Point(420, 560),
                    BackColor = Color.FromArgb(232, 17, 35),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };

                webcamForm.Controls.Add(webcamPictureBox);
                webcamForm.Controls.Add(btnCapture);
                webcamForm.Controls.Add(btnClose);

                // Create timer to update frames
                webcamTimer = new System.Windows.Forms.Timer();
                webcamTimer.Interval = 33; // ~30 FPS
                webcamTimer.Tick += (s, args) =>
                {
                    if (videoCapture != null && videoCapture.IsOpened())
                    {
                        Mat frame = new Mat();
                        if (videoCapture.Read(frame) && !frame.Empty())
                        {
                            var oldImage = webcamPictureBox.Image;
                            webcamPictureBox.Image = BitmapConverter.ToBitmap(frame);
                            oldImage?.Dispose();
                            frame.Dispose();
                        }
                    }
                };

                btnCapture.Click += (s, args) =>
                {
                    if (webcamPictureBox.Image != null)
                    {
                        System.Drawing.Image capturedImage = (System.Drawing.Image)webcamPictureBox.Image.Clone();
                        AddImageToPanel(capturedImage);
                        // Image captured silently - no annoying popup
                    }
                };

                btnClose.Click += (s, args) =>
                {
                    webcamTimer?.Stop();
                    videoCapture?.Release();
                    videoCapture?.Dispose();
                    videoCapture = null;
                    webcamForm.Close();
                };

                webcamForm.FormClosing += (s, args) =>
                {
                    webcamTimer?.Stop();
                    webcamTimer?.Dispose();
                    webcamTimer = null;
                    videoCapture?.Release();
                    videoCapture?.Dispose();
                    videoCapture = null;
                };

                webcamTimer.Start();
                webcamForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening webcam: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                videoCapture?.Dispose();
                videoCapture = null;
            }
        }

        private void btnSaveToPDF_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanel1.Controls.Count == 0)
            {
                MessageBox.Show("No images to save!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "PDF files (*.pdf)|*.pdf";
                saveFileDialog.Title = "Save Images as PDF";
                saveFileDialog.FileName = $"ScannedDocument_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        PdfDocument document = new PdfDocument();

                        progressBar1.Visible = true;
                        progressBar1.Minimum = 0;
                        progressBar1.Maximum = flowLayoutPanel1.Controls.Count;
                        progressBar1.Value = 0;

                        for (int i = 0; i < flowLayoutPanel1.Controls.Count; i++)
                        {
                            Control control = flowLayoutPanel1.Controls[i];
                            if (control is Panel panel && panel.Tag is PictureBox pb && pb.Image != null)
                            {
                                PdfPage page = document.AddPage();
                                page.Width = XUnit.FromPoint(595);  // A4 width
                                page.Height = XUnit.FromPoint(842); // A4 height

                                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                                {
                                    using (MemoryStream ms = new MemoryStream())
                                    {
                                        pb.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        ms.Position = 0;
                                        XImage img = XImage.FromStream(ms);

                                        // Calculate scaling to fit page with margins
                                        double margin = 40;
                                        double maxWidth = page.Width.Point - (2 * margin);
                                        double maxHeight = page.Height.Point - (2 * margin);

                                        double scale = Math.Min(maxWidth / img.PixelWidth, maxHeight / img.PixelHeight);
                                        double width = img.PixelWidth * scale;
                                        double height = img.PixelHeight * scale;

                                        // Center the image
                                        double x = (page.Width.Point - width) / 2;
                                        double y = (page.Height.Point - height) / 2;

                                        gfx.DrawImage(img, x, y, width, height);
                                    }
                                }
                            }
                            progressBar1.Value = i + 1;
                            Application.DoEvents();
                        }

                        document.Save(saveFileDialog.FileName);
                        document.Close();

                        progressBar1.Visible = false;

                        MessageBox.Show($"PDF saved successfully!\n{saveFileDialog.FileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        progressBar1.Visible = false;
                        MessageBox.Show($"Error saving PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            if (selectedPictureBox == null || selectedPictureBox.Parent == null)
            {
                MessageBox.Show("Please select an image to delete first!", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show("Delete the selected image?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Panel? containerPanel = selectedPictureBox.Parent as Panel;
                if (containerPanel != null)
                {
                    if (selectedPictureBox.Image != null)
                    {
                        selectedPictureBox.Image.Dispose();
                    }
                    flowLayoutPanel1.Controls.Remove(containerPanel);
                    containerPanel.Dispose();
                    selectedPictureBox = null;

                    // Update image labels
                    UpdateImageLabels();
                }
            }
        }

        private void UpdateImageLabels()
        {
            for (int i = 0; i < flowLayoutPanel1.Controls.Count; i++)
            {
                Control control = flowLayoutPanel1.Controls[i];
                if (control is Panel panel)
                {
                    foreach (Control child in panel.Controls)
                    {
                        if (child is Label lbl)
                        {
                            lbl.Text = $"Image {i + 1}";
                        }
                    }
                }
            }
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            if (flowLayoutPanel1.Controls.Count == 0)
            {
                return;
            }

            var result = MessageBox.Show("Are you sure you want to clear all images?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                foreach (Control control in flowLayoutPanel1.Controls)
                {
                    if (control is Panel panel && panel.Tag is PictureBox pb && pb.Image != null)
                    {
                        pb.Image.Dispose();
                    }
                }
                flowLayoutPanel1.Controls.Clear();
                selectedPictureBox = null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            webcamTimer?.Stop();
            webcamTimer?.Dispose();
            videoCapture?.Release();
            videoCapture?.Dispose();
            base.OnFormClosing(e);
        }
    }
}