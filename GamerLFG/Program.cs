using GamerLFG.Models;
using GamerLFG.service;
using GamerLFG.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<MongoDBservice>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<LobbyService>();
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
    var productService = scope.ServiceProvider.GetRequiredService<ProductService>();
    await productService.SeedAsync();
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