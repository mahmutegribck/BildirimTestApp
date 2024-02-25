using BildirimTestApp.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BildirimTestApp.Server.Servisler.Kullanici
{
    public class KullaniciBilgiServisi : IKullaniciBilgiServisi
    {
        private readonly ILogger<KullaniciBilgiServisi> _logger;

        public KullaniciBilgiServisi(ILogger<KullaniciBilgiServisi> logger)
        {
            _logger = logger;
        }


        public async Task<SisKullanici> GetKullaniciBilgi<T>(T param)
        {
            try
            {
                if (param == null)
                    throw new ArgumentException("Parametre geçerli değil.");

                using (var context = new TestDbContext())
                {
                    if (param is int kullaniciID)
                    {
                        if (kullaniciID <= 0)
                            throw new ArgumentException("Geçersiz kullanıcı ID.");

                        return await context.SisKullanicis
                            .SingleOrDefaultAsync(k => k.KullaniciId == kullaniciID)
                            ?? throw new Exception("Kullanıcı bulunamadı.");
                    }
                    else if (param is string kullaniciAdi)
                    {
                        if (string.IsNullOrEmpty(kullaniciAdi))
                            throw new ArgumentException("Geçersiz kullanıcı adı.");

                        return await context.SisKullanicis
                            .SingleOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi)
                            ?? throw new Exception("Kullanıcı bulunamadı.");
                    }
                    else
                    {
                        throw new ArgumentException("Geçersiz parametre türü.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Kullanıcı bilgisi getirilirken bir hata oluştu.");
                throw;
            }
        }


        public async Task<SisKullanici> TryGetKullaniciBilgi(string kullaniciAdi)
        {
            try
            {
                if (kullaniciAdi == null)
                    throw new Exception("Kullanici Adi Bulunamadi.");

                using (var context = new TestDbContext())
                {
                    var sisKullanici = await context.SisKullanicis
                        .SingleOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi);

                    return sisKullanici ?? throw new Exception("Kullanici Bulunamadi.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Kullanıcı bilgisi getirilirken bir hata oluştu.");
                throw;
            }
        }
    }
}
