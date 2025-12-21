using System;

namespace SmartLibrary.Models
{
    /// <summary>
    /// Kitap Sınıfı
    /// 
    /// Kaynak abstract sınıfından türeyen, kitap türü kaynakları temsil eden sınıf.
    /// 
    /// Özel Özellikler:
    /// - SayfaSayisi: Kitabın sayfa sayısı
    /// - YayinEvi: Kitabın yayınlandığı yayın evi
    /// - Dil: Kitabın yazıldığı dil
    /// 
    /// İş Kuralları:
    /// - Teslim Süresi: 14 gün
    /// - Ceza Ücreti: 2 TL/gün gecikme
    /// 
    /// Veritabanı: Table Per Hierarchy (TPH) ile Kaynaklar tablosunda saklanır
    /// Discriminator değeri: "Kitap"
    /// </summary>
    public class Kitap : Kaynak
    {
        // ==================== KİTAP ÖZEL ÖZELLİKLER ====================

        /// <summary>
        /// Kitabın toplam sayfa sayısı
        /// </summary>
        public int SayfaSayisi { get; set; }

        /// <summary>
        /// Kitabı yayınlayan yayın evi
        /// Örnek: "İletişim Yayınları", "Can Yayınları", "Yapı Kredi Yayınları"
        /// </summary>
        public string? YayinEvi { get; set; }

        /// <summary>
        /// Kitabın yazıldığı dil
        /// Örnek: "Türkçe", "İngilizce", "Fransızca"
        /// Varsayılan: "Türkçe"
        /// </summary>
        public string? Dil { get; set; }

        // ==================== CONSTRUCTOR'LAR ====================

        /// <summary>
        /// Parametresiz constructor
        /// Entity Framework Core için gerekli
        /// Base class'ın parametresiz constructor'ını çağırır
        /// </summary>
        public Kitap() : base()
        {
        }

        /// <summary>
        /// Parametreli constructor
        /// Yeni kitap oluştururken kullanılır
        /// </summary>
        /// <param name="isbn">Kitabın ISBN numarası</param>
        /// <param name="baslik">Kitabın başlığı</param>
        /// <param name="yazar">Yazar adı</param>
        /// <param name="yayinTarihi">Yayın tarihi</param>
        /// <param name="kategori">Kategori bilgisi</param>
        /// <param name="sayfaSayisi">Sayfa sayısı</param>
        /// <param name="yayinEvi">Yayın evi</param>
        /// <param name="dil">Dil</param>
        public Kitap(string isbn, string baslik, string yazar, DateTime yayinTarihi, 
                     string kategori, int sayfaSayisi, string? yayinEvi, string? dil)
            : base(isbn, baslik, yazar, yayinTarihi, kategori)
        {
            SayfaSayisi = sayfaSayisi;
            YayinEvi = yayinEvi;
            Dil = dil;
        }

        // ==================== ABSTRACT METOT IMPLEMENTASYONLARI ====================

        /// <summary>
        /// Kitap için özelleştirilmiş özet gösterimi
        /// 
        /// Format:
        /// - Kitap emojisi (📚)
        /// - Tüm kitap bilgileri
        /// - Ödünç durumu
        /// </summary>
        /// <returns>Formatlı kitap özeti</returns>
        public override string OzetGoster()
        {
            return $"📚 KİTAP\n" +
                   $"Başlık: {Baslik}\n" +
                   $"Yazar: {Yazar}\n" +
                   $"ISBN: {ISBN}\n" +
                   $"Sayfa Sayısı: {SayfaSayisi}\n" +
                   $"Yayın Evi: {YayinEvi}\n" +
                   $"Dil: {Dil}\n" +
                   $"Kategori: {Kategori}\n" +
                   $"Okunma Sayısı: {OkunmaSayisi}\n" +
                   $"Durum: {(OduncDurumu ? "Ödünç Verildi" : "Kütüphanede")}";
        }

        /// <summary>
        /// Kitap için ceza hesaplama
        /// 
        /// İş Kuralı:
        /// - Kitaplar için günlük ceza: 2 TL
        /// - Hesaplama: Gecikme Gün Sayısı × 2 TL
        /// 
        /// Örnek:
        /// - 5 gün gecikme = 5 × 2 = 10 TL
        /// </summary>
        /// <param name="gecikmeGunSayisi">Gecikme gün sayısı</param>
        /// <returns>Hesaplanan ceza tutarı (TL)</returns>
        public override decimal CezaHesapla(int gecikmeGunSayisi)
        {
            // Kitaplar için günlük 2 TL ceza
            return gecikmeGunSayisi * 2.0m;
        }

        /// <summary>
        /// Kitap için teslim süresi
        /// 
        /// İş Kuralı:
        /// - Kitaplar 14 gün süreyle ödünç verilir
        /// - Bu süre sonunda iade edilmesi gerekir
        /// </summary>
        /// <returns>Teslim süresi (14 gün)</returns>
        public override int TeslimSuresi()
        {
            // Kitaplar için 14 gün
            return 14;
        }
    }
}
