using System.Collections.Generic;
using System.Linq;
using SmartLibrary.Models;
using SmartLibrary.Patterns.ChainOfResponsibility;
using SmartLibrary.Patterns.Singleton;

namespace SmartLibrary.Services
{
    /// <summary>
    /// Öneri Sistemi Servis Sınıfı
    /// 
    /// Bu sınıf, akıllı öneri algoritmasını yönetir ve kullanıcılara
    /// kişiselleştirilmiş kaynak önerileri üretir.
    /// 
    /// Design Pattern: Chain of Responsibility
    /// - 5 aşamalı filtre zinciri kullanılır
    /// - Her filtre bağımsız çalışır
    /// - Filtreler sırayla uygulanır ve kaynak listesi daraltılır
    /// 
    /// Öneri Algoritması:
    /// 1. Filtreleme: Chain of Responsibility ile 5 aşamalı filtreleme
    /// 2. Skorlama: Her kaynak için 0-100 arası öneri skoru hesaplanır
    /// 3. Nedenlendirme: Her önerinin nedenleri belirlenir
    /// 4. Sıralama: Skorlara göre azalan sırada sıralanır
    /// 
    /// Özellikler:
    /// - Kullanıcıya özel öneriler
    /// - Benzer kaynak bulma
    /// - Trend kaynaklar
    /// - Kategori bazlı öneriler
    /// </summary>
    public class OneriSistemi
    {
        /// <summary>
        /// Singleton kütüphane yöneticisi
        /// Kaynak ve kullanıcı verilerine erişim için
        /// </summary>
        private readonly KutuphaneYoneticisi _yonetici;

        /// <summary>
        /// Filtre zinciri - Chain of Responsibility pattern
        /// 
        /// Zincir Yapısı:
        /// KategoriFiltresi → IlgiAlaniFiltresi → OkumaGecmisiFiltresi → 
        /// YasFiltresi → PopulariteFiltresi
        /// </summary>
        private readonly OneriFiltresi _filtreZinciri;

        /// <summary>
        /// Constructor
        /// 
        /// İşlemler:
        /// 1. Singleton yöneticiyi al
        /// 2. Filtre zincirini oluştur
        /// 3. Filtreleri birbirine bağla
        /// </summary>
        public OneriSistemi()
        {
            // Singleton instance'ını al
            _yonetici = KutuphaneYoneticisi.Instance;
            
            // ========== FİLTRE ZİNCİRİNİ OLUŞTUR ==========
            
            // 1. Kategori Filtresi - İlk filtre
            var kategoriFiltresi = new KategoriFiltresi();
            
            // 2. İlgi Alanı Filtresi
            var ilgiAlaniFiltresi = new IlgiAlaniFiltresi();
            
            // 3. Okuma Geçmişi Filtresi
            var okuymaGecmisiFiltresi = new OkumaGecmisiFiltresi();
            
            // 4. Yaş Filtresi
            var yasFiltresi = new YasFiltresi();
            
            // 5. Popülarite Filtresi - Son filtre
            var populariteFiltresi = new PopulariteFiltresi();

            // ========== ZİNCİR BAĞLANTILARINI KUR ==========
            // Her filtre bir sonraki filtreye bağlanır
            kategoriFiltresi.SonrakiFiltreyiAyarla(ilgiAlaniFiltresi);
            ilgiAlaniFiltresi.SonrakiFiltreyiAyarla(okuymaGecmisiFiltresi);
            okuymaGecmisiFiltresi.SonrakiFiltreyiAyarla(yasFiltresi);
            yasFiltresi.SonrakiFiltreyiAyarla(populariteFiltresi);

            // Zincirin başlangıcını sakla (ilk filtre)
            _filtreZinciri = kategoriFiltresi;
        }

        // ==================== ÖNERİ ÜRETME METOTLARI ====================

