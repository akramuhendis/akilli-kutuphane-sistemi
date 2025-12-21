using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Models;
using SmartLibrary.Services;
using SmartLibrary.Patterns.Singleton;
using System;
using System.Collections.Generic;

namespace SmartLibrary.Controllers
{
    /// <summary>
    /// Kaynak Yönetimi API Controller
    /// Bu controller, kütüphane kaynaklarının (Kitap, Dergi, Tez) yönetimi için 
    /// tüm CRUD operasyonlarını ve arama/filtreleme işlemlerini sağlar.
    /// Polimorfizm kullanılarak farklı kaynak türleri tek bir interface üzerinden yönetilir.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class KaynakController : ControllerBase
    {
        // Kaynak işlemleri için Kutuphane servisi (Indexer kullanımı için)
        private readonly Kutuphane _kutuphane;
        
        // Singleton pattern ile merkezi yönetim
        private readonly KutuphaneYoneticisi _yonetici;

        /// <summary>
        /// Constructor - Dependency injection yerine direkt instance kullanılıyor
        /// </summary>
        public KaynakController()
        {
            _kutuphane = new Kutuphane();
            _yonetici = KutuphaneYoneticisi.Instance;
        }

        /// <summary>
        /// GET: /api/kaynak
        /// Tüm kaynakları listeler (Kitap, Dergi, Tez dahil tüm türler)
        /// </summary>
        /// <returns>Başarılı ise 200 OK ile kaynak listesi döner</returns>
        [HttpGet]
        public ActionResult<List<Kaynak>> TumKaynaklariGetir()
        {
            return Ok(_kutuphane.TumKaynaklar());
        }

        /// <summary>
        /// GET: /api/kaynak/{isbn}
        /// ISBN numarası ile belirli bir kaynağı getirir
        /// Indexer pattern kullanılarak kaynak erişimi sağlanır
        /// </summary>
        /// <param name="isbn">Kaynağın benzersiz ISBN numarası</param>
        /// <returns>200 OK: Kaynak bulundu, 404 NotFound: Kaynak bulunamadı</returns>
        [HttpGet("{isbn}")]
        public ActionResult<Kaynak> KaynakGetir(string isbn)
        {
            try
            {
                // Indexer kullanımı: kutuphane[isbn] şeklinde erişim
                var kaynak = _kutuphane[isbn];
                return Ok(kaynak);
            }
            catch (KeyNotFoundException ex)
            {
                // ISBN ile kaynak bulunamadığında hata döndür
                return NotFound(new { mesaj = ex.Message });
            }
        }

        /// <summary>
        /// POST: /api/kaynak
        /// Yeni bir kaynak ekler. Kaynak türüne göre (Kitap/Dergi/Tez) 
        /// uygun sınıf oluşturulur (Polimorfizm)
        /// </summary>
        /// <param name="dto">Kaynak bilgilerini içeren DTO nesnesi</param>
        /// <returns>201 Created: Kaynak oluşturuldu, 400 BadRequest: Geçersiz veri</returns>
        [HttpPost]
        public ActionResult<Kaynak> KaynakEkle([FromBody] KaynakEkleDto dto)
        {
            try
            {
                // DTO validation - gerekli alanları kontrol et
                if (string.IsNullOrWhiteSpace(dto.ISBN))
                    return BadRequest(new { mesaj = "ISBN gereklidir" });
                
                if (string.IsNullOrWhiteSpace(dto.Baslik))
                    return BadRequest(new { mesaj = "Başlık gereklidir" });
                
                if (string.IsNullOrWhiteSpace(dto.Yazar))
                    return BadRequest(new { mesaj = "Yazar gereklidir" });
                
                if (string.IsNullOrWhiteSpace(dto.Tur))
                    return BadRequest(new { mesaj = "Kaynak türü gereklidir" });

                Kaynak kaynak;

                // Kaynak türüne göre uygun sınıfı oluştur (Polimorfizm)
                switch (dto.Tur.ToLower())
                {
                    case "kitap":
                        // Kitap için özel alanlar: SayfaSayisi, YayinEvi, Dil
                        kaynak = new Kitap(
                            dto.ISBN, dto.Baslik, dto.Yazar, dto.YayinTarihi,
                            dto.Kategori ?? string.Empty, dto.SayfaSayisi ?? 0, 
                            dto.YayinEvi ?? string.Empty, dto.Dil ?? "Türkçe"
                        );
                        break;
                    case "dergi":
                        // Dergi için özel alanlar: SayiNo, YayinPeriyodu, ISSN
                        kaynak = new Dergi(
                            dto.ISBN, dto.Baslik, dto.Yazar, dto.YayinTarihi,
                            dto.Kategori ?? string.Empty, dto.SayiNo ?? 0, 
                            dto.YayinPeriyodu ?? "Aylık", dto.ISSN ?? string.Empty
                        );
                        break;
                    case "tez":
                        // Tez için özel alanlar: Universite, Bolum, DanismanAdi, TezTuru
                        kaynak = new Tez(
                            dto.ISBN, dto.Baslik, dto.Yazar, dto.YayinTarihi,
                            dto.Kategori ?? string.Empty, dto.Universite ?? string.Empty, 
                            dto.Bolum ?? string.Empty, dto.DanismanAdi ?? string.Empty, 
                            dto.TezTuru ?? "Yüksek Lisans"
                        );
                        break;
                    default:
                        return BadRequest(new { mesaj = $"Geçersiz kaynak türü: '{dto.Tur}'. Geçerli türler: kitap, dergi, tez" });
                }

                // Indexer kullanarak kaynağı ekle
                _kutuphane[dto.ISBN] = kaynak;
                
                // 201 Created status kodu ile yeni oluşturulan kaynağı döndür
                return CreatedAtAction(nameof(KaynakGetir), new { isbn = kaynak.ISBN }, kaynak);
            }
            catch (Exception ex)
            {
                // Hata mesajı - production'da stack trace gösterilmez, sadece mesaj
                return BadRequest(new { mesaj = $"Kaynak eklenirken hata oluştu: {ex.Message}" });
            }
        }

        /// <summary>
        /// PUT: /api/kaynak/{isbn}
        /// Mevcut bir kaynağı günceller. 
        /// Not: Mevcut implementasyonda kaynak silinip yenisi oluşturuluyor
        /// </summary>
        /// <param name="isbn">Güncellenecek kaynağın ISBN numarası</param>
        /// <param name="dto">Güncellenmiş kaynak bilgileri</param>
        /// <returns>200 OK: Kaynak güncellendi, 400 BadRequest: Geçersiz veri, 404 NotFound: Kaynak bulunamadı</returns>
        [HttpPut("{isbn}")]
        public ActionResult<Kaynak> KaynakGuncelle(string isbn, [FromBody] KaynakEkleDto dto)
        {
            // ISBN uyumsuzluğu kontrolü
            if (isbn != dto.ISBN)
                return BadRequest(new { mesaj = "ISBN uyuşmuyor" });

            try
            {
                // Mevcut kaynağın var olup olmadığını kontrol et
                var mevcutKaynak = _kutuphane[isbn];
                
                // Mevcut kaynağı sil
                _yonetici.KaynakSil(isbn);
                
                // Yeni kaynak oluştur (tür değişikliği de mümkün)
                Kaynak yeniKaynak;
                switch (dto.Tur.ToLower())
                {
                    case "kitap":
                        yeniKaynak = new Kitap(
                            dto.ISBN, dto.Baslik, dto.Yazar, dto.YayinTarihi,
                            dto.Kategori, dto.SayfaSayisi ?? 0, dto.YayinEvi ?? string.Empty, dto.Dil ?? "Türkçe"
                        );
                        break;
                    case "dergi":
                        yeniKaynak = new Dergi(
                            dto.ISBN, dto.Baslik, dto.Yazar, dto.YayinTarihi,
                            dto.Kategori, dto.SayiNo ?? 0, dto.YayinPeriyodu ?? "Aylık", dto.ISSN ?? string.Empty
                        );
                        break;
                    case "tez":
                        yeniKaynak = new Tez(
                            dto.ISBN, dto.Baslik, dto.Yazar, dto.YayinTarihi,
                            dto.Kategori, dto.Universite ?? string.Empty, dto.Bolum ?? string.Empty, 
                            dto.DanismanAdi ?? string.Empty, dto.TezTuru ?? "Yüksek Lisans"
                        );
                        break;
                    default:
                        return BadRequest(new { mesaj = "Geçersiz kaynak türü" });
                }
                
                // Yeni kaynağı ekle
                _kutuphane[isbn] = yeniKaynak;
                return Ok(yeniKaynak);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mesaj = ex.Message });
            }
        }

        /// <summary>
        /// DELETE: /api/kaynak/{isbn}
        /// Belirtilen ISBN'ye sahip kaynağı siler
        /// </summary>
        /// <param name="isbn">Silinecek kaynağın ISBN numarası</param>
        /// <returns>200 OK: Kaynak silindi, 404 NotFound: Kaynak bulunamadı</returns>
        [HttpDelete("{isbn}")]
        public ActionResult KaynakSil(string isbn)
        {
            try
            {
                // Önce kaynağın var olup olmadığını kontrol et
                var kaynak = _kutuphane[isbn];
                
                // Singleton yönetici üzerinden kaynağı sil
                _yonetici.KaynakSil(isbn);
                
                return Ok(new { mesaj = "Kaynak silindi" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { mesaj = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/kaynak/kategori/{kategori}
        /// Belirli bir kategoriye ait tüm kaynakları getirir
        /// </summary>
        /// <param name="kategori">Filtrelenecek kategori adı</param>
        /// <returns>Kategoriye uygun kaynak listesi</returns>
        [HttpGet("kategori/{kategori}")]
        public ActionResult<List<Kaynak>> KategoriyeGoreGetir(string kategori)
        {
            var kaynaklar = _kutuphane.KategoriyeGoreFiltrele(kategori);
            return Ok(kaynaklar);
        }

        /// <summary>
        /// GET: /api/kaynak/ara/{aramaMetni}
        /// Gelişmiş arama fonksiyonu - Başlık, Yazar, ISBN ve Kategori alanlarında arama yapar
        /// </summary>
        /// <param name="aramaMetni">Aranacak metin</param>
        /// <returns>Bulunan kaynakların listesi</returns>
        [HttpGet("ara/{aramaMetni}")]
        public ActionResult<List<Kaynak>> GelismisArama(string aramaMetni)
        {
            var kaynaklar = _kutuphane.GelismisArama(aramaMetni);
            return Ok(kaynaklar);
        }

        /// <summary>
        /// GET: /api/kaynak/mevcut
        /// Ödünç verilmemiş, kütüphanede mevcut olan kaynakları getirir
        /// </summary>
        /// <returns>Mevcut kaynakların listesi</returns>
        [HttpGet("mevcut")]
        public ActionResult<List<Kaynak>> MevcutKaynaklar()
        {
            return Ok(_kutuphane.MevcutKaynaklar());
        }

        /// <summary>
        /// GET: /api/kaynak/odunc
        /// Şu anda ödünç verilmiş kaynakları listeler
        /// </summary>
        /// <returns>Ödünç verilmiş kaynakların listesi</returns>
        [HttpGet("odunc")]
        public ActionResult<List<Kaynak>> OduncVerilenKaynaklar()
        {
            return Ok(_kutuphane.OduncVerilenKaynaklar());
        }
    }

    /// <summary>
    /// Kaynak Ekleme/Güncelleme için Data Transfer Object (DTO) sınıfı
    /// Bu sınıf, HTTP isteklerinden gelen verileri taşır ve tüm kaynak türleri için
    /// ortak ve özel alanları içerir.
    /// </summary>
    public class KaynakEkleDto
    {
        /// <summary>
        /// Kaynak türü: "kitap", "dergi" veya "tez"
        /// </summary>
        public string Tur { get; set; } = string.Empty;
        
        /// <summary>
        /// Kaynağın benzersiz ISBN/ISSN numarası
        /// </summary>
        public string ISBN { get; set; } = string.Empty;
        
        /// <summary>
        /// Kaynağın başlığı
        /// </summary>
        public string Baslik { get; set; } = string.Empty;
        
        /// <summary>
        /// Yazar/Yayınevi adı
        /// </summary>
        public string Yazar { get; set; } = string.Empty;
        
        /// <summary>
        /// Yayın tarihi
        /// </summary>
        public DateTime YayinTarihi { get; set; }
        
        /// <summary>
        /// Kaynağın kategorisi (örn: "Klasik Edebiyat", "Bilim", vb.)
        /// </summary>
        public string Kategori { get; set; } = string.Empty;
        
        // ========== KİTAP İÇİN ÖZEL ALANLAR ==========
        /// <summary>
        /// Kitabın sayfa sayısı (sadece Kitap türü için)
        /// </summary>
        public int? SayfaSayisi { get; set; }
        
        /// <summary>
        /// Yayın evi adı (sadece Kitap türü için)
        /// </summary>
        public string? YayinEvi { get; set; }
        
        /// <summary>
        /// Kitabın dili (sadece Kitap türü için)
        /// </summary>
        public string? Dil { get; set; }
        
        // ========== DERGİ İÇİN ÖZEL ALANLAR ==========
        /// <summary>
        /// Derginin sayı numarası (sadece Dergi türü için)
        /// </summary>
        public int? SayiNo { get; set; }
        
        /// <summary>
        /// Yayın periyodu (Aylık, Haftalık, vb.) (sadece Dergi türü için)
        /// </summary>
        public string? YayinPeriyodu { get; set; }
        
        /// <summary>
        /// Derginin ISSN numarası (sadece Dergi türü için)
        /// </summary>
        public string? ISSN { get; set; }
        
        // ========== TEZ İÇİN ÖZEL ALANLAR ==========
        /// <summary>
        /// Tezin yapıldığı üniversite (sadece Tez türü için)
        /// </summary>
        public string? Universite { get; set; }
        
        /// <summary>
        /// Tezin yapıldığı bölüm (sadece Tez türü için)
        /// </summary>
        public string? Bolum { get; set; }
        
        /// <summary>
        /// Danışman adı (sadece Tez türü için)
        /// </summary>
        public string? DanismanAdi { get; set; }
        
        /// <summary>
        /// Tez türü: "Yüksek Lisans" veya "Doktora" (sadece Tez türü için)
        /// </summary>
        public string? TezTuru { get; set; }
    }
}
