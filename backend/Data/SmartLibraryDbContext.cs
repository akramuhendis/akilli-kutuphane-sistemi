using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SmartLibrary.Models;
using SmartLibrary.Patterns.Singleton;

namespace SmartLibrary.Data
{
    /// <summary>
    /// Akıllı Kütüphane Yönetim Sistemi için Entity Framework Core DbContext sınıfı
    /// 
    /// Bu sınıf, veritabanı işlemlerini yönetir ve Entity Framework Core'un 
    /// Code First yaklaşımını kullanarak veritabanı şemasını oluşturur.
    /// 
    /// Özellikler:
    /// - Table Per Hierarchy (TPH) pattern ile polimorfik kayıtlar
    /// - List<string> türündeki özelliklerin CSV formatında saklanması
    /// - İlişkisel veritabanı yapılandırmaları
    /// - Cascade delete kuralları
    /// 
    /// Veritabanı: SQL Server
    /// Migration: Entity Framework Core Migrations
    /// </summary>
    public class SmartLibraryDbContext : DbContext
    {
        /// <summary>
        /// Constructor - DbContextOptions ile yapılandırma alır
        /// Bu constructor, dependency injection ile otomatik olarak çağrılır
        /// </summary>
        /// <param name="options">Veritabanı bağlantı seçenekleri (Connection string, provider vb.)</param>
        public SmartLibraryDbContext(DbContextOptions<SmartLibraryDbContext> options)
            : base(options)
        {
        }

        // ==================== DB SET'LER (TABLOLAR) ====================

        /// <summary>
        /// Kullanicilar tablosu - Tüm kullanıcı kayıtlarını içerir
        /// </summary>
        public DbSet<Kullanici> Kullanicilar { get; set; }

        /// <summary>
        /// OduncKayitlari tablosu - Ödünç işlemlerinin geçmiş kayıtlarını içerir
        /// Not: Bu tablo, Kullanici.OduncGecmisi property'si için kullanılır
        /// </summary>
        public DbSet<OduncKaydi> OduncKayitlari { get; set; }

        /// <summary>
        /// IslemKayitlari tablosu - Sistemdeki tüm işlemlerin log kayıtlarını içerir
        /// İşlem türleri: KAYNAK_EKLENDI, KULLANICI_SILINDI, ODUNC_VERILDI, vb.
        /// </summary>
        public DbSet<IslemKaydi> IslemKayitlari { get; set; }

        /// <summary>
        /// Kaynaklar tablosu - Tüm kaynakları (Kitap, Dergi, Tez) içerir
        /// 
        /// Table Per Hierarchy (TPH) Pattern:
        /// - Kitap, Dergi ve Tez sınıfları Kaynak abstract sınıfından türer
        /// - Tüm kaynaklar tek bir "Kaynaklar" tablosunda saklanır
        /// - "KaynakTuru" discriminator kolonu ile kaynak türü ayırt edilir
        /// - Bu sayede polimorfizm veritabanı seviyesinde korunur
        /// 
        /// Avantajları:
        /// - Tek tablo sorguları (performans)
        /// - Kolay JOIN işlemleri
        /// - Polimorfik sorgular kolaylaşır
        /// </summary>
        public DbSet<Kaynak> Kaynaklar { get; set; }

        // ==================== MODEL YAPILANDIRMASI ====================

        /// <summary>
        /// Model oluşturma metodunu override eder
        /// Bu metod, veritabanı şemasının yapılandırmasını yapar
        /// </summary>
        /// <param name="modelBuilder">Model yapılandırıcı nesnesi</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== KAYNAK TABLOSU YAPILANDIRMASI ==========

            /// <summary>
            /// Table Per Hierarchy (TPH) yapılandırması
            /// Kaynak abstract sınıfının alt sınıfları (Kitap, Dergi, Tez) 
            /// tek bir tabloda saklanır ve "KaynakTuru" kolonu ile ayırt edilir
            /// </summary>
            modelBuilder.Entity<Kaynak>()
                // Discriminator kolonu: Kaynağın türünü belirten kolon
                .HasDiscriminator<string>("KaynakTuru")
                // Her alt sınıf için discriminator değerleri
                .HasValue<Kitap>("Kitap")      // Kitap kayıtları için "Kitap"
                .HasValue<Dergi>("Dergi")      // Dergi kayıtları için "Dergi"
                .HasValue<Tez>("Tez");         // Tez kayıtları için "Tez"

