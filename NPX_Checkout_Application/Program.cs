using NPX_Checkout_Application.Models;

var builder = WebApplication.CreateBuilder(args);
// Register application settings and services
ConfigureServices(builder);

var app = builder.Build();

// Configure the HTTP request pipeline
ConfigurePipeline(app);

app.Run();
static void ConfigureServices(WebApplicationBuilder builder)
{
    // Register application settings
    var appSettings = new AppSettings();
    builder.Configuration.GetSection("AppSettings").Bind(appSettings);
    builder.Services.AddSingleton(appSettings);

    // Register merchant data
    var merchantData = new MerchantData();
    builder.Configuration.GetSection("MerchantData").Bind(merchantData);
    builder.Services.AddSingleton(merchantData);

    builder.Services.AddHttpClient(); // Register HttpClient for DI
    
    // Add services to the container.
    builder.Services.AddControllersWithViews();

}

static void ConfigurePipeline(WebApplication app)
{
    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
}