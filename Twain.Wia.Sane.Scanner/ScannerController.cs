namespace Twain.Wia.Sane.Scanner;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.IO;

/// <summary>
/// A class that provides methods to interact with Dynamsoft Service API.
/// </summary>
public class ScannerController
{
    private HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Get a list of available devices.
    /// </summary>
    /// <param name="host">The URL of the Dynamsoft Service API.</param>
    /// <param name="scannerType">The type of scanner. Defaults to null.</param>
    /// <returns>A list of available devices.</returns>
    public async Task<List<Dictionary<string, object>>> GetDevices(string host, int? scannerType = null)
    {
        List<Dictionary<string, object>> devices = new List<Dictionary<string, object>>();
        string url = $"{host}/DWTAPI/Scanners";
        if (scannerType.HasValue)
        {
            url += $"?type={scannerType.Value}";
        }

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseBody))
                {
                    devices = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(responseBody) ?? new List<Dictionary<string, object>>();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return devices;
    }

    /// <summary>
    /// Scan a document.
    /// </summary>
    /// <param name="host">The URL of the Dynamsoft Service API.</param>
    /// <param name="parameters">The parameters for the scan.</param>
    /// <returns>The ID of the job.</returns>
    public async Task<string> ScanDocument(string host, Dictionary<string, object> parameters)
    {
        string url = $"{host}/DWTAPI/ScanJobs";
        try
        {
            var json = JsonSerializer.Serialize(parameters);
            var response = await _httpClient.PostAsync(url, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return "";
    }

    /// <summary>
    /// Delete a job.
    /// </summary>
    /// <param name="host">The URL of the Dynamsoft Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    public async void DeleteJob(string host, string jobId)
    {
        if (string.IsNullOrEmpty(jobId))
        {
            return;
        }
        string url = $"{host}/DWTAPI/ScanJobs/{jobId}";

        try
        {
            var response = await _httpClient.DeleteAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    /// <summary>
    /// Get an image file.
    /// </summary>
    /// <param name="host">The URL of the Dynamsoft Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// <param name="directory">The directory to save the image file.</param>
    /// <returns>The image file path.</returns>
    public async Task<string> GetImageFile(string host, string jobId, string directory)
    {
        string url = $"{host}/DWTAPI/ScanJobs/{jobId}/NextDocument";
        try
        {
            var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            if (response.IsSuccessStatusCode)
            {
                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                string filename = $"image_{timestamp}.jpg";
                string imagePath = Path.Combine(directory, filename);
                using (FileStream fs = new FileStream(imagePath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
                return filename;
            }
        }
        catch
        {
            Console.WriteLine("No more images.");
            return "";
        }

        return "";
    }

    /// <summary>
    /// Get a list of image files.
    /// </summary>
    /// <param name="host">The URL of the Dynamsoft Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// <param name="directory">The directory to save the image files.</param>
    /// <returns>A list of image file paths.</returns>
    public async Task<List<string>> GetImageFiles(string host, string jobId, string directory)
    {
        List<string> images = new List<string>();
        while (true)
        {
            string filename = await GetImageFile(host, jobId, directory);
            if (string.IsNullOrEmpty(filename))
            {
                break;
            }
            else
            {
                images.Add(filename);
            }
        }

        return images;
    }

    /// <summary>
    /// Get a list of image streams.
    /// </summary>
    /// <param name="host">The URL of the Dynamsoft Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// <returns>A list of image streams.</returns>
    public List<byte[]> GetImageStreams(string host, string jobId)
    {
        var streams = new List<byte[]>();
        var url = $"{host}/DWTAPI/ScanJobs/{jobId}/NextDocument";

        while (true)
        {
            try
            {
                var response = _httpClient.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    streams.Add(response.Content.ReadAsByteArrayAsync().Result);
                }
                else if ((int)response.StatusCode == 410)
                {
                    break;
                }
            }
            catch (Exception)
            {
                break;
            }
        }

        return streams;
    }
}
