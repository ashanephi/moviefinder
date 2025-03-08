using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ✅ Register MVC controllers
builder.Services.AddControllers();

// ✅ Configure large file upload limit (100MB)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB limit
});

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

// ✅ Ensure controllers are mapped correctly
app.MapControllers();

app.Run();
