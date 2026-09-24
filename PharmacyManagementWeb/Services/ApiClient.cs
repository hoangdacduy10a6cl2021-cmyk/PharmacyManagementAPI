using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PharmacyManagementWeb.Services
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public interface IApiClient
    {
        Task<ApiResult<T>> GetAsync<T>(string path);
        Task<ApiResult<T>> PostAsync<T>(string path, object body);
        Task<ApiResult<T>> PutAsync<T>(string path, object body);
        Task<ApiResult<T>> PatchAsync<T>(string path, object body);
        Task<ApiResult<bool>> DeleteAsync(string path);
    }

    public class ApiClient : IApiClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient CreateClient()
        {
            var client = _httpClientFactory.CreateClient("PharmacyApi");

            // Lấy JWT token đã lưu trong Cookie claim (sau khi đăng nhập) để đính vào mọi request
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("ApiToken")?.Value;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private async Task<ApiResult<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                if (string.IsNullOrWhiteSpace(content))
                    return new ApiResult<T> { Success = true, Data = default };

                try
                {
                    var data = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                    return new ApiResult<T> { Success = true, Data = data };
                }
                catch
                {
                    return new ApiResult<T> { Success = true, Data = default };
                }
            }

            // Cố gắng đọc message lỗi từ API dạng { "message": "..." }
            string? errorMessage = $"Lỗi {(int)response.StatusCode}";
            try
            {
                using var doc = JsonDocument.Parse(content);
                if (doc.RootElement.TryGetProperty("message", out var msgEl))
                    errorMessage = msgEl.GetString();
            }
            catch { /* ignore parse error, dùng message mặc định */ }

            return new ApiResult<T> { Success = false, ErrorMessage = errorMessage };
        }

        public async Task<ApiResult<T>> GetAsync<T>(string path)
        {
            var client = CreateClient();
            var response = await client.GetAsync(path);
            return await HandleResponse<T>(response);
        }

        public async Task<ApiResult<T>> PostAsync<T>(string path, object body)
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(path, content);
            return await HandleResponse<T>(response);
        }

        public async Task<ApiResult<T>> PutAsync<T>(string path, object body)
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(path, content);
            return await HandleResponse<T>(response);
        }

        public async Task<ApiResult<T>> PatchAsync<T>(string path, object body)
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Patch, path) { Content = content };
            var response = await client.SendAsync(request);
            return await HandleResponse<T>(response);
        }

        public async Task<ApiResult<bool>> DeleteAsync(string path)
        {
            var client = CreateClient();
            var response = await client.DeleteAsync(path);
            var result = await HandleResponse<object>(response);
            return new ApiResult<bool> { Success = result.Success, ErrorMessage = result.ErrorMessage, Data = result.Success };
        }
    }
}