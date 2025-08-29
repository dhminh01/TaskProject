using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure HTTP client settings
builder.Services.ConfigureHttpClientDefaults(http =>
{
    // Set default HTTP/2 version
    http.ConfigureHttpClient((context, client) =>
    {
        client.DefaultRequestVersion = new Version(2, 0);
    });

    if (builder.Environment.IsDevelopment())
    {
        // Accept any SSL certificate in development
        http.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // React app
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

// Configure endpoints
app.MapReverseProxy();

await app.RunAsync();
