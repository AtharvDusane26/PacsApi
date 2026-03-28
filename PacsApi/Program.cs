using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using PacsApi;
using PacsApi.Authentication;
using PacsApi.Context;
using PacsApi.DataBank;
using PacsApi.DataManagement;
using PacsApi.Services;
using SQLitePCL;

var builder = WebApplication.CreateBuilder(args);

// =========================
// 🔥 SQLite Init
// =========================
Batteries.Init();

// =========================
// 🔥 DB CONTEXT (for normal EF usage)
// =========================
var dbPath = Path.Combine(
    GeneralSettings.BaseDirectory,
    GeneralSettings.DatabaseName);

builder.Services.AddDbContext<PacsDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// =========================
// 🔥 CORE SERVICES
// =========================

// DB Handler (uses DbContext)
//builder.Services.AddScoped<IDbHandler, DBHandler>();

// DICOM Processing
builder.Services.AddScoped<DicomService>();

// =========================
// 🔥 NEW ARCHITECTURE SERVICES
// =========================

// 🔥 REQUIRED (Fix for your error)
builder.Services.AddSingleton<PacsDbContextFactory>();

// User + Batch + Manager (stateful services)
builder.Services.AddSingleton<UserManager>();
builder.Services.AddSingleton<BatchManager>();
builder.Services.AddSingleton<Manager>();

// =========================
// 🔥 CONTROLLERS + CORS
// =========================
builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});


// =========================
// 🔥 BUILD APP
// =========================
var app = builder.Build();

app.UseCors("AllowAll");
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PacsDbContext>();
    db.Database.Migrate();
}
// =========================
// 🔥 PIPELINE
// =========================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();