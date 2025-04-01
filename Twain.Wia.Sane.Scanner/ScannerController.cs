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
    private HttpClient _httpClient = new HttpClient();

    /// <summary>
    /// Get a list of available devices.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="scannerType">The type of scanner. Defaults to null.</param>
    /// <returns>A list of available devices.</returns>
    public async Task<string> GetDevices(string host, int? scannerType = null)
    {
        try
        {
            var response = await GetDevicesHttpResponse(host, scannerType);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            return ex.ToString();
        }
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
    public async Task<string> CreateJob(string host, Dictionary<string, object> parameters)
    {
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

            return responseText;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

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
    /// <returns>HTTP response</returns>
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

    /// <summary>
    /// Check the status of a job.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// <returns>The job status.</returns>
    public async Task<string> CheckJob(string host, string jobId)
    {
        string url = $"{host}/api/device/scanners/jobs/{jobId}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Check job failed: " + ex.Message);
            return ex.Message;
        }
    }

    /// <summary>
    /// Update a job.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// <param name="parameters">The parameters to update the job.</param>
    /// <returns>The HTTP response.</returns>
    public async Task<HttpResponseMessage> UpdateJob(string host, string jobId, Dictionary<string, object> parameters)
    {
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
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Update job failed: " + ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Get the capabilities of a scanner.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// <returns>The scanner capabilities.</returns>
    public async Task<string> GetScannerCapabilities(string host, string jobId)
    {
        string url = $"{host}/api/device/scanners/jobs/{jobId}/scanner/capabilities";

        try
        {
            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Scanner capabilities fetch failed: " + ex.Message);
            return ex.Message;
        }
    }

    /// <summary>
    /// Get the image info.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="jobId">The ID of the job.</param>
    /// <returns>The image info.</returns>
    public async Task<string> GetImageInfo(string host, string jobId)
    {
        string url = $"{host}/api/device/scanners/jobs/{jobId}/next-page-info";

        try
        {
            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Image info fetch failed: " + ex.Message);
            return ex.Message;
        }
    }

    /// <summary>
    /// Create a document.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="parameters">The parameters for the document.</param>
    /// <returns>A dictionary containing the document ID or an error message.</returns>
    public async Task<string> CreateDocument(string host, Dictionary<string, object> parameters)
    {
        string url = $"{host}/api/storage/documents";
        var json = JsonSerializer.Serialize(parameters);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Document creation failed: " + ex.Message);
            return ex.Message;
        }
    }

    /// <summary>
    /// Get a document.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="docId">The ID of the document.</param>
    /// <returns>The document info.</returns>
    public async Task<string> GetDocumentInfo(string host, string docId)
    {
        string url = $"{host}/api/storage/documents/{docId}";

        try
        {
            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Document info fetch failed: " + ex.Message);
            return ex.Message;
        }
    }

    /// <summary>
    /// Delete a document.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="docId">The ID of the document.</param>
    /// <returns>The HTTP response.</returns>
    public async Task<HttpResponseMessage> DeleteDocument(string host, string docId)
    {
        string url = $"{host}/api/storage/documents/{docId}";

        try
        {
            var response = await _httpClient.DeleteAsync(url);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Document deletion failed: " + ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Get a document file.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="docId">The ID of the document.</param>
    /// <param name="directory">The directory to save the document file.</param>
    /// <returns>The document file path.</returns>
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

    /// <summary>
    /// Get a document stream.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="docId">The ID of the document.</param>
    /// <returns>The document stream.</returns>
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

    /// <summary>
    /// Insert a page into a document.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="docId">The ID of the document.</param>
    /// <param name="parameters">The parameters for the page insertion.</param>
    /// <returns>The page ID or an error message.</returns>
    public async Task<string> InsertPage(string host, string docId, Dictionary<string, object> parameters)
    {
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
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Page insertion failed: " + ex.Message);
            return ex.Message;
        }
    }

    /// <summary>
    /// Delete a page from a document.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <param name="docId">The ID of the document.</param>
    /// <param name="pageId">The ID of the page.</param>
    /// <returns>The HTTP response.</returns>
    public async Task<HttpResponseMessage> DeletePage(string host, string docId, string pageId)
    {
        string url = $"{host}/api/storage/documents/{docId}/pages/{pageId}";

        try
        {
            var response = await _httpClient.DeleteAsync(url);
            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Page deletion failed: " + ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Get Dynamic Web TWAIN Service API server info.
    /// </summary>
    /// <param name="host">The URL of the Dynamic Web TWAIN Service API.</param>
    /// <returns>The server info.</returns>
    public async Task<string> GetServerInfo(string host)
    {
        string url = $"{host}/api/server/version";

        try
        {
            var response = await _httpClient.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            return responseContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Server info fetch failed: " + ex.Message);
            return ex.Message;
        }
    }

}
