using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SmartLibrary.Patterns.Singleton;
using SmartLibrary.Models;
using SmartLibrary.Data;
using System;
using System.Data;
using System.Linq;
using System.Text.Json.Serialization;

// ==================== UYGULAMA BAŞLATMA ====================

/// <summary>
/// Program.cs - ASP.NET Core 8.0 Minimal API Yapılandırması
/// 
/// Bu dosya, akıllı kütüphane yönetim sisteminin giriş noktasıdır.
/// Top-level statements kullanılarak yazılmıştır (C# 10+ özelliği).
/// 
/// İşlevleri:
/// 1. Dependency Injection (DI) Container Yapılandırması
///    - DbContext (Entity Framework Core)
///    - Controllers (API endpoints)
///    - Swagger (API dokümantasyonu)
///    - CORS (Cross-Origin Resource Sharing)
/// 
/// 2. Database Migration ve Initialization
///    - Pending migration kontrolü
///    - Tablo varlık kontrolü
///    - Database oluşturma (gerekirse)
///    - Migration history senkronizasyonu
/// 
/// 3. Singleton Service Provider Setup
///    - KutuphaneYoneticisi için DbContext erişimi
/// 
/// 4. Middleware Pipeline Yapılandırması
///    - Swagger (Development ortamında)
///    - CORS middleware
///    - Authorization
///    - Controllers mapping
/// 
/// 5. Örnek Veri Initialization
///    - Database boşsa örnek kaynaklar, kullanıcılar ve ödünç işlemleri
/// </summary>

// ==================== WEB APPLICATION BUILDER ====================

/// <summary>
/// WebApplicationBuilder oluşturulur
/// 
/// Bu builder, uygulama yapılandırmasını ve servis kayıtlarını yönetir.
/// appsettings.json'dan yapılandırma değerlerini okur.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

// ==================== DEPENDENCY INJECTION YAPILANDIRMASI ====================

/// <summary>
/// Entity Framework Core DbContext Kaydı
/// 
/// SmartLibraryDbContext, SQL Server veritabanına bağlanmak için yapılandırılır.
/// Connection string appsettings.json'dan alınır ("DefaultConnection").
/// 
/// Önemli:
/// - Scoped lifetime: Her HTTP request için yeni DbContext instance'ı oluşturulur
/// - Connection string'in SQL Server'a işaret ettiğinden emin olun
/// </summary>
builder.Services.AddDbContext<SmartLibraryDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

/// <summary>
/// API Controllers Servis Kaydı
/// 
/// Controllers eklenir ve JSON serialization ayarları yapılandırılır:
/// 
/// 1. ReferenceHandler.IgnoreCycles:
///    - Circular reference hatalarını önler
///    - Örnek: Kullanıcı → OduncKaydi → Kullanıcı döngüsü
///    - Çözüm: Döngüye girildiğinde referanslar ignore edilir
/// 
/// 2. JsonIgnoreCondition.WhenWritingNull:
///    - Null değerler JSON'a yazılmaz
///    - Response boyutunu küçültür
///    - Örnek: Null listeler "null" yerine hiç gösterilmez
/// </summary>
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Circular reference hatalarını önle (ör: Kullanıcı → OduncKaydi → Kullanıcı)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        
        // Null değerleri JSON'a yazma (response boyutunu küçültür)
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

/// <summary>
/// API Endpoints Explorer Servis Kaydı
/// 
/// Swagger için gerekli servis. Endpoint'lerin keşfedilmesini sağlar.
/// </summary>
builder.Services.AddEndpointsApiExplorer();

/// <summary>
/// Swagger Generator Servis Kaydı
/// 
/// API dokümantasyonu için Swagger/OpenAPI generator eklenir.
/// Development ortamında Swagger UI erişilebilir olacak.
/// </summary>
builder.Services.AddSwaggerGen();