            /// <summary>
            /// Kaynak entity'si için özellik yapılandırmaları
            /// </summary>
            modelBuilder.Entity<Kaynak>(entity =>
            {
                // Primary Key: ISBN numarası (her kaynağın benzersiz kimliği)
                entity.HasKey(e => e.ISBN);
                
                // ISBN kolonu: Maksimum 50 karakter
                entity.Property(e => e.ISBN).HasMaxLength(50);
                
                // Baslik kolonu: Zorunlu, maksimum 200 karakter
                entity.Property(e => e.Baslik).IsRequired().HasMaxLength(200);
                
                // Yazar kolonu: Zorunlu, maksimum 100 karakter
                entity.Property(e => e.Yazar).IsRequired().HasMaxLength(100);
                
                // Kategori kolonu: İsteğe bağlı, maksimum 100 karakter
                entity.Property(e => e.Kategori).HasMaxLength(100);
            });

            // ========== KULLANICI TABLOSU YAPILANDIRMASI ==========

            /// <summary>
            /// Kullanici entity'si için özellik yapılandırmaları
            /// </summary>
            modelBuilder.Entity<Kullanici>(entity =>
            {
                // Primary Key: Kullanıcı ID'si (GUID string)
                entity.HasKey(e => e.Id);
                
                // Id kolonu: Maksimum 50 karakter (GUID string formatı)
                entity.Property(e => e.Id).HasMaxLength(50);
                
                // Ad kolonu: Zorunlu, maksimum 50 karakter
                entity.Property(e => e.Ad).IsRequired().HasMaxLength(50);
                
                // Soyad kolonu: Zorunlu, maksimum 50 karakter
                entity.Property(e => e.Soyad).IsRequired().HasMaxLength(50);
                
                // Email kolonu: Zorunlu, maksimum 100 karakter
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);

                // ========== LİSTE ÖZELLİKLERİNİN CSV'YE DÖNÜŞTÜRÜLMESİ ==========
                
                /// <summary>
                /// IlgiAlanlari property'si List<string> türünde
                /// Veritabanında CSV (virgülle ayrılmış) formatında saklanır
                /// 
                /// Örnek:
                /// - C#: ["Bilim", "Teknoloji", "Edebiyat"]
                /// - DB:  "Bilim,Teknoloji,Edebiyat"
                /// 
                /// ValueConverter: List<string> ↔ string (CSV) dönüşümü yapar
                /// ValueComparer: List karşılaştırmaları için kullanılır
                /// </summary>
                entity.Property(e => e.IlgiAlanlari)
                    .HasConversion(
                        // C#'tan DB'ye: List'i CSV string'e çevir
                        v => string.Join(',', v),
                        // DB'den C#'a: CSV string'i List'e çevir
                        v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>(),
                        // Değer karşılaştırıcı: İki listeyi karşılaştırmak için
                        new ValueComparer<List<string>>(
                            // İki listenin eşit olup olmadığını kontrol et
                            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                            // Liste için hash code hesapla
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            // Listeyi kopyala (change tracking için)
                            c => c.ToList()
                        )
                    );

                /// <summary>
                /// FavoriKategoriler property'si List<string> türünde
                /// IlgiAlanlari ile aynı mantıkla CSV formatında saklanır
                /// 
                /// Örnek:
                /// - C#: ["Klasik Edebiyat", "Roman"]
                /// - DB:  "Klasik Edebiyat,Roman"
                /// </summary>
                entity.Property(e => e.FavoriKategoriler)
                    .HasConversion(
                        // List'i CSV string'e çevir
                        v => string.Join(',', v),
                        // CSV string'i List'e çevir
                        v => v != null ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>(),
                        // Değer karşılaştırıcı
                        new ValueComparer<List<string>>(
                            // Eşitlik kontrolü
                            (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                            // Hash code hesaplama
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            // Liste kopyalama
                            c => c.ToList()
                        )
                    );

                // ========== İLİŞKİSEL YAPILANDIRMALAR ==========

                /// <summary>
                /// Kullanici → OduncGecmisi (One-to-Many ilişki)
                /// 
                /// İlişki Açıklaması:
                /// - Bir kullanıcının birden fazla ödünç geçmişi kaydı olabilir
                /// - OduncKaydi tablosunda "KullaniciId" foreign key kolonu vardır
                /// - Cascade Delete: Kullanıcı silindiğinde tüm ödünç geçmişi kayıtları da silinir
                /// </summary>
                entity.HasMany(e => e.OduncGecmisi)
                    .WithOne()  // OduncKaydi tarafında navigation property yok
                    .HasForeignKey("KullaniciId")  // Foreign key kolon adı
                    .OnDelete(DeleteBehavior.Cascade);  // Kaskad silme aktif

                /// <summary>
                /// Kullanici → AktifOduncler (One-to-Many ilişki)
                /// 
                /// İlişki Açıklaması:
                /// - Bir kullanıcının birden fazla aktif ödüncü olabilir
                /// - OduncKaydi tablosunda "KullaniciIdAktif" foreign key kolonu vardır
                /// 
                /// ÖNEMLİ: NoAction Delete Behavior
                /// - SQL Server'da "multiple cascade paths" hatası önlenir
                /// - Hem OduncGecmisi hem de AktifOduncler Cascade olsaydı hata verirdi
                /// - Bu nedenle AktifOduncler için NoAction kullanılıyor
                /// - Manuel olarak silme işlemi yapılmalı
                /// </summary>
                entity.HasMany(e => e.AktifOduncler)
                    .WithOne()  // OduncKaydi tarafında navigation property yok
                    .HasForeignKey("KullaniciIdAktif")  // Foreign key kolon adı (farklı)
                    .OnDelete(DeleteBehavior.NoAction);  // Kaskad silme YOK (hatayı önlemek için)
            });

