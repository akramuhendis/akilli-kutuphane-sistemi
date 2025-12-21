using Microsoft.AspNetCore.Mvc;
using SmartLibrary.Services;
using SmartLibrary.Patterns.Singleton;
using System;

namespace SmartLibrary.Controllers
{
    /// <summary>
    /// İstatistik ve Raporlama API Controller
    /// Bu controller, kütüphane sistemi için istatistiksel verileri ve 
    /// raporları sağlar. CSV formatında dışa aktarma özellikleri içerir.
    /// 
    /// Özellikler:
    /// - Özet istatistikler (toplam kaynak, kullanıcı, ödünç sayıları)
    /// - Popüler kaynaklar analizi
    /// - Gecikme raporları
    /// - Kullanıcı aktivite raporları
    /// - Kategori analizi
    /// - CSV export fonksiyonları
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class IstatistikController : ControllerBase
    {
        // İstatistik servisi (CSV export işlemleri için)
        private readonly IstatistikServisi _istatistikServisi;
        
        // Singleton yönetici (popüler kaynaklar, işlem geçmişi için)
        private readonly KutuphaneYoneticisi _yonetici;

        /// <summary>
        /// Constructor - Servisleri başlatır
        /// </summary>
        public IstatistikController()
        {
            _istatistikServisi = new IstatistikServisi();
            _yonetici = KutuphaneYoneticisi.Instance;
        }

        /// <summary>
        /// GET: /api/istatistik/ozet
        /// Sistem genelinde özet istatistikleri getirir
        /// 
        /// İçerik:
        /// - Toplam kaynak sayısı
        /// - Toplam kullanıcı sayısı
        /// - Ödünç verilen kaynak sayısı
        /// - Mevcut kaynak sayısı
        /// - Toplam okunma sayısı
        /// - Gecikmeli ödünç sayısı
        /// - Toplam ceza tutarı
        /// - Kategori sayısı
        /// </summary>
        /// <returns>Özet istatistikler dictionary formatında</returns>
        [HttpGet("ozet")]
        public ActionResult OzetIstatistikler()
        {
            var istatistikler = _istatistikServisi.OzetIstatistiklerGetir();
            return Ok(istatistikler);
        }

        /// <summary>
        /// GET: /api/istatistik/populer
        /// En popüler 10 kaynağı getirir
        /// 
        /// Popülerlik Kriteri:
        /// - Okunma sayısına göre sıralanır
        /// - En çok okunan kaynaklar en üstte
        /// </summary>
        /// <returns>Popüler kaynakların listesi (okunma sayısına göre sıralı)</returns>
        [HttpGet("populer")]
        public ActionResult PopulerKaynaklar()
        {
            var populerKaynaklar = _yonetici.EnPopuler10Kaynak();
            return Ok(populerKaynaklar);
        }

        /// <summary>
        /// GET: /api/istatistik/export/gunluk/{tarih}
        /// Belirtilen tarihe ait günlük istatistikleri CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Günlük işlem türleri ve sayıları
        /// - Saatlik işlem detayları
        /// - İşlem açıklamaları
        /// 
        /// Dosya Konumu:
        /// - backend/exports/ klasörüne kaydedilir
        /// - Dosya adı: gunluk_istatistik_YYYY-MM-DD.csv
        /// </summary>
        /// <param name="tarih">Raporlanacak tarih</param>
        /// <returns>Dosya yolunu içeren başarı mesajı</returns>
        [HttpGet("export/gunluk/{tarih}")]
        public ActionResult GunlukIstatistikExport(DateTime tarih)
        {
            var dosyaYolu = _istatistikServisi.GunlukIstatistikleriDisaAktar(tarih);
            return Ok(new { mesaj = "CSV oluşturuldu", dosyaYolu });
        }

        /// <summary>
        /// GET: /api/istatistik/export/populer
        /// En popüler kaynakları CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Sıra numarası
        /// - ISBN
        /// - Başlık
        /// - Yazar
        /// - Kategori
        /// - Okunma sayısı
        /// - Kaynak türü
        /// 
        /// Dosya Konumu:
        /// - backend/exports/ klasörüne kaydedilir
        /// - Dosya adı: populer_kaynaklar_YYYY-MM-DD_HHmmss.csv
        /// </summary>
        /// <returns>Dosya yolunu içeren başarı mesajı</returns>
        [HttpGet("export/populer")]
        public ActionResult PopulerKaynaklarExport()
        {
            var dosyaYolu = _istatistikServisi.PopulerKaynaklariDisaAktar();
            return Ok(new { mesaj = "CSV oluşturuldu", dosyaYolu });
        }

        /// <summary>
        /// GET: /api/istatistik/export/gecikme
        /// Gecikmeli ödünçleri CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Kullanıcı adı
        /// - Kaynak başlığı
        /// - Gecikme gün sayısı
        /// - Ceza tutarı (TL)
        /// - Toplam gecikme sayısı
        /// - Toplam ceza tutarı
        /// 
        /// Dosya Konumu:
        /// - backend/exports/ klasörüne kaydedilir
        /// - Dosya adı: gecikme_raporu_YYYY-MM-DD_HHmmss.csv
        /// </summary>
        /// <returns>Dosya yolunu içeren başarı mesajı</returns>
        [HttpGet("export/gecikme")]
        public ActionResult GecikmeRaporuExport()
        {
            var dosyaYolu = _istatistikServisi.GecikmeRaporuDisaAktar();
            return Ok(new { mesaj = "CSV oluşturuldu", dosyaYolu });
        }

        /// <summary>
        /// GET: /api/istatistik/export/kullanici-aktivite
        /// Kullanıcı aktivite raporunu CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Ad Soyad
        /// - E-posta
        /// - Yaş
        /// - Toplam ödünç sayısı
        /// - Aktif ödünç sayısı
        /// - Favori kategoriler
        /// 
        /// Dosya Konumu:
        /// - backend/exports/ klasörüne kaydedilir
        /// - Dosya adı: kullanici_aktivite_YYYY-MM-DD_HHmmss.csv
        /// </summary>
        /// <returns>Dosya yolunu içeren başarı mesajı</returns>
        [HttpGet("export/kullanici-aktivite")]
        public ActionResult KullaniciAktiviteExport()
        {
            var dosyaYolu = _istatistikServisi.KullaniciAktiviteRaporuDisaAktar();
            return Ok(new { mesaj = "CSV oluşturuldu", dosyaYolu });
        }

        /// <summary>
        /// GET: /api/istatistik/export/kategori-analizi
        /// Kategori bazlı analiz raporunu CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Kategori adı
        /// - Toplam kaynak sayısı
        /// - Toplam okunma sayısı
        /// - Ortalama okunma sayısı
        /// - Şu an ödünçte olan kaynak sayısı
        /// 
        /// Analiz:
        /// - Kategoriler popülerlik sırasına göre listelenir
        /// - Her kategorinin performansı görülebilir
        /// 
        /// Dosya Konumu:
        /// - backend/exports/ klasörüne kaydedilir
        /// - Dosya adı: kategori_analizi_YYYY-MM-DD_HHmmss.csv
        /// </summary>
        /// <returns>Dosya yolunu içeren başarı mesajı</returns>
        [HttpGet("export/kategori-analizi")]
        public ActionResult KategoriAnaliziExport()
        {
            var dosyaYolu = _istatistikServisi.KategoriAnaliziDisaAktar();
            return Ok(new { mesaj = "CSV oluşturuldu", dosyaYolu });
        }

        /// <summary>
        /// GET: /api/istatistik/islem-gecmisi
        /// Sistemdeki tüm işlem geçmişini getirir
        /// 
        /// İşlem Türleri:
        /// - KAYNAK_EKLENDI
        /// - KAYNAK_SILINDI
        /// - KULLANICI_EKLENDI
        /// - KULLANICI_GUNCELLENDI
        /// - KULLANICI_SILINDI
        /// - ODUNC_VERILDI
        /// - IADE_ALINDI
        /// 
        /// Sıralama:
        /// - Tarihe göre azalan sırada (en yeni önce)
        /// </summary>
        /// <returns>İşlem kayıtlarının listesi</returns>
        [HttpGet("islem-gecmisi")]
        public ActionResult IslemGecmisi()
        {
            var gecmis = _yonetici.IslemGecmisiGetir();
            return Ok(gecmis);
        }
    }
}
