using System;
using System.Collections.Generic;

namespace BildirimTestApp.Server.Models
{
    public partial class SisKullanici
    {
        public SisKullanici()
        {
            SisBildirims = new HashSet<SisBildirim>();
        }

        public int KullaniciId { get; set; }
        public string KullaniciAdi { get; set; } = null!;
        public string? Rol { get; set; }

        public virtual ICollection<SisBildirim> SisBildirims { get; set; }
    }
}
