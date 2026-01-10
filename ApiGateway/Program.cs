var builder = WebApplication.CreateBuilder(args);

// Thêm YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        // Bỏ qua kiểm tra SSL certificate
        handler.SslOptions.RemoteCertificateValidationCallback =
            (sender, certificate, chain, errors) => true;
    });

// Cấu hình CORS (nếu cần gọi từ domain khác)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Sử dụng CORS
app.UseCors("AllowAll");

// Phục vụ static files (HTML, CSS, JS, IMG)
app.UseDefaultFiles();  // Tự động load index.html
app.UseStaticFiles();   // Phục vụ files từ wwwroot

// YARP Reverse Proxy - chuyển tiếp API requests
app.MapReverseProxy();

// Fallback cho SPA routing
app.MapFallbackToFile("index.html");

app.Run();
