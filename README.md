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
using Newtonsoft.Json;
using Twain.Wia.Sane.Scanner;

public class Program
{
    private static string licenseKey = "DLS2eyJoYW5kc2hha2VDb2RlIjoiMjAwMDAxLTE2NDk4Mjk3OTI2MzUiLCJvcmdhbml6YXRpb25JRCI6IjIwMDAwMSIsInNlc3Npb25QYXNzd29yZCI6IndTcGR6Vm05WDJrcEQ5YUoifQ==";
    private static ScannerController scannerController = new ScannerController();
    private static List<Dictionary<string, object>> devices = new List<Dictionary<string, object>>();
    private static string host = "http://127.0.0.1:18622";
    private static string questions = @"
Please select an operation:
1. Get scanners
2. Acquire documents by scanner index
3. Quit
";

    public static async Task Main()
    {
        var info = await scannerController.GetServerInfo(host);
        Console.WriteLine($"Server info: {info}");
        await AskQuestion();
    }

    private static async Task<int> AskQuestion()
    {
        while (true)
        {
            Console.WriteLine(".............................................");
            Console.WriteLine(questions);
            string? answer = Console.ReadLine();

            if (string.IsNullOrEmpty(answer))
            {
                continue;
            }

            if (answer == "3")
            {
                break;
            }
            else if (answer == "1")
            {
                var scannerInfo = await scannerController.GetDevices(host, ScannerType.TWAINSCANNER | ScannerType.TWAINX64SCANNER);
                devices.Clear();

                try { 
                    var scanners = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(scannerInfo);
                    for (int i = 0; i < scanners.Count; i++)
                    {
                        var scanner = scanners[i];
                        devices.Add(scanner);
                        Console.WriteLine($"\nIndex: {i}, Name: {scanner["name"]}");
                    }
                } catch (Exception ex) { 
                   Console.WriteLine($"Error: {ex.Message}");
                }
                
            }
            else if (answer == "2")
            {
                if (devices.Count == 0)
                {
                    Console.WriteLine("Please get scanners first!\n");
                    continue;
                }

                Console.Write($"\nSelect an index (<= {devices.Count - 1}): ");
                int index;
                if (!int.TryParse(Console.ReadLine(), out index))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                    continue;
                }

                if (index < 0 || index >= devices.Count)
                {
                    Console.WriteLine("It is out of range.");
                    continue;
                }

                var parameters = new Dictionary<string, object>
                {
                    {"license", licenseKey},
                    {"device", devices[index]["device"]},
                    {"autoRun", true}
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
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    continue;
                }

                var images = await scannerController.GetImageFiles(host, jobId, "./");
                for (int i = 0; i < images.Count; i++)
                {
                    Console.WriteLine($"Image {i}: {images[i]}");
                }

                await scannerController.DeleteJob(host, jobId);
            }
            else
            {
                continue;
            }
        }
        return 0;
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