using System.Reflection;
using BildirimTestApp.Server.Models;
using BildirimTestApp.Server.Servisler.Bildirim.Hublar;
using BildirimTestApp.Server.Servisler.Kullanici;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace BildirimTestApp.Server.Servisler.Bildirim;

public class PeriyodikBildirimOkuyucu : BackgroundService
{
    private const int kOutboxDenemeSayisi = 15;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<PeriyodikBildirimOkuyucu> logger;
    private Type[] donusturucuTipler;
    private Type[] bildirimTipler;
    private readonly TestDbContext _context;

    private readonly IMemoryCache _memoryCache;


    public PeriyodikBildirimOkuyucu(
        IServiceProvider serviceProvider,
        ILogger<PeriyodikBildirimOkuyucu> logger,
        IMemoryCache memoryCache
    )
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
        _memoryCache = memoryCache;
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
        BildirimHubKok.BagliKullaniciIdler.Add(1);
        BildirimHubKok.BagliKullaniciIdler.Add(2);
        BildirimHubKok.BagliKullaniciIdler.Add(3);
        BildirimHubKok.BagliKullaniciIdler.Add(4);
        BildirimHubKok.BagliKullaniciIdler.Add(5);
        var bagliKullaniciIdler = BildirimHubKok.BagliKullaniciIdler;

        if (bagliKullaniciIdler.Count == 0)
        {
            //logger.LogInformation("bağlı kullanıcı bulunamadı");
            return;
        }

        // di
        using var scope = serviceProvider.CreateScope();
        var testDbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var kullaniciBilgiServisi = scope.ServiceProvider.GetRequiredService<IKullaniciBilgiServisi>();
        var anlikBildirimHubContext = scope.ServiceProvider.GetRequiredService<
            IHubContext<AnlikBildirimHub>
        >();



        #region
        //await testDbContext
        //    .SisKullanicis.Where(x => bagliKullaniciIdler.Contains(x.KullaniciId))
        //    .Select(x => x.KullaniciId)
        //    .ToArrayAsync(cancellationToken);

        //var bildirimlerTest = await testDbContext
        //    .SisBildirimOutboxes.Where(x =>
        //        bagliKullaniciIdler.Contains(x.Bildirim.GonderilecekKullaniciId)
        //    )
        //    .Select(x => new
        //    {
        //        x.Bildirim,
        //        x.Bildirim.BildirimIcerik,
        //        Outbox = x
        //    })
        //    .ToArrayAsync();

        //var sisBildirimOutboxes1 = _memoryCache.Get<List<SisBildirimOutbox>>("SisBildirimOutboxes");
        #endregion



        var sisBildirimOutboxes = await _memoryCache.GetOrCreateAsync("SisBildirimOutboxes", async entry =>
        {
            var bildirimler = await testDbContext.SisBildirimOutboxes.ToListAsync();
            entry.SetValue(bildirimler);
            return bildirimler.AsQueryable();
        });

        var kullaniciBildirimleri = await sisBildirimOutboxes
        .Where(x => bagliKullaniciIdler.Contains(1))
        .Select(x => new
        {
            x.Bildirim, // Get Bildirim
            x.Bildirim.BildirimIcerik,
            Outbox = x
        }).ToArrayAsync();



        foreach (var bildirim in kullaniciBildirimleri)
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
            var bildirimGonderilecekKullanici = await kullaniciBilgiServisi.GetKullaniciBilgi(outbox.Bildirim.GonderilecekKullaniciId);
            await anlikBildirimHubContext
                .Clients.Group(bildirimGonderilecekKullanici.KullaniciAdi).SendAsync(fonksiyonIsim, bildirim);

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
