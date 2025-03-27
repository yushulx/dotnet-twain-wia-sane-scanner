namespace Twain.Wia.Sane.Scanner;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.IO;
using System.Net.Http.Json;

/// <summary>
/// A class that defines constants for job status.
/// </summary>
public static class JobStatus
{
    public const string RUNNING = "running";
    public const string CANCELED = "canceled";
}


/// <summary>
/// A class that defines constants for different types of scanners.
/// </summary>
public static class ScannerType
{
    public const int TWAINSCANNER = 0x10;
    public const int WIASCANNER = 0x20;
    public const int TWAINX64SCANNER = 0x40;
    public const int ICASCANNER = 0x80;
    public const int SANESCANNER = 0x100;
    public const int ESCLSCANNER = 0x200;
    public const int WIFIDIRECTSCANNER = 0x400;
    public const int WIATWAINSCANNER = 0x800;
}

/// <summary>
/// A class that provides methods to interact with Dynamic Web TWAIN Service API.
/// </summary>
public class ScannerController
{
    public static string SCAN_ERROR = "error";

    private HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Get a list of available devices.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
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
            else
            {
                Console.WriteLine("Get devices failed: " + response.StatusCode);
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
        string url = $"{host}/api/device/scanners";
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
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="parameters">The parameters for the scan.</param>
    /// <returns>A dictionary containing the job ID or an error message.</returns>
    public async Task<Dictionary<string, object>> CreateJob(string host, Dictionary<string, object> parameters)
    {
        var result = new Dictionary<string, object>();
        string url = $"{host}/api/device/scanners/jobs";

        try
        {
            // Extract license key from parameters
            string licenseKey = parameters.ContainsKey("license") ? parameters["license"].ToString() : "";

            // Prepare JSON body
            string json = JsonSerializer.Serialize(parameters);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Create request with headers
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;
            request.Headers.Add("X-DICS-LICENSE-KEY", licenseKey);

            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var data = JsonSerializer.Deserialize<Dictionary<string, object>>(responseText);
                return data;
            }
            else
            {
                result[SCAN_ERROR] = responseText;
            }
        }
        catch (Exception ex)
        {
            result[SCAN_ERROR] = ex.Message;
        }

        return result;
    }


