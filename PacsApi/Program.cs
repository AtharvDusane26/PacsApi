using EFCore.lib.Services;
using EFCore.lib.Utility;
using Logging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using PacsApi;
using PacsApi.Authentication;
using PacsApi.Context;
using PacsApi.DataManagement;
using PacsApi.Services;

var builder = WebApplication.CreateBuilder(args);

GeneralSettings.Initialize(builder.Configuration);

// ✅ OPTIONAL (keep only if you still need default DB usage)
builder.Services.AddDbContext<PacsDbContext>(options =>
    options.UseSqlServer($"{GeneralSettings.ConnectionString}"));
// ============================
// 🔥 NEW ARCHITECTURE SERVICES
// ============================

// ✅ Factory for creating DbContext dynamically
builder.Services.AddSingleton<PacsDbContextFactory>();

// ✅ Unit of Work Factory (IMPORTANT)
builder.Services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();

// ✅ DICOM Processing
builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<ImageService>();

// ============================
// 🔥 STATEFUL SERVICES
// ============================

builder.Services.AddSingleton<UserManager>();
builder.Services.AddSingleton<Validator>();

builder.Services.AddSingleton<LoggerService>(sp =>
{
    return new LoggerService(LoggerType.Console);
});

// ============================
// 🔥 API CONFIG
// ============================

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

// ============================
// 🔥 FILE UPLOAD LIMITS (DICOM)
// ============================

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});


// ============================
// 🔥 BUILD APP
// ============================

var app = builder.Build();

app.UseCors("AllowAll");

// ============================
// 🔥 DB MIGRATION
// ============================

using (var scope = app.Services.CreateScope())
{
    await Battries.Init(GeneralSettings.ConnectionString);

    var db = scope.ServiceProvider.GetRequiredService<PacsDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();