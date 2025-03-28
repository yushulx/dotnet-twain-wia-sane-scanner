# .NET Document Scanner for TWAIN, WIA, SANE, ICA, and eSCL
The package provides methods for calling [Dynamic Web TWAIN Service REST APIs](https://www.dynamsoft.com/blog/announcement/dynamsoft-service-restful-api/). This allows developers to build .NET applications for digitizing documents from **TWAIN (32-bit/64-bit)**, **WIA**, **SANE**, **ICA** and **eSCL** scanners.

https://github.com/yushulx/dotnet-twain-wia-sane-scanner/assets/2202306/1046f5f4-2009-4905-95b5-c750195df715

## Prerequisites
1. Install Dynamic Web TWAIN Service.
    - Windows: [Dynamsoft-Service-Setup.msi](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.msi)
    - macOS: [Dynamsoft-Service-Setup.pkg](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.pkg)
    - Linux: 
        - [Dynamsoft-Service-Setup.deb](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.deb)
        - [Dynamsoft-Service-Setup-arm64.deb](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup-arm64.deb)
        - [Dynamsoft-Service-Setup-mips64el.deb](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup-mips64el.deb)
        - [Dynamsoft-Service-Setup.rpm](https://demo.dynamsoft.com/DWT/DWTResources/dist/DynamsoftServiceSetup.rpm)
        
2. Request a [free trial license](https://www.dynamsoft.com/customer/license/trialLicense?product=dwt).

## Dynamic Web TWAIN Service REST API
By default, the REST API's host address is set to `http://127.0.0.1:18622`. 

| Method | Endpoint        | Description                   | Parameters                         | Response                      |
|--------|-----------------|-------------------------------|------------------------------------|-------------------------------|
| GET    | `/DWTAPI/Scanners`    | Get a list of scanners  | None                               | `200 OK` with scanner list       |
| POST   | `/DWTAPI/ScanJobs`    | Creates a scan job      | `license`, `device`, `config`      | `201 Created` with job ID    |
| GET    | `/DWTAPI/ScanJobs/:id/NextDocument`| Retrieves a document image     | `id`: Job ID   | `200 OK` with image stream    |
| DELETE | `/DWTAPI/ScanJobs/:id`| Deletes a scan job       | `id`: Job ID                      | `200 OK`              |

You can navigate to `http://127.0.0.1:18625/` to access the service. To make it accessible from desktop, mobile, and web applications on the same network, you can change the host address to a LAN IP address. For example, you might use `http://192.168.8.72`.

![dynamsoft-service-config](https://user-images.githubusercontent.com/2202306/266243200-e2b1292e-dfbd-4821-bf41-70e2847dd51e.png)

The scanner parameter configuration is based on [Dynamsoft Web TWAIN documentation](https://www.dynamsoft.com/web-twain/docs/info/api/Interfaces.html#DeviceConfiguration). 

## Quick Start
Replace the license key in the code below with a valid one and run the code.

```csharp
using System;
using System.Collections.Generic;
using Twain.Wia.Sane.Scanner;

public class Program
{
    private static string licenseKey = "LICENSE-KEY";
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
                var scanners = await scannerController.GetDevices(host, ScannerType.TWAINSCANNER | ScannerType.TWAINX64SCANNER);
                devices.Clear();
                for (int i = 0; i < scanners.Count; i++)
                {
                    var scanner = scanners[i];
                    devices.Add(scanner);
                    Console.WriteLine($"\nIndex: {i}, Name: {scanner["name"]}");
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
                    {"device", devices[index]["device"]}
                };

                parameters["config"] = new Dictionary<string, object>
                {
                    {"IfShowUI", false},
                    {"PixelType", 2},
                    {"Resolution", 200},
                    {"IfFeederEnabled", false},
                    {"IfDuplexEnabled", false}
                };

                var text = await scannerController.ScanDocument(host, parameters);
                string jobId = "";
                if (text.ContainsKey(ScannerController.SCAN_SUCCESS))
                {
                    jobId = text[ScannerController.SCAN_SUCCESS];
                }

                string error = "";
                if (text.ContainsKey(ScannerController.SCAN_ERROR))
                {
                    error = text[ScannerController.SCAN_ERROR];
                }

                if (!string.IsNullOrEmpty(jobId))
                {
                    var images = await scannerController.GetImageFiles(host, jobId, "./");
                    for (int i = 0; i < images.Count; i++)
                    {
                        Console.WriteLine($"Image {i}: {images[i]}");
                    }

                    scannerController.DeleteJob(host, jobId);
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error: {error}");
                }
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

## Example
- [command-line](https://github.com/yushulx/dotnet-twain-wia-sane-scanner/tree/main/examples/command-line)
- [.NET MAUI](https://github.com/yushulx/dotnet-twain-wia-sane-scanner/tree/main/examples/MauiAppDocScan)
  
    ![dotnet-maui-twain-document-scanner](https://github.com/yushulx/dotnet-twain-wia-sane-scanner/assets/2202306/5df6b2de-80f0-45b7-b8f9-3a394c07153c)


- [WinForm](https://github.com/yushulx/dotnet-twain-wia-sane-scanner/tree/main/examples/WinFormsDocScan)
  
    ![dotnet-winform-twain-document-scanner](https://user-images.githubusercontent.com/2202306/273767486-c79fe659-9049-4ee8-b76a-24881d48140c.png)


## API
- `public async Task<List<Dictionary<string, object>>> GetDevices(string host, int? scannerType = null)`: Get a list of available devices.
- `public async Task<HttpResponseMessage> GetDevicesHttpResponse(string host, int? scannerType = null)`: Return the HTTP response of getting devices.
- `public async Task<Dictionary<string, string>> ScanDocument(string host, Dictionary<string, object> parameters)`: Initiate the scanner operation and return a dictionary containing either the job ID or an error message.
- `public async Task<HttpResponseMessage> ScanDocumentHttpResponse(string host, Dictionary<string, object> parameters)`: Return the HTTP response of initiating the scanner operation.
- `public async Task<HttpResponseMessage> DeleteJob(string host, string jobId)`: Return the HTTP response of deleting a job.
- `public async Task<string> GetImageFile(string host, string jobId, string directory)`: Get an image file.
- `public async Task<List<string>> GetImageFiles(string host, string jobId, string directory)`: Get a list of image files.
- `public async Task<List<byte[]>> GetImageStreams(string host, string jobId)`: Get a list of image streams.
- `public async Task<<byte[]> GetImageStream(string host, string jobId)`: Get an image stream.
- `public async Task<HttpResponseMessage> GetImageStreamHttpResponse(string host, string jobId)` : Return the HTTP response of getting an image stream.


## Build the NuGet Package

```bash
dotnet build --configuration Release
```
