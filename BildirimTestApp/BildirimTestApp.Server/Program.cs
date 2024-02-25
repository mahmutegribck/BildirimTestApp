using BildirimTestApp.Server.Models;
using BildirimTestApp.Server.Servisler.Bildirim;
using BildirimTestApp.Server.Servisler.Kullanici;
using BildirimTestApp.Server.Servisler.OturumYonetimi;

namespace BildirimTestApp.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddDbContext<TestDbContext>();
        builder.Services.BildirimEkle(builder.Environment.IsDevelopment(), builder.Configuration);

        builder.Services.AddHostedService<PeriyodikBildirimOkuyucu>();

        builder.Services.AddScoped<IKullaniciBilgiServisi, KullaniciBilgiServisi>();
        builder.Services.AddScoped<IOturumYonetimiServisi, OturumYonetimiServisi>();

        builder.Services.AddMemoryCache();


        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.BildirimKullan();

        app.UseAuthorization();

        app.MapControllers();

        app.MapFallbackToFile("/index.html");

        app.Run();
    }
}
