using System;

namespace SmartLibrary.Models
{
    /// <summary>
    /// Kaynak Abstract (Soyut) Sınıfı
    /// 
    /// Bu sınıf, kütüphane kaynaklarının (Kitap, Dergi, Tez) temel sınıfıdır.
    /// Polimorfizm (Çok Biçimlilik) prensibini kullanarak, farklı kaynak türlerinin
    /// ortak özelliklerini ve davranışlarını tanımlar.
    /// 
    /// Tasarım Deseni: Template Method Pattern
    /// - Abstract metotlar alt sınıflarda zorunlu olarak implement edilir
    /// - Virtual metotlar isteğe bağlı olarak override edilebilir
    /// 
    /// Veritabanı: Table Per Hierarchy (TPH) pattern ile tek tabloda saklanır
    /// </summary>
    public abstract class Kaynak
    {
        // ==================== ORTAK ÖZELLİKLER ====================

        /// <summary>
        /// Kaynağın benzersiz kimlik numarası (ISBN, ISSN vb.)
        /// Primary Key olarak kullanılır
        /// </summary>
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// Kaynağın başlığı
        /// </summary>
        public string Baslik { get; set; } = string.Empty;

        /// <summary>
        /// Yazar/Yayınevi adı
        /// Kitap için yazar, Dergi için yayınevi, Tez için yazar bilgisi
        /// </summary>
        public string Yazar { get; set; } = string.Empty;

        /// <summary>
        /// Yayın tarihi
        /// </summary>
        public DateTime YayinTarihi { get; set; }

        /// <summary>
        /// Kaynağın ödünç durumu
        /// true: Ödünç verilmiş
        /// false: Kütüphanede mevcut
        /// </summary>
        public bool OduncDurumu { get; set; }

        /// <summary>
        /// Ödünç verilme tarihi
        /// Nullable: Kaynak ödünçte değilse null
        /// </summary>
        public DateTime? OduncTarihi { get; set; }

        /// <summary>
        /// Kaynağın toplam okunma sayısı
        /// Her ödünç verme işleminde artırılır
        /// Popülerlik analizi için kullanılır
        /// </summary>
        public int OkunmaSayisi { get; set; }

        /// <summary>
        /// Kaynağın kategorisi
        /// Örnek: "Klasik Edebiyat", "Bilim", "Teknoloji", "Roman"
        /// Öneri sistemi ve filtreleme için kullanılır
        /// </summary>
        public string? Kategori { get; set; }

        // ==================== CONSTRUCTOR'LAR ====================

        /// <summary>
        /// Parametresiz constructor
        /// Entity Framework Core için gerekli (reflection kullanır)
        /// Protected: Sadece türetilmiş sınıflar kullanabilir (abstract class)
        /// </summary>
        protected Kaynak()
        {
        }

        /// <summary>
        /// Parametreli constructor
        /// Yeni kaynak oluştururken kullanılır
        /// </summary>
        /// <param name="isbn">Kaynağın ISBN/ISSN numarası</param>
        /// <param name="baslik">Kaynağın başlığı</param>
        /// <param name="yazar">Yazar/Yayınevi adı</param>
        /// <param name="yayinTarihi">Yayın tarihi</param>
        /// <param name="kategori">Kategori bilgisi (isteğe bağlı)</param>
        protected Kaynak(string isbn, string baslik, string yazar, DateTime yayinTarihi, string? kategori)
        {
            ISBN = isbn;
            Baslik = baslik;
            Yazar = yazar;
            YayinTarihi = yayinTarihi;
            // Varsayılan değerler
            OduncDurumu = false;  // Başlangıçta kütüphanede
            OkunmaSayisi = 0;      // Henüz okunmamış
            Kategori = kategori;
        }

        // ==================== ABSTRACT METOTLAR (ZORUNLU OVERRIDE) ====================

        /// <summary>
        /// Kaynağın özet bilgilerini gösterir
        /// Her kaynak türü kendi formatında özet gösterir
        /// 
        /// Polimorfizm: Aynı metod çağrısı farklı sonuçlar üretir
        /// </summary>
        /// <returns>Kaynağın formatlı özet bilgisi</returns>
        public abstract string OzetGoster();

        /// <summary>
        /// Gecikme cezasını hesaplar
        /// Her kaynak türü farklı ceza ücreti uygular
        /// 
        /// Polimorfizm Örneği:
        /// - Kitap: 2 TL/gün
        /// - Dergi: 1 TL/gün
        /// - Tez: 3 TL/gün
        /// </summary>
        /// <param name="gecikmeGunSayisi">Gecikme gün sayısı</param>
        /// <returns>Hesaplanan ceza tutarı (decimal)</returns>
        public abstract decimal CezaHesapla(int gecikmeGunSayisi);

        /// <summary>
        /// Kaynağın teslim süresini belirler (gün cinsinden)
        /// Her kaynak türü farklı süreye sahiptir
        /// 
        /// Polimorfizm Örneği:
        /// - Kitap: 14 gün
        /// - Dergi: 7 gün
        /// - Tez: 21 gün
        /// </summary>
        /// <returns>Teslim süresi (gün sayısı)</returns>
        public abstract int TeslimSuresi();

        // ==================== VIRTUAL METOTLAR (İSTEĞE BAĞLI OVERRIDE) ====================

        /// <summary>
        /// Kaynağı ödünç verir
        /// 
        /// İşlemler:
        /// - OduncDurumu = true yapılır
        /// - OduncTarihi = şu anki zaman atanır
        /// - OkunmaSayisi artırılır (popülerlik takibi için)
        /// 
        /// Virtual: Alt sınıflar bu metodu override edebilir
        /// </summary>
        public virtual void OduncVer()
        {
            OduncDurumu = true;
            OduncTarihi = DateTime.Now;
            OkunmaSayisi++;  // Her ödünç vermede popülerlik artar
        }

        /// <summary>
        /// Kaynağı iade alır
        /// 
        /// İşlemler:
        /// - OduncDurumu = false yapılır
        /// - OduncTarihi = null yapılır (temizlik)
        /// 
        /// Virtual: Alt sınıflar bu metodu override edebilir
        /// </summary>
        public virtual void IadeAl()
        {
            OduncDurumu = false;
            OduncTarihi = null;
        }

        // ==================== ORTAK METOTLAR ====================

        /// <summary>
        /// Gecikme gün sayısını hesaplar
        /// 
        /// Hesaplama Mantığı:
        /// 1. Ödünç tarihi yoksa gecikme yok (0 döner)
        /// 2. Teslim tarihi = Ödünç Tarihi + Teslim Süresi
        /// 3. Gecikme = Şu Anki Tarih - Teslim Tarihi
        /// 4. Negatif değer 0 olarak döndürülür
        /// 
        /// Polimorfizm: TeslimSuresi() metodu her kaynak türü için farklı değer döner
        /// </summary>
        /// <returns>Gecikme gün sayısı (0 veya pozitif)</returns>
        public int GecikmeGunSayisi()
        {
            // Ödünç tarihi yoksa gecikme yok
            if (!OduncTarihi.HasValue) return 0;
            
            // Teslim tarihini hesapla (Ödünç tarihi + Teslim süresi)
            // TeslimSuresi() abstract metot - her kaynak türü kendi süresini döndürür
            var teslimTarihi = OduncTarihi.Value.AddDays(TeslimSuresi());
            
            // Gecikme gün sayısını hesapla
            var gecikme = (DateTime.Now - teslimTarihi).Days;
            
            // Negatif değerleri 0 olarak döndür (henüz teslim tarihi gelmediyse)
            return gecikme > 0 ? gecikme : 0;
        }
    }
}
