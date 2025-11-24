using Microsoft.AspNetCore.WebUtilities;
using MyShop.Contracts.Common;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MyShop.WebMvc.Services;


public class ApiClient
{
    private readonly HttpClient _client;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(IHttpClientFactory httpClientFactory, ILogger<ApiClient> logger)
    {
        _logger = logger;
        _client = httpClientFactory.CreateClient("MyShop.WebApi");
    }

    public async Task<ApiResponse<TResponse>> GetAsync<TResponse>(string path, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<object, TResponse>(HttpMethod.Get, path, null, queryParams, cancellationToken);
    }

    public async Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string path, TRequest data, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<TRequest, TResponse>(HttpMethod.Post, path, data, queryParams, cancellationToken);
    }

    public async Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string path, TRequest data, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<TRequest, TResponse>(HttpMethod.Put, path, data, queryParams, cancellationToken);
    }

    public async Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string path, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<object, TResponse>(HttpMethod.Delete, path, null, queryParams, cancellationToken);
    }

    public async Task<ApiResponse<TResponse>> PatchAsync<TRequest, TResponse>(string path, TRequest body, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
    {
        return await SendRequestAsync<TRequest, TResponse>(new HttpMethod("PATCH"), path, body, queryParams, cancellationToken);
    }

    // A single, unified method for requests with or without query strings
    private async Task<ApiResponse<TResponse>> SendRequestAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest? data, Dictionary<string, object>? queryParams, CancellationToken cancellationToken = default)
    {
        string requestPath = path;
        if (queryParams != null && queryParams.Any())
        {
            var queryString = queryParams.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString());
            requestPath = QueryHelpers.AddQueryString(path, queryString);
        }

        var request = new HttpRequestMessage(method, requestPath);

        if (data != null && method != HttpMethod.Get && method != HttpMethod.Delete)
        {
            request.Content = JsonContent.Create(data);
        }

        var response = await _client.SendAsync(request, cancellationToken);
        return await HandleResponse<TResponse>(response, cancellationToken);
    }

    public async Task<byte[]?> GetRawBytesAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.GetAsync(path, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Raw byte GET failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
                return null;
            }

            return await response.Content.ReadAsByteArrayAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during raw byte GET: {Path}", path);
            return null;
        }
    }

    public async Task<ApiResponse<TResponse>> PostMultipartAsync<TResponse>(string path, MultipartFormDataContent formData, CancellationToken cancellationToken = default)
    {
        var serializerOptions = new JsonSerializerOptions
        {
            // JSON'dan gelen camelCase ile C#'taki PascalCase eşleşmesini sağlar.
            PropertyNameCaseInsensitive = true,

            // Diğer olası hataları es geçmek için
            // NumberHandling = JsonNumberHandling.AllowReadingFromString 
        };

        try
        {
            var response = await _client.PostAsync(path, formData, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Multipart POST failed: {StatusCode} - {Reason}", response.StatusCode, response.ReasonPhrase);
                //return ApiResponse<TResponse>.ErrorResponse($"HTTP {response.StatusCode}");
                var errorJson = await response.Content.ReadAsStringAsync();
                var apiError = JsonSerializer.Deserialize<ApiResponse<TResponse>>(errorJson);

                // API'den gelen detaylı hata nesnesi varsa onu dön, yoksa genel hata dön
                return apiError?.Success == false
                    ? apiError
                    : ApiResponse<TResponse>.ErrorResponse($"HTTP {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<ApiResponse<TResponse>>(json, serializerOptions);
            if (data == null)
            {
                _logger.LogError("Failed to deserialize API response: {Json}", json);
                return ApiResponse<TResponse>.ErrorResponse("API yanıtı deserializasyon başarısız. Tip uyuşmazlığı olabilir.");
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during multipart POST");
            return ApiResponse<TResponse>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<TResponse>> PostMultipartAsync<TResponse>(
    string path,
    IFormFileCollection files,
    IDictionary<string, string?>? fields = null,
    CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();

        // 1) Text alanları
        if (fields is not null)
        {
            foreach (var kv in fields)
            {
                // null değerler için boş string gönderelim
                form.Add(new StringContent(kv.Value ?? string.Empty), kv.Key);
            }
        }

        // 2) Dosyalar
        if (files is not null)
        {
            foreach (var file in files)
            {
                if (file?.Length > 0)
                {
                    var streamContent = new StreamContent(file.OpenReadStream());
                    // İçerik tipi biliniyorsa set edelim, değilse octet-stream
                    var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType;
                    streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);

                    // ÖNEMLİ: 'name' paramı API tarafında beklenen alan adıdır.
                    // HTML formundaki 'name' değerini (file.Name) aynen geçiyoruz.
                    form.Add(streamContent, file.Name, file.FileName);
                }
            }
        }

        // 3) Mevcut yöntemi çağır
        return await PostMultipartAsync<TResponse>(path, form, cancellationToken);
    }

    private async Task<ApiResponse<TResponse>> HandleResponse<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                // Using System.Text.Json built-in extension method
                var data = await response.Content.ReadFromJsonAsync<ApiResponse<TResponse>>(cancellationToken);
                return data ?? ApiResponse<TResponse>.ErrorResponse("Empty response.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON Deserialization Error");
                return ApiResponse<TResponse>.ErrorResponse(new List<string> { "JSON Deserialization Error: " + ex.Message });
            }
        }
        else
        {
            _logger.LogError($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
            return ApiResponse<TResponse>.ErrorResponse(new List<string> { $"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}" });
        }
    }

    public static string ToQueryString<T>(T obj)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.GetValue(obj) != null)
            .Select(p =>
            {
                var value = p.GetValue(obj);
                // URL kodlaması (encoding) için her bir değeri kodlarız
                return $"{p.Name.ToLowerInvariant()}={Uri.EscapeDataString(value.ToString())}";
            });

        return "?" + string.Join("&", properties);
    }
}
