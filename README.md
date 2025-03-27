# .NET Document Scanner for TWAIN, WIA, SANE, ICA, and eSCL

This .NET package provides a wrapper for calling the **Dynamic Web TWAIN Service REST API**. It enables developers to create **desktop** or **cross-platform** applications to scan and digitize documents using:

- **TWAIN (32-bit / 64-bit)**
- **WIA (Windows Image Acquisition)**
- **SANE (Linux)**
- **ICA (macOS)**
- **eSCL (AirScan / Mopria)**

https://github.com/yushulx/dotnet-twain-wia-sane-scanner/assets/2202306/1046f5f4-2009-4905-95b5-c750195df715

---

## ⚙️ Prerequisites

### ✅ Install Dynamic Web TWAIN Service

- **Windows**: [Dynamsoft-Service-Setup.msi](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.msi)  
- **macOS**: [Dynamsoft-Service-Setup.pkg](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.pkg)  
- **Linux**:  
  - [Dynamsoft-Service-Setup.deb](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.deb)  
  - [Dynamsoft-Service-Setup-arm64.deb](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup-arm64.deb)  
  - [Dynamsoft-Service-Setup-mips64el.deb](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup-mips64el.deb)  
  - [Dynamsoft-Service-Setup.rpm](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.rpm)

### 🔑 Get a License

Request a [free trial license](https://www.dynamsoft.com/customer/license/trialLicense?product=dwt).

---

## 🧩 Configuration

After installation, open `http://127.0.0.1:18625/` in your browser to configure the **host** and **port** settings.

> By default, the service is bound to `127.0.0.1`. To access it across the LAN, change the host to your local IP (e.g., `192.168.8.72`).

![dynamsoft-service-config](https://github.com/yushulx/dynamsoft-service-REST-API/assets/2202306/e2b1292e-dfbd-4821-bf41-70e2847dd51e)

---

## 📡 REST API Endpoints

[https://www.dynamsoft.com/web-twain/docs/info/api/restful.html](https://www.dynamsoft.com/web-twain/docs/info/api/restful.html)

## 🧪 Quick Start

Replace the license key in the following code and run it in a .NET project:

```csharp
using System;
using System.Collections.Generic;
using Twain.Wia.Sane.Scanner;

public class Program
{
    private static string licenseKey = "LICENSE-KEY";
    private static ScannerController scannerController = new ScannerController();
    private static List<Dictionary<string, object>> devices = new();
    private static string host = "http://127.0.0.1:18622";

    public static async Task Main()
    {
        Console.WriteLine("1. Get scanners\n2. Scan document\n3. Quit");

        while (true)
        {
            var input = Console.ReadLine();
            if (input == "1")
            {
                var scanners = await scannerController.GetDevices(host, ScannerType.TWAINSCANNER | ScannerType.TWAINX64SCANNER);
                devices.Clear();
                for (int i = 0; i < scanners.Count; i++)
                {
                    devices.Add(scanners[i]);
                    Console.WriteLine($"Index: {i}, Name: {scanners[i]["name"]}");
                }
            }
            else if (input == "2")
            {
                if (devices.Count == 0)
                {
                    Console.WriteLine("Run step 1 to get scanners first.");
                    continue;
                }

                Console.Write("Select index: ");
                int.TryParse(Console.ReadLine(), out int index);
                if (index < 0 || index >= devices.Count)
                {
                    Console.WriteLine("Invalid index.");
                    continue;
                }

                var parameters = new Dictionary<string, object>
                {
                    ["license"] = licenseKey,
                    ["device"] = devices[index]["device"],
                    ["config"] = new Dictionary<string, object>
                    {
                        {"IfShowUI", false},
                        {"PixelType", 2},
                        {"Resolution", 200},
                        {"IfFeederEnabled", false},
                        {"IfDuplexEnabled", false}
                    }
                };

                var response = await scannerController.CreateJob(host, parameters);
                if (response.ContainsKey(ScannerController.SCAN_SUCCESS))
                {
                    var jobObj = (Dictionary<string, object>)response[ScannerController.SCAN_SUCCESS];
                    string jobId = jobObj["jobuid"].ToString();

                    var images = await scannerController.GetImageFiles(host, jobId, "./");
                    foreach (var img in images)
                    {
                        Console.WriteLine($"Image saved: {img}");
                    }

                    await scannerController.DeleteJob(host, jobId);
                }
                else
                {
                    Console.WriteLine("Scan failed: " + response[ScannerController.SCAN_ERROR]);
                }
            }
            else if (input == "3")
            {
                break;
            }
        }
    }
}
```

---

## 📁 Examples

- 🔧 [Command-line](https://github.com/yushulx/dotnet-twain-wia-sane-scanner/tree/main/examples/command-line)
- 📱 [.NET MAUI](https://github.com/yushulx/dotnet-twain-wia-sane-scanner/tree/main/examples/MauiAppDocScan)

    ![maui-doc-scan](https://github.com/yushulx/dotnet-twain-wia-sane-scanner/assets/2202306/5df6b2de-80f0-45b7-b8f9-3a394c07153c)

- 🪟 [WinForms](https://github.com/yushulx/dotnet-twain-wia-sane-scanner/tree/main/examples/WinFormsDocScan)

    ![winform-doc-scan](https://user-images.githubusercontent.com/2202306/273767486-c79fe659-9049-4ee8-b76a-24881d48140c.png)

---

## 🧩 API Reference

### 🎛️ Scanner APIs

- `GetDevices(string host, int? scannerType = null)`
- `CreateJob(string host, Dictionary<string, object> parameters)`
- `CheckJob(string host, string jobId)`
- `UpdateJob(string host, string jobId, Dictionary<string, object> parameters)`
- `DeleteJob(string host, string jobId)`
- `GetScannerCapabilities(string host, string jobId)`
- `GetImageInfo(string host, string jobId)`

### 📸 Image APIs

- `GetImageFile(string host, string jobId, string directory)`
- `GetImageFiles(string host, string jobId, string directory)`
- `GetImageStream(string host, string jobId)`
- `GetImageStreams(string host, string jobId)`

### 📄 Document APIs

- `CreateDocument(string host, Dictionary<string, object> parameters)`
- `GetDocumentInfo(string host, string docId)`
- `DeleteDocument(string host, string docId)`
- `GetDocumentFile(string host, string docId, string directory)`
- `GetDocumentStream(string host, string docId)`
- `InsertPage(string host, string docId, Dictionary<string, object> parameters)`
- `DeletePage(string host, string docId, string pageId)`

---

## 📦 Build the NuGet Package

```bash
dotnet build --configuration Release
```