using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SmartLibrary.Models;
using SmartLibrary.Patterns.Singleton;

namespace SmartLibrary.Services
{
    /// <summary>
    /// İstatistik Servis Sınıfı
    /// 
    /// Bu sınıf, kütüphane sistemi için istatistiksel verileri üretir
    /// ve CSV formatında raporlar oluşturur.
    /// 
    /// Özellikler:
    /// - Özet istatistikler (Dashboard için)
    /// - CSV export işlemleri (5 farklı rapor türü)
    /// - Günlük istatistik hesaplamaları
    /// - Kategori bazlı analizler
    /// 
    /// CSV Export Raporları:
    /// 1. Günlük İstatistikler
    /// 2. Popüler Kaynaklar
    /// 3. Gecikme Raporu
    /// 4. Kullanıcı Aktivite Raporu
    /// 5. Kategori Analizi
    /// 
    /// Dosya Konumu:
    /// - backend/exports/ klasörüne kaydedilir
    /// - UTF-8 encoding kullanılır (Türkçe karakterler için)
    /// </summary>
    public class IstatistikServisi
    {
        /// <summary>
        /// Singleton kütüphane yöneticisi
        /// Veri erişimi için kullanılır
        /// </summary>
        private readonly KutuphaneYoneticisi _yonetici;

        /// <summary>
        /// CSV dosyalarının kaydedileceği dizin
        /// Varsayılan: "exports" (backend/exports/)
        /// </summary>
        private readonly string _exportDizini;

        /// <summary>
        /// Constructor
        /// 
        /// İşlemler:
        /// 1. Singleton yöneticiyi al
        /// 2. Export dizinini ayarla
        /// 3. Export dizini yoksa oluştur
        /// </summary>
        /// <param name="exportDizini">CSV dosyalarının kaydedileceği dizin (varsayılan: "exports")</param>
        public IstatistikServisi(string exportDizini = "exports")
        {
            _yonetici = KutuphaneYoneticisi.Instance;
            _exportDizini = exportDizini;

            // Export dizini yoksa oluştur
            // Böylece CSV dosyaları kaydedilebilir
            if (!Directory.Exists(_exportDizini))
            {
                Directory.CreateDirectory(_exportDizini);
            }
        }

        // ==================== CSV EXPORT METOTLARI ====================

        /// <summary>
        /// Belirtilen tarihe ait günlük istatistikleri CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Başlık bilgileri (rapor adı, tarih)
        /// - İşlem türü bazlı istatistikler (Örn: "KAYNAK_EKLENDI,5")
        /// - Saatlik işlem detayları
        /// 
        /// İşlem Türleri:
        /// - KAYNAK_EKLENDI: Eklenen kaynak sayısı
        /// - KAYNAK_SILINDI: Silinen kaynak sayısı
        /// - ODUNC_VERILDI: Ödünç verilen kaynak sayısı
        /// - IADE_ALINDI: İade alınan kaynak sayısı
        /// - KULLANICI_EKLENDI: Eklenen kullanıcı sayısı
        /// 
        /// Dosya Adı Formatı:
        /// gunluk_istatistik_YYYY-MM-DD.csv
        /// Örnek: gunluk_istatistik_2024-12-16.csv
        /// </summary>
        /// <param name="tarih">Raporlanacak tarih</param>
        /// <returns>Oluşturulan CSV dosyasının yolu</returns>
        public string GunlukIstatistikleriDisaAktar(DateTime tarih)
        {
            // Dosya adını oluştur
            var dosyaAdi = Path.Combine(_exportDizini, 
                $"gunluk_istatistik_{tarih:yyyy-MM-dd}.csv");

            // Günlük istatistikleri hesapla (işlem türü bazlı)
            var istatistikler = _yonetici.GunlukIstatistikler(tarih);
            
            // Belirtilen tarihe ait tüm işlemleri getir
            var islemler = _yonetici.IslemGecmisiGetir()
                .Where(i => i.Tarih.Date == tarih.Date)
                .ToList();

            // CSV içeriğini StringBuilder ile oluştur
            var sb = new StringBuilder();
            
            // ========== BAŞLIK BİLGİLERİ ==========
            sb.AppendLine("GÜNLÜK KÜTÜPHANE İSTATİSTİKLERİ");
            sb.AppendLine($"Tarih: {tarih:dd.MM.yyyy}");
            sb.AppendLine(); // Boş satır

            // ========== GENEL İSTATİSTİKLER ==========
            sb.AppendLine("İşlem Türü,Sayı");
            foreach (var stat in istatistikler)
            {
                // Her işlem türü ve sayısı
                sb.AppendLine($"{stat.Key},{stat.Value}");
            }
            sb.AppendLine(); // Boş satır

            // ========== DETAYLI İŞLEMLER ==========
            sb.AppendLine("Saat,İşlem Türü,Açıklama");
            
            // Tarih sırasına göre sırala (sabah'tan akşam'a)
            foreach (var islem in islemler.OrderBy(i => i.Tarih))
            {
                // Saat, işlem türü, açıklama
                sb.AppendLine($"{islem.Tarih:HH:mm:ss},{islem.IslemTuru},{islem.Aciklama}");
            }

            // CSV dosyasını UTF-8 encoding ile kaydet (Türkçe karakterler için)
            File.WriteAllText(dosyaAdi, sb.ToString(), Encoding.UTF8);
            
            return dosyaAdi;
        }

