using System.Diagnostics;
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
2. Acquire documents (blocking)
3. Acquire documents (non-blocking)
4. Quit
";

    public static async Task Main()
    {
        var info = await scannerController.GetServerInfo(host);
        Console.WriteLine($"Server info: {info}");
        await AskQuestion();
    }

    // ---------------------------------------------------------------
    // Shared helpers
    // ---------------------------------------------------------------

    private static int PromptScannerIndex()
    {
        Console.Write($"\nSelect a scanner index (<= {devices.Count - 1}): ");
        if (!int.TryParse(Console.ReadLine(), out int index))
        {
            Console.WriteLine("Invalid input. Please enter a number.");
            return -1;
        }
        if (index < 0 || index >= devices.Count)
        {
            Console.WriteLine("Index is out of range.");
            return -1;
        }
        return index;
    }

    private static async Task<(string jobId, string docId)> SetupJobAndDocument(int deviceIndex)
    {
        var parameters = new Dictionary<string, object>
        {
            {"license", licenseKey},
            {"device", devices[deviceIndex]["device"]},
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
        var job = JsonConvert.DeserializeObject<Dictionary<string, object>>(jobInfo);
        string jobId = (string)job!["jobuid"];
        if (string.IsNullOrEmpty(jobId))
            throw new Exception("Failed to create scan job.");

        var docInfo = await scannerController.CreateDocument(host, new Dictionary<string, object>());
        var doc = JsonConvert.DeserializeObject<Dictionary<string, object>>(docInfo);
        string docId = (string)doc!["uid"];
        if (string.IsNullOrEmpty(docId))
            throw new Exception("Failed to create document.");

        return (jobId, docId);
    }

    private static void PrintBenchmarkResult(string label, int pageCount, long elapsedMs)
    {
        Console.WriteLine($"\n[Benchmark] {label}");
        Console.WriteLine($"  Pages fetched : {pageCount}");
        Console.WriteLine($"  Elapsed time  : {elapsedMs} ms");
        if (pageCount > 0)
            Console.WriteLine($"  Avg per page  : {elapsedMs / pageCount} ms");
        Console.WriteLine();
    }

    // ---------------------------------------------------------------
    // Blocking acquisition
    // ---------------------------------------------------------------

    private static async Task AcquireBlocking(int deviceIndex)
    {
        var (jobId, docId) = await SetupJobAndDocument(deviceIndex);

        var sw = Stopwatch.StartNew();
        var images = await scannerController.GetImageFiles(host, jobId, "./");
        sw.Stop();

        for (int i = 0; i < images.Count; i++)
        {
            Console.WriteLine($"Image {i}: {images[i]}");
            var imageInfo = await scannerController.GetImageInfo(host, jobId);
            var image = JsonConvert.DeserializeObject<Dictionary<string, object>>(imageInfo);
            if (image != null && image.ContainsKey("url"))
            {
                var pageParams = new Dictionary<string, object>
                {
                    {"password", ""},
                    {"source", image["url"]}
                };
                await scannerController.InsertPage(host, docId, pageParams);
            }
        }

        PrintBenchmarkResult("Blocking (GetImageFiles)", images.Count, sw.ElapsedMilliseconds);

        var docFile = await scannerController.GetDocumentFile(host, docId, "./");
        Console.WriteLine($"Document file: {docFile}");

        await scannerController.DeleteDocument(host, docId);
        await scannerController.DeleteJob(host, jobId);
    }

    // ---------------------------------------------------------------
    // Non-blocking acquisition
    // ---------------------------------------------------------------

    private static async Task AcquireNonBlocking(int deviceIndex)
    {
        var (jobId, docId) = await SetupJobAndDocument(deviceIndex);

        var fetchTasks = new List<Task>();
        int imageIndex = 0;

        var sw = Stopwatch.StartNew();

        while (true)
        {
            var imageInfo = await scannerController.GetImageInfo(host, jobId);
            if (string.IsNullOrEmpty(imageInfo))
                break;

            Dictionary<string, object>? image;
            try { image = JsonConvert.DeserializeObject<Dictionary<string, object>>(imageInfo); }
            catch { break; }

            if (image == null || !image.ContainsKey("url"))
                break;

            int currentIndex = imageIndex++;
            string imageUrl = image["url"]?.ToString() ?? "";

            fetchTasks.Add(Task.Run(async () =>
            {
                string filename = await scannerController.GetImageFileByIndex(host, jobId, currentIndex, "./", "image/jpeg");
                Console.WriteLine($"Image {currentIndex}: {filename}");

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    await scannerController.InsertPage(host, docId, new Dictionary<string, object>
                    {
                        {"password", ""},
                        {"source", imageUrl}
                    });
                }
            }));
        }

        await Task.WhenAll(fetchTasks);
        sw.Stop();

        PrintBenchmarkResult("Non-blocking (GetImageFileByIndex + Task.Run)", imageIndex, sw.ElapsedMilliseconds);

        var docFile = await scannerController.GetDocumentFile(host, docId, "./");
        Console.WriteLine($"Document file: {docFile}");

        await scannerController.DeleteDocument(host, docId);
        await scannerController.DeleteJob(host, jobId);
    }

    // ---------------------------------------------------------------
    // Menu loop
    // ---------------------------------------------------------------

    private static async Task<int> AskQuestion()
    {
        while (true)
        {
            Console.WriteLine(".............................................");
            Console.WriteLine(questions);
            string? answer = Console.ReadLine();

            if (string.IsNullOrEmpty(answer))
                continue;

            if (answer == "4")
            {
                break;
            }
            else if (answer == "1")
            {
                var scannerInfo = await scannerController.GetDevices(host, ScannerType.TWAINSCANNER | ScannerType.TWAINX64SCANNER);
                devices.Clear();
                try
                {
                    var scanners = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(scannerInfo);
                    for (int i = 0; i < scanners!.Count; i++)
                    {
                        devices.Add(scanners[i]);
                        Console.WriteLine($"\nIndex: {i}, Name: {scanners[i]["name"]}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            else if (answer == "2" || answer == "3")
            {
                if (devices.Count == 0)
                {
                    Console.WriteLine("Please get scanners first!\n");
                    continue;
                }

                int deviceIndex = PromptScannerIndex();
                if (deviceIndex < 0) continue;

                try
                {
                    if (answer == "2")
                        await AcquireBlocking(deviceIndex);
                    else
                        await AcquireNonBlocking(deviceIndex);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        return 0;
    }
}