using BildirimTestApp.Server.Models;

namespace BildirimTestApp.Server.Servisler.Kullanici
{
    public interface IKullaniciBilgiServisi
    {
        Task<SisKullanici> GetKullaniciBilgi<T>(T param);

        Task<SisKullanici> TryGetKullaniciBilgi(string kullaniciAdi);
    }
}