/// <summary>
/// CORS (Cross-Origin Resource Sharing) Yapılandırması
/// 
/// Frontend uygulamasının (React) backend API'ye istek yapabilmesi için gerekli.
/// 
/// Yapılandırma:
/// - Allowed Origins: 
///   * http://localhost:3000 (React default port)
///   * http://localhost:5173 (Vite default port)
/// - Allowed Headers: Tüm header'lar (Authorization, Content-Type, vb.)
/// - Allowed Methods: Tüm HTTP metotları (GET, POST, PUT, DELETE, vb.)
/// - AllowCredentials: Cookie ve authentication header'ları için gerekli
/// 
/// Güvenlik Notu:
/// - Production'da sadece gerçek frontend domain'i eklenmelidir
/// - "*" wildcard kullanılmamalıdır (AllowCredentials ile uyumsuz)
/// </summary>
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            // React ve Vite development portları
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
                  .AllowAnyHeader()      // Tüm header'lara izin ver
                  .AllowAnyMethod()      // Tüm HTTP metotlarına izin ver (GET, POST, PUT, DELETE, vb.)
                  .AllowCredentials();   // Cookie ve authentication header'ları için
        });
});

// ==================== WEB APPLICATION OLUŞTURMA ====================

/// <summary>
/// WebApplication instance'ı oluşturulur
/// 
/// Builder'dan uygulama instance'ı alınır.
/// Bu noktadan sonra middleware pipeline yapılandırılabilir.
/// </summary>
var app = builder.Build();

// ==================== DATABASE MIGRATION VE INITIALIZATION ====================

