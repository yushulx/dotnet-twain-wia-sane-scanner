namespace WinFormsDocScan
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // Create main container panels
            mainSplitContainer = new SplitContainer();
            rightSplitContainer = new SplitContainer();
            operationPanel = new Panel();

            // Scanner controls
            scannerGroupBox = new GroupBox();
            button1 = new Button();
            comboBox1 = new ComboBox();
            button2 = new Button();
            loadImageButton = new Button();
            deleteImageButton = new Button();
            deleteAllButton = new Button();

            // OCR controls
            ocrGroupBox = new GroupBox();
            languageComboBox = new ComboBox();
            languageLabel = new Label();
            ocrButton = new Button();
            clearTextButton = new Button();

            // Display areas
            imagePanel = new Panel();
            flowLayoutPanel1 = new FlowLayoutPanel();
            imageLabel = new Label();
            textPanel = new Panel();
            ocrTextBox = new TextBox();
            textLabel = new Label();

            // Configure split containers
            ((System.ComponentModel.ISupportInitialize)(mainSplitContainer)).BeginInit();
            mainSplitContainer.Panel1.SuspendLayout();
            mainSplitContainer.Panel2.SuspendLayout();
            mainSplitContainer.SuspendLayout();

            ((System.ComponentModel.ISupportInitialize)(rightSplitContainer)).BeginInit();
            rightSplitContainer.Panel1.SuspendLayout();
            rightSplitContainer.Panel2.SuspendLayout();
            rightSplitContainer.SuspendLayout();

            scannerGroupBox.SuspendLayout();
            ocrGroupBox.SuspendLayout();
            imagePanel.SuspendLayout();
            textPanel.SuspendLayout();
            SuspendLayout();

            // 
            // mainSplitContainer - Horizontal split: Images (left) and Controls (right) - 2:1 ratio
            // 
            mainSplitContainer.Dock = DockStyle.Fill;
            mainSplitContainer.FixedPanel = FixedPanel.Panel2;
            mainSplitContainer.Location = new Point(0, 0);
            mainSplitContainer.Name = "mainSplitContainer";
            mainSplitContainer.Orientation = Orientation.Vertical;
            mainSplitContainer.Panel1.Controls.Add(imagePanel);
            mainSplitContainer.Panel2.Controls.Add(rightSplitContainer);
            mainSplitContainer.Size = new Size(1200, 800);
            mainSplitContainer.SplitterDistance = 800;
            mainSplitContainer.TabIndex = 0;

            // 
            // rightSplitContainer - Vertical split: Operations (top) and Results (bottom) - 1:1 ratio
            // 
            rightSplitContainer.Dock = DockStyle.Fill;
            rightSplitContainer.FixedPanel = FixedPanel.Panel1;
            rightSplitContainer.Location = new Point(0, 0);
            rightSplitContainer.Name = "rightSplitContainer";
            rightSplitContainer.Orientation = Orientation.Horizontal;
            rightSplitContainer.Panel1.Controls.Add(operationPanel);
            rightSplitContainer.Panel2.Controls.Add(textPanel);
            rightSplitContainer.Size = new Size(396, 800);
            rightSplitContainer.SplitterDistance = 450;
            rightSplitContainer.TabIndex = 0;

            // 
            // operationPanel - Contains grouped scanner and OCR controls
            // 
            operationPanel.Controls.Add(ocrGroupBox);
            operationPanel.Controls.Add(scannerGroupBox);
            operationPanel.Dock = DockStyle.Fill;
            operationPanel.Location = new Point(0, 0);
            operationPanel.Name = "operationPanel";
            operationPanel.Padding = new Padding(10);
            operationPanel.Size = new Size(396, 450);
            operationPanel.TabIndex = 0;
            operationPanel.BorderStyle = BorderStyle.FixedSingle;

            // 
            // scannerGroupBox
            // 
            scannerGroupBox.Controls.Add(deleteAllButton);
            scannerGroupBox.Controls.Add(deleteImageButton);
            scannerGroupBox.Controls.Add(loadImageButton);
            scannerGroupBox.Controls.Add(button2);
            scannerGroupBox.Controls.Add(comboBox1);
            scannerGroupBox.Controls.Add(button1);
            scannerGroupBox.Location = new Point(13, 13);
            scannerGroupBox.Name = "scannerGroupBox";
            scannerGroupBox.Size = new Size(370, 160);
            scannerGroupBox.TabIndex = 0;
            scannerGroupBox.TabStop = false;
            scannerGroupBox.Text = "Scanner & Image Operations";

            // 
            // button1 - Get Devices
            // 
            button1.Location = new Point(6, 22);
            button1.Name = "button1";
            button1.Size = new Size(110, 30);
            button1.TabIndex = 0;
            button1.Text = "Get Devices";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;

            // 
            // comboBox1
            // 
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(125, 22);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(235, 23);
            comboBox1.TabIndex = 1;

            // 
            // button2 - Scan Document
            // 
            button2.Location = new Point(6, 58);
            button2.Name = "button2";
            button2.Size = new Size(110, 30);
            button2.TabIndex = 2;
            button2.Text = "Scan Document";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;

            // 
            // loadImageButton
            // 
            loadImageButton.Location = new Point(125, 58);
            loadImageButton.Name = "loadImageButton";
            loadImageButton.Size = new Size(110, 30);
            loadImageButton.TabIndex = 3;
            loadImageButton.Text = "Load Files";
            loadImageButton.UseVisualStyleBackColor = true;
            loadImageButton.Click += loadImageButton_Click;

            // 
            // deleteImageButton
            // 
            deleteImageButton.Location = new Point(6, 94);
            deleteImageButton.Name = "deleteImageButton";
            deleteImageButton.Size = new Size(110, 30);
            deleteImageButton.TabIndex = 4;
            deleteImageButton.Text = "Delete Selected";
            deleteImageButton.UseVisualStyleBackColor = true;
            deleteImageButton.Enabled = false;
            deleteImageButton.Click += deleteImageButton_Click;

            // 
            // deleteAllButton
            // 
            deleteAllButton.Location = new Point(125, 94);
            deleteAllButton.Name = "deleteAllButton";
            deleteAllButton.Size = new Size(110, 30);
            deleteAllButton.TabIndex = 5;
            deleteAllButton.Text = "Delete All";
            deleteAllButton.UseVisualStyleBackColor = true;
            deleteAllButton.Click += deleteAllButton_Click;

            // 
            // ocrGroupBox
            // 
            ocrGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ocrGroupBox.Controls.Add(clearTextButton);
            ocrGroupBox.Controls.Add(ocrButton);
            ocrGroupBox.Controls.Add(languageComboBox);
            ocrGroupBox.Controls.Add(languageLabel);
            ocrGroupBox.Location = new Point(13, 185);
            ocrGroupBox.Name = "ocrGroupBox";
            ocrGroupBox.Size = new Size(370, 94);
            ocrGroupBox.TabIndex = 1;
            ocrGroupBox.TabStop = false;
            ocrGroupBox.Text = "OCR Operations";

            // 
            // languageLabel
            // 
            languageLabel.AutoSize = true;
            languageLabel.Location = new Point(6, 28);
            languageLabel.Name = "languageLabel";
            languageLabel.Size = new Size(62, 15);
            languageLabel.TabIndex = 0;
            languageLabel.Text = "Language:";

            // 
            // languageComboBox
            // 
            languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            languageComboBox.FormattingEnabled = true;
            languageComboBox.Location = new Point(74, 25);
            languageComboBox.Name = "languageComboBox";
            languageComboBox.Size = new Size(150, 23);
            languageComboBox.TabIndex = 1;

            // 
            // ocrButton
            // 
            ocrButton.Location = new Point(6, 54);
            ocrButton.Name = "ocrButton";
            ocrButton.Size = new Size(100, 30);
            ocrButton.TabIndex = 2;
            ocrButton.Text = "Run OCR";
            ocrButton.UseVisualStyleBackColor = true;
            ocrButton.Click += ocrButton_Click;

            // 
            // clearTextButton
            // 
            clearTextButton.Location = new Point(112, 54);
            clearTextButton.Name = "clearTextButton";
            clearTextButton.Size = new Size(100, 30);
            clearTextButton.TabIndex = 3;
            clearTextButton.Text = "Clear Text";
            clearTextButton.UseVisualStyleBackColor = true;
            clearTextButton.Click += clearTextButton_Click;

            // 
            // imagePanel - Contains scanned images
            // 
            imagePanel.Controls.Add(imageLabel);
            imagePanel.Controls.Add(flowLayoutPanel1);
            imagePanel.Dock = DockStyle.Fill;
            imagePanel.Location = new Point(0, 0);
            imagePanel.Name = "imagePanel";
            imagePanel.Size = new Size(800, 800);
            imagePanel.TabIndex = 0;
            imagePanel.BorderStyle = BorderStyle.FixedSingle;

            // 
            // imageLabel
            // 
            imageLabel.BackColor = SystemColors.ControlDark;
            imageLabel.Dock = DockStyle.Top;
            imageLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            imageLabel.ForeColor = SystemColors.ControlLightLight;
            imageLabel.Location = new Point(0, 0);
            imageLabel.Name = "imageLabel";
            imageLabel.Padding = new Padding(5);
            imageLabel.Size = new Size(598, 25);
            imageLabel.TabIndex = 1;
            imageLabel.Text = "📄 Scanned Images (Click to select for OCR)";
            imageLabel.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(0, 25);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(10);
            flowLayoutPanel1.Size = new Size(598, 649);
            flowLayoutPanel1.TabIndex = 0;
            flowLayoutPanel1.WrapContents = false;

            // 
            // textPanel - Contains OCR text results
            // 
            textPanel.Controls.Add(textLabel);
            textPanel.Controls.Add(ocrTextBox);
            textPanel.Dock = DockStyle.Fill;
            textPanel.Location = new Point(0, 0);
            textPanel.Name = "textPanel";
            textPanel.Size = new Size(396, 346);
            textPanel.TabIndex = 0;
            textPanel.BorderStyle = BorderStyle.FixedSingle;

            // 
            // textLabel
            // 
            textLabel.BackColor = SystemColors.ControlDark;
            textLabel.Dock = DockStyle.Top;
            textLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            textLabel.ForeColor = SystemColors.ControlLightLight;
            textLabel.Location = new Point(0, 0);
            textLabel.Name = "textLabel";
            textLabel.Padding = new Padding(5);
            textLabel.Size = new Size(394, 25);
            textLabel.TabIndex = 1;
            textLabel.Text = "📝 OCR Text Results";
            textLabel.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // ocrTextBox
            // 
            ocrTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ocrTextBox.Font = new Font("Segoe UI", 10F);
            ocrTextBox.Location = new Point(15, 35);
            ocrTextBox.Multiline = true;
            ocrTextBox.Name = "ocrTextBox";
            ocrTextBox.ReadOnly = true;
            ocrTextBox.ScrollBars = ScrollBars.Both;
            ocrTextBox.Size = new Size(366, 296);
            ocrTextBox.TabIndex = 0;
            ocrTextBox.WordWrap = true;

            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 800);
            Controls.Add(mainSplitContainer);
            MinimumSize = new Size(1000, 700);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Document Scanner with OCR - Three Panel Layout";

            mainSplitContainer.Panel1.ResumeLayout(false);
            mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(mainSplitContainer)).EndInit();
            mainSplitContainer.ResumeLayout(false);

            rightSplitContainer.Panel1.ResumeLayout(false);
            rightSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(rightSplitContainer)).EndInit();
            rightSplitContainer.ResumeLayout(false);

            scannerGroupBox.ResumeLayout(false);
            scannerGroupBox.PerformLayout();
            ocrGroupBox.ResumeLayout(false);
            ocrGroupBox.PerformLayout();
            imagePanel.ResumeLayout(false);
            textPanel.ResumeLayout(false);
            textPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private ComboBox comboBox1;
        private Button button2;
        private Button loadImageButton;
        private Button deleteImageButton;
        private Button deleteAllButton;
        private FlowLayoutPanel flowLayoutPanel1;
        private SplitContainer mainSplitContainer;
        private SplitContainer rightSplitContainer;
        private Panel operationPanel;
        private GroupBox scannerGroupBox;
        private GroupBox ocrGroupBox;
        private ComboBox languageComboBox;
        private Label languageLabel;
        private Button ocrButton;
        private Button clearTextButton;
        private Panel imagePanel;
        private Panel textPanel;
        private Label imageLabel;
        private Label textLabel;
        private TextBox ocrTextBox;
    }
}