using Logging;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using PacsApi;
using PacsApi.Authentication;
using PacsApi.Context;
using PacsApi.DataBank;
using PacsApi.DataManagement;
using PacsApi.Services;

var builder = WebApplication.CreateBuilder(args);


//  DB CONTEXT (for normal EF usage)

builder.Services.AddDbContext<PacsDbContext>(options =>
    options.UseSqlServer($"{GeneralSettings.ConnectionString}"));


// DICOM Processing
builder.Services.AddScoped<DicomService>();

//  NEW ARCHITECTURE SERVICES

// REQUIRED (Fix for your error)
builder.Services.AddSingleton<PacsDbContextFactory>();

// User + Batch + Manager (stateful services)
builder.Services.AddSingleton<UserManager>();
builder.Services.AddSingleton<BatchManager>();
builder.Services.AddSingleton<Manager>();

builder.Services.AddSingleton<LoggerService>(sp =>
{
    return new LoggerService(LoggerType.Console); // or Console
});
//  CONTROLLERS + CORS
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


var app = builder.Build();

app.UseCors("AllowAll");
using (var scope = app.Services.CreateScope())
{
    await Battries.Init();

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