/// <summary>
/// Database Migration ve Initialization Bloğu
/// 
/// Bu blok, uygulama başlatılırken database'in hazır olduğundan emin olur.
/// 
/// İşlem Adımları:
/// 1. Scope oluştur (scoped servisler için)
/// 2. Pending migration kontrolü
///    - Varsa uygula (Migrate())
/// 3. Migration yoksa tablo varlık kontrolü
///    - INFORMATION_SCHEMA ile tablo kontrolü
///    - Test sorgusu ile doğrulama
/// 4. Tablo yoksa database oluştur
///    - EnsureDeleted() → EnsureCreated()
///    - Migration history senkronizasyonu
/// 5. Singleton Service Provider setup
/// 6. Database'den veri yükleme
/// 7. Örnek veri ekleme (gerekirse)
/// 
/// Hata Yönetimi:
/// - Database hatalarında in-memory mode'a düşer
/// - Örnek veriler yüklenir (sistem çalışmaya devam eder)
/// </summary>
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        // DbContext'i al
        var context = services.GetRequiredService<SmartLibraryDbContext>();
        
        Console.WriteLine("🔄 Database migration başlatılıyor...");
        
        // ========== MIGRATION KONTROLÜ ==========
        
        /// <summary>
        /// Pending Migration Kontrolü
        /// 
        /// GetPendingMigrations(): Henüz uygulanmamış migration'ları getirir.
        /// Migration'lar varsa, Migrate() metodu ile uygulanır.
        /// </summary>
        var pendingMigrations = context.Database.GetPendingMigrations().ToList();
        
        if (pendingMigrations.Any())
        {
            // Pending migration'lar var - uygula
            Console.WriteLine($"📦 {pendingMigrations.Count} migration uygulanacak...");
            context.Database.Migrate();
            Console.WriteLine("✅ Database migration tamamlandı!");
        }
        else
        {
            // ========== TABLO VARLIK KONTROLÜ ==========
            
            /// <summary>
            /// Migration'lar uygulanmış görünüyor, ancak tablolar gerçekten var mı?
            /// 
            /// Durumlar:
            /// - Migration history var ama tablolar yok (manuel silinmiş olabilir)
            /// - İlk kurulum (migration history yok)
            /// 
            /// Kontrol Stratejisi:
            /// 1. INFORMATION_SCHEMA ile tablo kontrolü (SQL Server system view)
            /// 2. Test sorgusu ile doğrulama (gerçekten çalışıyor mu?)
            /// </summary>
            bool tablesExist = false;
            
            try
            {
                // Database connection'ını al
                var connection = context.Database.GetDbConnection();
                var wasOpen = connection.State == ConnectionState.Open;
                
                // Connection açık değilse aç
                if (!wasOpen) connection.Open();
                
                try
                {
                    // INFORMATION_SCHEMA ile tablo kontrolü
                    // SQL Server system view - tüm tabloları listeler
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Kaynaklar'";
                        var result = command.ExecuteScalar();
                        var tableCount = result != null ? Convert.ToInt32(result) : 0;
                        
                        if (tableCount > 0)
                        {
                            // Tablo var - gerçekten çalışıp çalışmadığını test et
                            // Kaynaklar tablosuna basit bir sorgu gönder
                            _ = context.Kaynaklar.Count();
                            tablesExist = true;
                            Console.WriteLine("✅ Database zaten güncel!");
                        }
                    }
                }
                finally
                {
                    // Connection'ı kapat (eğer biz açtıysak)
                    if (!wasOpen && connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
            catch (Exception ex) when (ex.Message.Contains("Invalid object name") || ex.Message.Contains("Kaynaklar"))
            {
                // Tablo yok hatası - devam et (tablesExist = false kalacak)
                tablesExist = false;
            }
            catch
            {
                // Diğer hatalar (connection hatası, vb.) - tablolar yok kabul et
                tablesExist = false;
            }
            
            // ========== DATABASE OLUŞTURMA ==========
            
            if (!tablesExist)
            {
                /// <summary>
                /// Tablolar yok - Database'i oluştur
                /// 
                /// Strateji:
                /// 1. EnsureDeleted(): Var olan database'i sil (temiz başlangıç)
                /// 2. EnsureCreated(): Model'den direkt tabloları oluştur
                ///    - Migration history kullanmaz
                ///    - Model'deki tüm entity'ler için tablolar oluşturur
                /// 3. Migration History Senkronizasyonu:
                ///    - Migration history tablosunu oluştur
                ///    - Initial migration kaydını ekle
                ///    - Böylece sonraki migration'lar çalışabilir
                /// </summary>
                Console.WriteLine("⚠️ Tablolar bulunamadı, database oluşturuluyor...");
                
                try
                {
                    // Var olan database'i sil (temiz başlangıç için)
                    context.Database.EnsureDeleted();
                    
                    /// <summary>
                    /// Database'i oluştur (Migration history kullanmadan)
                    /// 
                    /// EnsureCreated() vs Migrate():
                    /// - EnsureCreated(): Model'den direkt tablo oluşturur, migration history kullanmaz
                    /// - Migrate(): Migration dosyalarını uygular, migration history kullanır
                    /// 
                    /// Burada EnsureCreated() kullanılıyor çünkü:
                    /// - Migration history yoksa bile çalışır
                    /// - İlk kurulum için uygundur
                    /// - Sonra migration history senkronize edilecek
                    /// </summary>
                    context.Database.EnsureCreated();
                    Console.WriteLine("✅ Database oluşturuldu - Tablolar hazır!");
                    
                    // ========== MIGRATION HISTORY SENKRONIZASYONU ==========
                    
                    /// <summary>
                    /// Migration History Tablosu Oluşturma ve Senkronizasyon
                    /// 
                    /// Neden Gerekli:
                    /// - EnsureCreated() migration history kullanmaz
                    /// - Ancak sonraki migration'lar için migration history gerekli
                    /// - Bu yüzden manuel olarak oluşturulup senkronize ediliyor
                    /// 
                    /// İşlem:
                    /// 1. __EFMigrationsHistory tablosu yoksa oluştur
                    /// 2. InitialCreate migration kaydını ekle
                    /// 3. Böylece sistem, migration'ların uygulandığını düşünür
                    /// </summary>
                    try
                    {
                        // SQL Server'a direkt SQL komutu gönder
                        context.Database.ExecuteSqlRaw(@"
                            -- Migration history tablosu yoksa oluştur
                            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[__EFMigrationsHistory]') AND type in (N'U'))
                            BEGIN
                                CREATE TABLE [dbo].[__EFMigrationsHistory] (
                                    [MigrationId] nvarchar(150) NOT NULL,
                                    [ProductVersion] nvarchar(32) NOT NULL,
                                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                                );
                            END
                            
                            -- InitialCreate migration kaydı yoksa ekle
                            IF NOT EXISTS (SELECT 1 FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId] = '20241216000000_InitialCreate')
                            BEGIN
                                INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                                VALUES ('20241216000000_InitialCreate', '8.0.0');
                            END
                        ");
                    }
                    catch
                    {
                        // Migration history oluşturulamazsa devam et - kritik değil
                        // Sonraki migration'lar sorun çıkarabilir ama sistem çalışır
                    }
                }
                catch (Exception createEx)
                {
                    Console.WriteLine($"❌ Database oluşturma hatası: {createEx.Message}");
                    throw; // Hatayı yukarı fırlat
                }
            }
        }
        
        // ========== SINGLETON SERVICE PROVIDER SETUP ==========
        
        /// <summary>
        /// KutuphaneYoneticisi Singleton'a Service Provider Set Etme
        /// 
        /// Neden Gerekli:
        /// - KutuphaneYoneticisi Singleton pattern kullanır
        /// - Ancak DbContext'e ihtiyaç duyar (scoped service)
        /// - Singleton, scoped service'i direkt inject edemez
        /// - Çözüm: Service Provider'ı geç, gerektiğinde scope oluştur
        /// 
        /// Strateji:
        /// - app.Services (root service provider) Singleton'a geçirilir
        /// - Singleton, gerektiğinde scope oluşturup DbContext alır
        /// - Her istek için yeni DbContext instance'ı (doğru lifetime)
        /// </summary>
        KutuphaneYoneticisi.SetServiceProvider(app.Services);
        
        // ========== VERİ YÜKLEME VE ÖRNEK VERİ EKLEME ==========
        
        /// <summary>
        /// Database'den Veri Yükleme
        /// 
        /// İşlem Adımları:
        /// 1. LoadFromDatabase(): Database'deki tüm kaynak/kullanıcı verilerini Singleton'a yükle
        /// 2. Database boşsa örnek veriler ekle
        /// 3. Hata durumunda in-memory mode'a düş (sistem çalışmaya devam etsin)
        /// </summary>
        try
        {
            // Database'den verileri Singleton'a yükle
            KutuphaneYoneticisi.Instance.LoadFromDatabase();
            
            // Database boş mu kontrol et
            if (!context.Kaynaklar.Any())
            {
                Console.WriteLine("📚 Database boş, örnek veriler yükleniyor...");
                InitializeData(); // Örnek verileri ekle
            }
            else
            {
                // Database'de veri var
                Console.WriteLine($"✅ Database hazır: {context.Kaynaklar.Count()} kaynak mevcut");
            }
        }
        catch (Exception loadEx)
        {
            // Database'den yükleme hatası - in-memory mode'a düş
            Console.WriteLine($"⚠️ Database'den yükleme hatası: {loadEx.Message}");
            Console.WriteLine("📚 In-memory mode'da örnek veriler yükleniyor...");
            InitializeData(); // Örnek verileri yükle (in-memory)
        }
    }
    catch (Exception ex)
    {
        // Genel database hatası - in-memory mode'da devam et
        Console.WriteLine($"❌ Database hatası: {ex.Message}");
        Console.WriteLine("⚠️ In-memory mode'da devam ediliyor...");
        InitializeData(); // Örnek verileri yükle (sistem çalışmaya devam etsin)
    }
}

