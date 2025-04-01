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
                    {"IfFeederEnabled", true},
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

                //var checkJob = await scannerController.CheckJob(host, jobId);
                //Console.WriteLine($"Check job: {checkJob}");

                //var caps = await scannerController.GetScannerCapabilities(host, jobId);
                //Console.WriteLine($"Capabilities: {caps}");

                //var status = new Dictionary<string, object>() {
                //    { "status", JobStatus.RUNNING}
                //};
                //var updateJob = await scannerController.UpdateJob(host, jobId, status);
                //Console.WriteLine($"Update job: {updateJob}");

                var docInfo = await scannerController.CreateDocument(host, new Dictionary<string, object>());
                string docId = "";
                try
                {
                    var doc = JsonConvert.DeserializeObject<Dictionary<string, object>>(docInfo);
                    docId = (string)doc["uid"];

                    if (string.IsNullOrEmpty(docId))
                    {
                        Console.WriteLine("Failed to create a document.");
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

                    var imageInfo = await scannerController.GetImageInfo(host, jobId);
                    //Console.WriteLine($"Image info: {imageInfo}");

                    var image = JsonConvert.DeserializeObject<Dictionary<string, object>>(imageInfo);

                    parameters = new Dictionary<string, object>
                    {
                        {"password", ""},
                        {"source", image["url"]}
                    };

                    var insertPage = await scannerController.InsertPage(host, docId, parameters);
                    //Console.WriteLine($"Insert page: {insertPage}");

                    //var pageList = JsonConvert.DeserializeObject<Dictionary<string, object>>(insertPage);
                    //var pages = (JArray)pageList["pages"];
                    //JObject firstPage = (JObject)pages[0];
                    //string uid = firstPage["uid"]?.ToString();
                    //var deletePage = await scannerController.DeletePage(host, docId, uid);
                }

                //var info = await scannerController.GetDocumentInfo(host, docId);
                //Console.WriteLine($"Document info: {info}");

                var docFile = await scannerController.GetDocumentFile(host, docId, "./");
                Console.WriteLine($"Document file: {docFile}");

                await scannerController.DeleteDocument(host, docId);
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