using System;

namespace SmartLibrary.Models
{
    /// <summary>
    /// Tez Sınıfı
    /// 
    /// Kaynak abstract sınıfından türeyen, akademik tez türü kaynakları temsil eden sınıf.
    /// 
    /// Özel Özellikler:
    /// - Universite: Tezin yapıldığı üniversite
    /// - Bolum: Tezin yapıldığı bölüm
    /// - DanismanAdi: Tez danışmanının adı
    /// - TezTuru: Tez türü (Yüksek Lisans, Doktora)
    /// 
    /// İş Kuralları:
    /// - Teslim Süresi: 21 gün (en uzun - akademik çalışma gerektirir)
    /// - Ceza Ücreti: 3 TL/gün gecikme (en yüksek - nadir kaynak)
    /// 
    /// Veritabanı: Table Per Hierarchy (TPH) ile Kaynaklar tablosunda saklanır
    /// Discriminator değeri: "Tez"
    /// </summary>
    public class Tez : Kaynak
    {
        // ==================== TEZ ÖZEL ÖZELLİKLER ====================

        /// <summary>
        /// Tezin yapıldığı üniversite adı
        /// Örnek: "İstanbul Teknik Üniversitesi", "Orta Doğu Teknik Üniversitesi"
        /// </summary>
        public string? Universite { get; set; }

        /// <summary>
        /// Tezin yapıldığı bölüm/departman
        /// Örnek: "Bilgisayar Mühendisliği", "Elektrik-Elektronik Mühendisliği"
        /// </summary>
        public string? Bolum { get; set; }

        /// <summary>
        /// Tez danışmanının adı
        /// Akademik unvanı ile birlikte olabilir
        /// Örnek: "Prof. Dr. Mehmet Kaya", "Doç. Dr. Ali Vural"
        /// </summary>
        public string? DanismanAdi { get; set; }

        /// <summary>
        /// Tez türü
        /// Olası değerler:
        /// - "Yüksek Lisans" - Master's thesis
        /// - "Doktora" - PhD thesis
        /// Varsayılan: "Yüksek Lisans"
        /// </summary>
        public string? TezTuru { get; set; }

        // ==================== CONSTRUCTOR'LAR ====================

        /// <summary>
        /// Parametresiz constructor
        /// Entity Framework Core için gerekli
        /// Base class'ın parametresiz constructor'ını çağırır
        /// </summary>
        public Tez() : base()
        {
        }

        /// <summary>
        /// Parametreli constructor
        /// Yeni tez oluştururken kullanılır
        /// </summary>
        /// <param name="isbn">Tezin ISBN/benzersiz numarası</param>
        /// <param name="baslik">Tez başlığı</param>
        /// <param name="yazar">Tez yazarı (öğrenci adı)</param>
        /// <param name="yayinTarihi">Tez teslim/tamamlanma tarihi</param>
        /// <param name="kategori">Kategori bilgisi (genellikle bölüm ile aynı)</param>
        /// <param name="universite">Üniversite adı</param>
        /// <param name="bolum">Bölüm adı</param>
        /// <param name="danismanAdi">Danışman adı</param>
        /// <param name="tezTuru">Tez türü</param>
        public Tez(string isbn, string baslik, string yazar, DateTime yayinTarihi, 
                   string kategori, string? universite, string? bolum, string? danismanAdi, string? tezTuru)
            : base(isbn, baslik, yazar, yayinTarihi, kategori)
        {
            Universite = universite;
            Bolum = bolum;
            DanismanAdi = danismanAdi;
            TezTuru = tezTuru;
        }

        // ==================== ABSTRACT METOT IMPLEMENTASYONLARI ====================

        /// <summary>
        /// Tez için özelleştirilmiş özet gösterimi
        /// 
        /// Format:
        /// - Tez emojisi (🎓)
        /// - Tüm tez bilgileri
        /// - Akademik bilgiler: Üniversite, Bölüm, Danışman
        /// - Tez türü bilgisi
        /// - Ödünç durumu
        /// </summary>
        /// <returns>Formatlı tez özeti</returns>
        public override string OzetGoster()
        {
            return $"🎓 TEZ\n" +
                   $"Başlık: {Baslik}\n" +
                   $"Yazar: {Yazar}\n" +
                   $"ISBN: {ISBN}\n" +
                   $"Tez Türü: {TezTuru}\n" +
                   $"Üniversite: {Universite}\n" +
                   $"Bölüm: {Bolum}\n" +
                   $"Danışman: {DanismanAdi}\n" +
                   $"Kategori: {Kategori}\n" +
                   $"Okunma Sayısı: {OkunmaSayisi}\n" +
                   $"Durum: {(OduncDurumu ? "Ödünç Verildi" : "Kütüphanede")}";
        }

        /// <summary>
        /// Tez için ceza hesaplama
        /// 
        /// İş Kuralı:
        /// - Tezler için günlük ceza: 3 TL (en yüksek)
        /// - Hesaplama: Gecikme Gün Sayısı × 3 TL
        /// - Yüksek ceza nedeni: 
        ///   * Nadir kaynaklar (tezler genellikle tek kopya)
        ///   * Akademik çalışma için kritik öneme sahip
        ///   * Geri dönüşü olmayan kayıp riski yüksek
        /// 
        /// Örnek:
        /// - 5 gün gecikme = 5 × 3 = 15 TL
        /// </summary>
        /// <param name="gecikmeGunSayisi">Gecikme gün sayısı</param>
        /// <returns>Hesaplanan ceza tutarı (TL)</returns>
        public override decimal CezaHesapla(int gecikmeGunSayisi)
        {
            // Tezler için günlük 3 TL ceza (en yüksek - nadir kaynak)
            return gecikmeGunSayisi * 3.0m;
        }

        /// <summary>
        /// Tez için teslim süresi
        /// 
        /// İş Kuralı:
        /// - Tezler 21 gün süreyle ödünç verilir (en uzun süre)
        /// - Uzun süre nedeni:
        ///   * Akademik çalışma için daha fazla zaman gerektirir
        ///   * Tezler genellikle uzun ve detaylıdır
        ///   * Araştırma amaçlı kullanım için yeterli süre sağlanır
        /// </summary>
        /// <returns>Teslim süresi (21 gün)</returns>
        public override int TeslimSuresi()
        {
            // Tezler için 21 gün (en uzun süre - akademik çalışma gerektirir)
            return 21;
        }
    }
}
