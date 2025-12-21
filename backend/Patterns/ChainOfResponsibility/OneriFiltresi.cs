using System.Collections.Generic;
using System.Linq;
using SmartLibrary.Models;

namespace SmartLibrary.Patterns.ChainOfResponsibility
{
    /// <summary>
    /// Öneri Filtresi - Abstract Base Class
    /// 
    /// Chain of Responsibility Design Pattern implementasyonu.
    /// Bu pattern, öneri sisteminde sıralı filtrelerin uygulanması için kullanılır.
    /// 
    /// Chain of Responsibility Pattern Nedenleri:
    /// - Single Responsibility: Her filtre tek bir sorumluluğa sahip
    /// - Flexible Ordering: Filtrelerin sırası değiştirilebilir
    /// - Easy Extension: Yeni filtre eklemek kolaydır
    /// - Decoupled: Filtreler birbirinden bağımsızdır
    /// 
    /// Filtre Zinciri Yapısı:
    /// ```
    /// KategoriFiltresi
    ///     ↓ (sonraki filtre)
    /// IlgiAlaniFiltresi
    ///     ↓ (sonraki filtre)
    /// OkumaGecmisiFiltresi
    ///     ↓ (sonraki filtre)
    /// YasFiltresi
    ///     ↓ (sonraki filtre)
    /// PopulariteFiltresi
    ///     ↓ (zincir sonu)
    /// Sonuç
    /// ```
    /// 
    /// Her filtre, gelen kaynak listesini işler ve bir sonraki filtreye aktarır.
    /// Zincir sonunda finalize edilmiş öneri listesi elde edilir.
    /// </summary>
    public abstract class OneriFiltresi
    {
        /// <summary>
        /// Zincirdeki bir sonraki filtre
        /// 
        /// Chain Pattern Özelliği:
        /// - Her filtre bir sonraki filtrenin referansını tutar
        /// - Null ise zincir sonu demektir
        /// - Protected: Alt sınıflar erişebilir
        /// </summary>
        protected OneriFiltresi? _sonrakiFiltre;

        /// <summary>
        /// Zincirdeki bir sonraki filtreyi ayarlar
        /// 
        /// Bu metot ile filtre zinciri oluşturulur.
        /// 
        /// Kullanım Örneği:
        /// ```csharp
        /// var kategori = new KategoriFiltresi();
        /// var ilgiAlani = new IlgiAlaniFiltresi();
        /// kategori.SonrakiFiltreyiAyarla(ilgiAlani);
        /// ```
        /// </summary>
        /// <param name="filtre">Zincire eklenecek bir sonraki filtre</param>
        public void SonrakiFiltreyiAyarla(OneriFiltresi filtre)
        {
            _sonrakiFiltre = filtre;
        }

        /// <summary>
        /// Filtreleme işlemini gerçekleştirir - Abstract Metot
        /// 
        /// Her filtre bu metodu implement eder ve kendi filtreleme mantığını uygular.
        /// Filtreleme sonunda SonrakiFiltreUygula() çağrılarak zincir devam ettirilir.
        /// 
        /// Parametreler:
        /// - kaynaklar: Filtrelenecek kaynak listesi
        /// - kullanici: Öneriler üretilecek kullanıcı
        /// - hedefSayi: İstenen öneri sayısı
        /// </summary>
        /// <param name="kaynaklar">Filtrelenecek kaynak listesi</param>
        /// <param name="kullanici">Öneriler üretilecek kullanıcı</param>
        /// <param name="hedefSayi">Hedef öneri sayısı</param>
        /// <returns>Filtrelenmiş kaynak listesi</returns>
        public abstract List<Kaynak> Filtrele(List<Kaynak> kaynaklar, Kullanici kullanici, int hedefSayi);

        /// <summary>
        /// Zincirdeki bir sonraki filtreyi uygular
        /// 
        /// Chain Pattern'in temel metodu:
        /// - Eğer sonraki filtre varsa, ona filtreleme işlemini devreder
        /// - Sonraki filtre yoksa (zincir sonu), hedef sayıya kadar kaynak döndürür
        /// 
        /// Bu metot, tüm filtrelerin ortak kullandığı yardımcı metottur.
        /// </summary>
        /// <param name="kaynaklar">Sonraki filtreye aktarılacak kaynak listesi</param>
        /// <param name="kullanici">Kullanıcı bilgisi</param>
        /// <param name="hedefSayi">Hedef öneri sayısı</param>
        /// <returns>Zincir sonundaki filtreleme sonucu</returns>
        protected List<Kaynak> SonrakiFiltreUygula(List<Kaynak> kaynaklar, Kullanici kullanici, int hedefSayi)
        {
            // Sonraki filtre varsa, ona devret
            if (_sonrakiFiltre != null)
            {
                return _sonrakiFiltre.Filtrele(kaynaklar, kullanici, hedefSayi);
            }
            
            // Zincir sonu - hedef sayıya kadar kaynak döndür
            return kaynaklar.Take(hedefSayi).ToList();
        }
    }

