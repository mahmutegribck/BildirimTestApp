using System.Reflection;
using BildirimTestApp.Server.Models;
using BildirimTestApp.Server.Servisler.Bildirim.Hublar;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BildirimTestApp.Server.Servisler.Bildirim;

public class PeriyodikBildirimOkuyucu : BackgroundService
{
    private const int kOutboxDenemeSayisi = 15;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<PeriyodikBildirimOkuyucu> logger;
    private Type[] donusturucuTipler;
    private Type[] bildirimTipler;

    public PeriyodikBildirimOkuyucu(
        IServiceProvider serviceProvider,
        ILogger<PeriyodikBildirimOkuyucu> logger
    )
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        donusturucuTipler = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => !x.IsAbstract && x.IsAssignableTo(typeof(IBildirimDonusturucuKok)))
            .ToArray();
        bildirimTipler = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => !x.IsAbstract && x.IsAssignableTo(typeof(IBildirimKok)))
            .ToArray();
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await BildirimleriIsle(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
        }
    }

    private async Task BildirimleriIsle(CancellationToken cancellationToken)
    {
        var bagliKullaniciIdler = BildirimHubKok.BagliKullaniciIdler;

        if (bagliKullaniciIdler.Count == 0)
        {
            //logger.LogInformation("bağlı kullanıcı bulunamadı");
            return;
        }

        // di
        var testDbContext = serviceProvider.GetRequiredService<TestDbContext>();
        var kullaniciBilgiServisi = serviceProvider.GetRequiredService<IKullaniciBilgiServisi>();
        var anlikBildirimHubContext = serviceProvider.GetRequiredService<
            IHubContext<AnlikBildirimHub>
        >();

        await testDbContext
            .SisKullanicis.Where(x => bagliKullaniciIdler.Contains(x.KullaniciId))
            .Select(x => x.KullaniciId)
            .ToArrayAsync(cancellationToken);

        var bildirimler = await testDbContext
            .SisBildirimOutboxes.Where(x =>
                bagliKullaniciIdler.Contains(x.Bildirim.GonderilecekKullaniciId)
            )
            .Select(x => new
            {
                x.Bildirim,
                x.Bildirim.BildirimIcerik,
                Outbox = x
            })
            .ToArrayAsync();

        foreach (var bildirim in bildirimler)
        {
            var bildirimSerilestirmeKonteyner =
                JsonConvert.DeserializeObject<BildirimSerilestirmeKonteyner>(
                    bildirim.BildirimIcerik.Json
                )!;

            var tipIsmi = bildirimSerilestirmeKonteyner.BildirimId;

            var bildirimTip = bildirimTipler.FirstOrDefault(x => x.Name == tipIsmi);

            if (bildirimTip == null)
                logger.LogInformation("Tip bulunamadı");

            var bildirimIcerik = JsonConvert.DeserializeObject(
                bildirimSerilestirmeKonteyner.BildirimJSon,
                bildirimTip
            );

            if (bildirimIcerik == null)
                logger.LogInformation("Bildirim deserialize hata");

            var gonderilecekObje = bildirimIcerik;
            if (
                bildirimTip
                    .GetInterfaces()
                    .Any(x =>
                        x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IBildirim<>)
                    )
            )
            {
                Type? donusturucuTip = null;

                foreach (var dt in donusturucuTipler)
                {
                    var genTypeDef = dt.GetInterfaces()
                        .FirstOrDefault(x =>
                            x.IsGenericType
                            && x.GetGenericTypeDefinition() == typeof(IBildirimDonusturucu<,>)
                        );

                    if (genTypeDef == null)
                        throw new Exception("Dönüştürücü tipler oluşturulurken hata");

                    if (genTypeDef.GenericTypeArguments.First() == bildirimTip)
                    {
                        donusturucuTip = dt;
                        break;
                    }
                }

                if (donusturucuTip == null)
                    throw new Exception($"{tipIsmi} için dönüştürücü tip bulunamadı");

                var donusturucu = ActivatorUtilities.CreateInstance(
                    serviceProvider,
                    donusturucuTip
                );

                var metodInfo = donusturucuTip
                    .GetMethods()
                    .First(x => x.Name == nameof(IBildirimDonusturucu<int, int>.Donustur))!;

                gonderilecekObje = metodInfo.Invoke(donusturucu, new[] { bildirimIcerik });
            }

            if (gonderilecekObje == null)
                logger.LogInformation("gönderilecek bildirim boş geldi.");
            if (bildirimTip.IsAssignableTo(typeof(IAnlikBildirimKok)))
            {
                await OutboxBildirimGonder(
                    testDbContext,
                    anlikBildirimHubContext,
                    bildirim,
                    bildirim.Outbox,
                    "AnlikBildirimAl",
                    kullaniciBilgiServisi
                );
            }
            else if (bildirimTip.IsAssignableTo(typeof(IDuyuruBildirimKok)))
            {
                await OutboxBildirimGonder(
                    testDbContext,
                    anlikBildirimHubContext,
                    bildirim,
                    bildirim.Outbox,
                    "DuyuruAl",
                    kullaniciBilgiServisi
                );
            }
            else if (bildirimTip.IsAssignableTo(typeof(IEPostaBildirimKok))) { }
            else
            {
                logger.LogInformation($"Bildirim türü bulunamadı {bildirimTip}");
            }
        }
    }

    private static async Task OutboxBildirimGonder(
        TestDbContext testDbContext,
        IHubContext<BildirimHubKok> anlikBildirimHubContext,
        object bildirim,
        SisBildirimOutbox outbox,
        string fonksiyonIsim,
        IKullaniciBilgiServisi kullaniciBilgiServisi
    )
    {
        if (outbox.GonderimDenemeSayisi <= kOutboxDenemeSayisi)
        {
            await anlikBildirimHubContext
                .Clients.Group(
                    kullaniciBilgiServisi
                        .GetKullaniciBilgi(outbox.Bildirim.GonderilecekKullaniciId)
                        .KullaniciAdi
                )
                .SendAsync(fonksiyonIsim, bildirim);
            testDbContext.SisBildirimOutboxes.Remove(outbox);
            testDbContext.SaveChanges();
            outbox.Bildirim.GonderimDurumu = 1;
        }
        else
        {
            outbox.GonderimDenemeSayisi += 1;
        }
        testDbContext.SaveChanges();
    }
}