        /// <summary>
        /// Kullanıcıya özel öneriler üretir
        /// 
        /// İşlem Adımları:
        /// 1. Kullanıcıyı getir (validasyon)
        /// 2. Mevcut kaynakları al (ödünç verilmemiş)
        /// 3. Filtre zincirini uygula (Chain of Responsibility)
        /// 4. Her kaynak için öneri skoru hesapla (0-100)
        /// 5. Öneri nedenlerini belirle
        /// 6. Skorlara göre sırala (azalan sıra)
        /// 7. Hedef sayıya kadar öneri döndür
        /// 
        /// Skorlama Sistemi:
        /// - Kategori uyumu: 0-30 puan
        /// - İlgi alanı uyumu: 0-25 puan
        /// - Popülarite: 0-20 puan
        /// - Yazar tanıdıklığı: 0-15 puan
        /// - Yenilik: 0-10 puan
        /// - Toplam: 0-100 puan
        /// </summary>
        /// <param name="kullaniciId">Öneriler üretilecek kullanıcının ID'si</param>
        /// <param name="oneriSayisi">Üretilecek öneri sayısı (varsayılan: 10)</param>
        /// <returns>Önerilerin listesi (skora göre sıralı)</returns>
        public List<OneriSonucu> OnerilerUret(string kullaniciId, int oneriSayisi = 10)
        {
            // Kullanıcıyı getir
            var kullanici = _yonetici.KullaniciGetir(kullaniciId);
            
            // Kullanıcı yoksa boş liste döndür
            if (kullanici == null)
                return new List<OneriSonucu>();

            // Tüm mevcut kaynakları al (sadece ödünç verilmemiş olanlar)
            var tumKaynaklar = _yonetici.TumKaynaklariGetir()
                .Where(k => !k.OduncDurumu) // Sadece mevcut kaynaklar
                .ToList();

            // ========== FİLTRE ZİNCİRİNİ UYGULA ==========
            // Chain of Responsibility pattern - filtreler sırayla uygulanır
            var filtrelenmisKaynaklar = _filtreZinciri.Filtrele(tumKaynaklar, kullanici, oneriSayisi);

            // ========== SONUÇLARI OLUŞTUR ==========
            var sonuclar = new List<OneriSonucu>();
            int sira = 1;

            // Her kaynak için öneri sonucu oluştur
            foreach (var kaynak in filtrelenmisKaynaklar.Take(oneriSayisi))
            {
                // Öneri skorunu hesapla (0-100)
                var skor = OneriSkoruHesapla(kaynak, kullanici);
                
                // Öneri nedenlerini belirle (kullanıcıya gösterilecek)
                var nedenler = OneriNedenleriniBelirleme(kaynak, kullanici);

                // Öneri sonucu nesnesi oluştur
                sonuclar.Add(new OneriSonucu
                {
                    Kaynak = kaynak,
                    OneriSkoru = skor,
                    Sira = sira++,
                    OneriNedenleri = nedenler
                });
            }

            // Skorlara göre azalan sırada sırala (en yüksek skor en üstte)
            return sonuclar.OrderByDescending(s => s.OneriSkoru).ToList();
        }

        /// <summary>
        /// Belirli bir kaynağa benzer kaynakları bulur
        /// 
        /// Benzerlik Kriterleri (OR mantığı):
        /// - Aynı kategoriye ait kaynaklar
        /// - Aynı yazarın eserleri
        /// 
        /// Filtreleme:
        /// - İlgili kaynak hariç (kendisi dahil edilmez)
        /// - Sadece mevcut kaynaklar (ödünç verilmemiş)
        /// 
        /// Sıralama:
        /// - Okunma sayısına göre azalan sıra (popülerlik)
        /// 
        /// Kullanım:
        /// - "Bu kaynağa benzer öneriler" özelliği
        /// - İlgili içerik önerileri
        /// </summary>
        /// <param name="isbn">Benzeri aranacak kaynağın ISBN numarası</param>
        /// <param name="sayi">Döndürülecek benzer kaynak sayısı (varsayılan: 5)</param>
        /// <returns>Benzer kaynakların listesi (popülerlik sırasına göre)</returns>
        public List<Kaynak> BenzerKaynaklarBul(string isbn, int sayi = 5)
        {
            // İlgili kaynağı getir
            var kaynak = _yonetici.KaynakGetir(isbn);
            
            // Kaynak yoksa boş liste döndür
            if (kaynak == null)
                return new List<Kaynak>();

            // Benzer kaynakları bul
            var benzerKaynaklar = _yonetici.TumKaynaklariGetir()
                .Where(k => k.ISBN != isbn && !k.OduncDurumu) // İlgili kaynak hariç, mevcut olanlar
                .Where(k => 
                    k.Kategori == kaynak.Kategori ||  // Aynı kategori
                    k.Yazar == kaynak.Yazar)          // Aynı yazar
                .OrderByDescending(k => k.OkunmaSayisi) // Popülerliğe göre sırala
                .Take(sayi)                            // İstenen sayıda al
                .ToList();

            return benzerKaynaklar;
        }

