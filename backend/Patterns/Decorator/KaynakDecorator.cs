using SmartLibrary.Models;
using System.Collections.Generic;

namespace SmartLibrary.Patterns.Decorator
{
    /// <summary>
    /// Kaynak Decorator - Abstract Base Class
    /// 
    /// Decorator Design Pattern implementasyonu.
    /// Bu pattern, kaynak nesnelerine runtime'da dinamik olarak özellikler eklemek için kullanılır.
    /// 
    /// Decorator Pattern Nedenleri:
    /// - Inheritance Explosion Önleme: Her özellik kombinasyonu için yeni sınıf oluşturmaya gerek yok
    /// - Runtime Composition: Çalışma zamanında özellikler eklenebilir
    /// - Open/Closed Principle: Mevcut kodu değiştirmeden yeni özellikler eklenebilir
    /// - Flexible Composition: Özellikler istenildiği gibi birleştirilebilir
    /// 
    /// Kullanım Örneği:
    /// ```csharp
    /// Kaynak kitap = new Kitap(...);
    /// kitap = new PopulerKaynakDecorator(kitap, 85, true);
    /// kitap = new EtiketliKaynakDecorator(kitap, new List<string> { "Klasik", "Edebiyat" });
    /// string ozet = kitap.OzetGoster(); // Tüm özellikler dahil özet
    /// ```
    /// 
    /// Yapı:
    /// - Base class: Kaynak abstract sınıfından türer
    /// - Composition: İçeride bir Kaynak nesnesi tutar (wraps)
    /// - Delegation: Çoğu metot iç kaynağa delegate eder
    /// - Extension: OzetGoster() metodu extend edilerek yeni bilgiler eklenir
    /// </summary>
    public abstract class KaynakDecorator : Kaynak
    {
        /// <summary>
        /// Sarmalanan (wrapped) kaynak nesnesi
        /// Protected: Türetilmiş sınıflar erişebilir
        /// 
        /// Decorator pattern'in temel öğesi:
        /// - Decorator, içeride bir Kaynak nesnesi tutar
        /// - Çoğu işlemi bu nesneye delegate eder
        /// - Sadece belirli metotları extend eder (örn: OzetGoster)
        /// </summary>
        protected Kaynak _kaynak;

        /// <summary>
        /// Constructor
        /// 
        /// İşlemler:
        /// 1. Base class constructor'ını çağırır (Kaynak abstract sınıfı)
        /// 2. Sarmalanacak kaynak nesnesini saklar
        /// 3. Kaynak durum bilgilerini kopyalar (ödünç durumu, okunma sayısı vb.)
        /// </summary>
        /// <param name="kaynak">Sarmalanacak kaynak nesnesi</param>
        protected KaynakDecorator(Kaynak kaynak) 
            : base(kaynak.ISBN, kaynak.Baslik, kaynak.Yazar, kaynak.YayinTarihi, kaynak.Kategori)
        {
            _kaynak = kaynak;
            
            // Durum bilgilerini kopyala
            this.OduncDurumu = kaynak.OduncDurumu;
            this.OduncTarihi = kaynak.OduncTarihi;
            this.OkunmaSayisi = kaynak.OkunmaSayisi;
        }

        /// <summary>
        /// Özet gösterimi - Decorator pattern'in extend edilen metodu
        /// 
        /// Delegation Pattern:
        /// - Temel implementasyon iç kaynağa delegate edilir
        /// - Alt sınıflar bu metodu override ederek yeni bilgiler ekler
        /// 
        /// Default davranış: İç kaynağın özetini döndürür
        /// Alt sınıflar: Ek özellikleri ekleyerek extend eder
        /// </summary>
        /// <returns>Kaynağın formatlı özeti</returns>
        public override string OzetGoster()
        {
            // Default: İç kaynağın özetini döndür
            // Alt sınıflar bu metodu override ederek yeni bilgiler ekler
            return _kaynak.OzetGoster();
        }

        /// <summary>
        /// Ceza hesaplama - Delegation
        /// 
        /// İç kaynağa delegate edilir, decorator değişiklik yapmaz
        /// Polimorfizm korunur - kaynak türüne göre ceza hesaplanır
        /// </summary>
        /// <param name="gecikmeGunSayisi">Gecikme gün sayısı</param>
        /// <returns>Hesaplanan ceza tutarı</returns>
        public override decimal CezaHesapla(int gecikmeGunSayisi)
        {
            // İç kaynağa delegate et
            return _kaynak.CezaHesapla(gecikmeGunSayisi);
        }

