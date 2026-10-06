using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using PharmacyManagementAPI.Models.DTOs;
using PharmacyManagementAPI.Services;
using System.Security.Claims;

namespace PharmacyManagementAPI.Filters
{
    // Tự động ghi nhật ký cho mọi thao tác thay đổi dữ liệu (POST / PUT / PATCH / DELETE) và đăng nhập.
    // Không ghi nội dung request nên không lộ mật khẩu.
    public class AuditLogFilter : IAsyncActionFilter
    {
        private readonly IAuditService _audit;

        private static readonly Dictionary<string, string> Nouns = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Medicine"] = "thuốc",
            ["Order"] = "hoá đơn",
            ["Customer"] = "khách hàng",
            ["Category"] = "danh mục",
            ["Supplier"] = "nhà cung cấp",
            ["Promotion"] = "khuyến mãi",
            ["PurchaseOrder"] = "phiếu nhập",
            ["Prescription"] = "đơn thuốc",
            ["User"] = "tài khoản",
            ["Auth"] = "tài khoản"
        };

        public AuditLogFilter(IAuditService audit)
        {
            _audit = audit;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executed = await next();

            var http = context.HttpContext;
            var method = http.Request.Method;
            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
                return;

            var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
            var action = context.RouteData.Values["action"]?.ToString() ?? "";
            var ip = http.Connection.RemoteIpAddress?.ToString();

            int status = executed.Exception != null && !executed.ExceptionHandled
                ? 500
                : (executed.Result as IStatusCodeActionResult)?.StatusCode ?? 200;
            bool ok = status < 400;

            // ===== Đăng nhập =====
            if (controller.Equals("Auth", StringComparison.OrdinalIgnoreCase)
                && action.Equals("Login", StringComparison.OrdinalIgnoreCase))
            {
                var dto = context.ActionArguments.Values.OfType<LoginDto>().FirstOrDefault();
                var auth = (executed.Result as OkObjectResult)?.Value as AuthResponseDto;

                if (auth != null)
                    await _audit.LogAsync(auth.UserId, auth.Username, "Đăng nhập", "User",
                        auth.UserId.ToString(), "Đăng nhập hệ thống", true, ip);
                else
                    await _audit.LogAsync(null, dto?.Username ?? "", "Đăng nhập thất bại", "User", null,
                        $"Đăng nhập thất bại với tài khoản '{dto?.Username}'", false, ip);
                return;
            }

            // ===== Các thao tác khác =====
            int? userId = int.TryParse(http.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid) ? uid : null;
            var username = http.User.Identity?.Name ?? "";

            var idText = context.RouteData.Values["id"]?.ToString();
            if (idText == null && (executed.Result as ObjectResult)?.Value is object created)
                idText = created.GetType().GetProperty("Id")?.GetValue(created)?.ToString();

            var (label, description) = Describe(method, controller, action, idText);
            if (!ok) description += $" (thất bại, mã {status})";

            await _audit.LogAsync(userId, username, label, controller, idText, description, ok, ip);
        }

        private static (string label, string description) Describe(string method, string controller, string action, string? id)
        {
            var noun = Nouns.TryGetValue(controller, out var n) ? n : controller.ToLower();
            var suffix = string.IsNullOrEmpty(id) ? "" : $" #{id}";

            switch (action.ToLowerInvariant())
            {
                case "cancel": return ("Hủy", $"Hủy hoá đơn{suffix}");
                case "uploadimage": return ("Cập nhật", $"Cập nhật ảnh thuốc{suffix}");
                case "deleteimage": return ("Xoá", $"Xoá ảnh thuốc{suffix}");
                case "changepassword": return ("Đổi mật khẩu", "Đổi mật khẩu cá nhân");
                case "updateme": return ("Cập nhật", "Cập nhật thông tin cá nhân");
                case "register": return ("Thêm", "Tạo tài khoản mới");
                case "resetpassword": return ("Đặt lại mật khẩu", $"Đặt lại mật khẩu tài khoản{suffix}");
                case "toggleactive": return ("Cập nhật", $"Khoá / mở khoá tài khoản{suffix}");
            }

            var verb = method.ToUpperInvariant() switch
            {
                "POST" => "Thêm",
                "PUT" => "Cập nhật",
                "PATCH" => "Cập nhật",
                "DELETE" => "Xoá",
                _ => method
            };

            return (verb, $"{verb} {noun}{suffix}");
        }
    }
}