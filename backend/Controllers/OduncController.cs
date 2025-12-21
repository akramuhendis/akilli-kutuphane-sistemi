using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Patterns.Singleton;

namespace SmartLibrary.Controllers
{
    /// <summary>
    /// Ödünç İşlemleri API Controller
    /// Bu controller, kütüphane kaynaklarının ödünç verilmesi, iade edilmesi
    /// ve gecikme takibi gibi işlemleri yönetir.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OduncController : ControllerBase
    {
        // Singleton pattern ile merkezi işlem yönetimi
        private readonly KutuphaneYoneticisi _yonetici;

        /// <summary>
        /// Constructor - Singleton instance kullanılıyor
        /// </summary>
        public OduncController()
        {
            _yonetici = KutuphaneYoneticisi.Instance;
        }

        /// <summary>
        /// POST: /api/odunc/odunc-ver
        /// Bir kaynağı kullanıcıya ödünç verir
        /// İşlem Kuralları:
        /// - Kaynak mevcut olmalı (ödünç verilmemiş)
        /// - Kullanıcı sistemde kayıtlı olmalı
        /// - Kaynak ödünç verildikten sonra durumu güncellenir
        /// - Ödünç kaydı oluşturulur ve kullanıcının aktif ödünçlerine eklenir
        /// </summary>
        /// <param name="dto">Kullanıcı ID ve ISBN bilgilerini içeren DTO</param>
        /// <returns>200 OK: Ödünç verme başarılı, 400 BadRequest: İşlem başarısız (kaynak mevcut değil veya zaten ödünçte)</returns>
        [HttpPost("odunc-ver")]
        public ActionResult OduncVer([FromBody] OduncDto dto)
        {
            // Singleton yönetici üzerinden ödünç verme işlemi
            // İşlem başarılı ise true, başarısız ise false döner
            var basarili = _yonetici.OduncVer(dto.KullaniciId, dto.ISBN);
            
            if (basarili)
            {
                // Başarılı durumda 200 OK ve başarı mesajı döndür
                return Ok(new { mesaj = "Kaynak ödünç verildi", basarili = true });
            }
            
            // Başarısız durumda 400 BadRequest döndür
            // Başarısızlık sebepleri:
            // - Kullanıcı bulunamadı
            // - Kaynak bulunamadı
            // - Kaynak zaten ödünç verilmiş
            return BadRequest(new { mesaj = "Ödünç verme işlemi başarısız", basarili = false });
        }

        /// <summary>
        /// POST: /api/odunc/iade-al
        /// Bir kullanıcının ödünç aldığı kaynağı iade alır
        /// İşlem Kuralları:
        /// - Kaynak ödünç verilmiş olmalı
        /// - Kullanıcının aktif ödünçlerinde olmalı
        /// - İade tarihi kaydedilir
        /// - Kaynak durumu "mevcut" olarak güncellenir
        /// - Ödünç kaydı geçmişe taşınır
        /// - Gecikme varsa ceza hesaplanır (polimorfik - kaynak türüne göre farklı)
        /// </summary>
        /// <param name="dto">Kullanıcı ID ve ISBN bilgilerini içeren DTO</param>
        /// <returns>200 OK: İade alma başarılı, 400 BadRequest: İşlem başarısız</returns>
        [HttpPost("iade-al")]
        public ActionResult IadeAl([FromBody] OduncDto dto)
        {
            // Singleton yönetici üzerinden iade alma işlemi
            var basarili = _yonetici.IadeAl(dto.KullaniciId, dto.ISBN);
            
            if (basarili)
            {
                // Başarılı durumda 200 OK ve başarı mesajı döndür
                return Ok(new { mesaj = "Kaynak iade alındı", basarili = true });
            }
            
            // Başarısız durumda 400 BadRequest döndür
            // Başarısızlık sebepleri:
            // - Kullanıcı bulunamadı
            // - Kaynak bulunamadı
            // - Kaynak ödünç verilmemiş
            // - Kullanıcının bu kaynağı ödünç almamış
            return BadRequest(new { mesaj = "İade alma işlemi başarısız", basarili = false });
        }

        /// <summary>
        /// GET: /api/odunc/gecikme-uyarilari
        /// Sistemdeki tüm gecikmeli ödünçleri listeler
        /// Gecikme Hesaplama:
        /// - Her kaynak türü için farklı teslim süresi vardır (polimorfik)
        /// - Kitap: 14 gün
        /// - Dergi: 7 gün
        /// - Tez: 21 gün
        /// Ceza Hesaplama:
        /// - Her kaynak türü için farklı ceza ücreti vardır (polimorfik)
        /// - Kitap: 2 TL/gün
        /// - Dergi: 1 TL/gün
        /// - Tez: 3 TL/gün
        /// </summary>
        /// <returns>Gecikme uyarılarının listesi (Kullanıcı adı, Kaynak başlığı, Gecikme gün sayısı, Ceza tutarı)</returns>
        [HttpGet("gecikme-uyarilari")]
        public ActionResult GecikmeUyarilari()
        {
            // Tüm kullanıcıların aktif ödünçlerini kontrol ederek
            // gecikmiş olanları tespit eder ve listeler
            var uyarilar = _yonetici.GecikmeUyarilariGetir();
            
            return Ok(uyarilar);
        }
    }

    /// <summary>
    /// Ödünç Verme/İade İşlemleri için Data Transfer Object (DTO) sınıfı
    /// Bu sınıf, ödünç verme ve iade alma işlemleri için gerekli bilgileri taşır
    /// </summary>
    public class OduncDto
    {
        /// <summary>
        /// İşlemi yapacak veya iade edecek kullanıcının benzersiz ID'si
        /// </summary>
        public string KullaniciId { get; set; } = string.Empty;
        
        /// <summary>
        /// Ödünç verilecek veya iade edilecek kaynağın ISBN numarası
        /// </summary>
        public string ISBN { get; set; } = string.Empty;
    }
}