        /// <summary>
        /// En popüler kaynakları CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Başlık bilgileri
        /// - Sıra, ISBN, Başlık, Yazar, Kategori, Okunma Sayısı, Tür
        /// 
        /// Sıralama:
        /// - Okunma sayısına göre azalan sıra
        /// - En popüler kaynak en üstte
        /// 
        /// Dosya Adı Formatı:
        /// populer_kaynaklar_YYYY-MM-DD_HHmmss.csv
        /// Örnek: populer_kaynaklar_2024-12-16_143025.csv
        /// </summary>
        /// <param name="topN">Raporlanacak kaynak sayısı (varsayılan: 10)</param>
        /// <returns>Oluşturulan CSV dosyasının yolu</returns>
        public string PopulerKaynaklariDisaAktar(int topN = 10)
        {
            // Dosya adını oluştur (zaman damgası ile benzersiz)
            var dosyaAdi = Path.Combine(_exportDizini, 
                $"populer_kaynaklar_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv");

            // En popüler kaynakları getir
            var populerKaynaklar = _yonetici.EnPopuler10Kaynak();

            // CSV içeriğini oluştur
            var sb = new StringBuilder();
            
            // Başlık bilgileri
            sb.AppendLine("EN POPÜLER KAYNAKLAR");
            sb.AppendLine($"Oluşturma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine();
            
            // Kolon başlıkları
            sb.AppendLine("Sıra,ISBN,Başlık,Yazar,Kategori,Okunma Sayısı,Tür");

            // Her kaynak için satır ekle
            int sira = 1;
            foreach (var kaynak in populerKaynaklar)
            {
                // Kaynak türünü al (Kitap, Dergi, Tez)
                var tur = kaynak.GetType().Name;
                
                // CSV satırı: Sıra, ISBN, Başlık, Yazar, Kategori, Okunma Sayısı, Tür
                sb.AppendLine($"{sira},{kaynak.ISBN},{kaynak.Baslik},{kaynak.Yazar}," +
                             $"{kaynak.Kategori},{kaynak.OkunmaSayisi},{tur}");
                sira++;
            }

            // UTF-8 encoding ile kaydet
            File.WriteAllText(dosyaAdi, sb.ToString(), Encoding.UTF8);
            
            return dosyaAdi;
        }

        /// <summary>
        /// Gecikmeli ödünçleri CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Başlık bilgileri
        /// - Her gecikme için: Kullanıcı, Kaynak, Gecikme Günü, Ceza Tutarı
        /// - Özet: Toplam gecikme sayısı, Toplam ceza tutarı
        /// 
        /// Kullanım:
        /// - Yönetim raporları
        /// - Gecikme takibi
        /// - Mali hesaplamalar
        /// 
        /// Dosya Adı Formatı:
        /// gecikme_raporu_YYYY-MM-DD_HHmmss.csv
        /// </summary>
        /// <returns>Oluşturulan CSV dosyasının yolu</returns>
        public string GecikmeRaporuDisaAktar()
        {
            // Dosya adını oluştur
            var dosyaAdi = Path.Combine(_exportDizini, 
                $"gecikme_raporu_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv");

            // Gecikme uyarılarını getir
            var uyarilar = _yonetici.GecikmeUyarilariGetir();

            // CSV içeriğini oluştur
            var sb = new StringBuilder();
            
            // Başlık bilgileri
            sb.AppendLine("GECİKME RAPORU");
            sb.AppendLine($"Oluşturma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine();
            
            // Kolon başlıkları
            sb.AppendLine("Kullanıcı,Kaynak,Gecikme Gün Sayısı,Ceza (TL)");

            // Her gecikme için satır ekle
            foreach (var uyari in uyarilar)
            {
                // CSV satırı: Kullanıcı, Kaynak, Gecikme Günü, Ceza
                sb.AppendLine($"{uyari.KullaniciAd},{uyari.KaynakBaslik}," +
                             $"{uyari.GecikmeGunSayisi},{uyari.Ceza:F2}");
            }

            // Özet bilgiler
            sb.AppendLine();
            sb.AppendLine($"Toplam Gecikme Sayısı: {uyarilar.Count}");
            sb.AppendLine($"Toplam Ceza: {uyarilar.Sum(u => u.Ceza):F2} TL");

            // UTF-8 encoding ile kaydet
            File.WriteAllText(dosyaAdi, sb.ToString(), Encoding.UTF8);
            
            return dosyaAdi;
        }

        /// <summary>
        /// Kullanıcı aktivite raporunu CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Her kullanıcı için: Ad Soyad, Email, Yaş, Toplam Ödünç, Aktif Ödünç, Favori Kategoriler
        /// 
        /// Analiz İçeriği:
        /// - Toplam Ödünç: Kullanıcının geçmişte ödünç aldığı toplam kaynak sayısı
        /// - Aktif Ödünç: Şu anda ödünç aldığı kaynak sayısı
        /// - Favori Kategoriler: Kullanıcının favori kategorileri (noktalı virgülle ayrılmış)
        /// 
        /// Kullanım:
        /// - Kullanıcı profili analizi
        /// - Aktivite bazlı raporlar
        /// - Kullanıcı segmentasyonu
        /// 
        /// Dosya Adı Formatı:
        /// kullanici_aktivite_YYYY-MM-DD_HHmmss.csv
        /// </summary>
        /// <returns>Oluşturulan CSV dosyasının yolu</returns>
        public string KullaniciAktiviteRaporuDisaAktar()
        {
            // Dosya adını oluştur
            var dosyaAdi = Path.Combine(_exportDizini, 
                $"kullanici_aktivite_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv");

            // Tüm kullanıcıları getir
            var kullanicilar = _yonetici.TumKullanicilariGetir();

            // CSV içeriğini oluştur
            var sb = new StringBuilder();
            
            // Başlık bilgileri
            sb.AppendLine("KULLANICI AKTİVİTE RAPORU");
            sb.AppendLine($"Oluşturma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine();
            
            // Kolon başlıkları
            sb.AppendLine("Ad Soyad,Email,Yaş,Toplam Ödünç,Aktif Ödünç,Favori Kategoriler");

            // Her kullanıcı için satır ekle
            foreach (var kullanici in kullanicilar)
            {
                // Favori kategorileri noktalı virgülle birleştir
                var favoriKategoriler = string.Join("; ", kullanici.FavoriKategoriler);
                
                // CSV satırı: Ad Soyad, Email, Yaş, Toplam Ödünç, Aktif Ödünç, Favori Kategoriler
                sb.AppendLine($"{kullanici.Ad} {kullanici.Soyad},{kullanici.Email}," +
                             $"{kullanici.Yas},{kullanici.OduncGecmisi.Count}," +
                             $"{kullanici.AktifOduncler.Count},{favoriKategoriler}");
            }

            // UTF-8 encoding ile kaydet
            File.WriteAllText(dosyaAdi, sb.ToString(), Encoding.UTF8);
            
            return dosyaAdi;
        }

        /// <summary>
        /// Kategori bazlı analiz raporunu CSV dosyasına aktarır
        /// 
        /// CSV İçeriği:
        /// - Her kategori için: Kategori, Toplam Kaynak, Toplam Okunma, Ortalama Okunma, Şu An Ödünçte
        /// 
        /// Analiz Metrikleri:
        /// - Toplam Kaynak: Kategoriye ait toplam kaynak sayısı
        /// - Toplam Okunma: Kategoriye ait tüm kaynakların okunma sayısı toplamı
        /// - Ortalama Okunma: Kategori ortalaması
        /// - Şu An Ödünçte: Kategoriye ait ödünç verilmiş kaynak sayısı
        /// 
        /// Sıralama:
        /// - Toplam okunma sayısına göre azalan sıra
        /// - En popüler kategori en üstte
        /// 
        /// Kullanım:
        /// - Kategori performans analizi
        /// - Popülerlik trend analizi
        /// - Koleksiyon yönetimi kararları
        /// 
        /// Dosya Adı Formatı:
        /// kategori_analizi_YYYY-MM-DD_HHmmss.csv
        /// </summary>
        /// <returns>Oluşturulan CSV dosyasının yolu</returns>
        public string KategoriAnaliziDisaAktar()
        {
            // Dosya adını oluştur
            var dosyaAdi = Path.Combine(_exportDizini, 
                $"kategori_analizi_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv");

            // Tüm kaynakları getir
            var kaynaklar = _yonetici.TumKaynaklariGetir();
            
            // Kategoriye göre grupla ve analiz et
            var kategoriGrup = kaynaklar
                .GroupBy(k => k.Kategori) // Kategoriye göre grupla
                .Select(g => new
                {
                    Kategori = g.Key,
                    ToplamKaynak = g.Count(),                                    // Kategoriye ait kaynak sayısı
                    ToplamOkunma = g.Sum(k => k.OkunmaSayisi),                   // Toplam okunma sayısı
                    OrtalamaOkunma = g.Average(k => k.OkunmaSayisi),             // Ortalama okunma
                    OduncVerilen = g.Count(k => k.OduncDurumu)                   // Ödünç verilmiş kaynak sayısı
                })
                .OrderByDescending(x => x.ToplamOkunma) // Popülerliğe göre sırala
                .ToList();

            // CSV içeriğini oluştur
            var sb = new StringBuilder();
            
            // Başlık bilgileri
            sb.AppendLine("KATEGORİ ANALİZİ");
            sb.AppendLine($"Oluşturma Tarihi: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine();
            
            // Kolon başlıkları
            sb.AppendLine("Kategori,Toplam Kaynak,Toplam Okunma,Ortalama Okunma,Şu An Ödünçte");

            // Her kategori için satır ekle
            foreach (var kategori in kategoriGrup)
            {
                // CSV satırı: Kategori, Toplam Kaynak, Toplam Okunma, Ortalama Okunma, Şu An Ödünçte
                sb.AppendLine($"{kategori.Kategori},{kategori.ToplamKaynak}," +
                             $"{kategori.ToplamOkunma},{kategori.OrtalamaOkunma:F2}," +
                             $"{kategori.OduncVerilen}");
            }

            // UTF-8 encoding ile kaydet
            File.WriteAllText(dosyaAdi, sb.ToString(), Encoding.UTF8);
            
            return dosyaAdi;
        }

        // ==================== ÖZET İSTATİSTİKLER ====================

        /// <summary>
        /// Sistem genelinde özet istatistikleri getirir
        /// 
        /// İstatistikler:
        /// - toplamKaynak: Sistemdeki toplam kaynak sayısı
        /// - toplamKullanici: Sistemdeki toplam kullanıcı sayısı
        /// - oduncVerilenKaynak: Şu anda ödünç verilmiş kaynak sayısı
        /// - mevcutKaynak: Kütüphanede mevcut kaynak sayısı
        /// - toplamOkunma: Tüm kaynakların toplam okunma sayısı
        /// - gecikmeliOdunc: Gecikmeli ödünç sayısı
        /// - toplamCeza: Toplam gecikme cezası (TL)
        /// - kategoriSayisi: Farklı kategori sayısı
        /// 
        /// Kullanım:
        /// - Dashboard özet bilgileri
        /// - Genel sistem durumu
        /// - Hızlı istatistik görüntüleme
        /// </summary>
        /// <returns>İstatistikler dictionary'si (key-value çiftleri)</returns>
        public Dictionary<string, object> OzetIstatistiklerGetir()
        {
            // Verileri getir
            var kaynaklar = _yonetici.TumKaynaklariGetir();
            var kullanicilar = _yonetici.TumKullanicilariGetir();
            var gecikmeUyarilari = _yonetici.GecikmeUyarilariGetir();

            // Özet istatistikleri hesapla ve dictionary olarak döndür
            return new Dictionary<string, object>
            {
                { "toplamKaynak", kaynaklar.Count },
                { "toplamKullanici", kullanicilar.Count },
                { "oduncVerilenKaynak", kaynaklar.Count(k => k.OduncDurumu) },
                { "mevcutKaynak", kaynaklar.Count(k => !k.OduncDurumu) },
                { "toplamOkunma", kaynaklar.Sum(k => k.OkunmaSayisi) },
                { "gecikmeliOdunc", gecikmeUyarilari.Count },
                { "toplamCeza", gecikmeUyarilari.Sum(u => u.Ceza) },
                { "kategoriSayisi", kaynaklar.Select(k => k.Kategori).Distinct().Count() }
            };
        }
    }
}
