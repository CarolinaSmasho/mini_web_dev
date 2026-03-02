using GamerLFG.Models;
using GamerLFG.service;
using GamerLFG.Services;
using GamerLFG.Data;
using MongoDB.Driver;
using GamerLFG.Services.Interface;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.Cookies;

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
builder.Services.AddSingleton<AuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // ถ้ายังไม่ได้ Login ให้เด้งไปหน้านี้
        options.LogoutPath = "/Auth/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // ให้จำ Login ไว้ 60 นาที
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<ILobbyService,LobbyService>();


var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDBSettings>();


builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFriendRequestService, FriendRequestService>();
var mongoSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDBSettings>();

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(mongoSettings.ConnectionString);
});
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoSettings.DatabaseName);
});
var app = builder.Build();
// --- เพิ่มส่วนนี้เข้าไป ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GamerLFG API V1");
        // ถ้าอยากให้เปิดหน้าเว็บมาแล้วเจอ Swagger เลย (ไม่ต้องพิมพ์ /swagger) ให้ใส่บรรทัดนี้:
        // c.RoutePrefix = string.Empty; 
    });
}
// -----------------------

// --- Seed ข้อมูลตัวอย่าง (คงเดิม) ---
using (var scope = app.Services.CreateScope())
{
    var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
    await productService.SeedAsync();
}




// --- Seed ข้อมูลตัวอย่าง (Run Phase) ---
// สร้าง product ตัวอย่างใน MongoDB ถ้ายังว่างอยู่
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    // Seed Product
    try 
    {
        var productService = services.GetRequiredService<ProductService>();
        await productService.SeedAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error seeding Product: {ex.Message}");
    }

    // Seed User / Lobby
    try
    {
        // ตอนนี้บรรทัดล่างนี้จะไม่ Error แล้ว เพราะเราลงทะเบียน IMongoDatabase ไว้ด้านบนแล้ว!
        var database = services.GetRequiredService<IMongoDatabase>();
        MongoDbSeeder.SeedAsync(database).Wait();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An error occurred while seeding the database: {ex.Message}");
    }
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();