            // ========== ÖDÜNÇ KAYDI TABLOSU YAPILANDIRMASI ==========

            /// <summary>
            /// OduncKaydi entity'si için özellik yapılandırmaları
            /// 
            /// Composite Primary Key:
            /// - ISBN ve OduncTarihi birlikte primary key oluşturur
            /// - Bu sayede aynı kaynak farklı tarihlerde ödünç verilebilir
            /// </summary>
            modelBuilder.Entity<OduncKaydi>(entity =>
            {
                // Composite Primary Key: ISBN + OduncTarihi
                entity.HasKey(e => new { e.ISBN, e.OduncTarihi });
                
                // ISBN kolonu: Maksimum 50 karakter (Kaynak tablosuna referans)
                entity.Property(e => e.ISBN).HasMaxLength(50);
                
                // KaynakBaslik kolonu: Maksimum 200 karakter (denormalizasyon - performans için)
                entity.Property(e => e.KaynakBaslik).HasMaxLength(200);
                
                // Kategori kolonu: Maksimum 100 karakter (denormalizasyon - sorgu performansı için)
                entity.Property(e => e.Kategori).HasMaxLength(100);
            });

            // ========== İŞLEM KAYDI TABLOSU YAPILANDIRMASI ==========

            /// <summary>
            /// IslemKaydi entity'si için özellik yapılandırmaları
            /// Bu tablo, sistemdeki tüm işlemlerin log kayıtlarını tutar
            /// </summary>
            modelBuilder.Entity<IslemKaydi>(entity =>
            {
                // Primary Key: İşlem ID'si (GUID string)
                entity.HasKey(e => e.Id);
                
                // Id kolonu: Maksimum 50 karakter
                entity.Property(e => e.Id).HasMaxLength(50);
                
                // IslemTuru kolonu: Maksimum 50 karakter
                // Örnek değerler: "KAYNAK_EKLENDI", "ODUNC_VERILDI", "IADE_ALINDI"
                entity.Property(e => e.IslemTuru).HasMaxLength(50);
                
                // Aciklama kolonu: Maksimum 500 karakter
                // İşlemle ilgili detaylı açıklama
                entity.Property(e => e.Aciklama).HasMaxLength(500);
            });
        }
    }
}
