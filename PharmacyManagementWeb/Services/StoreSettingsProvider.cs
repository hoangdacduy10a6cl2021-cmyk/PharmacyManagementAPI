using Microsoft.Extensions.Caching.Memory;
using PharmacyManagementWeb.Models.ApiModels;

namespace PharmacyManagementWeb.Services
{
    // Lấy Cài đặt cửa hàng từ API và nhớ tạm 5 phút để các trang không phải gọi API liên tục
    public interface IStoreSettingsProvider
    {
        Task<StoreSettingModel> GetAsync();
        void Invalidate();
    }

    public class StoreSettingsProvider : IStoreSettingsProvider
    {
        private const string CacheKey = "store-settings";

        private readonly IApiClient _apiClient;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _http;

        public StoreSettingsProvider(IApiClient apiClient, IMemoryCache cache, IHttpContextAccessor http)
        {
            _apiClient = apiClient;
            _cache = cache;
            _http = http;
        }

        public async Task<StoreSettingModel> GetAsync()
        {
            if (_cache.TryGetValue(CacheKey, out StoreSettingModel? cached) && cached != null)
                return cached;

            // Chưa đăng nhập thì chưa gọi được API, dùng giá trị mặc định
            if (_http.HttpContext?.User.Identity?.IsAuthenticated != true)
                return new StoreSettingModel();

            try
            {
                var result = await _apiClient.GetAsync<StoreSettingModel>("/api/StoreSetting");
                if (result.Success && result.Data != null)
                {
                    _cache.Set(CacheKey, result.Data, TimeSpan.FromMinutes(5));
                    return result.Data;
                }
            }
            catch
            {
                // API lỗi tạm thời: không làm hỏng trang, dùng giá trị mặc định
            }

            return new StoreSettingModel();
        }

        public void Invalidate() => _cache.Remove(CacheKey);
    }
}