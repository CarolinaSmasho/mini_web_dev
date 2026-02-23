using GamerLFG.Models;
using GamerLFG.service;
using GamerLFG.Services;
using GamerLFG.Data;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDBservice>();
builder.Services.AddSingleton<ProductService>();
// builder.Services.AddScoped<ILobbyService, LobbyService>();
builder.Services.AddScoped<IUserService, UserService>();
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

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();