using System;

namespace SmartLibrary.Patterns.Singleton
{
    /// <summary>
    /// İşlem Kaydı Sınıfı
    /// 
    /// Sistemdeki tüm işlemlerin log kayıtlarını temsil eden sınıf.
    /// Bu sınıf, audit trail (denetim izi) ve istatistik hesaplamaları için kullanılır.
    /// 
    /// İşlem Türleri:
    /// - KAYNAK_EKLENDI: Yeni kaynak eklendiğinde
    /// - KAYNAK_SILINDI: Kaynak silindiğinde
    /// - KULLANICI_EKLENDI: Yeni kullanıcı eklendiğinde
    /// - KULLANICI_GUNCELLENDI: Kullanıcı güncellendiğinde
    /// - KULLANICI_SILINDI: Kullanıcı silindiğinde
    /// - ODUNC_VERILDI: Kaynak ödünç verildiğinde
    /// - IADE_ALINDI: Kaynak iade alındığında
    /// 
    /// Veritabanı: IslemKayitlari tablosunda saklanır
    /// </summary>
    public class IslemKaydi
    {
        /// <summary>
        /// İşlem kaydının benzersiz kimlik numarası
        /// GUID formatında string olarak saklanır
        /// Primary Key olarak kullanılır
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// İşlem türü
        /// Örnek değerler:
        /// - "KAYNAK_EKLENDI"
        /// - "ODUNC_VERILDI"
        /// - "IADE_ALINDI"
        /// 
        /// İstatistik hesaplamalarında gruplama için kullanılır
        /// </summary>
        public string? IslemTuru { get; set; }

        /// <summary>
        /// İşlemle ilgili detaylı açıklama
        /// 
        /// Örnek açıklamalar:
        /// - "Suç ve Ceza eklendi"
        /// - "Mehmet Yılmaz - 1984" (ödünç verme)
        /// - "Zeynep Kaya - Bilim ve Teknik" (iade)
        /// 
        /// Audit trail ve hata ayıklama için kullanılır
        /// </summary>
        public string? Aciklama { get; set; }

        /// <summary>
        /// İşlemin gerçekleştiği tarih ve saat
        /// 
        /// Kullanım:
        /// - Günlük istatistik hesaplamaları
        /// - Tarih bazlı filtreleme
        /// - İşlem sıralaması
        /// </summary>
        public DateTime Tarih { get; set; }
    }

    /// <summary>
    /// Gecikme Uyarısı Sınıfı
    /// 
    /// Gecikmeli ödünçler için bilgi taşıyan DTO (Data Transfer Object) sınıfı.
    /// Bu sınıf, gecikme raporları ve kullanıcı uyarıları için kullanılır.
    /// 
    /// Kullanım:
    /// - Gecikme uyarıları listesi
    /// - CSV export (gecikme raporu)
    /// - Dashboard gecikme gösterimi
    /// </summary>
    public class GecikmeUyarisi
    {
        /// <summary>
        /// Gecikmeli ödüncü olan kullanıcının tam adı
        /// Format: "Ad Soyad"
        /// </summary>
        public string KullaniciAd { get; set; } = string.Empty;

        /// <summary>
        /// Gecikmeli kaynağın başlığı
        /// Denormalizasyon ile OduncKaydi'den alınır
        /// </summary>
        public string KaynakBaslik { get; set; } = string.Empty;

        /// <summary>
        /// Gecikme gün sayısı
        /// 
        /// Hesaplama:
        /// - Teslim tarihi = Ödünç Tarihi + Teslim Süresi
        /// - Gecikme = Şu Anki Tarih - Teslim Tarihi
        /// 
        /// Örnek: 5 gün gecikme
        /// </summary>
        public int GecikmeGunSayisi { get; set; }

        /// <summary>
        /// Hesaplanan ceza tutarı (TL)
        /// 
        /// Hesaplama:
        /// - Polimorfik olarak kaynak türüne göre hesaplanır
        /// - Kitap: 2 TL/gün
        /// - Dergi: 1 TL/gün
        /// - Tez: 3 TL/gün
        /// 
        /// Formül: Gecikme Gün Sayısı × Günlük Ceza
        /// 
        /// Örnek: 5 gün gecikme × 2 TL = 10 TL (kitap için)
        /// </summary>
        public decimal Ceza { get; set; }
    }
}
