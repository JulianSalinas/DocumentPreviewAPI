using Azure.Identity;
using DocumentPreviewAPI.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Retrieve the connection string
var connectionString = new Uri(builder.Configuration.GetConnectionString("AppConfig")!);

// Load configuration from Azure App Configuration
builder.Configuration.AddAzureAppConfiguration(options =>
{
    var managedIdentityId = builder.Configuration["ManagedIdentityClientId"];

    var credential = new ChainedTokenCredential(
        //new ManagedIdentityCredential(managedIdentityId),
        new VisualStudioCredential());

    options.Connect(connectionString, credential)
           // Load all keys that start with `TestApp:` and have no label
           .Select("DynamicSettings:*")
           // Configure to reload configuration if the registered sentinel key is modified
           .ConfigureRefresh(refreshOptions =>
                refreshOptions.Register("DynamicSettings:Sentinel", refreshAll: true));
});

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
    .AddInMemoryTokenCaches();

builder.Services.AddControllers();

// Add Azure App Configuration middleware to the container of services.
builder.Services.AddAzureAppConfiguration();

// Bind configuration "TestApp:Settings" section to the Settings object
builder.Services.Configure<DynamicSettings>(builder.Configuration.GetSection("DynamicSettings"));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
});

builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("my-category", LogLevel.Trace);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use Azure App Configuration middleware for dynamic configuration refresh.
app.UseAzureAppConfiguration(); 

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
