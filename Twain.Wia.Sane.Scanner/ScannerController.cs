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
    public static string SCAN_SUCCESS = "success";
    public static string SCAN_ERROR = "error";

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

        try
        {
            var response = await GetDevicesHttpResponse(host, scannerType);
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
    /// Get a list of available devices and return the HTTP response.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="scannerType"></param>
    /// <returns>HTTP response</returns>
    public async Task<HttpResponseMessage> GetDevicesHttpResponse(string host, int? scannerType = null)
    {
        string url = $"{host}/DWTAPI/Scanners";
        if (scannerType.HasValue)
        {
            url += $"?type={scannerType.Value}";
        }

        try
        {
            var response = await _httpClient.GetAsync(url);
            return response;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Scan a document.
    /// </summary>
    /// <param name="host">The URL of the Dynamsoft Service API.</param>
    /// <param name="parameters">The parameters for the scan.</param>
    /// <returns>A dictionary containing the job ID or an error message.</returns>
    public async Task<Dictionary<string, string>> ScanDocument(string host, Dictionary<string, object> parameters)
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();

        try
        {
            var response = await ScanDocumentHttpResponse(host, parameters);
            var text = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {

                dict.Add(SCAN_SUCCESS, text);
            }
            else
            {
                dict.Add(SCAN_ERROR, text);
            }
        }
        catch (Exception ex)
        {
            dict.Add(SCAN_ERROR, ex.ToString());
        }

        return dict;
    }

    /// <summary>
    /// Scan a document and return the HTTP response.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="parameters"></param>
    /// <returns>HTTP response</returns>
    public async Task<HttpResponseMessage> ScanDocumentHttpResponse(string host, Dictionary<string, object> parameters)
    {
        string url = $"{host}/DWTAPI/ScanJobs";
        var json = JsonSerializer.Serialize(parameters);
        try
        {
            var response = await _httpClient.PostAsync(url, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
            return response;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Delete a job and return the HTTP response.
    /// </summary>
    /// <param name="host">The URL of the Dynamsoft Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// /// <returns>HTTP response</returns>
    public async Task<HttpResponseMessage> DeleteJob(string host, string jobId)
    {
        string url = $"{host}/DWTAPI/ScanJobs/{jobId}";

        try
        {
            var response = await _httpClient.DeleteAsync(url);
            return response;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            throw;
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
        try
        {
            var response = await GetImageStreamHttpResponse(host, jobId);
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
    public async Task<List<byte[]>> GetImageStreams(string host, string jobId)
    {
        var streams = new List<byte[]>();

        while (true)
        {
            byte[] bytes = await GetImageStream(host, jobId);

            if (bytes.Length == 0)
            {
                break;
            }
            else
            {
                streams.Add(bytes);
            }
        }

        return streams;
    }

    /// <summary>
    /// Get an image stream.
    /// </summary>
    /// <param name="host">The URL of the Dynamsoft Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// <returns>An image stream.</returns>
    public async Task<byte[]> GetImageStream(string host, string jobId)
    {
        try
        {
            var response = await GetImageStreamHttpResponse(host, jobId);

            if (response.IsSuccessStatusCode)
            {
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                return bytes;
            }
            else if ((int)response.StatusCode == 410)
            {
                return Array.Empty<byte>();
            }
        }
        catch (Exception)
        {
            return Array.Empty<byte>();
        }

        return Array.Empty<byte>();
    }

    /// <summary>
    /// Get an image stream and return the HTTP response.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="jobId"></param>
    /// <returns>HTTP response</returns>
    public async Task<HttpResponseMessage> GetImageStreamHttpResponse(string host, string jobId)
    {
        var url = $"{host}/DWTAPI/ScanJobs/{jobId}/NextDocument";

        try
        {
            var response = await _httpClient.GetAsync(url);

            return response;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            throw;
        }
    }
}
