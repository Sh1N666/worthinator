using ApiInator.Application;
using ApiInator.Application.HowLongToBeatApi;
using ApiInator.Application.SteamApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using SlimMessageBus.Host;
using SlimMessageBus.Host.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSlimMessageBus(mbb =>
{
    mbb
        .WithProviderMemory()
        .AutoDeclareFrom(typeof(Program).Assembly); 
});
builder.Services.AddGrpc();
builder.Services.AddSteamApi();
builder.Services.AddHLTBApi();

var app = builder.Build();

app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });
app.MapGet("/status", ([FromServices]SteamApi steamApi) => "I'm Alive");

app.MapGrpcService<ApiInatorController>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    //db.Database.Migrate();
}

app.Run();

