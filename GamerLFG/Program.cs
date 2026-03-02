using GamerLFG.Models;
using GamerLFG.service;
using GamerLFG.Services;
using GamerLFG.Services.Interface;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Configurations & Database ---
builder.Services.AddControllersWithViews();
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));

// ลงทะเบียน MongoDB Client
builder.Services.AddSingleton<IMongoClient>(sp => 
{
    var connectionString = builder.Configuration.GetSection("MongoDB")["ConnectionString"];
    return new MongoClient(connectionString);
});

// *** จุดสำคัญ: ลงทะเบียน Database ให้ Service อื่นๆ เรียกใช้ได้ ***
builder.Services.AddSingleton<IMongoDatabase>(sp => 
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration.GetSection("MongoDB")["DatabaseName"]; 
    return client.GetDatabase(databaseName);
});

// --- 2. Custom Services ---
builder.Services.AddSingleton<MongoDBservice>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddSingleton<ILobbyService, LobbyService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();

// --- 3. Auth & Session ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- 4. Tools (Swagger) ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 5. Pipeline Setup ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "GamerLFG API V1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Seed Data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var productService = services.GetRequiredService<ProductService>();
    await productService.SeedAsync();
    
    var dbService = services.GetRequiredService<MongoDBservice>();
    await LobbySeeder.SeedAsync(dbService);
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // เปลี่ยนจาก MapStaticAssets ถ้าเป็นเวอร์ชันเก่ากว่า .NET 9
app.UseRouting();

app.UseAuthentication();
app.UseSession(); 
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();