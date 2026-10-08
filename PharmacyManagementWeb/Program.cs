using Microsoft.AspNetCore.Authentication.Cookies;
using PharmacyManagementWeb.Services;
// Tạo sẵn thư mục wwwroot/uploads/medicines TRƯỚC khi build để UseStaticFiles nhận wwwroot
Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "medicines"));
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// HttpClient dùng để gọi sang PharmacyManagementAPI
builder.Services.AddHttpClient("PharmacyApi", client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
    client.BaseAddress = new Uri(baseUrl!);
});

builder.Services.AddScoped<IApiClient, ApiClient>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IStoreSettingsProvider, StoreSettingsProvider>();

// Đăng nhập bằng Cookie - JWT token từ API được lưu trong claim của cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();