﻿using BildirimTestApp.Server.Servisler.Kullanici;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BildirimTestApp.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class KullaniciController : ControllerBase
    {
        private readonly IKullaniciBilgiServisi _kullaniciBilgiServisi;

        public KullaniciController(IKullaniciBilgiServisi kullaniciBilgiServisi)
        {
            _kullaniciBilgiServisi = kullaniciBilgiServisi;
        }


        [HttpGet]
        public async Task<IActionResult> GetKullanici([FromQuery] int kullaniciId)
        {
            var sisKullanici = await _kullaniciBilgiServisi.GetKullaniciBilgi(kullaniciId);

            if (sisKullanici is not null)
            {
                Console.WriteLine("denemecont");

                return Ok(sisKullanici);
            }
            return NotFound();
        }
    }
}