        /// <summary>
        /// Teslim süresi - Delegation
        /// 
        /// İç kaynağa delegate edilir, decorator değişiklik yapmaz
        /// Polimorfizm korunur - kaynak türüne göre süre döner
        /// </summary>
        /// <returns>Teslim süresi (gün)</returns>
        public override int TeslimSuresi()
        {
            // İç kaynağa delegate et
            return _kaynak.TeslimSuresi();
        }
    }

    // ==================== SOMUT DECORATOR SINIFLARI ====================

    /// <summary>
    /// Popülerlik Özelliği Ekleyen Decorator
    /// 
    /// Bir kaynağa popülerlik ve editör seçimi özelliği ekler.
    /// 
    /// Özellikler:
    /// - PopuleriteSeviyesi: 0-100 arası popülerlik skoru
    /// - EditorSecimi: Editör tarafından önerilip önerilmediği
    /// 
    /// Kullanım Senaryosu:
    /// - Popüler kaynakları vurgulamak için
    /// - Editörün özel seçimlerini göstermek için
    /// - Öneri sisteminde önceliklendirme için
    /// 
    /// Örnek:
    /// ```csharp
    /// Kaynak kitap = new Kitap(...);
    /// kitap = new PopulerKaynakDecorator(kitap, populeriteSeviyesi: 85, editorSecimi: true);
    /// // Artık özetinde popülerlik bilgisi görünür
    /// ```
    /// </summary>
    public class PopulerKaynakDecorator : KaynakDecorator
    {
        /// <summary>
        /// Popülerlik seviyesi (0-100 arası)
        /// 
        /// Hesaplama Kriterleri:
        /// - Okunma sayısı
        /// - Son dönemdeki talep
        /// - Kullanıcı değerlendirmeleri (varsa)
        /// 
        /// Örnek: 85/100 (çok popüler)
        /// </summary>
        public int PopuleriteSeviyesi { get; set; }

        /// <summary>
        /// Editörün seçimi mi?
        /// 
        /// true: Editör tarafından özel olarak önerilmiş
        /// false: Normal kaynak
        /// 
        /// Özel durumlar için kullanılır:
        /// - Ödüllü kitaplar
        /// - Yeni çıkan eserler
        /// - Özel koleksiyonlar
        /// </summary>
        public bool EditorSecimi { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kaynak">Sarmalanacak kaynak</param>
        /// <param name="populeriteSeviyesi">Popülerlik seviyesi (varsayılan: 0)</param>
        /// <param name="editorSecimi">Editör seçimi mi? (varsayılan: false)</param>
        public PopulerKaynakDecorator(Kaynak kaynak, int populeriteSeviyesi = 0, bool editorSecimi = false) 
            : base(kaynak)
        {
            PopuleriteSeviyesi = populeriteSeviyesi;
            EditorSecimi = editorSecimi;
        }

        /// <summary>
        /// Özet gösterimini extend eder
        /// 
        /// İşlem:
        /// 1. İç kaynağın özetini al
        /// 2. Popülerlik bilgisini ekle
        /// 3. Editör seçimi varsa ek bilgi ekle
        /// 
        /// Sonuç: Temel özet + Popülerlik bilgisi
        /// </summary>
        /// <returns>Extend edilmiş özet</returns>
        public override string OzetGoster()
        {
            // İç kaynağın özetini al
            var baseOzet = base.OzetGoster();
            
            // Popülerlik bilgisini ekle
            var ekOzellikler = $"\n⭐ Popülerite: {PopuleriteSeviyesi}/100";
            
            // Editör seçimi varsa ek bilgi ekle
            if (EditorSecimi)
                ekOzellikler += "\n🏆 Editörün Seçimi!";
            
            // Temel özet + Ek özellikler
            return baseOzet + ekOzellikler;
        }
    }

