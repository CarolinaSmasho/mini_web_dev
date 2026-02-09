

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Swagger for API testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// MongoDB Configuration
builder.Services.Configure<GamerLFG.Data.MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<GamerLFG.Data.MongoDbContext>();

// Repositories
builder.Services.AddScoped<GamerLFG.Repositories.IUserRepository, GamerLFG.Repositories.MongoUserRepository>();
builder.Services.AddScoped<GamerLFG.Repositories.ILobbyRepository, GamerLFG.Repositories.MongoLobbyRepository>();
builder.Services.AddScoped<GamerLFG.Repositories.IApplicationRepository, GamerLFG.Repositories.MongoApplicationRepository>();
builder.Services.AddScoped<GamerLFG.Repositories.IEndorsementRepository, GamerLFG.Repositories.MongoEndorsementRepository>();
builder.Services.AddScoped<GamerLFG.Repositories.IFriendRequestRepository, GamerLFG.Repositories.MongoFriendRequestRepository>();
builder.Services.AddScoped<GamerLFG.Repositories.INotificationRepository, GamerLFG.Repositories.MongoNotificationRepository>();
builder.Services.AddScoped<GamerLFG.Repositories.IKarmaRepository, GamerLFG.Repositories.MongoKarmaRepository>();

// Services
builder.Services.AddScoped<GamerLFG.Services.KarmaService>();
builder.Services.AddScoped<GamerLFG.Services.NotificationService>();

// Register Seeder
builder.Services.AddScoped<GamerLFG.Services.MongoDbSeeder>();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".GamerLFG.Session";
});

var app = builder.Build();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<GamerLFG.Services.MongoDbSeeder>();
    await seeder.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

// Enable session middleware
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();

