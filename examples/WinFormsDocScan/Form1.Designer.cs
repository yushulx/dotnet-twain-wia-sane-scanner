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
            panel1 = new Panel();
            btnDeleteSelected = new Button();
            btnSaveToPDF = new Button();
            btnWebcam = new Button();
            btnLoadFiles = new Button();
            btnClearAll = new Button();
            button2 = new Button();
            comboBox1 = new ComboBox();
            button1 = new Button();
            label1 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(245, 245, 245);
            panel1.BorderStyle = BorderStyle.FixedSingle;
            panel1.Controls.Add(btnDeleteSelected);
            panel1.Controls.Add(btnSaveToPDF);
            panel1.Controls.Add(btnWebcam);
            panel1.Controls.Add(btnLoadFiles);
            panel1.Controls.Add(btnClearAll);
            panel1.Controls.Add(button2);
            panel1.Controls.Add(comboBox1);
            panel1.Controls.Add(button1);
            panel1.Controls.Add(label1);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(1200, 100);
            panel1.TabIndex = 0;
            // 
            // btnDeleteSelected
            // 
            btnDeleteSelected.BackColor = Color.FromArgb(255, 140, 0);
            btnDeleteSelected.FlatAppearance.BorderColor = Color.FromArgb(230, 126, 0);
            btnDeleteSelected.FlatStyle = FlatStyle.Flat;
            btnDeleteSelected.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnDeleteSelected.ForeColor = Color.White;
            btnDeleteSelected.Location = new Point(670, 55);
            btnDeleteSelected.Name = "btnDeleteSelected";
            btnDeleteSelected.Size = new Size(150, 38);
            btnDeleteSelected.TabIndex = 8;
            btnDeleteSelected.Text = "🗑️ Delete";
            btnDeleteSelected.UseVisualStyleBackColor = false;
            btnDeleteSelected.Click += btnDeleteSelected_Click;
            // 
            // btnSaveToPDF
            // 
            btnSaveToPDF.BackColor = Color.FromArgb(16, 137, 62);
            btnSaveToPDF.FlatAppearance.BorderColor = Color.FromArgb(14, 123, 56);
            btnSaveToPDF.FlatStyle = FlatStyle.Flat;
            btnSaveToPDF.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSaveToPDF.ForeColor = Color.White;
            btnSaveToPDF.Location = new Point(1020, 55);
            btnSaveToPDF.Name = "btnSaveToPDF";
            btnSaveToPDF.Size = new Size(160, 38);
            btnSaveToPDF.TabIndex = 7;
            btnSaveToPDF.Text = "💾 Save to PDF";
            btnSaveToPDF.UseVisualStyleBackColor = false;
            btnSaveToPDF.Click += btnSaveToPDF_Click;
            // 
            // btnWebcam
            // 
            btnWebcam.BackColor = Color.FromArgb(138, 43, 226);
            btnWebcam.FlatAppearance.BorderColor = Color.FromArgb(124, 39, 203);
            btnWebcam.FlatStyle = FlatStyle.Flat;
            btnWebcam.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnWebcam.ForeColor = Color.White;
            btnWebcam.Location = new Point(845, 55);
            btnWebcam.Name = "btnWebcam";
            btnWebcam.Size = new Size(150, 38);
            btnWebcam.TabIndex = 6;
            btnWebcam.Text = "📷 Webcam";
            btnWebcam.UseVisualStyleBackColor = false;
            btnWebcam.Click += btnWebcam_Click;
            // 
            // btnLoadFiles
            // 
            btnLoadFiles.BackColor = Color.FromArgb(0, 120, 215);
            btnLoadFiles.FlatAppearance.BorderColor = Color.FromArgb(0, 108, 193);
            btnLoadFiles.FlatStyle = FlatStyle.Flat;
            btnLoadFiles.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLoadFiles.ForeColor = Color.White;
            btnLoadFiles.Location = new Point(495, 55);
            btnLoadFiles.Name = "btnLoadFiles";
            btnLoadFiles.Size = new Size(150, 38);
            btnLoadFiles.TabIndex = 5;
            btnLoadFiles.Text = "📁 Load Files";
            btnLoadFiles.UseVisualStyleBackColor = false;
            btnLoadFiles.Click += btnLoadFiles_Click;
            // 
            // btnClearAll
            // 
            btnClearAll.BackColor = Color.FromArgb(232, 17, 35);
            btnClearAll.FlatAppearance.BorderColor = Color.FromArgb(208, 15, 31);
            btnClearAll.FlatStyle = FlatStyle.Flat;
            btnClearAll.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnClearAll.ForeColor = Color.White;
            btnClearAll.Location = new Point(1020, 10);
            btnClearAll.Name = "btnClearAll";
            btnClearAll.Size = new Size(160, 38);
            btnClearAll.TabIndex = 4;
            btnClearAll.Text = "🗑️ Clear All";
            btnClearAll.UseVisualStyleBackColor = false;
            btnClearAll.Click += btnClearAll_Click;
            // 
            // button2
            // 
            button2.BackColor = Color.FromArgb(0, 120, 215);
            button2.FlatAppearance.BorderColor = Color.FromArgb(0, 108, 193);
            button2.FlatStyle = FlatStyle.Flat;
            button2.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button2.ForeColor = Color.White;
            button2.Location = new Point(320, 55);
            button2.Name = "button2";
            button2.Size = new Size(150, 38);
            button2.TabIndex = 3;
            button2.Text = "🖨️ Scan";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // comboBox1
            // 
            comboBox1.DropDownHeight = 120;
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox1.Font = new Font("Segoe UI", 9.5F);
            comboBox1.FormattingEnabled = true;
            comboBox1.IntegralHeight = false;
            comboBox1.Location = new Point(20, 62);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(280, 25);
            comboBox1.TabIndex = 2;
            // 
            // button1
            // 
            button1.BackColor = Color.FromArgb(0, 120, 215);
            button1.FlatAppearance.BorderColor = Color.FromArgb(0, 108, 193);
            button1.FlatStyle = FlatStyle.Flat;
            button1.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            button1.ForeColor = Color.White;
            button1.Location = new Point(20, 10);
            button1.Name = "button1";
            button1.Size = new Size(160, 38);
            button1.TabIndex = 1;
            button1.Text = "🔍 Get Devices";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(0, 120, 215);
            label1.Location = new Point(200, 16);
            label1.Name = "label1";
            label1.Size = new Size(305, 30);
            label1.TabIndex = 0;
            label1.Text = "📄 Document Scanner Pro";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.BackColor = Color.FromArgb(250, 250, 250);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 100);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Padding = new Padding(15);
            flowLayoutPanel1.Size = new Size(1200, 600);
            flowLayoutPanel1.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1200, 700);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(panel1);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Document Scanner Pro";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Label label1;
        private Button button1;
        private ComboBox comboBox1;
        private Button button2;
        private Button btnClearAll;
        private Button btnLoadFiles;
        private Button btnWebcam;
        private Button btnSaveToPDF;
        private Button btnDeleteSelected;
        private FlowLayoutPanel flowLayoutPanel1;
    }
}