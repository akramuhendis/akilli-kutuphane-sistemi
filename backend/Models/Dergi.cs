using System;

namespace SmartLibrary.Models
{
    /// <summary>
    /// Dergi Sınıfı
    /// 
    /// Kaynak abstract sınıfından türeyen, dergi/magazin türü kaynakları temsil eden sınıf.
    /// 
    /// Özel Özellikler:
    /// - SayiNo: Derginin sayı numarası
    /// - YayinPeriyodu: Yayın periyodu (Aylık, Haftalık, vb.)
    /// - ISSN: Derginin ISSN numarası
    /// 
    /// İş Kuralları:
    /// - Teslim Süresi: 7 gün (kitaptan daha kısa - daha hızlı dönüş)
    /// - Ceza Ücreti: 1 TL/gün gecikme (kitaptan daha düşük)
    /// 
    /// Veritabanı: Table Per Hierarchy (TPH) ile Kaynaklar tablosunda saklanır
    /// Discriminator değeri: "Dergi"
    /// </summary>
    public class Dergi : Kaynak
    {
        // ==================== DERGİ ÖZEL ÖZELLİKLER ====================

        /// <summary>
        /// Derginin sayı numarası
        /// Örnek: 1, 2, 3... (aylık dergiler için)
        /// </summary>
        public int SayiNo { get; set; }

        /// <summary>
        /// Derginin yayın periyodu
        /// Örnek değerler:
        /// - "Aylık" - Her ay yayınlanır
        /// - "Haftalık" - Her hafta yayınlanır
        /// - "Üç Aylık" - Üç ayda bir yayınlanır
        /// </summary>
        public string? YayinPeriyodu { get; set; }

        /// <summary>
        /// Derginin ISSN (International Standard Serial Number) numarası
        /// Dergiler için benzersiz tanımlayıcı numara
        /// Örnek: "ISSN-2536-4618", "ISSN-1303-6092"
        /// </summary>
        public string? ISSN { get; set; }

        // ==================== CONSTRUCTOR'LAR ====================

        /// <summary>
        /// Parametresiz constructor
        /// Entity Framework Core için gerekli
        /// Base class'ın parametresiz constructor'ını çağırır
        /// </summary>
        public Dergi() : base()
        {
        }

        /// <summary>
        /// Parametreli constructor
        /// Yeni dergi oluştururken kullanılır
        /// </summary>
        /// <param name="isbn">Derginin ISBN/ISSN numarası</param>
        /// <param name="baslik">Derginin başlığı</param>
        /// <param name="yazar">Yayınevi adı (Yazar property'si burada yayınevi olarak kullanılır)</param>
        /// <param name="yayinTarihi">Yayın tarihi</param>
        /// <param name="kategori">Kategori bilgisi</param>
        /// <param name="sayiNo">Sayı numarası</param>
        /// <param name="yayinPeriyodu">Yayın periyodu</param>
        /// <param name="issn">ISSN numarası</param>
        public Dergi(string isbn, string baslik, string yazar, DateTime yayinTarihi, 
                     string kategori, int sayiNo, string? yayinPeriyodu, string? issn)
            : base(isbn, baslik, yazar, yayinTarihi, kategori)
        {
            SayiNo = sayiNo;
            YayinPeriyodu = yayinPeriyodu;
            ISSN = issn;
        }

        // ==================== ABSTRACT METOT IMPLEMENTASYONLARI ====================

        /// <summary>
        /// Dergi için özelleştirilmiş özet gösterimi
        /// 
        /// Format:
        /// - Dergi emojisi (📰)
        /// - Tüm dergi bilgileri
        /// - Özel alanlar: Sayı No, Periyot, ISSN
        /// - Ödünç durumu
        /// 
        /// Not: Base class'taki "Yazar" property'si burada "Yayınevi" olarak gösterilir
        /// </summary>
        /// <returns>Formatlı dergi özeti</returns>
        public override string OzetGoster()
        {
            return $"📰 DERGİ\n" +
                   $"Başlık: {Baslik}\n" +
                   $"Yayınevi: {Yazar}\n" +  // Base class'taki Yazar property'si dergi için yayınevi
                   $"ISBN: {ISBN}\n" +
                   $"ISSN: {ISSN}\n" +
                   $"Sayı No: {SayiNo}\n" +
                   $"Periyot: {YayinPeriyodu}\n" +
                   $"Kategori: {Kategori}\n" +
                   $"Okunma Sayısı: {OkunmaSayisi}\n" +
                   $"Durum: {(OduncDurumu ? "Ödünç Verildi" : "Kütüphanede")}";
        }

        /// <summary>
        /// Dergi için ceza hesaplama
        /// 
        /// İş Kuralı:
        /// - Dergiler için günlük ceza: 1 TL (kitaptan daha düşük)
        /// - Hesaplama: Gecikme Gün Sayısı × 1 TL
        /// - Düşük ceza nedeni: Dergiler daha sık yayınlanır ve daha az değerlidir
        /// 
        /// Örnek:
        /// - 5 gün gecikme = 5 × 1 = 5 TL
        /// </summary>
        /// <param name="gecikmeGunSayisi">Gecikme gün sayısı</param>
        /// <returns>Hesaplanan ceza tutarı (TL)</returns>
        public override decimal CezaHesapla(int gecikmeGunSayisi)
        {
            // Dergiler için günlük 1 TL ceza (kitaptan daha düşük)
            return gecikmeGunSayisi * 1.0m;
        }

        /// <summary>
        /// Dergi için teslim süresi
        /// 
        /// İş Kuralı:
        /// - Dergiler 7 gün süreyle ödünç verilir (kitaptan daha kısa)
        /// - Daha kısa süre nedeni: Daha hızlı dönüş sağlanması
        /// - Dergiler genellikle daha kısa ve hızlı okunur
        /// </summary>
        /// <returns>Teslim süresi (7 gün)</returns>
        public override int TeslimSuresi()
        {
            // Dergiler için 7 gün (kitaptan daha kısa süre)
            return 7;
        }
    }
}
