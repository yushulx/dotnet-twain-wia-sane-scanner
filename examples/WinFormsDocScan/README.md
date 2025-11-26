# 📄 Document Scanner Pro

A modern, feature-rich document scanning application built with .NET 8 and Windows Forms. Scan documents from TWAIN/WIA scanners, capture images from webcam, load images/PDFs from disk, and save everything as a PDF file.

https://github.com/user-attachments/assets/aec29d54-294c-4957-9919-ca47c1733fc7

## ✨ Features

### 📥 Multiple Input Methods
- **Scanner Support**: Scan documents directly from TWAIN/WIA compatible scanners
- **Webcam Capture**: Capture images using your computer's webcam
- **File Loading**: Load images (JPG, PNG, BMP, GIF, TIFF) and PDF files from disk
- **Drag & Drop**: Simply drag and drop image/PDF files onto the application window

### 🖼️ Image Management
- **Visual Gallery**: View all scanned/loaded images in a clean, organized gallery
- **Select & Highlight**: Click any image to select it (highlighted with blue background)
- **Delete Images**: Remove individual selected images or clear all at once
- **Reorder Images**: Drag and drop images to rearrange their order
- **Auto-numbering**: Images are automatically numbered and updated after changes

### 💾 Export & Save
- **PDF Export**: Save all images as a single PDF document
- **Auto-scaling**: Images automatically scale to fit A4 page size
- **Multi-page Support**: Each image becomes a separate page in the PDF

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- .NET 8.0 SDK or later
- Webcam (optional, for webcam capture feature)
- TWAIN/WIA compatible scanner (optional, for scanning feature)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yushulx/dotnet-twain-wia-sane-scanner.git
   cd dotnet-twain-wia-sane-scanner/examples/WinFormsDocScan
   ```

2. Obtain a [30-day free trial license](https://www.dynamsoft.com/customer/license/trialLicense/?product=dcv&package=cross-platform) and set the license key in `Form1.cs`:
   ```csharp
   private static string licenseKey = "YOUR_LICENSE_KEY_HERE";
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

   ![.NET document scanner](https://www.dynamsoft.com/codepool/img/2025/11/dotnet-document-scanner-webcam-file.png)
   
## 📖 Usage Guide

### Scanning Documents

1. Click **🔍 Get Devices** to detect available scanners
2. Select your scanner from the dropdown
3. Click **🖨️ Scan** to start scanning
4. Scanned images will appear in the gallery

### Capturing with Webcam

1. Click **📷 Webcam** to open the webcam window
2. Position your document in front of the camera
3. Click **📸 Capture** to take a snapshot
4. Click **❌ Close** when finished
5. Captured images appear in the main gallery

### Loading Files

**Method 1: Using the Load Button**
1. Click **📁 Load Files**
2. Select one or multiple images or PDF files
3. Click Open

**Method 2: Drag & Drop**
1. Drag image/PDF files from File Explorer
2. Drop them anywhere on the application window
3. Files are automatically loaded

### Managing Images

- **Select an Image**: Click on any image to select it (blue highlight)
- **Delete Selected**: Click **🗑️ Delete** to remove the selected image
- **Reorder Images**: Click and drag an image to a new position
- **Clear All**: Click **🗑️ Clear All** to remove all images (with confirmation)

### Saving as PDF

1. Click **💾 Save to PDF**
2. Choose a location and filename
3. Click Save
4. All images are saved as a multi-page PDF

## 🛠️ Technologies Used

- **.NET 8.0**: Modern cross-platform framework
- **Windows Forms**: Desktop UI framework
- **OpenCvSharp4**: Webcam capture and image processing
- **PDFtoImage**: High-quality PDF to image conversion
- **PdfSharp**: PDF document creation
- **SkiaSharp**: Cross-platform 2D graphics
- **Twain.Wia.Sane.Scanner**: Scanner integration

