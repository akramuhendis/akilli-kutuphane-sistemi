using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Models;
using SmartLibrary.Patterns.Singleton;
using System;
using System.Collections.Generic;

namespace SmartLibrary.Controllers
{
    /// <summary>
    /// Kullanıcı Yönetimi API Controller
    /// Bu controller, kütüphane sistemindeki kullanıcıların yönetimi için 
    /// CRUD operasyonlarını ve kullanıcı bazlı sorgulamaları sağlar.
    /// Kullanıcıların ödünç geçmişi, aktif ödünçleri ve okuma alışkanlıkları takip edilir.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class KullaniciController : ControllerBase
    {
        // Singleton pattern ile merkezi kullanıcı yönetimi
        private readonly KutuphaneYoneticisi _yonetici;

        /// <summary>
        /// Constructor - Singleton instance kullanılıyor
        /// </summary>
        public KullaniciController()
        {
            _yonetici = KutuphaneYoneticisi.Instance;
        }

        /// <summary>
        /// GET: /api/kullanici
        /// Sistemdeki tüm kullanıcıları listeler
        /// </summary>
        /// <returns>Kullanıcı listesi</returns>
        [HttpGet]
        public ActionResult<List<Kullanici>> TumKullanicilariGetir()
        {
            return Ok(_yonetici.TumKullanicilariGetir());
        }

        /// <summary>
        /// GET: /api/kullanici/{id}
        /// Belirli bir kullanıcının detaylı bilgilerini getirir
        /// </summary>
        /// <param name="id">Kullanıcının benzersiz ID'si (GUID string)</param>
        /// <returns>200 OK: Kullanıcı bulundu, 404 NotFound: Kullanıcı bulunamadı</returns>
        [HttpGet("{id}")]
        public ActionResult<Kullanici> KullaniciGetir(string id)
        {
            var kullanici = _yonetici.KullaniciGetir(id);
            
            // Kullanıcı yoksa 404 hatası döndür
            if (kullanici == null)
                return NotFound(new { mesaj = "Kullanıcı bulunamadı" });
            
            return Ok(kullanici);
        }

        /// <summary>
        /// POST: /api/kullanici
        /// Yeni bir kullanıcı kaydı oluşturur
        /// Kullanıcı ID'si otomatik olarak oluşturulur (GUID)
        /// </summary>
        /// <param name="dto">Kullanıcı bilgilerini içeren DTO nesnesi</param>
        /// <returns>201 Created: Kullanıcı oluşturuldu ve oluşturulan kullanıcı bilgileri döner</returns>
        [HttpPost]
        public ActionResult<Kullanici> KullaniciEkle([FromBody] KullaniciDto dto)
        {
            // DTO'dan Kullanici nesnesi oluştur
            var kullanici = new Kullanici
            {
                Ad = dto.Ad,
                Soyad = dto.Soyad,
                Email = dto.Email,
                Yas = dto.Yas,
                // Liste alanları null ise boş liste oluştur
                IlgiAlanlari = dto.IlgiAlanlari ?? new List<string>(),
                FavoriKategoriler = dto.FavoriKategoriler ?? new List<string>()
            };

            // Singleton yönetici üzerinden kullanıcıyı ekle
            _yonetici.KullaniciEkle(kullanici);
            
            // 201 Created status kodu ile yeni kullanıcıyı döndür
            return CreatedAtAction(nameof(KullaniciGetir), new { id = kullanici.Id }, kullanici);
        }

        /// <summary>
        /// PUT: /api/kullanici/{id}
        /// Mevcut bir kullanıcının bilgilerini günceller
        /// Not: Ödünç geçmişi ve aktif ödünçler korunur, sadece profil bilgileri güncellenir
        /// </summary>
        /// <param name="id">Güncellenecek kullanıcının ID'si</param>
        /// <param name="dto">Güncellenmiş kullanıcı bilgileri</param>
        /// <returns>200 OK: Kullanıcı güncellendi, 400 BadRequest: Hata, 404 NotFound: Kullanıcı bulunamadı</returns>
        [HttpPut("{id}")]
        public ActionResult<Kullanici> KullaniciGuncelle(string id, [FromBody] KullaniciDto dto)
        {
            // Önce kullanıcının var olup olmadığını kontrol et
            var mevcutKullanici = _yonetici.KullaniciGetir(id);
            if (mevcutKullanici == null)
                return NotFound(new { mesaj = "Kullanıcı bulunamadı" });

            try
            {
                // Güncellenmiş kullanıcı nesnesi oluştur
                var guncellenmisKullanici = new Kullanici
                {
                    Id = id, // ID değişmez
                    Ad = dto.Ad,
                    Soyad = dto.Soyad,
                    Email = dto.Email,
                    Yas = dto.Yas,
                    IlgiAlanlari = dto.IlgiAlanlari ?? new List<string>(),
                    FavoriKategoriler = dto.FavoriKategoriler ?? new List<string>()
                };

                // Singleton yönetici üzerinden güncelleme yap
                // İçeride ödünç geçmişi ve aktif ödünçler korunur
                _yonetici.KullaniciGuncelle(id, guncellenmisKullanici);
                
                // Güncellenmiş kullanıcıyı döndür
                return Ok(_yonetici.KullaniciGetir(id));
            }
            catch (Exception ex)
            {
                return BadRequest(new { mesaj = $"Kullanıcı güncellenirken hata oluştu: {ex.Message}" });
            }
        }

        /// <summary>
        /// GET: /api/kullanici/{id}/gecmis
        /// Kullanıcının tüm ödünç geçmişini getirir
        /// Bu liste, iade edilmiş tüm kaynakları içerir
        /// </summary>
        /// <param name="id">Kullanıcının ID'si</param>
        /// <returns>Ödünç geçmişi kayıtlarının listesi, 404 NotFound: Kullanıcı bulunamadı</returns>
        [HttpGet("{id}/gecmis")]
        public ActionResult<List<OduncKaydi>> OduncGecmisiGetir(string id)
        {
            var kullanici = _yonetici.KullaniciGetir(id);
            if (kullanici == null)
                return NotFound(new { mesaj = "Kullanıcı bulunamadı" });

            // Kullanıcının ödünç geçmişini döndür
            return Ok(kullanici.OduncGecmisi);
        }

        /// <summary>
        /// GET: /api/kullanici/{id}/aktif-oduncler
        /// Kullanıcının şu anda ödünç aldığı ve henüz iade etmediği kaynakları listeler
        /// Bu liste, gecikme kontrolü için de kullanılır
        /// </summary>
        /// <param name="id">Kullanıcının ID'si</param>
        /// <returns>Aktif ödünç kayıtlarının listesi, 404 NotFound: Kullanıcı bulunamadı</returns>
        [HttpGet("{id}/aktif-oduncler")]
        public ActionResult<List<OduncKaydi>> AktifOdunclerGetir(string id)
        {
            var kullanici = _yonetici.KullaniciGetir(id);
            if (kullanici == null)
                return NotFound(new { mesaj = "Kullanıcı bulunamadı" });

            // Kullanıcının aktif ödünçlerini döndür
            return Ok(kullanici.AktifOduncler);
        }

        /// <summary>
        /// GET: /api/kullanici/{id}/kategoriler
        /// Kullanıcının okuduğu kaynaklardan çıkarılan kategori listesini getirir
        /// Bu bilgi öneri sistemi için kullanılır
        /// </summary>
        /// <param name="id">Kullanıcının ID'si</param>
        /// <returns>Okunan kategorilerin listesi, 404 NotFound: Kullanıcı bulunamadı</returns>
        [HttpGet("{id}/kategoriler")]
        public ActionResult<List<string>> OkunanKategorilerGetir(string id)
        {
            var kullanici = _yonetici.KullaniciGetir(id);
            if (kullanici == null)
                return NotFound(new { mesaj = "Kullanıcı bulunamadı" });

            // Kullanıcının okuduğu kategorileri hesapla ve döndür
            // Bu metod, ödünç geçmişinden kategorileri çıkarır
            return Ok(kullanici.OkunanKategoriler());
        }

        /// <summary>
        /// DELETE: /api/kullanici/{id}
        /// Belirtilen kullanıcıyı sistemden siler
        /// Güvenlik Kontrolü: Kullanıcının aktif ödünçleri varsa silme işlemi yapılmaz
        /// </summary>
        /// <param name="id">Silinecek kullanıcının ID'si</param>
        /// <returns>200 OK: Kullanıcı silindi, 400 BadRequest: Aktif ödünç var, 404 NotFound: Kullanıcı bulunamadı</returns>
        [HttpDelete("{id}")]
        public ActionResult KullaniciSil(string id)
        {
            // Önce kullanıcının var olup olmadığını kontrol et
            var kullanici = _yonetici.KullaniciGetir(id);
            if (kullanici == null)
                return NotFound(new { mesaj = "Kullanıcı bulunamadı" });

            try
            {
                // Singleton yönetici üzerinden silme işlemi
                // İçeride aktif ödünç kontrolü yapılır
                _yonetici.KullaniciSil(id);
                
                return Ok(new { mesaj = "Kullanıcı başarıyla silindi" });
            }
            catch (InvalidOperationException ex)
            {
                // Aktif ödünç varsa bu hata fırlatılır
                return BadRequest(new { mesaj = ex.Message });
            }
            catch (Exception ex)
            {
                // Diğer hatalar için genel hata mesajı
                return BadRequest(new { mesaj = $"Kullanıcı silinirken hata oluştu: {ex.Message}" });
            }
        }
    }

