using Microsoft.AspNetCore.Mvc;
using PharmacyManagementWeb.Models.ApiModels;
using PharmacyManagementWeb.Services;

namespace PharmacyManagementWeb.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly IApiClient _apiClient;

        public NotificationBellViewComponent(IApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
                return Content(string.Empty);

            try
            {
                var result = await _apiClient.GetAsync<AlertsModel>("/api/Report/alerts");
                return View(result.Data ?? new AlertsModel());
            }
            catch
            {
                // API tạm thời lỗi thì không làm hỏng cả trang, chỉ không hiện thông báo
                return View(new AlertsModel());
            }
        }
    }
}