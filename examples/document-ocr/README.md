# .NET Document Scanner with OCR

A Windows Forms application for document scanning and **Optical Character Recognition (OCR)** built with **.NET 8** and **C#**.

## Prerequisites
- Install [Dynamic Web TWAIN Service for Windows](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.msi)  
- Get a [free trial license](https://www.dynamsoft.com/customer/license/trialLicense/?product=dcv&package=cross-platform)

## Features

### 📄 Document Scanning
- **TWAIN Scanner Support**: Connect and scan documents using TWAIN-compatible scanners

### 🖼️ Image Management
- **Load Images**: Import existing image files from your system
- **Image Gallery**: View all scanned and loaded images in an organized flow layout
- **Interactive Selection**: Click to select images for OCR processing
- **Delete Operations**: Remove individual images or clear all images with confirmation

### 🔤 Optical Character Recognition (OCR)
- **Multi-Language Support**: Powered by **Windows.Media.Ocr** API
- **Language Selection**: Choose from available OCR languages (`C:\Windows\OCR`)
- **Results Display**: View extracted text in a dedicated results panel

## Build and Run
1. Clone or download the project
2. Navigate to the project directory:
   ```bash
   cd document-ocr
   ```
3. Restore NuGet packages:
   ```bash
   dotnet restore
   ```
4. Build the application:
   ```bash
   dotnet build
   ```
5. Run the application:
   ```bash
   dotnet run
   ```

