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