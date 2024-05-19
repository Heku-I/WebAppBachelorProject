using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using WebAppBachelorProject.Areas.Identity.Data;
using WebAppBachelorProject.DAL.Context;
using WebAppBachelorProject.DAL.Repositories;
using WebAppBachelorProject.Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// Add services to the container.
//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

/*
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
*/

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:ApplicationDbContextConnection"]);
});


builder.Services.AddDefaultIdentity<WebAppBachelorProjectUser>(options => options.SignIn.RequireConfirmedAccount = false) //Set to false so we do not require to confirm account. 
    .AddEntityFrameworkStores<ApplicationDbContext>();


builder.Services.AddTransient<IImageProcessingService, ImageProcessingService>();



builder.Services.AddControllersWithViews();


builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IExternalApiService, ExternalApiService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddHttpClient();





builder.Services.Configure<FormOptions>(options =>
{
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartBodyLengthLimit = int.MaxValue; // Set the limit for the length of each multipart body
    options.MemoryBufferThreshold = int.MaxValue;
});

var apiKey = builder.Configuration["OpenAI:ApiKey"];

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