    /// <summary>
    /// Etiket Özelliği Ekleyen Decorator
    /// 
    /// Bir kaynağa dinamik etiketler ekler.
    /// Etiketler, kaynakları daha iyi kategorize etmek ve arama yapmak için kullanılır.
    /// 
    /// Kullanım Senaryoları:
    /// - Çoklu kategorizasyon (örn: "Klasik", "Edebiyat", "Rus Edebiyatı")
    /// - Arama ve filtreleme
    /// - Öneri sistemi için ek kriterler
    /// - Tag-based recommendation
    /// 
    /// Örnek:
    /// ```csharp
    /// Kaynak kitap = new Kitap(...);
    /// kitap = new EtiketliKaynakDecorator(kitap, new List<string> { "Klasik", "Edebiyat", "Felsefe" });
    /// ```
    /// </summary>
    public class EtiketliKaynakDecorator : KaynakDecorator
    {
        /// <summary>
        /// Kaynağın etiket listesi
        /// 
        /// Örnek etiketler:
        /// - ["Klasik", "Edebiyat", "Rus Edebiyatı"]
        /// - ["Bestseller", "Yeni Çıkan", "Ödüllü"]
        /// - ["Felsefe", "Varoluşçuluk", "20. Yüzyıl"]
        /// 
        /// Birden fazla etiket aynı kaynağa eklenebilir
        /// </summary>
        public List<string> Etiketler { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kaynak">Sarmalanacak kaynak</param>
        /// <param name="etiketler">Eklenecek etiket listesi (null ise boş liste)</param>
        public EtiketliKaynakDecorator(Kaynak kaynak, List<string> etiketler) 
            : base(kaynak)
        {
            Etiketler = etiketler ?? new List<string>();
        }

        /// <summary>
        /// Özet gösterimini extend eder
        /// 
        /// Etiketler varsa özete eklenir.
        /// Etiketler yoksa sadece temel özet döner.
        /// </summary>
        /// <returns>Extend edilmiş özet (etiketler varsa)</returns>
        public override string OzetGoster()
        {
            // İç kaynağın özetini al
            var baseOzet = base.OzetGoster();
            
            // Etiketler varsa ekle
            if (Etiketler.Count > 0)
            {
                // Etiketleri virgülle ayırarak ekle
                var etiketMetni = $"\n🏷️ Etiketler: {string.Join(", ", Etiketler)}";
                return baseOzet + etiketMetni;
            }
            
            // Etiket yoksa sadece temel özet
            return baseOzet;
        }
    }

    /// <summary>
    /// Özel Koleksiyon Özelliği Ekleyen Decorator
    /// 
    /// Bir kaynağa koleksiyon bilgisi ekler.
    /// Koleksiyonlar, kaynakları özel seriler veya setler halinde gruplamak için kullanılır.
    /// 
    /// Kullanım Senaryoları:
    /// - Yazar serileri (örn: "Harry Potter Serisi - 3. Kitap")
    /// - Özel koleksiyonlar (örn: "Nobel Ödüllü Eserler - 5. Eser")
    /// - Seri numaralandırma
    /// - Set bazlı ödünç verme
    /// 
    /// Örnek:
    /// ```csharp
    /// Kaynak kitap = new Kitap(...);
    /// kitap = new KoleksiyonKaynakDecorator(kitap, "Harry Potter Serisi", 3);
    /// ```
    /// </summary>
    public class KoleksiyonKaynakDecorator : KaynakDecorator
    {
        /// <summary>
        /// Koleksiyon adı
        /// 
        /// Örnek koleksiyonlar:
        /// - "Harry Potter Serisi"
        /// - "Nobel Ödüllü Eserler"
        /// - "Klasikler Koleksiyonu"
        /// - "Bilim Kurgu Serisi"
        /// </summary>
        public string KoleksiyonAdi { get; set; }

        /// <summary>
        /// Kaynağın koleksiyon içindeki sıra numarası
        /// 
        /// Örnek: "Harry Potter Serisi" koleksiyonunda 3. kitap
        /// </summary>
        public int SiraNo { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kaynak">Sarmalanacak kaynak</param>
        /// <param name="koleksiyonAdi">Koleksiyon adı</param>
        /// <param name="siraNo">Koleksiyon içindeki sıra numarası</param>
        public KoleksiyonKaynakDecorator(Kaynak kaynak, string koleksiyonAdi, int siraNo) 
            : base(kaynak)
        {
            KoleksiyonAdi = koleksiyonAdi;
            SiraNo = siraNo;
        }

        /// <summary>
        /// Özet gösterimini extend eder
        /// 
        /// Koleksiyon bilgisi özete eklenir.
        /// </summary>
        /// <returns>Extend edilmiş özet (koleksiyon bilgisi ile)</returns>
        public override string OzetGoster()
        {
            // İç kaynağın özetini al
            var baseOzet = base.OzetGoster();
            
            // Koleksiyon bilgisini ekle
            return baseOzet + $"\n📚 Koleksiyon: {KoleksiyonAdi} (Sıra: {SiraNo})";
        }
    }
}