// ==================== MIDDLEWARE PIPELINE YAPILANDIRMASI ====================

/// <summary>
/// HTTP Request Pipeline Yapılandırması
/// 
/// Middleware'ler sırayla çalışır (dikey çizgi | sembolü ile gösterilir).
/// Sıra önemlidir!
/// 
/// Pipeline Sırası:
/// 1. Swagger (Development)
/// 2. CORS
/// 3. Authorization
/// 4. Controllers
/// </summary>

/// <summary>
/// Swagger Middleware (Development Ortamında)
/// 
/// Swagger UI ve Swagger JSON endpoint'leri eklenir.
/// Sadece Development ortamında aktif olur.
/// 
/// Endpoints:
/// - /swagger: Swagger UI (API dokümantasyonu görüntüleme)
/// - /swagger/v1/swagger.json: OpenAPI JSON schema
/// 
/// Production'da genellikle kapatılır (güvenlik).
/// </summary>
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/// <summary>
/// CORS Middleware
/// 
/// ÖNEMLİ: Authorization'dan ÖNCE olmalı!
/// 
/// Neden:
/// - CORS preflight (OPTIONS) request'leri authorization'dan geçemez
/// - CORS middleware, preflight request'leri handle eder
/// - Authorization middleware'den önce çalışması gerekir
/// 
/// Çalışma:
/// - "AllowReactApp" policy'si uygulanır
/// - Frontend'den gelen request'ler kontrol edilir
/// - Origin, method, header kontrolü yapılır
/// </summary>
app.UseCors("AllowReactApp");

