//1.
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PjPRN222.Models;
using PjPRN222.Services;





var builder = WebApplication.CreateBuilder(args);

// Đăng ký cấu hình SMTP từ appsettings.json
var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();
builder.Services.AddSingleton(smtpSettings);

// Đăng ký EmailService
builder.Services.AddTransient<EmailService>();

// editprofile
builder.Services.AddDbContext<ShoeStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyDatabase")));
//2.
builder.Services.AddAuthentication(option =>
{
    option.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
}).AddCookie().AddGoogle(GoogleDefaults.AuthenticationScheme, option =>
{
    option.ClientId = builder.Configuration.GetSection("GoogleKey:ClientId").Value;
    option.ClientSecret = builder.Configuration.GetSection("GoogleKey:ClientSecret").Value;
    option.CallbackPath = "/signin-google";
});


// Đọc cấu hình từ appsettings.json
var ghnConfig = builder.Configuration.GetSection("GHN");

// Đăng ký HttpClient cho GHN
builder.Services.AddHttpClient("GHN", client =>
{
    client.BaseAddress = new Uri(ghnConfig["BaseUrl"]);
    client.DefaultRequestHeaders.Add("Token", ghnConfig["ApiKey"]);
});
builder.Services.AddScoped<GhnService>(); // đảm bảo bạn đã tạo class GhnService


builder.Services.AddSession(); 

// Lưu thông tin user

// Add services to the container.
builder.Services.AddControllersWithViews();

//builder.Services.AddScoped<GhnService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LoginASignUp}/{action=Index}/{id?}");
//LoginASignUp
app.Run();
