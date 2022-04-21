using CertificateHelpers;
using WindowsFirewallFeed.Services;
using Microsoft.Extensions.Hosting.WindowsServices;

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default,
};

var builder = WebApplication.CreateBuilder(options);

//configure Kestrel
builder.WebHost.ConfigureKestrel(kestrelOptions => {
    kestrelOptions.ListenAnyIP(8444);
    kestrelOptions.ListenAnyIP(8443, listenOptions => {
        listenOptions.UseHttps(CertHelpers.GetSelfSignedCertificate());
    });
});

// Add services to the container
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add our background service to keep running.
//builder.Services.AddHostedService<FeedSvc>();

// Lets be a windows service.
builder.Host.UseWindowsService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
