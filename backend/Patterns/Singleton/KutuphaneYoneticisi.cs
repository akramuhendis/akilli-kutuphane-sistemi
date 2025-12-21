using System;
using System.Collections.Generic;
using System.Linq;
using SmartLibrary.Models;
using SmartLibrary.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace SmartLibrary.Patterns.Singleton
{
    /// <summary>
    /// Kütüphane Yöneticisi - Singleton Pattern Implementasyonu
    /// 
    /// Bu sınıf, Singleton Design Pattern kullanarak sistem genelinde tek bir
    /// kütüphane yöneticisi instance'ı sağlar.
    /// 
    /// Singleton Pattern Nedenleri:
    /// - Veri Tutarlılığı: Tüm sistem aynı veri kaynağına erişir
    /// - Memory Efficiency: Tek instance, düşük bellek kullanımı
    /// - Global Erişim: Her yerden aynı instance'a erişilebilir
    /// - Thread Safety: Double-check locking ile thread-safe erişim
    /// 
    /// Özellikler:
    /// - Kaynak yönetimi (CRUD işlemleri)
    /// - Kullanıcı yönetimi (CRUD işlemleri)
    /// - Ödünç/iade işlemleri
    /// - İstatistik hesaplamaları
    /// - Database senkronizasyonu
    /// - İşlem kayıt sistemi (logging)
    /// 
    /// Veri Yapıları:
    /// - Dictionary<string, Kaynak>: ISBN bazlı hızlı erişim (O(1))
    /// - Dictionary<string, Kullanici>: ID bazlı hızlı erişim (O(1))
    /// - List<IslemKaydi>: İşlem geçmişi kayıtları
    /// </summary>
    public sealed class KutuphaneYoneticisi
    {
        // ==================== SINGLETON PATTERN ALANLARI ====================

        /// <summary>
        /// Singleton instance - lazy initialization ile oluşturulur
        /// Static: Tüm sınıf için tek instance
        /// Nullable: İlk çağrıda null olacak
        /// </summary>
        private static KutuphaneYoneticisi? _instance = null;

        /// <summary>
        /// Thread safety için lock objesi
        /// Readonly: Sadece bir kez atanabilir
        /// Multi-threaded ortamlarda güvenli erişim sağlar
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// Entity Framework Core için service provider
        /// Dependency injection container'dan alınır
        /// Her istek için scoped DbContext oluşturmak için kullanılır
        /// </summary>
        private static IServiceProvider? _serviceProvider = null;

        // ==================== VERİ YAPILARI ====================

        /// <summary>
        /// Kaynakların saklandığı dictionary
        /// Key: ISBN numarası (string)
        /// Value: Kaynak nesnesi (Kitap, Dergi veya Tez)
        /// O(1) erişim süresi sağlar
        /// </summary>
        private Dictionary<string, Kaynak> _kaynaklar;

        /// <summary>
        /// Kullanıcıların saklandığı dictionary
        /// Key: Kullanıcı ID'si (string - GUID)
        /// Value: Kullanıcı nesnesi
        /// O(1) erişim süresi sağlar
        /// </summary>
        private Dictionary<string, Kullanici> _kullanicilar;

        /// <summary>
        /// Tüm işlemlerin log kayıtları
        /// Sistemdeki her işlem (ekleme, silme, ödünç verme, vb.) buraya kaydedilir
        /// İstatistik hesaplamaları için kullanılır
        /// </summary>
        private List<IslemKaydi> _islemGecmisi;

        // ==================== CONSTRUCTOR (PRIVATE) ====================

        /// <summary>
        /// Private constructor - Singleton pattern'in temel özelliği
        /// 
        /// Dışarıdan doğrudan instance oluşturulmasını engeller
        /// Sadece Instance property üzerinden erişim sağlanır
        /// </summary>
        private KutuphaneYoneticisi()
        {
            // Veri yapılarını initialize et
            _kaynaklar = new Dictionary<string, Kaynak>();
            _kullanicilar = new Dictionary<string, Kullanici>();
            _islemGecmisi = new List<IslemKaydi>();
            
            Console.WriteLine("Kütüphane Yöneticisi başlatıldı.");
        }

        // ==================== SERVİS SAĞLAYICI YÖNETİMİ ====================

        /// <summary>
        /// Entity Framework DbContext'i ayarlar (Eski metod)
        /// Geriye dönük uyumluluk için korunmuştur
        /// Artık kullanılmamaktadır - SetServiceProvider kullanılmalı
        /// </summary>
        /// <param name="dbContext">DbContext instance'ı (kullanılmıyor)</param>
        public static void SetDbContext(SmartLibraryDbContext dbContext)
        {
            // Eski metod - geriye dönük uyumluluk için boş bırakıldı
            // Artık SetServiceProvider kullanılıyor
        }

        /// <summary>
        /// Service Provider'ı ayarlar
        /// Dependency injection container'dan alınan service provider
        /// Her istek için scoped DbContext oluşturmak için kullanılır
        /// 
        /// Program.cs'de uygulama başlangıcında çağrılmalıdır
        /// </summary>
        /// <param name="serviceProvider">Dependency injection service provider</param>
        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Yeni bir service scope oluşturur
        /// 
        /// Önemli:
        /// - Entity Framework DbContext scoped bir service'tir
        /// - Her istek için yeni scope oluşturulmalıdır
        /// - Scope dispose edildiğinde DbContext de dispose edilir
        /// 
        /// Exception: ServiceProvider ayarlanmamışsa hata fırlatır
        /// </summary>
        /// <returns>Yeni service scope instance'ı</returns>
        /// <exception cref="InvalidOperationException">ServiceProvider ayarlanmamışsa</exception>
        private IServiceScope CreateScope()
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("ServiceProvider ayarlanmamış. Program.cs'de SetServiceProvider çağrılmalı.");

            // Her istek için yeni bir scoped DbContext oluştur
            // using statement ile otomatik dispose edilir
            return _serviceProvider.CreateScope();
        }

        // ==================== SINGLETON INSTANCE PROPERTY ====================

        /// <summary>
        /// Singleton instance'a erişim property'si
        /// 
        /// Thread-Safe Double-Check Locking Pattern:
        /// 1. İlk null kontrolü (performans - lock'tan kaçınmak için)
        /// 2. Lock mekanizması (thread safety)
        /// 3. İkinci null kontrolü (ilk kontrol ile lock arasında oluşturulmuş olabilir)
        /// 4. Instance oluşturma
        /// 
        /// Avantajları:
        /// - Thread-safe: Multi-threaded ortamlarda güvenli
        /// - Lazy initialization: Sadece gerektiğinde oluşturulur
        /// - Performanslı: Lock sadece ilk oluşturmada kullanılır
        /// </summary>
        public static KutuphaneYoneticisi Instance
        {
            get
            {
                // İlk null kontrolü (lock'tan önce - performans optimizasyonu)
                if (_instance == null)
                {
                    // Thread safety için lock
                    lock (_lock)
                    {
                        // İkinci null kontrolü (double-check)
                        // İlk kontrol ile lock arasında başka thread oluşturmuş olabilir
                        if (_instance == null)
                        {
                            _instance = new KutuphaneYoneticisi();
                        }
                    }
                }
                return _instance;
            }
        }

        // ==================== KAYNAK YÖNETİMİ ====================

        /// <summary>
        /// Yeni bir kaynak ekler
        /// 
        /// İşlem Adımları:
        /// 1. ISBN kontrolü (duplicate kontrolü)
        /// 2. Memory'e ekleme (Dictionary)
        /// 3. Database'e kaydetme (varsa)
        /// 4. İşlem kaydı oluşturma
        /// 
        /// Veri Tutarlılığı:
        /// - Hem memory hem database senkronize tutulur
        /// - Database hatası durumunda memory'de kalır (in-memory mode)
        /// </summary>
        /// <param name="kaynak">Eklenecek kaynak nesnesi</param>
        public void KaynakEkle(Kaynak kaynak)
        {
            // Duplicate kontrolü - aynı ISBN'li kaynak varsa ekleme
            if (!_kaynaklar.ContainsKey(kaynak.ISBN))
            {
                // Memory'e ekle (Dictionary - O(1) insertion)
                _kaynaklar.Add(kaynak.ISBN, kaynak);
                
                // Database'e de ekle (eğer service provider varsa)
                if (_serviceProvider != null)
                {
                    // Scoped DbContext oluştur
                    using var scope = CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<SmartLibraryDbContext>();
                    
                    // Kaynağı database'e ekle
                    dbContext.Kaynaklar.Add(kaynak);
                    dbContext.SaveChanges();
                }
                
                // İşlem kaydı oluştur (audit log)
                IslemKaydet("KAYNAK_EKLENDI", $"{kaynak.Baslik} eklendi");
            }
        }

        /// <summary>
        /// ISBN numarası ile kaynak getirir
        /// 
        /// Performans:
        /// - Dictionary lookup: O(1) zaman karmaşıklığı
        /// - Çok hızlı erişim
        /// </summary>
        /// <param name="isbn">Aranacak kaynağın ISBN numarası</param>
        /// <returns>Kaynak nesnesi veya null (bulunamazsa)</returns>
        public Kaynak? KaynakGetir(string isbn)
        {
            // Dictionary'den O(1) erişim
            return _kaynaklar.ContainsKey(isbn) ? _kaynaklar[isbn] : null;
        }

        /// <summary>
        /// Sistemdeki tüm kaynakları listeler
        /// 
        /// Kullanım:
        /// - Tüm kaynakları görüntüleme
        /// - İstatistik hesaplamaları
        /// - Öneri sistemi için kaynak listesi
        /// </summary>
        /// <returns>Tüm kaynakların listesi</returns>
        public List<Kaynak> TumKaynaklariGetir()
        {
            // Dictionary values'ları listeye çevir
            return _kaynaklar.Values.ToList();
        }

        /// <summary>
        /// Bir kaynağı sistemden siler
        /// 
        /// İşlem Adımları:
        /// 1. Kaynağın var olup olmadığını kontrol et
        /// 2. Memory'den sil (Dictionary)
        /// 3. Database'den sil (varsa)
        /// 4. İşlem kaydı oluştur
        /// 
        /// Not: Ödünç verilmiş kaynakların silinmesi kontrol edilmiyor
        /// (İş kuralı olarak eklenebilir)
        /// </summary>
        /// <param name="isbn">Silinecek kaynağın ISBN numarası</param>
        public void KaynakSil(string isbn)
        {
            // Kaynak var mı kontrol et
            if (_kaynaklar.ContainsKey(isbn))
            {
                // Silinmeden önce bilgileri sakla (log için)
                var kaynak = _kaynaklar[isbn];
                
                // Memory'den sil
                _kaynaklar.Remove(isbn);
                
                // Database'den de sil
                if (_serviceProvider != null)
                {
                    using var scope = CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<SmartLibraryDbContext>();
                    
                    // Database'de kaynağı bul ve sil
                    var dbKaynak = dbContext.Kaynaklar.Find(isbn);
                    if (dbKaynak != null)
                    {
                        dbContext.Kaynaklar.Remove(dbKaynak);
                        dbContext.SaveChanges();
                    }
                }
                
                // İşlem kaydı oluştur
                IslemKaydet("KAYNAK_SILINDI", $"{kaynak.Baslik} silindi");
            }
        }

        // ==================== KULLANICI YÖNETİMİ ====================

        /// <summary>
        /// Yeni bir kullanıcı ekler
        /// 
        /// İşlem Adımları:
        /// 1. ID kontrolü (duplicate kontrolü)
        /// 2. Memory'e ekleme (Dictionary)
        /// 3. Database'e kaydetme (varsa)
        /// 4. İşlem kaydı oluşturma
        /// </summary>
        /// <param name="kullanici">Eklenecek kullanıcı nesnesi</param>
        public void KullaniciEkle(Kullanici kullanici)
        {
            // Duplicate kontrolü
            if (!_kullanicilar.ContainsKey(kullanici.Id))
            {
                // Memory'e ekle
                _kullanicilar.Add(kullanici.Id, kullanici);
                
                // Database'e de ekle
                if (_serviceProvider != null)
                {
                    using var scope = CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<SmartLibraryDbContext>();
                    dbContext.Kullanicilar.Add(kullanici);
                    dbContext.SaveChanges();
                }
                
                // İşlem kaydı oluştur
                IslemKaydet("KULLANICI_EKLENDI", $"{kullanici.Ad} {kullanici.Soyad} eklendi");
            }
        }

        /// <summary>
        /// ID ile kullanıcı getirir
        /// 
        /// Performans: O(1) Dictionary lookup
        /// </summary>
        /// <param name="id">Kullanıcı ID'si</param>
        /// <returns>Kullanıcı nesnesi veya null</returns>
        public Kullanici? KullaniciGetir(string id)
        {
            return _kullanicilar.ContainsKey(id) ? _kullanicilar[id] : null;
        }

        /// <summary>
        /// Tüm kullanıcıları listeler
        /// </summary>
        /// <returns>Kullanıcı listesi</returns>
        public List<Kullanici> TumKullanicilariGetir()
        {
            return _kullanicilar.Values.ToList();
        }

        /// <summary>
        /// Mevcut bir kullanıcıyı günceller
        /// 
        /// Önemli: Ödünç geçmişi ve aktif ödünçler korunur
        /// Sadece profil bilgileri güncellenir
        /// 
        /// İşlem Adımları:
        /// 1. Mevcut kullanıcıyı bul
        /// 2. Önemli verileri koru (ödünç geçmişi, aktif ödünçler, kayıt tarihi)
        /// 3. Yeni bilgileri ata
        /// 4. Memory ve database'i güncelle
        /// </summary>
        /// <param name="id">Güncellenecek kullanıcı ID'si</param>
        /// <param name="guncellenmisKullanici">Yeni kullanıcı bilgileri</param>
        public void KullaniciGuncelle(string id, Kullanici guncellenmisKullanici)
        {
            if (_kullanicilar.ContainsKey(id))
            {
                // Mevcut kullanıcıyı al
                var mevcutKullanici = _kullanicilar[id];
                
                // Önemli verileri koru (ödünç geçmişi, aktif ödünçler, kayıt tarihi)
                guncellenmisKullanici.Id = id;  // ID değişmez
                guncellenmisKullanici.OduncGecmisi = mevcutKullanici.OduncGecmisi;
                guncellenmisKullanici.AktifOduncler = mevcutKullanici.AktifOduncler;
                guncellenmisKullanici.KayitTarihi = mevcutKullanici.KayitTarihi;

                // Memory'de güncelle
                _kullanicilar[id] = guncellenmisKullanici;
                
                // Database'i de güncelle
                if (_serviceProvider != null)
                {
                    using var scope = CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<SmartLibraryDbContext>();
                    
                    var dbKullanici = dbContext.Kullanicilar.Find(id);
                    if (dbKullanici != null)
                    {
                        // Sadece profil bilgilerini güncelle
                        dbKullanici.Ad = guncellenmisKullanici.Ad;
                        dbKullanici.Soyad = guncellenmisKullanici.Soyad;
                        dbKullanici.Email = guncellenmisKullanici.Email;
                        dbKullanici.Yas = guncellenmisKullanici.Yas;
                        dbKullanici.IlgiAlanlari = guncellenmisKullanici.IlgiAlanlari;
                        dbKullanici.FavoriKategoriler = guncellenmisKullanici.FavoriKategoriler;
                        dbContext.SaveChanges();
                    }
                }
                
                // İşlem kaydı
                IslemKaydet("KULLANICI_GUNCELLENDI", $"{guncellenmisKullanici.Ad} {guncellenmisKullanici.Soyad} güncellendi");
            }
        }

        /// <summary>
        /// Bir kullanıcıyı sistemden siler
        /// 
        /// Güvenlik Kontrolü:
        /// - Aktif ödünçleri olan kullanıcı silinemez
        /// - Önce tüm ödünçler iade edilmelidir
        /// 
        /// İşlem Adımları:
        /// 1. Kullanıcıyı bul
        /// 2. Aktif ödünç kontrolü yap
        /// 3. Memory'den sil
        /// 4. Database'den sil
        /// 5. İşlem kaydı oluştur
        /// </summary>
        /// <param name="id">Silinecek kullanıcı ID'si</param>
        /// <exception cref="InvalidOperationException">Aktif ödünç varsa hata fırlatır</exception>
        public void KullaniciSil(string id)
        {
            if (_kullanicilar.ContainsKey(id))
            {
                var kullanici = _kullanicilar[id];
                
                // Güvenlik kontrolü: Aktif ödünçleri kontrol et
                if (kullanici.AktifOduncler != null && kullanici.AktifOduncler.Count > 0)
                {
                    // Aktif ödünç varsa silme işlemi yapılamaz
                    throw new InvalidOperationException($"Kullanıcının {kullanici.AktifOduncler.Count} aktif ödüncü var. Önce iade alınmalı.");
                }

                // Memory'den sil
                _kullanicilar.Remove(id);
                
                // Database'den de sil
                if (_serviceProvider != null)
                {
                    using var scope = CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<SmartLibraryDbContext>();
                    
                    var dbKullanici = dbContext.Kullanicilar.Find(id);
                    if (dbKullanici != null)
                    {
                        dbContext.Kullanicilar.Remove(dbKullanici);
                        dbContext.SaveChanges();
                    }
                }
                
                // İşlem kaydı
                IslemKaydet("KULLANICI_SILINDI", $"{kullanici.Ad} {kullanici.Soyad} silindi");
            }
        }

        // ==================== ÖDÜNÇ İŞLEMLERİ ====================

        /// <summary>
        /// Bir kaynağı kullanıcıya ödünç verir
        /// 
        /// İşlem Kuralları:
        /// - Kullanıcı sistemde kayıtlı olmalı
        /// - Kaynak mevcut olmalı (ödünç verilmemiş)
        /// - Kaynak durumu güncellenir
        /// - Ödünç kaydı oluşturulur
        /// - İşlem log'a kaydedilir
        /// 
        /// Polimorfizm:
        /// - kaynak.OduncVer() metodu kaynak türüne göre farklı çalışır
        /// - kaynak.TeslimSuresi() metodu kaynak türüne göre farklı değer döner
        /// </summary>
        /// <param name="kullaniciId">Ödünç alacak kullanıcı ID'si</param>
        /// <param name="isbn">Ödünç verilecek kaynağın ISBN numarası</param>
        /// <returns>true: Başarılı, false: Başarısız (kullanıcı/kaynak yok veya kaynak zaten ödünçte)</returns>
        public bool OduncVer(string kullaniciId, string isbn)
        {
            // Kullanıcı ve kaynağı getir
            var kullanici = KullaniciGetir(kullaniciId);
            var kaynak = KaynakGetir(isbn);

            // Validasyon: Kullanıcı var mı? Kaynak var mı? Kaynak mevcut mu?
            if (kullanici == null || kaynak == null || kaynak.OduncDurumu)
                return false;

            // Polimorfik metot çağrısı - kaynak türüne göre farklı çalışır
            kaynak.OduncVer();  // OduncDurumu = true, OkunmaSayisi++
            
            // Ödünç kaydı oluştur (denormalizasyon ile başlık ve kategori de saklanır)
            var kayit = new OduncKaydi
            {
                ISBN = isbn,
                KaynakBaslik = kaynak.Baslik,  // Denormalizasyon
                Kategori = kaynak.Kategori,     // Denormalizasyon
                OduncTarihi = DateTime.Now,
                // Polimorfik metot - her kaynak türü kendi süresini döndürür
                TeslimSuresi = kaynak.TeslimSuresi()  // Kitap: 14, Dergi: 7, Tez: 21
            };
            
            // Kullanıcının aktif ödünçlerine ekle
            kullanici.OduncEkle(kayit);

            // İşlem kaydı
            IslemKaydet("ODUNC_VERILDI", $"{kullanici.Ad} {kullanici.Soyad} - {kaynak.Baslik}");
            
            return true;
        }

        /// <summary>
        /// Bir kullanıcının ödünç aldığı kaynağı iade alır
        /// 
        /// İşlem Adımları:
        /// 1. Kullanıcı ve kaynak kontrolü
        /// 2. Kaynak durumunu güncelle (OduncDurumu = false)
        /// 3. Kullanıcının aktif ödünçlerinden çıkar
        /// 4. Kullanıcının ödünç geçmişine ekle
        /// 5. İşlem kaydı oluştur
        /// 
        /// Not: Gecikme kontrolü yapılmaz (sadece iade alınır)
        /// </summary>
        /// <param name="kullaniciId">İade yapacak kullanıcı ID'si</param>
        /// <param name="isbn">İade edilecek kaynağın ISBN numarası</param>
        /// <returns>true: Başarılı, false: Başarısız</returns>
        public bool IadeAl(string kullaniciId, string isbn)
        {
            // Kullanıcı ve kaynağı getir
            var kullanici = KullaniciGetir(kullaniciId);
            var kaynak = KaynakGetir(isbn);

            // Validasyon: Kullanıcı var mı? Kaynak var mı? Kaynak ödünçte mi?
            if (kullanici == null || kaynak == null || !kaynak.OduncDurumu)
                return false;

            // Polimorfik metot - kaynak durumunu güncelle
            kaynak.IadeAl();  // OduncDurumu = false, OduncTarihi = null
            
            // Kullanıcının aktif ödünçlerinden çıkar, geçmişe ekle
            kullanici.IadeYap(isbn);

            // İşlem kaydı
            IslemKaydet("IADE_ALINDI", $"{kullanici.Ad} {kullanici.Soyad} - {kaynak.Baslik}");
            
            return true;
        }

        // ==================== İSTATİSTİKLER ====================

        /// <summary>
        /// En popüler 10 kaynağı getirir
        /// 
        /// Popülerlik Kriteri:
        /// - Okunma sayısına göre sıralama
        /// - En çok okunan kaynaklar en üstte
        /// 
        /// Kullanım:
        /// - Dashboard istatistikleri
        /// - Popüler kaynaklar listesi
        /// - Trend analizi
        /// </summary>
        /// <returns>Okunma sayısına göre sıralanmış en popüler 10 kaynak</returns>
        public List<Kaynak> EnPopuler10Kaynak()
        {
            return _kaynaklar.Values
                .OrderByDescending(k => k.OkunmaSayisi)  // Okunma sayısına göre azalan sıra
                .Take(10)                                 // İlk 10 kayıt
                .ToList();
        }

        /// <summary>
        /// Sistemdeki tüm gecikmeli ödünçleri tespit eder ve listeler
        /// 
        /// Tespit Mantığı:
        /// 1. Tüm kullanıcıların aktif ödünçleri taranır
        /// 2. Her ödünç kaydı için GeciktiMi() metodu çağrılır
        /// 3. Gecikmiş olanlar için ceza hesaplanır (polimorfik)
        /// 4. Gecikme uyarısı oluşturulur
        /// 
        /// Polimorfizm:
        /// - kaynak.CezaHesapla() metodu kaynak türüne göre farklı ceza hesaplar
        /// - Kitap: 2 TL/gün, Dergi: 1 TL/gün, Tez: 3 TL/gün
        /// </summary>
        /// <returns>Gecikme uyarılarının listesi (Kullanıcı adı, Kaynak, Gecikme günü, Ceza)</returns>
        public List<GecikmeUyarisi> GecikmeUyarilariGetir()
        {
            var uyarilar = new List<GecikmeUyarisi>();

            // Tüm kullanıcıları tara
            foreach (var kullanici in _kullanicilar.Values)
            {
                // Her kullanıcının aktif ödünçlerini kontrol et
                foreach (var odunc in kullanici.AktifOduncler)
                {
                    // Ödünç kaydı gecikmiş mi kontrol et
                    if (odunc.GeciktiMi())
                    {
                        // Kaynağı getir (ceza hesaplama için gerekli)
                        var kaynak = KaynakGetir(odunc.ISBN);
                        
                        // Gecikme uyarısı oluştur
                        uyarilar.Add(new GecikmeUyarisi
                        {
                            KullaniciAd = $"{kullanici.Ad} {kullanici.Soyad}",
                            KaynakBaslik = odunc.KaynakBaslik ?? "Bilinmeyen Kaynak",
                            GecikmeGunSayisi = odunc.GecikmeGunSayisi(),
                            // Polimorfik ceza hesaplama - kaynak türüne göre farklı
                            Ceza = kaynak?.CezaHesapla(odunc.GecikmeGunSayisi()) ?? 0
                        });
                    }
                }
            }

            return uyarilar;
        }

        /// <summary>
        /// Belirtilen tarihe ait günlük istatistikleri hesaplar
        /// 
        /// İstatistik Türleri:
        /// - KAYNAK_EKLENDI: Eklenen kaynak sayısı
        /// - KAYNAK_SILINDI: Silinen kaynak sayısı
        /// - KULLANICI_EKLENDI: Eklenen kullanıcı sayısı
        /// - ODUNC_VERILDI: Ödünç verilen kaynak sayısı
        /// - IADE_ALINDI: İade alınan kaynak sayısı
        /// 
        /// Hesaplama:
        /// - Belirtilen tarihe ait işlemler filtrelenir
        /// - İşlem türüne göre gruplandırılır
        /// - Her türün sayısı hesaplanır
        /// </summary>
        /// <param name="tarih">İstatistiklerin hesaplanacağı tarih</param>
        /// <returns>İşlem türü ve sayısı dictionary'si</returns>
        public Dictionary<string, int> GunlukIstatistikler(DateTime tarih)
        {
            // Belirtilen tarihe ait işlemleri filtrele ve grupla
            var gunlukIslemler = _islemGecmisi
                .Where(i => i.Tarih.Date == tarih.Date && !string.IsNullOrEmpty(i.IslemTuru))
                .GroupBy(i => i.IslemTuru!)
                .ToDictionary(g => g.Key, g => g.Count());

            return gunlukIslemler;
        }

        // ==================== İŞLEM KAYIT SİSTEMİ ====================

        /// <summary>
        /// Bir işlemi log sistemine kaydeder
        /// 
        /// İşlem Türleri:
        /// - KAYNAK_EKLENDI, KAYNAK_SILINDI
        /// - KULLANICI_EKLENDI, KULLANICI_GUNCELLENDI, KULLANICI_SILINDI
        /// - ODUNC_VERILDI, IADE_ALINDI
        /// 
        /// Kayıt Yerleri:
        /// - Memory: _islemGecmisi listesi (hızlı erişim)
        /// - Database: IslemKayitlari tablosu (kalıcı saklama)
        /// 
        /// Kullanım:
        /// - Audit log (denetim kaydı)
        /// - İstatistik hesaplamaları
        /// - Hata ayıklama (debugging)
        /// </summary>
        /// <param name="islemTuru">İşlem türü</param>
        /// <param name="aciklama">İşlem açıklaması</param>
        private void IslemKaydet(string islemTuru, string aciklama)
        {
            // İşlem kaydı oluştur
            var kayit = new IslemKaydi
            {
                Id = Guid.NewGuid().ToString(),
                IslemTuru = islemTuru,
                Aciklama = aciklama,
                Tarih = DateTime.Now
            };
            
            // Memory'e ekle
            _islemGecmisi.Add(kayit);
            
            // Database'e de kaydet (varsa)
            if (_serviceProvider != null)
            {
                using var scope = CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SmartLibraryDbContext>();
                dbContext.IslemKayitlari.Add(kayit);
                dbContext.SaveChanges();
            }
        }

        // ==================== DATABASE YÜKLEME ====================

        /// <summary>
        /// Uygulama başlangıcında database'den verileri yükler
        /// 
        /// Yüklenen Veriler:
        /// - Tüm kaynaklar (Kitap, Dergi, Tez)
        /// - Tüm kullanıcılar
        /// - İşlem geçmişi
        /// 
        /// Önemli:
        /// - OduncGecmisi ve AktifOduncler runtime'da doldurulur
        /// - Database'den sadece temel veriler yüklenir
        /// - Duplicate kontrolü yapılır (memory'de varsa tekrar eklenmez)
        /// 
        /// Kullanım:
        /// - Program.cs'de uygulama başlangıcında çağrılır
        /// - Memory ve database senkronizasyonu için
        /// </summary>
        public void LoadFromDatabase()
        {
            // ServiceProvider yoksa database yükleme yapılamaz
            if (_serviceProvider == null) return;

            using var scope = CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<SmartLibraryDbContext>();

            // ========== KAYNAKLARI YÜKLE ==========
            var kaynaklar = dbContext.Kaynaklar.ToList();
            foreach (var kaynak in kaynaklar)
            {
                // Duplicate kontrolü - memory'de yoksa ekle
                if (!_kaynaklar.ContainsKey(kaynak.ISBN))
                {
                    _kaynaklar.Add(kaynak.ISBN, kaynak);
                }
            }

            // ========== KULLANICILARI YÜKLE ==========
            // Not: OduncGecmisi ve AktifOduncler runtime'da doldurulacak
            // Database'den sadece kullanıcı profilleri yüklenir
            var kullanicilar = dbContext.Kullanicilar.ToList();
            foreach (var kullanici in kullanicilar)
            {
                // Duplicate kontrolü
                if (!_kullanicilar.ContainsKey(kullanici.Id))
                {
                    _kullanicilar.Add(kullanici.Id, kullanici);
                }
            }

            // ========== İŞLEM GEÇMİŞİNİ YÜKLE ==========
            _islemGecmisi = dbContext.IslemKayitlari.ToList();
            
            Console.WriteLine($"✅ Database'den yüklendi: {_kaynaklar.Count} kaynak, {_kullanicilar.Count} kullanıcı");
        }

        /// <summary>
        /// İşlem geçmişini getirir
        /// 
        /// Sıralama:
        /// - Tarihe göre azalan sırada (en yeni önce)
        /// 
        /// Kullanım:
        /// - İstatistik raporları
        /// - İşlem log görüntüleme
        /// - Audit trail (denetim izi)
        /// </summary>
        /// <returns>Tarihe göre sıralanmış işlem kayıtları</returns>
        public List<IslemKaydi> IslemGecmisiGetir()
        {
            return _islemGecmisi.OrderByDescending(i => i.Tarih).ToList();
        }
    }
}