    /// <summary>
    /// Kullanıcı Ekleme/Güncelleme için Data Transfer Object (DTO) sınıfı
    /// Bu sınıf, HTTP isteklerinden gelen kullanıcı verilerini taşır
    /// </summary>
    public class KullaniciDto
    {
        /// <summary>
        /// Kullanıcının adı
        /// </summary>
        public string Ad { get; set; } = string.Empty;
        
        /// <summary>
        /// Kullanıcının soyadı
        /// </summary>
        public string Soyad { get; set; } = string.Empty;
        
        /// <summary>
        /// Kullanıcının e-posta adresi (benzersiz olmalı)
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Kullanıcının yaşı (öneri sistemi için kullanılır)
        /// </summary>
        public int Yas { get; set; }
        
        /// <summary>
        /// Kullanıcının ilgi alanları listesi
        /// Örnek: ["Bilim", "Teknoloji", "Edebiyat"]
        /// Öneri sistemi bu bilgiyi kullanır
        /// </summary>
        public List<string> IlgiAlanlari { get; set; } = new List<string>();
        
        /// <summary>
        /// Kullanıcının favori kategorileri listesi
        /// Örnek: ["Klasik Edebiyat", "Roman", "Bilim"]
        /// Öneri sistemi bu bilgiyi kullanır
        /// </summary>
        public List<string> FavoriKategoriler { get; set; } = new List<string>();
    }
}