/// <summary>
/// Authorization Middleware
/// 
/// Authentication ve authorization işlemleri için.
/// Şu an basit yapılandırma (ileride JWT, Identity, vb. eklenebilir).
/// </summary>
app.UseAuthorization();

/// <summary>
/// Controllers Mapping
/// 
/// Controller'ları route'lara map eder.
/// Attribute routing kullanılır ([Route], [HttpGet], vb.).
/// 
/// Örnek:
/// - /api/kaynak → KaynakController
/// - /api/kullanici → KullaniciController
/// </summary>
app.MapControllers();

// ==================== BAŞLATMA MESAJLARI ====================

Console.WriteLine("🚀 Akıllı Kütüphane Yönetim Sistemi başlatıldı!");
Console.WriteLine("📚 API: http://localhost:5000");
Console.WriteLine("📖 Swagger: http://localhost:5000/swagger");

// ==================== UYGULAMA ÇALIŞTIRMA ====================

/// <summary>
/// Uygulamayı Çalıştır
/// 
/// app.Run() bloğu uygulamayı başlatır ve request'leri dinlemeye başlar.
/// Bu satır, uygulama kapatılana kadar bloklar (blocking call).
/// 
/// Port: launchSettings.json'dan okunur (varsayılan: 5000)
/// </summary>
app.Run();

// ==================== ÖRNEK VERİ INITIALIZATION ====================