    // ==================== SOMUT FİLTRE SINIFLARI ====================

    /// <summary>
    /// Kategori Filtresi - 1. Filtre
    /// 
    /// Kullanıcının okuduğu ve favori kategorilerine göre kaynakları filtreler.
    /// 
    /// Filtreleme Mantığı:
    /// 1. Kullanıcının okuduğu kategorileri al
    /// 2. Favori kategorileri al
    /// 3. İki listeyi birleştir (HashSet ile tekrarsız)
    /// 4. Kategori eşleşen kaynakları seç
    /// 5. Yeterli kaynak yoksa rastgele kaynaklar ekle
    /// 
    /// Önemi:
    /// - Kullanıcının ilgi alanlarına göre önceliklendirme
    /// - Kategori bazlı öneriler
    /// </summary>
    public class KategoriFiltresi : OneriFiltresi
    {
        /// <summary>
        /// Kategori bazlı filtreleme işlemi
        /// </summary>
        /// <param name="kaynaklar">Filtrelenecek kaynak listesi</param>
        /// <param name="kullanici">Kullanıcı bilgisi</param>
        /// <param name="hedefSayi">Hedef öneri sayısı</param>
        /// <returns>Kategori eşleşen kaynaklar + eksikse rastgele kaynaklar</returns>
        public override List<Kaynak> Filtrele(List<Kaynak> kaynaklar, Kullanici kullanici, int hedefSayi)
        {
            // Kullanıcının okuduğu kategorileri al
            var okunanKategoriler = kullanici.OkunanKategoriler();
            
            // Favori kategorileri al
            var favoriKategoriler = kullanici.FavoriKategoriler;
            
            // İki listeyi birleştir (HashSet ile tekrarsız)
            var tumKategoriler = new HashSet<string>(okunanKategoriler);
            foreach (var kategori in favoriKategoriler)
                tumKategoriler.Add(kategori);

            // Kategori eşleşen kaynakları filtrele
            var filtrelenmis = kaynaklar
                .Where(k => k.Kategori != null && tumKategoriler.Contains(k.Kategori))
                .ToList();

            // Yeterli kaynak yoksa eksik kadar rastgele kaynak ekle
            // Bu sayede her zaman hedef sayı kadar öneri sağlanır
            if (filtrelenmis.Count < hedefSayi)
            {
                var eksikKaynaklar = kaynaklar
                    .Where(k => !filtrelenmis.Contains(k))
                    .Take(hedefSayi - filtrelenmis.Count);
                filtrelenmis.AddRange(eksikKaynaklar);
            }

            // Bir sonraki filtreye aktar
            return SonrakiFiltreUygula(filtrelenmis, kullanici, hedefSayi);
        }
    }

