using System;
using System.Collections.Generic;

namespace SmartLibrary.Models
{
    /// <summary>
    /// Kullanıcı Sınıfı
    /// 
    /// Kütüphane sistemine kayıtlı kullanıcıları temsil eden sınıf.
    /// Kullanıcılar, kaynak ödünç alabilir, okuma geçmişi tutulur ve
    /// kişiselleştirilmiş öneriler alır.
    /// 
    /// Özellikler:
    /// - Profil bilgileri (Ad, Soyad, Email, Yaş)
    /// - İlgi alanları ve favori kategoriler (öneri sistemi için)
    /// - Ödünç geçmişi (iade edilmiş kaynaklar)
    /// - Aktif ödünçler (henüz iade edilmemiş kaynaklar)
    /// - Otomatik ID oluşturma (GUID)
    /// 
    /// Veritabanı: Kullanicilar tablosunda saklanır
    /// List<string> özellikleri CSV formatında saklanır
    /// </summary>
    public class Kullanici
    {
        // ==================== KULLANICI ÖZELLİKLER ====================

        /// <summary>
        /// Kullanıcının benzersiz kimlik numarası
        /// GUID formatında string olarak saklanır
        /// Primary Key olarak kullanılır
        /// Constructor'da otomatik oluşturulur
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının adı
        /// </summary>
        public string Ad { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının soyadı
        /// </summary>
        public string Soyad { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının e-posta adresi
        /// Benzersiz olmalı (uniqueness kontrolü gerekirse uygulamada yapılmalı)
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının yaşı
        /// Öneri sistemi için kullanılır (YaşFiltresi)
        /// Farklı yaş gruplarına göre farklı öneriler sunulur
        /// </summary>
        public int Yas { get; set; }

        /// <summary>
        /// Kullanıcının ilgi alanları listesi
        /// Öneri sistemi için kritik öneme sahip
        /// 
        /// Örnek değerler:
        /// - ["Bilim", "Teknoloji", "Edebiyat"]
        /// - ["Felsefe", "Roman", "Klasik Edebiyat"]
        /// 
        /// Veritabanı: CSV formatında saklanır (virgülle ayrılmış)
        /// </summary>
        public List<string> IlgiAlanlari { get; set; } = new List<string>();

        /// <summary>
        /// Kullanıcının iade ettiği kaynakların geçmişi
        /// İade edilmiş tüm ödünç kayıtları bu listede tutulur
        /// 
        /// Kullanım Alanları:
        /// - Okuma alışkanlıklarının analizi
        /// - Öneri sistemi için kategori çıkarımı
        /// - İstatistiksel raporlar
        /// 
        /// Veritabanı: OduncKayitlari tablosunda "KullaniciId" foreign key ile saklanır
        /// </summary>
        public List<OduncKaydi> OduncGecmisi { get; set; } = new List<OduncKaydi>();

        /// <summary>
        /// Kullanıcının şu anda ödünç aldığı ve henüz iade etmediği kaynaklar
        /// 
        /// Kullanım Alanları:
        /// - Aktif ödünçlerin takibi
        /// - Gecikme kontrolü
        /// - Kullanıcı silme kontrolü (aktif ödünç varsa silinemez)
        /// 
        /// Veritabanı: OduncKayitlari tablosunda "KullaniciIdAktif" foreign key ile saklanır
        /// </summary>
        public List<OduncKaydi> AktifOduncler { get; set; } = new List<OduncKaydi>();

        /// <summary>
        /// Kullanıcının favori kategorileri listesi
        /// Kullanıcı tarafından manuel olarak seçilebilir veya
        /// okuma geçmişine göre otomatik belirlenebilir
        /// 
        /// Örnek değerler:
        /// - ["Klasik Edebiyat", "Roman"]
        /// - ["Bilim", "Doğa"]
        /// 
        /// Öneri sistemi için kullanılır (KategoriFiltresi)
        /// Veritabanı: CSV formatında saklanır (virgülle ayrılmış)
        /// </summary>
        public List<string> FavoriKategoriler { get; set; } = new List<string>();

        /// <summary>
        /// Kullanıcının sisteme kayıt tarihi
        /// Constructor'da otomatik olarak şu anki zaman atanır
        /// </summary>
        public DateTime KayitTarihi { get; set; }

        // ==================== CONSTRUCTOR ====================

        /// <summary>
        /// Constructor
        /// Yeni kullanıcı oluştururken otomatik olarak:
        /// - Benzersiz ID (GUID) oluşturur
        /// - Kayıt tarihini şu anki zaman olarak ayarlar
        /// - List koleksiyonlarını boş liste olarak initialize eder
        /// </summary>
        public Kullanici()
        {
            // GUID string formatında benzersiz ID oluştur
            Id = Guid.NewGuid().ToString();
            
            // List'ler property initializer'da initialize edildi
            // Burada tekrar initialize etmeye gerek yok
            
            // Kayıt tarihini şu anki zaman olarak ayarla
            KayitTarihi = DateTime.Now;
        }

        // ==================== ÖDÜNÇ İŞLEM METOTLARI ====================

        /// <summary>
        /// Kullanıcıya yeni bir ödünç kaydı ekler
        /// 
        /// İşlem:
        /// - Yeni ödünç kaydı kullanıcının AktifOduncler listesine eklenir
        /// - Bu metot, ödünç verme işlemi sırasında çağrılır
        /// </summary>
        /// <param name="kayit">Eklenecek ödünç kaydı</param>
        public void OduncEkle(OduncKaydi kayit)
        {
            AktifOduncler.Add(kayit);
        }

        /// <summary>
        /// Kullanıcının bir kaynağı iade etmesini işler
        /// 
        /// İşlem Adımları:
        /// 1. Aktif ödünçler arasında belirtilen ISBN'ye sahip kaydı bul
        /// 2. İade tarihini şu anki zaman olarak ayarla
        /// 3. Kaydı AktifOduncler listesinden çıkar
        /// 4. Kaydı OduncGecmisi listesine ekle
        /// 
        /// Böylece kayıt, aktif ödünçten geçmişe taşınır
        /// </summary>
        /// <param name="isbn">İade edilecek kaynağın ISBN numarası</param>
        public void IadeYap(string isbn)
        {
            // Aktif ödünçler arasında kaydı bul
            var kayit = AktifOduncler.Find(k => k.ISBN == isbn);
            
            if (kayit != null)
            {
                // İade tarihini şu anki zaman olarak ayarla
                kayit.IadeTarihi = DateTime.Now;
                
                // Kaydı geçmişe taşı
                OduncGecmisi.Add(kayit);
                
                // Aktif ödünçlerden çıkar
                AktifOduncler.Remove(kayit);
            }
        }

        // ==================== ANALİZ METOTLARI ====================

        /// <summary>
        /// Kullanıcının okuduğu kaynaklardan çıkarılan kategorileri döndürür
        /// 
        /// Analiz Mantığı:
        /// - Ödünç geçmişindeki tüm kayıtlar taranır
        /// - Her kaydın kategori bilgisi alınır
        /// - Tekrarlayan kategoriler bir kez eklenir (HashSet kullanarak)
        /// - Kategori listesi döndürülür
        /// 
        /// Kullanım Alanları:
        /// - Öneri sistemi (KategoriFiltresi)
        /// - Kullanıcı profil analizi
        /// - İstatistiksel raporlar
        /// </summary>
        /// <returns>Kullanıcının okuduğu kategorilerin benzersiz listesi</returns>
        public List<string> OkunanKategoriler()
        {
            // HashSet kullanarak tekrarlayan kategorileri önle
            var kategoriler = new HashSet<string>();
            
            // Ödünç geçmişindeki tüm kayıtları tara
            foreach (var kayit in OduncGecmisi)
            {
                // Kategori bilgisi varsa HashSet'e ekle
                if (!string.IsNullOrEmpty(kayit.Kategori))
                {
                    kategoriler.Add(kayit.Kategori);
                }
            }
            
            // HashSet'i List'e çevir ve döndür
            return new List<string>(kategoriler);
        }
    }

    // ==================== ÖDÜNÇ KAYDI SINIFI ====================

    /// <summary>
    /// Ödünç Kaydı Sınıfı
    /// 
    /// Bir kullanıcının bir kaynağı ödünç aldığında oluşturulan kayıt sınıfı.
    /// Bu kayıt, hem aktif ödünçlerde hem de ödünç geçmişinde kullanılır.
    /// 
    /// Yaşam Döngüsü:
    /// 1. Ödünç verilirken oluşturulur → AktifOduncler listesine eklenir
    /// 2. İade edilirken IadeTarihi doldurulur → OduncGecmisi listesine taşınır
    /// 
    /// Veritabanı: OduncKayitlari tablosunda saklanır
    /// Composite Primary Key: ISBN + OduncTarihi
    /// </summary>
    public class OduncKaydi
    {
        /// <summary>
        /// Ödünç alınan kaynağın ISBN numarası
        /// Composite Primary Key'in bir parçası
        /// </summary>
        public string ISBN { get; set; } = string.Empty;

        /// <summary>
        /// Kaynağın başlığı (denormalizasyon)
        /// 
        /// Denormalizasyon Nedeni:
        /// - Performans: JOIN işlemi yapmadan başlığa erişilebilir
        /// - Geçmiş veri: Kaynak silinse bile geçmişte hangi kaynak ödünç alındı bilgisi korunur
        /// - Hızlı sorgu: Kullanıcı ödünç geçmişini görüntülerken hızlı erişim
        /// </summary>
        public string? KaynakBaslik { get; set; }

        /// <summary>
        /// Kaynağın kategorisi (denormalizasyon)
        /// 
        /// Denormalizasyon Nedeni:
        /// - Kategori bazlı analiz ve raporlama için hızlı erişim
        /// - JOIN işlemi gerektirmez
        /// - Öneri sistemi için kategori bilgisine hızlı erişim
        /// </summary>
        public string? Kategori { get; set; }

        /// <summary>
        /// Ödünç alınma tarihi
        /// Composite Primary Key'in bir parçası
        /// Aynı kaynak farklı tarihlerde ödünç verilebilir
        /// </summary>
        public DateTime OduncTarihi { get; set; }

        /// <summary>
        /// İade edilme tarihi
        /// Nullable: Henüz iade edilmediyse null
        /// İade edildiğinde şu anki zaman atanır
        /// </summary>
        public DateTime? IadeTarihi { get; set; }

        /// <summary>
        /// Kaynağın teslim süresi (gün cinsinden)
        /// 
        /// Önemli:
        /// - Bu değer, ödünç verilirken kaynağın türüne göre belirlenir
        /// - Kitap: 14 gün
        /// - Dergi: 7 gün
        /// - Tez: 21 gün
        /// - Gecikme hesaplaması için kullanılır
        /// </summary>
        public int TeslimSuresi { get; set; }

        // ==================== HESAPLAMA METOTLARI ====================

        /// <summary>
        /// Ödünç kaydının geciktiğini kontrol eder
        /// 
        /// Kontrol Mantığı:
        /// 1. Eğer kayıt zaten iade edildiyse (IadeTarihi dolu) → gecikme yok
        /// 2. Teslim tarihi = Ödünç Tarihi + Teslim Süresi
        /// 3. Şu anki tarih > Teslim tarihi ise → gecikme var
        /// 
        /// Kullanım:
        /// - Gecikme uyarıları oluştururken
        /// - Ceza hesaplaması yaparken
        /// </summary>
        /// <returns>true: Gecikmiş, false: Gecikmemiş veya iade edilmiş</returns>
        public bool GeciktiMi()
        {
            // İade edilmişse gecikme yok
            if (IadeTarihi.HasValue) return false;
            
            // Teslim tarihini hesapla
            var teslimTarihi = OduncTarihi.AddDays(TeslimSuresi);
            
            // Şu anki tarih teslim tarihinden sonraysa gecikme var
            return DateTime.Now > teslimTarihi;
        }

        /// <summary>
        /// Gecikme gün sayısını hesaplar
        /// 
        /// Hesaplama Mantığı:
        /// 1. Önce GeciktiMi() kontrolü yapılır
        /// 2. Gecikme yoksa 0 döner
        /// 3. Gecikme varsa: Şu Anki Tarih - Teslim Tarihi
        /// 
        /// Kullanım:
        /// - Ceza hesaplaması için
        /// - Gecikme raporları için
        /// - Kullanıcıya gecikme bilgisi gösterirken
        /// </summary>
        /// <returns>Gecikme gün sayısı (0 veya pozitif)</returns>
        public int GecikmeGunSayisi()
        {
            // Gecikme yoksa 0 döndür
            if (!GeciktiMi()) return 0;
            
            // Teslim tarihini hesapla
            var teslimTarihi = OduncTarihi.AddDays(TeslimSuresi);
            
            // Gecikme gün sayısını hesapla
            return (DateTime.Now - teslimTarihi).Days;
        }
    }
}
