using System.Diagnostics;
using System.Text.Json;
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
        Console.WriteLine($"Server info: {info["version"]}");
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

                var job = await scannerController.CreateJob(host, parameters);

                string error = "";
                if (job.ContainsKey(ScannerController.SCAN_ERROR))
                {
                    error = (string)job[ScannerController.SCAN_ERROR];
                    Debug.WriteLine($"Error: {error}");
                    continue;
                }

                string jobId = "";
                JsonElement jobUidElement = (JsonElement)job["jobuid"];
                jobId = jobUidElement.GetString();

                //var checkJob = await scannerController.CheckJob(host, jobId);

                //var caps = await scannerController.GetScannerCapabilities(host, jobId);

                //var status = new Dictionary<string, object>() {
                //    { "status", JobStatus.RUNNING}    
                //};
                //var updateJob = await scannerController.UpdateJob(host, jobId, status);

                var doc = await scannerController.CreateDocument(host, new Dictionary<string, object>());

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