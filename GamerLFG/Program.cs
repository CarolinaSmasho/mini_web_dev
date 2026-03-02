using GamerLFG.Models;
using GamerLFG.service;
using GamerLFG.Services;
using GamerLFG.Services.Interface;
using MongoDB.Driver;using Microsoft.AspNetCore.Authentication.Cookies;
using GamerLFG.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(sp => 
{
    var connectionString = builder.Configuration.GetSection("MongoDB")["ConnectionString"];
    return new MongoClient(connectionString);
});
builder.Services.AddSingleton<MongoDBservice>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ILobbyService, LobbyService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ILobbyService,LobbyService>();
// Session (ใช้สำหรับเก็บ UserId หลัง login)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});




builder.Services.AddSingleton<AuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // ถ้ายังไม่ได้ Login ให้เด้งไปหน้านี้
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // ให้จำ Login ไว้ 60 นาที
    });
var app = builder.Build();

// --- Swagger (dev only) ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GamerLFG API V1");
    });
}

// --- Seed Products ---
using (var scope = app.Services.CreateScope())
{
    var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
    await productService.SeedAsync();
}

// --- Seed Lobby test data ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MongoDBservice>();
    await LobbySeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();          // ต้องอยู่ก่อน UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