        /// <summary>
        /// Trend (popüler) kaynakları getirir
        /// 
        /// Trend Belirleme:
        /// - Okunma sayısına göre sıralama
        /// - En çok okunan kaynaklar trend olarak kabul edilir
        /// 
        /// Not: Şu anki implementasyonda son 30 günlük işlemler
        /// kontrol edilmiyor, tüm zamanların popüler kaynakları getiriliyor.
        /// Gelecek versiyonda zaman bazlı filtreleme eklenebilir.
        /// </summary>
        /// <param name="sayi">Döndürülecek trend kaynak sayısı (varsayılan: 10)</param>
        /// <returns>Trend kaynakların listesi (okunma sayısına göre sıralı)</returns>
        public List<Kaynak> TrendKaynaklarGetir(int sayi = 10)
        {
            // Son 30 gündeki ödünç işlemlerini al (şu an kullanılmıyor ama gelecek için hazır)
            var gecmisIslemler = _yonetici.IslemGecmisiGetir()
                .Where(i => i.IslemTuru == "ODUNC_VERILDI")
                .Where(i => (System.DateTime.Now - i.Tarih).Days <= 30)
                .ToList();

            // Tüm zamanların en popüler kaynaklarını getir
            // Okunma sayısına göre azalan sırada
            var kaynaklar = _yonetici.TumKaynaklariGetir()
                .OrderByDescending(k => k.OkunmaSayisi)
                .Take(sayi)
                .ToList();

            return kaynaklar;
        }

        /// <summary>
        /// Belirli bir kategoriye ait popüler kaynakları getirir
        /// 
        /// Filtreleme:
        /// - Belirtilen kategoriye ait kaynaklar
        /// - Sadece mevcut kaynaklar (ödünç verilmemiş)
        /// 
        /// Sıralama:
        /// - Okunma sayısına göre azalan sıra
        /// 
        /// Kullanım:
        /// - Kategori sayfası önerileri
        /// - Kategori bazlı keşif
        /// </summary>
        /// <param name="kategori">Kategori adı</param>
        /// <param name="sayi">Döndürülecek kaynak sayısı (varsayılan: 10)</param>
        /// <returns>Kategoriye ait popüler kaynakların listesi</returns>
        public List<Kaynak> KategoriyeGoreOneriler(string kategori, int sayi = 10)
        {
            return _yonetici.TumKaynaklariGetir()
                .Where(k => k.Kategori != null && k.Kategori.Equals(kategori, System.StringComparison.OrdinalIgnoreCase))
                .Where(k => !k.OduncDurumu) // Sadece mevcut kaynaklar
                .OrderByDescending(k => k.OkunmaSayisi) // Popülerliğe göre sırala
                .Take(sayi)
                .ToList();
        }

        // ==================== ÖNERİ SKORLAMA METOTLARI ====================

