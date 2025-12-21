using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Services;
using System.Collections.Generic;

namespace SmartLibrary.Controllers
{
    /// <summary>
    /// Öneri Sistemi API Controller
    /// Bu controller, akıllı öneri sistemi için endpoint'ler sağlar.
    /// Öneri sistemi, Chain of Responsibility pattern kullanarak
    /// 5 aşamalı filtre zinciri ile kullanıcıya özel öneriler üretir.
    /// 
    /// Filtre Zinciri:
    /// 1. Kategori Filtresi - Kullanıcının okuduğu kategorilerle eşleştir
    /// 2. İlgi Alanı Filtresi - Kullanıcının ilgi alanlarına göre filtrele
    /// 3. Okuma Geçmişi Filtresi - Daha önce okunmayanları öne çıkar
    /// 4. Yaş Filtresi - Yaş grubuna uygun kaynakları seç
    /// 5. Popülarite Filtresi - Popüler ve keşif dengesini koru
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OneriController : ControllerBase
    {
        // Öneri sistemi servisi (Chain of Responsibility pattern kullanır)
        private readonly OneriSistemi _oneriSistemi;

        /// <summary>
        /// Constructor - Öneri sistemi servisini başlatır
        /// </summary>
        public OneriController()
        {
            _oneriSistemi = new OneriSistemi();
        }

        /// <summary>
        /// GET: /api/oneri/kullanici/{kullaniciId}?sayi={count}
        /// Belirli bir kullanıcı için kişiselleştirilmiş öneriler üretir
        /// 
        /// Öneri Algoritması:
        /// - Kullanıcının okuma geçmişi analiz edilir
        /// - İlgi alanları ve favori kategorileri dikkate alınır
        /// - 5 aşamalı filtre zinciri uygulanır
        /// - Her kaynak için 0-100 arası öneri skoru hesaplanır
        /// - Öneri nedenleri açıklanır (kategori uyumu, popülerlik, vb.)
        /// 
        /// Skor Hesaplama:
        /// - Kategori uyumu: 0-30 puan
        /// - İlgi alanı uyumu: 0-25 puan
        /// - Popülarite: 0-20 puan
        /// - Yazar tanıdıklığı: 0-15 puan
        /// - Yenilik: 0-10 puan
        /// </summary>
        /// <param name="kullaniciId">Öneriler üretilecek kullanıcının ID'si</param>
        /// <param name="sayi">Döndürülecek öneri sayısı (varsayılan: 10)</param>
        /// <returns>Öneri listesi (Kaynak, Öneri Skoru, Öneri Nedenleri)</returns>
        [HttpGet("kullanici/{kullaniciId}")]
        public ActionResult GetirKullaniciOnerileri(string kullaniciId, [FromQuery] int sayi = 10)
        {
            // Öneri sistemi üzerinden kişiselleştirilmiş öneriler üret
            var oneriler = _oneriSistemi.OnerilerUret(kullaniciId, sayi);
            
            return Ok(oneriler);
        }

        /// <summary>
        /// GET: /api/oneri/benzer/{isbn}?sayi={count}
        /// Belirli bir kaynağa benzer kaynakları bulur
        /// 
        /// Benzerlik Kriterleri:
        /// - Aynı kategoriye ait kaynaklar
        /// - Aynı yazarın eserleri
        /// - Popülerlik sırasına göre sıralanır
        /// </summary>
        /// <param name="isbn">Benzeri aranacak kaynağın ISBN numarası</param>
        /// <param name="sayi">Döndürülecek benzer kaynak sayısı (varsayılan: 5)</param>
        /// <returns>Benzer kaynakların listesi</returns>
        [HttpGet("benzer/{isbn}")]
        public ActionResult BenzerKaynaklar(string isbn, [FromQuery] int sayi = 5)
        {
            // İlgili kaynağa benzer kaynakları bul
            var benzerKaynaklar = _oneriSistemi.BenzerKaynaklarBul(isbn, sayi);
            
            return Ok(benzerKaynaklar);
        }

        /// <summary>
        /// GET: /api/oneri/trend?sayi={count}
        /// En popüler (trend) kaynakları getirir
        /// 
        /// Trend Belirleme:
        /// - Son 30 gündeki ödünç sayısına göre sıralanır
        /// - Okunma sayısına göre önceliklendirilir
        /// - En çok talep gören kaynaklar listelenir
        /// </summary>
        /// <param name="sayi">Döndürülecek trend kaynak sayısı (varsayılan: 10)</param>
        /// <returns>Trend kaynakların listesi (popülerlik sırasına göre)</returns>
        [HttpGet("trend")]
        public ActionResult TrendKaynaklar([FromQuery] int sayi = 10)
        {
            // En popüler kaynakları getir
            var trendler = _oneriSistemi.TrendKaynaklarGetir(sayi);
            
            return Ok(trendler);
        }

        /// <summary>
        /// GET: /api/oneri/kategori/{kategori}?sayi={count}
        /// Belirli bir kategoriye ait en popüler kaynakları getirir
        /// 
        /// Kullanım Senaryoları:
        /// - Kullanıcılar belirli bir kategoriye ilgi duyduğunda
        /// - Kategori bazlı keşif önerileri için
        /// - Popüler kategori kaynaklarını görüntülemek için
        /// </summary>
        /// <param name="kategori">Filtrelenecek kategori adı</param>
        /// <param name="sayi">Döndürülecek kaynak sayısı (varsayılan: 10)</param>
        /// <returns>Kategoriye ait popüler kaynakların listesi</returns>
        [HttpGet("kategori/{kategori}")]
        public ActionResult KategoriOnerileri(string kategori, [FromQuery] int sayi = 10)
        {
            // Belirtilen kategoriye ait önerileri getir
            var oneriler = _oneriSistemi.KategoriyeGoreOneriler(kategori, sayi);
            
            return Ok(oneriler);
        }
    }
}