    /// <summary>
    /// İlgi Alanı Filtresi - 2. Filtre
    /// 
    /// Kullanıcının ilgi alanlarına göre kaynakları filtreler.
    /// İlgi alanları, hem kategori hem de başlıkta aranır.
    /// 
    /// Filtreleme Mantığı:
    /// 1. Kullanıcının ilgi alanlarını kontrol et
    /// 2. Kategori veya başlıkta ilgi alanı geçen kaynakları seç
    /// 3. Yeterli kaynak yoksa rastgele kaynaklar ekle
    /// 
    /// Önemi:
    /// - Kullanıcı tercihlerine göre özelleştirme
    /// - Daha geniş kapsamlı eşleştirme (kategori + başlık)
    /// </summary>
    public class IlgiAlaniFiltresi : OneriFiltresi
    {
        /// <summary>
        /// İlgi alanı bazlı filtreleme işlemi
        /// </summary>
        /// <param name="kaynaklar">Filtrelenecek kaynak listesi</param>
        /// <param name="kullanici">Kullanıcı bilgisi</param>
        /// <param name="hedefSayi">Hedef öneri sayısı</param>
        /// <returns>İlgi alanı eşleşen kaynaklar</returns>
        public override List<Kaynak> Filtrele(List<Kaynak> kaynaklar, Kullanici kullanici, int hedefSayi)
        {
            // İlgi alanı yoksa filtreleme yapma, direkt sonraki filtreye geç
            if (kullanici.IlgiAlanlari == null || kullanici.IlgiAlanlari.Count == 0)
            {
                return SonrakiFiltreUygula(kaynaklar, kullanici, hedefSayi);
            }

            // Kategori veya başlıkta ilgi alanı geçen kaynakları bul
            var filtrelenmis = kaynaklar
                .Where(k => kullanici.IlgiAlanlari.Any(ilgi => 
                    (k.Kategori != null && k.Kategori.Contains(ilgi, System.StringComparison.OrdinalIgnoreCase)) ||
                    k.Baslik.Contains(ilgi, System.StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Yeterli kaynak yoksa eksik kadar rastgele ekle
            if (filtrelenmis.Count < hedefSayi)
            {
                var eksikKaynaklar = kaynaklar
                    .Where(k => !filtrelenmis.Contains(k))
                    .Take(hedefSayi - filtrelenmis.Count);
                filtrelenmis.AddRange(eksikKaynaklar);
            }

            return SonrakiFiltreUygula(filtrelenmis, kullanici, hedefSayi);
        }
    }

    /// <summary>
    /// Okuma Geçmişi Filtresi - 3. Filtre
    /// 
    /// Kullanıcının okuma geçmişine göre kaynakları filtreler.
    /// 
    /// Filtreleme Mantığı:
    /// 1. Daha önce okunmayan kaynakları önceliklendir
    /// 2. Aynı yazarların diğer eserlerini öne çıkar
    /// 3. Yeni içerikleri keşfetmeyi destekler
    /// 
    /// Önemi:
    /// - Tekrar önleme: Aynı kaynak tekrar önerilmez
    /// - Yazar devamı: Beğenilen yazarların diğer eserleri
    /// - Keşif: Yeni içerik önerileri
    /// </summary>
    public class OkumaGecmisiFiltresi : OneriFiltresi
    {
        /// <summary>
        /// Okuma geçmişi bazlı filtreleme işlemi
        /// </summary>
        /// <param name="kaynaklar">Filtrelenecek kaynak listesi</param>
        /// <param name="kullanici">Kullanıcı bilgisi</param>
        /// <param name="hedefSayi">Hedef öneri sayısı</param>
        /// <returns>Okunmayan kaynaklar (aynı yazarların eserleri öncelikli)</returns>
        public override List<Kaynak> Filtrele(List<Kaynak> kaynaklar, Kullanici kullanici, int hedefSayi)
        {
            // Daha önce okunan ISBN'leri HashSet'e al (O(1) lookup için)
            var okunanISBNler = new HashSet<string>(
                kullanici.OduncGecmisi.Select(o => o.ISBN)
            );

            // Daha önce okunmayanları seç (yeni içerik önerisi)
            var filtrelenmis = kaynaklar
                .Where(k => !okunanISBNler.Contains(k.ISBN))
                .ToList();

            // Okunan kaynakların yazarlarını bul
            var okunanYazarlar = kullanici.OduncGecmisi
                .Select(o => kaynaklar.FirstOrDefault(k => k.ISBN == o.ISBN)?.Yazar)
                .Where(y => y != null)
                .Distinct()
                .ToList();

            // Aynı yazarların diğer eserlerini bul (okunmamış olanlar)
            var ayniYazarKaynaklari = kaynaklar
                .Where(k => okunanYazarlar.Contains(k.Yazar) && !okunanISBNler.Contains(k.ISBN))
                .ToList();

            // Önceliklendirme: Önce aynı yazarların eserleri, sonra diğerleri
            var sonuc = ayniYazarKaynaklari
                .Concat(filtrelenmis.Where(k => !ayniYazarKaynaklari.Contains(k)))
                .ToList();

            return SonrakiFiltreUygula(sonuc, kullanici, hedefSayi);
        }
    }

    /// <summary>
    /// Yaş Filtresi - 4. Filtre
    /// 
    /// Kullanıcının yaşına göre uygun kaynakları seçer.
    /// 
    /// Yaş Grupları:
    /// - < 18: Gençler - Popüler ve güncel kaynaklar (son 5 yıl)
    /// - 18-40: Yetişkinler - Tüm kaynaklar (çeşitli)
    /// - 40+: Yaşlılar - Klasik ve yerleşik kaynaklar (eski tarihli)
    /// 
    /// Filtreleme Mantığı:
    /// - Gençler için yeni yayınlar ve popüler içerikler
    /// - Yetişkinler için her türlü kaynak
    /// - Yaşlılar için klasik eserler
    /// 
    /// Önemi:
    /// - Yaş-uygun içerik önerileri
    /// - Farklı yaş gruplarının ilgi alanlarına hitap
    /// </summary>
    public class YasFiltresi : OneriFiltresi
    {
        /// <summary>
        /// Yaş bazlı filtreleme işlemi
        /// </summary>
        /// <param name="kaynaklar">Filtrelenecek kaynak listesi</param>
        /// <param name="kullanici">Kullanıcı bilgisi</param>
        /// <param name="hedefSayi">Hedef öneri sayısı</param>
        /// <returns>Yaş grubuna uygun kaynaklar</returns>
        public override List<Kaynak> Filtrele(List<Kaynak> kaynaklar, Kullanici kullanici, int hedefSayi)
        {
            var filtrelenmis = new List<Kaynak>();

            // Yaş grubuna göre filtreleme
            if (kullanici.Yas < 18)
            {
                // GENÇLER (< 18 yaş)
                // Son 5 yılda yayınlanmış, popüler kaynakları önceliklendir
                filtrelenmis = kaynaklar
                    .Where(k => k.YayinTarihi.Year >= System.DateTime.Now.Year - 5)
                    .OrderByDescending(k => k.OkunmaSayisi)
                    .ToList();
            }
            else if (kullanici.Yas >= 18 && kullanici.Yas < 40)
            {
                // YETİŞKİNLER (18-40 yaş)
                // Tüm kaynaklar - çeşitlilik önemli
                filtrelenmis = kaynaklar.ToList();
            }
            else
            {
                // YAŞLILAR (40+ yaş)
                // Klasik ve yerleşik kaynaklar - eski tarihli eserler
                filtrelenmis = kaynaklar
                    .OrderBy(k => k.YayinTarihi)  // Eski kaynaklar önce
                    .ToList();
            }

            // Yeterli kaynak yoksa tüm kaynakları al (fallback)
            if (filtrelenmis.Count < hedefSayi)
                filtrelenmis = kaynaklar.ToList();

            return SonrakiFiltreUygula(filtrelenmis, kullanici, hedefSayi);
        }
    }

    /// <summary>
    /// Popülarite Filtresi - 5. Filtre (Son Filtre)
    /// 
    /// Kaynakları popülerlik sırasına göre düzenler ve dengeli bir öneri listesi oluşturur.
    /// 
    /// Dengeleme Stratejisi:
    /// - %66 Popüler kaynaklar (yüksek okunma sayısı)
    /// - %33 Keşif kaynakları (az bilinen ama değerli)
    /// 
    /// Filtreleme Mantığı:
    /// 1. Kaynakları okunma sayısına göre sırala
    /// 2. Üst %66'yı popüler olarak seç
    /// 3. Alt %33'ü keşif olarak seç
    /// 4. İki listeyi birleştir
    /// 
    /// Önemi:
    /// - Popüler içerikleri öne çıkarır
    /// - Az bilinen değerli kaynakları da önerir (keşif)
    /// - Dengeli öneri listesi sağlar
    /// </summary>
    public class PopulariteFiltresi : OneriFiltresi
    {
        /// <summary>
        /// Popülarite bazlı filtreleme işlemi
        /// </summary>
        /// <param name="kaynaklar">Filtrelenecek kaynak listesi</param>
        /// <param name="kullanici">Kullanıcı bilgisi</param>
        /// <param name="hedefSayi">Hedef öneri sayısı</param>
        /// <returns>Popüler ve keşif kaynaklarının dengeli karışımı</returns>
        public override List<Kaynak> Filtrele(List<Kaynak> kaynaklar, Kullanici kullanici, int hedefSayi)
        {
            // Okunma sayısına göre azalan sırada sırala
            var filtrelenmis = kaynaklar
                .OrderByDescending(k => k.OkunmaSayisi)
                .ToList();

            // Dengeli öneri listesi oluştur
            // %66 popüler kaynaklar (üst sıralar)
            var populerKaynaklar = filtrelenmis.Take(hedefSayi * 2 / 3).ToList();
            
            // %33 keşif kaynakları (orta-alt sıralar)
            // Az bilinen ama değerli kaynakları da öner
            var kesifKaynaklari = filtrelenmis
                .Skip(hedefSayi * 2 / 3)  // Popülerlerden sonraki kaynaklar
                .Take(hedefSayi / 3)       // %33 kadar al
                .ToList();

            // İki listeyi birleştir: Popülerler + Keşifler
            var sonuc = populerKaynaklar.Concat(kesifKaynaklari).ToList();

            // Bu son filtre olduğu için SonrakiFiltreUygula() hedef sayıya kadar alır
            return SonrakiFiltreUygula(sonuc, kullanici, hedefSayi);
        }
    }
}