        /// <summary>
        /// Bir kaynak için öneri skorunu hesaplar (0-100 arası)
        /// 
        /// Skorlama Bileşenleri:
        /// 
        /// 1. Kategori Uyumu (0-30 puan):
        ///    - Kullanıcının okuduğu kategorilerle eşleşirse 30 puan
        ///    - En yüksek ağırlıklı kriter
        /// 
        /// 2. İlgi Alanı Uyumu (0-25 puan):
        ///    - Kullanıcının ilgi alanlarıyla eşleşirse 25 puan
        ///    - İkinci en önemli kriter
        /// 
        /// 3. Popülarite (0-20 puan):
        ///    - Okunma sayısına göre hesaplanır
        ///    - Formül: min(OkunmaSayisi / 10, 20)
        ///    - Örnek: 200 okunma = 20 puan, 50 okunma = 5 puan
        /// 
        /// 4. Yazar Tanıdıklığı (0-15 puan):
        ///    - Kullanıcı daha önce bu yazarın eserlerini okumuşsa 15 puan
        ///    - Yazar bazlı öneri için
        /// 
        /// 5. Yenilik (0-10 puan):
        ///    - 1 yaşında: 10 puan
        ///    - 1-3 yaş arası: 5 puan
        ///    - 3 yaşından eski: 0 puan
        /// 
        /// Toplam Maksimum Skor: 100 puan
        /// </summary>
        /// <param name="kaynak">Skorlanacak kaynak</param>
        /// <param name="kullanici">Öneriler üretilecek kullanıcı</param>
        /// <returns>Öneri skoru (0-100 arası double)</returns>
        private double OneriSkoruHesapla(Kaynak kaynak, Kullanici kullanici)
        {
            double skor = 0;

            // ========== 1. KATEGORİ UYUMU (0-30 PUAN) ==========
            // Kullanıcının okuduğu kategorileri al
            var okunanKategoriler = kullanici.OkunanKategoriler();
            
            // Kaynağın kategorisi kullanıcının okuduğu kategorilerden biriyse
            if (kaynak.Kategori != null && okunanKategoriler.Contains(kaynak.Kategori))
                skor += 30; // En yüksek puan

            // ========== 2. İLGİ ALANI UYUMU (0-25 PUAN) ==========
            // Kullanıcının ilgi alanları ile kaynak kategorisi eşleşiyor mu?
            if (kaynak.Kategori != null && kullanici.IlgiAlanlari.Any(ilgi => 
                kaynak.Kategori.Contains(ilgi, System.StringComparison.OrdinalIgnoreCase)))
                skor += 25; // İkinci en yüksek puan

            // ========== 3. POPÜLARİTE (0-20 PUAN) ==========
            // Okunma sayısına göre popülerlik hesapla
            // Formül: OkunmaSayisi / 10 (maksimum 20 puan)
            // Örnek: 200 okunma = 20 puan, 50 okunma = 5 puan, 5 okunma = 0.5 puan
            skor += (kaynak.OkunmaSayisi / 10.0) > 20 ? 20 : (kaynak.OkunmaSayisi / 10.0);

            // ========== 4. YAZAR TANIDIKLIĞI (0-15 PUAN) ==========
            // Kullanıcının daha önce okuduğu kaynakların yazarlarını bul
            var okunanYazarlar = kullanici.OduncGecmisi
                .Select(o => _yonetici.KaynakGetir(o.ISBN)?.Yazar)
                .ToList();
            
            // Bu kaynağın yazarı daha önce okunan yazarlardan biri mi?
            if (okunanYazarlar.Contains(kaynak.Yazar))
                skor += 15; // Yazar bazlı öneri

            // ========== 5. YENİLİK (0-10 PUAN) ==========
            // Kaynağın yaşını hesapla (yıl cinsinden)
            var yasYili = (System.DateTime.Now - kaynak.YayinTarihi).TotalDays / 365;
            
            if (yasYili < 1)
                skor += 10; // Çok yeni (1 yaşından küçük)
            else if (yasYili < 3)
                skor += 5;  // Yeni (1-3 yaş arası)

            // Toplam skor (maksimum 100)
            return skor;
        }