    /// <summary>
    /// Scan a document and return the HTTP response.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="parameters"></param>
    /// <returns>HTTP response</returns>
    public async Task<HttpResponseMessage> CreateJobHttpResponse(string host, Dictionary<string, object> parameters)
    {
        string url = $"{host}/api/device/scanners/jobs";
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
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// /// <returns>HTTP response</returns>
    public async Task<HttpResponseMessage> DeleteJob(string host, string jobId)
    {
        string url = $"{host}/api/device/scanners/jobs/{jobId}";

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
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// <param name="directory">The directory to save the image file.</param>
    /// <returns>The image file path.</returns>
    public async Task<string> GetImageFile(string host, string jobId, string directory)
    {
        try
        {
            var response = await GetImageStreamHttpResponse(host, jobId);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
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
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
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
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
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
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
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
        var url = $"{host}/api/device/scanners/jobs/{jobId}/next-page";

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

    public async Task<Dictionary<string, object>> CheckJob(string host, string jobId)
    {
        var result = new Dictionary<string, object>();
        string url = $"{host}/api/device/scanners/jobs/{jobId}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            else
            {
                Console.WriteLine("Check job failed: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Check job failed: " + ex.Message);
            result[SCAN_ERROR] = ex.Message;
        }

        return result;
    }

    public async Task<Dictionary<string, object>> UpdateJob(string host, string jobId, Dictionary<string, object> parameters)
    {
        var result = new Dictionary<string, object>();
        string url = $"{host}/api/device/scanners/jobs/{jobId}";
        var json = JsonSerializer.Serialize(parameters);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            }
            else
            {
                Console.WriteLine("Update job failed: " + response.StatusCode);
                result[SCAN_ERROR] = response.StatusCode.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Update job failed: " + ex.Message);
            result[SCAN_ERROR] = ex.Message;
        }

        return result;
    }

    public async Task<List<Dictionary<string, object>>> GetScannerCapabilities(string host, string jobId)
    {
        var result = new List<Dictionary<string, object>>();
        string url = $"{host}/api/device/scanners/jobs/{jobId}/scanner/capabilities";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            }
            else
            {
                Console.WriteLine("Scanner capabilities fetch failed: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Scanner capabilities fetch failed: " + ex.Message);
        }

        return result;
    }

    public async Task<Dictionary<string, object>> GetImageInfo(string host, string jobId)
    {
        var result = new Dictionary<string, object>();
        string url = $"{host}/api/device/scanners/jobs/{jobId}/next-page-info";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            else
            {
                Console.WriteLine("Image info fetch failed: " + response.StatusCode);
                result[SCAN_ERROR] = response.StatusCode.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Image info fetch failed: " + ex.Message);
            result[SCAN_ERROR] = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Create a document.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="parameters">The parameters for the document.</param>
    /// <returns>A dictionary containing the document ID or an error message.</returns>
    public async Task<Dictionary<string, object>> CreateDocument(string host, Dictionary<string, object> parameters)
    {
        var result = new Dictionary<string, object>();
        string url = $"{host}/api/storage/documents";
        var json = JsonSerializer.Serialize(parameters);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            }
            else
            {
                result[SCAN_ERROR] = responseBody;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Document creation failed: " + ex.Message);
            result[SCAN_ERROR] = ex.Message;
        }

        return result;
    }

    public async Task<Dictionary<string, object>> GetDocumentInfo(string host, string docId)
    {
        var result = new Dictionary<string, object>();
        string url = $"{host}/api/storage/documents/{docId}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            else
            {
                Console.WriteLine("Document info fetch failed: " + response.StatusCode);
                result[SCAN_ERROR] = response.StatusCode.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Document info fetch failed: " + ex.Message);
            result[SCAN_ERROR] = ex.Message;
        }

        return result;
    }

    public async Task<bool> DeleteDocument(string host, string docId)
    {
        string url = $"{host}/api/storage/documents/{docId}";

        try
        {
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Document deletion failed: " + ex.Message);
            return false;
        }
    }


    public async Task<string> GetDocumentFile(string host, string docId, string directory)
    {
        string url = $"{host}/api/storage/documents/{docId}/content";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string filename = $"document_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.pdf";
                string filePath = Path.Combine(directory, filename);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    await response.Content.CopyToAsync(fs);
                }
                return filename;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Document fetch failed: " + ex.Message);
        }

        return "";
    }


    public async Task<byte[]> GetDocumentStream(string host, string docId)
    {
        string url = $"{host}/api/storage/documents/{docId}/content";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Document stream fetch failed: " + ex.Message);
        }

        return Array.Empty<byte>();
    }


    public async Task<Dictionary<string, object>> InsertPage(string host, string docId, Dictionary<string, object> parameters)
    {
        var result = new Dictionary<string, object>();
        string url = $"{host}/api/storage/documents/{docId}/pages";
        var json = JsonSerializer.Serialize(parameters);
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        if (parameters.ContainsKey("password"))
        {
            request.Headers.Add("X-DICS-DOC-PASSWORD", parameters["password"].ToString());
        }

        try
        {
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);
            }
            else
            {
                result[SCAN_ERROR] = await response.Content.ReadAsStringAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Page insertion failed: " + ex.Message);
            result[SCAN_ERROR] = ex.Message;
        }

        return result;
    }

    public async Task<bool> DeletePage(string host, string docId, string pageId)
    {
        string url = $"{host}/api/storage/documents/{docId}/pages/{pageId}";

        try
        {
            var response = await _httpClient.DeleteAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Page deletion failed: " + ex.Message);
            return false;
        }
    }

    public async Task<Dictionary<string, object>> GetServerInfo(string host)
    {
        var result = new Dictionary<string, object>();
        string url = $"{host}/api/server/version";

        try
        {
            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            else
            {
                Console.WriteLine("Server info fetch failed: " + response.StatusCode);
                result[SCAN_ERROR] = response.StatusCode.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Server info fetch failed: " + ex.Message);
            result[SCAN_ERROR] = ex.Message;
        }

        return result;
    }

}