/// <summary>
/// InitializeData() Metodu
/// 
/// Database boşsa veya in-memory mode'da örnek veriler yükler.
/// 
/// Örnek Veriler:
/// - 5 Kitap (Klasik Edebiyat, Distopya, Felsefe, Roman türlerinde)
/// - 2 Dergi (Bilim ve Doğa kategorilerinde)
/// - 2 Tez (Doktora ve Yüksek Lisans)
/// - 3 Kullanıcı (Farklı ilgi alanları ve yaş grupları)
/// - 2 Ödünç İşlemi (Test için)
/// 
/// Kullanım:
/// - Development ve test için
/// - Demolar için
/// - İlk kurulum için
/// </summary>
void InitializeData()
{
    var yonetici = KutuphaneYoneticisi.Instance;

    // ========== ÖRNEK KİTAPLAR ==========

    /// <summary>
    /// Örnek Kitap 1: Suç ve Ceza
    /// Klasik Edebiyat kategorisinde, popüler kitap.
    /// </summary>
    var kitap1 = new Kitap(
        "978-975-342-556-8", "Suç ve Ceza", "Fyodor Dostoyevski",
        new DateTime(1866, 1, 1), "Klasik Edebiyat", 671, "İletişim Yayınları", "Türkçe"
    );
    kitap1.OkunmaSayisi = 45; // Popülerlik skoru

    /// <summary>
    /// Örnek Kitap 2: 1984
    /// Distopya türünde, çok popüler kitap.
    /// </summary>
    var kitap2 = new Kitap(
        "978-605-375-125-4", "1984", "George Orwell",
        new DateTime(1949, 6, 8), "Distopya", 352, "Can Yayınları", "Türkçe"
    );
    kitap2.OkunmaSayisi = 67; // En popüler

    /// <summary>
    /// Örnek Kitap 3: Simyacı
    /// Felsefe kategorisinde, çok popüler.
    /// </summary>
    var kitap3 = new Kitap(
        "978-605-07-0456-2", "Simyacı", "Paulo Coelho",
        new DateTime(1988, 1, 1), "Felsefe", 184, "Can Yayınları", "Türkçe"
    );
    kitap3.OkunmaSayisi = 89; // En popüler

    /// <summary>
    /// Örnek Kitap 4: İnce Memed
    /// Türk edebiyatı, Roman kategorisinde.
    /// </summary>
    var kitap4 = new Kitap(
        "978-975-08-3645-7", "İnce Memed", "Yaşar Kemal",
        new DateTime(1955, 1, 1), "Roman", 420, "Yapı Kredi Yayınları", "Türkçe"
    );
    kitap4.OkunmaSayisi = 34;

    /// <summary>
    /// Örnek Kitap 5: Satranç
    /// Klasik Edebiyat kategorisinde.
    /// </summary>
    var kitap5 = new Kitap(
        "978-605-375-789-8", "Satranç", "Stefan Zweig",
        new DateTime(1942, 1, 1), "Klasik Edebiyat", 96, "Türkiye İş Bankası Yayınları", "Türkçe"
    );
    kitap5.OkunmaSayisi = 52;

    // ========== ÖRNEK DERGİLER ==========

    /// <summary>
    /// Örnek Dergi 1: Bilim ve Teknik
    /// TÜBİTAK dergisi, Bilim kategorisinde, aylık yayın.
    /// </summary>
    var dergi1 = new Dergi(
        "ISSN-2536-4618", "Bilim ve Teknik", "TÜBİTAK",
        new DateTime(2024, 1, 1), "Bilim", 1, "Aylık", "2536-4618"
    );
    dergi1.OkunmaSayisi = 23;

    /// <summary>
    /// Örnek Dergi 2: National Geographic Türkiye
    /// Doğa kategorisinde, aylık yayın.
    /// </summary>
    var dergi2 = new Dergi(
        "ISSN-1303-6092", "National Geographic Türkiye", "National Geographic",
        new DateTime(2024, 2, 1), "Doğa", 2, "Aylık", "1303-6092"
    );
    dergi2.OkunmaSayisi = 18;

    // ========== ÖRNEK TEZLER ==========

    /// <summary>
    /// Örnek Tez 1: Yapay Zeka ve Makine Öğrenmesi
    /// Doktora tezi, Bilgisayar Mühendisliği bölümü.
    /// </summary>
    var tez1 = new Tez(
        "TEZ-2023-001", "Yapay Zeka ve Makine Öğrenmesi", "Ahmet Yılmaz",
        new DateTime(2023, 6, 15), "Bilgisayar Mühendisliği",
        "İstanbul Teknik Üniversitesi", "Bilgisayar Mühendisliği",
        "Prof. Dr. Mehmet Kaya", "Doktora"
    );
    tez1.OkunmaSayisi = 12;

    /// <summary>
    /// Örnek Tez 2: Sürdürülebilir Enerji Sistemleri
    /// Yüksek Lisans tezi, Elektrik-Elektronik Mühendisliği bölümü.
    /// </summary>
    var tez2 = new Tez(
        "TEZ-2023-002", "Sürdürülebilir Enerji Sistemleri", "Ayşe Demir",
        new DateTime(2023, 8, 20), "Enerji",
        "Orta Doğu Teknik Üniversitesi", "Elektrik-Elektronik Mühendisliği",
        "Prof. Dr. Ali Vural", "Yüksek Lisans"
    );
    tez2.OkunmaSayisi = 8;

    // ========== KAYNAKLARI EKLE ==========
    
    // Tüm kaynakları Singleton yöneticiye ekle
    yonetici.KaynakEkle(kitap1);
    yonetici.KaynakEkle(kitap2);
    yonetici.KaynakEkle(kitap3);
    yonetici.KaynakEkle(kitap4);
    yonetici.KaynakEkle(kitap5);
    yonetici.KaynakEkle(dergi1);
    yonetici.KaynakEkle(dergi2);
    yonetici.KaynakEkle(tez1);
    yonetici.KaynakEkle(tez2);

    // ========== ÖRNEK KULLANICILAR ==========

    /// <summary>
    /// Örnek Kullanıcı 1: Mehmet Yılmaz
    /// Edebiyat ve felsefe ilgisi olan kullanıcı.
    /// </summary>
    var kullanici1 = new Kullanici
    {
        Ad = "Mehmet",
        Soyad = "Yılmaz",
        Email = "mehmet.yilmaz@email.com",
        Yas = 25,
        IlgiAlanlari = new System.Collections.Generic.List<string> { "Klasik Edebiyat", "Felsefe", "Roman" },
        FavoriKategoriler = new System.Collections.Generic.List<string> { "Klasik Edebiyat", "Roman" }
    };

    /// <summary>
    /// Örnek Kullanıcı 2: Zeynep Kaya
    /// Bilim ve doğa ilgisi olan kullanıcı.
    /// </summary>
    var kullanici2 = new Kullanici
    {
        Ad = "Zeynep",
        Soyad = "Kaya",
        Email = "zeynep.kaya@email.com",
        Yas = 22,
        IlgiAlanlari = new System.Collections.Generic.List<string> { "Bilim", "Teknoloji", "Doğa" },
        FavoriKategoriler = new System.Collections.Generic.List<string> { "Bilim", "Doğa" }
    };

    /// <summary>
    /// Örnek Kullanıcı 3: Can Öztürk
    /// Teknoloji ve yapay zeka ilgisi olan kullanıcı.
    /// </summary>
    var kullanici3 = new Kullanici
    {
        Ad = "Can",
        Soyad = "Öztürk",
        Email = "can.ozturk@email.com",
        Yas = 30,
        IlgiAlanlari = new System.Collections.Generic.List<string> { "Bilgisayar Mühendisliği", "Yapay Zeka" },
        FavoriKategoriler = new System.Collections.Generic.List<string> { "Bilgisayar Mühendisliği" }
    };

    // Kullanıcıları ekle
    yonetici.KullaniciEkle(kullanici1);
    yonetici.KullaniciEkle(kullanici2);
    yonetici.KullaniciEkle(kullanici3);

    // ========== ÖRNEK ÖDÜNÇ İŞLEMLERİ ==========

    /// <summary>
    /// Örnek ödünç işlemleri
    /// 
    /// Test amaçlı 2 ödünç işlemi:
    /// - Mehmet Yılmaz → Suç ve Ceza (Kitap)
    /// - Zeynep Kaya → Bilim ve Teknik (Dergi)
    /// 
    /// Bu işlemler, sistemin çalıştığını test etmek için kullanılır.
    /// </summary>
    yonetici.OduncVer(kullanici1.Id, kitap1.ISBN);
    yonetici.OduncVer(kullanici2.Id, dergi1.ISBN);

    // ========== ÖZET BİLGİ ==========
    
    Console.WriteLine("✅ Örnek veriler yüklendi:");
    Console.WriteLine($"   - {yonetici.TumKaynaklariGetir().Count} kaynak");
    Console.WriteLine($"   - {yonetici.TumKullanicilariGetir().Count} kullanıcı");
}