        /// <summary>
        /// Öneri nedenlerini belirler (kullanıcıya gösterilecek açıklamalar)
        /// 
        /// Nedenler:
        /// - Kategori uyumu: "'Klasik Edebiyat' kategorisini okudunuz"
        /// - İlgi alanı: "İlgi alanlarınıza uygun"
        /// - Popülerlik: "Çok popüler" (20+ okunma)
        /// - Yazar: "George Orwell'ın diğer eseri"
        /// - Yenilik: "Yeni yayın" (1 yaşından küçük)
        /// 
        /// Kullanım:
        /// - Frontend'de önerilerin neden gösterildiği bilgisini sunmak için
        /// - Şeffaflık: Kullanıcı neden bu öneriyi aldığını anlar
        /// - Güven: Açıklanabilir öneriler kullanıcı güvenini artırır
        /// </summary>
        /// <param name="kaynak">Kaynak nesnesi</param>
        /// <param name="kullanici">Kullanıcı nesnesi</param>
        /// <returns>Öneri nedenlerinin listesi</returns>
        private List<string> OneriNedenleriniBelirleme(Kaynak kaynak, Kullanici kullanici)
        {
            var nedenler = new List<string>();

            // ========== KATEGORİ NEDENİ ==========
            var okunanKategoriler = kullanici.OkunanKategoriler();
            if (kaynak.Kategori != null && okunanKategoriler.Contains(kaynak.Kategori))
                nedenler.Add($"'{kaynak.Kategori}' kategorisini okudunuz");

            // ========== İLGİ ALANI NEDENİ ==========
            if (kaynak.Kategori != null && kullanici.IlgiAlanlari.Any(ilgi => 
                kaynak.Kategori.Contains(ilgi, System.StringComparison.OrdinalIgnoreCase)))
                nedenler.Add("İlgi alanlarınıza uygun");

            // ========== POPÜLARİTE NEDENİ ==========
            if (kaynak.OkunmaSayisi > 20)
                nedenler.Add("Çok popüler");

            // ========== YAZAR NEDENİ ==========
            // Kullanıcının daha önce okuduğu yazarları bul
            var okunanYazarlar = kullanici.OduncGecmisi
                .Select(o => _yonetici.KaynakGetir(o.ISBN)?.Yazar)
                .ToList();
            
            // Bu kaynağın yazarı daha önce okunmuş mu?
            if (okunanYazarlar.Contains(kaynak.Yazar))
                nedenler.Add($"{kaynak.Yazar}'ın diğer eseri");

            // ========== YENİLİK NEDENİ ==========
            var yasYili = (System.DateTime.Now - kaynak.YayinTarihi).TotalDays / 365;
            if (yasYili < 1)
                nedenler.Add("Yeni yayın");

            return nedenler;
        }
    }

    // ==================== ÖNERİ SONUÇ DTO ====================

    /// <summary>
    /// Öneri Sonucu DTO (Data Transfer Object)
    /// 
    /// Öneri sisteminden dönen sonuçları taşıyan sınıf.
    /// Bu sınıf, önerilen kaynak ve öneri bilgilerini içerir.
    /// 
    /// Kullanım:
    /// - API response'ları için
    /// - Frontend'e öneri bilgilerini aktarmak için
    /// </summary>
    public class OneriSonucu
    {
        /// <summary>
        /// Önerilen kaynak nesnesi
        /// Polimorfik olarak Kitap, Dergi veya Tez olabilir
        /// </summary>
        public Kaynak Kaynak { get; set; } = null!;

        /// <summary>
        /// Öneri skoru (0-100 arası)
        /// 
        /// Skor Anlamı:
        /// - 80-100: Çok yüksek uyumluluk
        /// - 60-79: Yüksek uyumluluk
        /// - 40-59: Orta uyumluluk
        /// - 0-39: Düşük uyumluluk
        /// 
        /// Sıralama: Skora göre azalan sırada listelenir
        /// </summary>
        public double OneriSkoru { get; set; }

        /// <summary>
        /// Önerinin sıra numarası (1'den başlar)
        /// 
        /// Kullanım:
        /// - UI'da gösterim sırası
        /// - "1. Öneri", "2. Öneri" gibi etiketler için
        /// </summary>
        public int Sira { get; set; }

        /// <summary>
        /// Önerinin nedenlerinin listesi
        /// 
        /// Örnek nedenler:
        /// - "'Klasik Edebiyat' kategorisini okudunuz"
        /// - "İlgi alanlarınıza uygun"
        /// - "Çok popüler"
        /// - "George Orwell'ın diğer eseri"
        /// - "Yeni yayın"
        /// 
        /// Kullanım:
        /// - Frontend'de öneri kartlarında gösterilir
        /// - Kullanıcıya önerinin gerekçesini açıklar
        /// </summary>
        public List<string> OneriNedenleri { get; set; } = new List<string>();
    }
}
