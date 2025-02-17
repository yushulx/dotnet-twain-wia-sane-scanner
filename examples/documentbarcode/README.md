# Scan Documents and Read Barcodes

This project demonstrates how to use the [Twain.Wia.Sane.Scanner](https://www.nuget.org/packages/Twain.Wia.Sane.Scanner/) and [Dynamsoft.DotNet.BarcodeReader.Bundle](https://www.nuget.org/packages/Dynamsoft.DotNet.BarcodeReader.Bundle) packages to scan documents and read barcodes from them in a .NET MAUI application.

## Features

- Scan documents using TWAIN, WIA, SANE, and ESCL protocols.
- Detect and read barcodes from scanned documents.
- Support for multiple barcode formats.

## Pre-requisites
- Install [Dynamsoft service for Windows](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.msi).
- Connect Your PC to a TWAIN, WIA, or SANE compatible scanner.
- Obtain a [Free trial license key](https://www.dynamsoft.com/customer/license/trialLicense/?product=dcv&package=cross-platform) for Dynamsoft Barcode Reader.

## Usage
1. Open the project in Visual Studio or Visual Studio Code.
2. Set the license key in `MainPage.xaml.cs`:
    ```csharp
    private static string licenseKey = "LICENSE-KEY";
    ```
2. Build and run the project.
    
    ![.NET MAUI document scanning and barcode reading app](https://www.dynamsoft.com/codepool/img/2025/02/maui-windows-scan-document-read-barcode.png)