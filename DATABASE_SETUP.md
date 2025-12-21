# 🗄️ SQL Server Database Setup

## Database Bilgileri

```
Server: DESKTOP-C75KDDR
Database: SmartLibraryDB
Authentication: Windows Authentication (Trusted_Connection)
```

## 🚀 Kurulum Adımları

### 1. SQL Server Kontrolü

SQL Server'ın çalıştığından emin olun:

```powershell
# Services'de SQL Server kontrol et
services.msc
# Veya
Get-Service MSSQLSERVER
```

### 2. Migration Oluşturma ve Uygulama

Backend dizininde:

```bash
# Entity Framework Tools yüklü mü kontrol et
dotnet ef --version

# Yüklü değilse:
dotnet tool install --global dotnet-ef

# İlk migration oluştur
dotnet ef migrations add InitialCreate

# Database'i oluştur ve migration'ı uygula
dotnet ef database update
```

### 3. Uygulamayı Çalıştır

```bash
dotnet run
```

## 📊 Database Şeması

### Tables

#### 1. Kaynaklar (Table Per Hierarchy - TPH)
```sql
CREATE TABLE Kaynaklar (
    ISBN NVARCHAR(50) PRIMARY KEY,
    Baslik NVARCHAR(200) NOT NULL,
    Yazar NVARCHAR(100) NOT NULL,
    YayinTarihi DATETIME2 NOT NULL,
    OduncDurumu BIT NOT NULL,
    OduncTarihi DATETIME2 NULL,
    OkunmaSayisi INT NOT NULL,
    Kategori NVARCHAR(100),
    KaynakTuru NVARCHAR(50) NOT NULL, -- 'Kitap', 'Dergi', 'Tez'
    
    -- Kitap specific
    SayfaSayisi INT NULL,
    YayinEvi NVARCHAR(100) NULL,
    Dil NVARCHAR(50) NULL,
    
    -- Dergi specific
    SayiNo INT NULL,
    YayinPeriyodu NVARCHAR(50) NULL,
    ISSN NVARCHAR(50) NULL,
    
    -- Tez specific
    Universite NVARCHAR(200) NULL,
    Bolum NVARCHAR(200) NULL,
    DanismanAdi NVARCHAR(100) NULL,
    TezTuru NVARCHAR(50) NULL
)
```

#### 2. Kullanicilar
```sql
CREATE TABLE Kullanicilar (
    Id NVARCHAR(50) PRIMARY KEY,
    Ad NVARCHAR(50) NOT NULL,
    Soyad NVARCHAR(50) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Yas INT NOT NULL,
    IlgiAlanlari NVARCHAR(MAX), -- CSV format
    FavoriKategoriler NVARCHAR(MAX), -- CSV format
    KayitTarihi DATETIME2 NOT NULL
)
```

#### 3. OduncKayitlari
```sql
CREATE TABLE OduncKayitlari (
    ISBN NVARCHAR(50),
    OduncTarihi DATETIME2,
    KaynakBaslik NVARCHAR(200),
    Kategori NVARCHAR(100),
    IadeTarihi DATETIME2 NULL,
    TeslimSuresi INT NOT NULL,
    KullaniciId NVARCHAR(50) NULL, -- Geçmiş için
    KullaniciIdAktif NVARCHAR(50) NULL, -- Aktif için
    
    PRIMARY KEY (ISBN, OduncTarihi)
)
```

#### 4. IslemKayitlari
```sql
CREATE TABLE IslemKayitlari (
    Id NVARCHAR(50) PRIMARY KEY,
    IslemTuru NVARCHAR(50),
    Aciklama NVARCHAR(500),
    Tarih DATETIME2 NOT NULL
)
```

## 🔧 Connection String

`appsettings.json` dosyasında:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DESKTOP-C75KDDR;Database=SmartLibraryDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

## 🎯 Özellikler

### 1. Table Per Hierarchy (TPH)
- Tek tablo tüm kaynak türlerini içerir
- `KaynakTuru` discriminator column
- Polimorfizm database seviyesinde desteklenir

### 2. Otomatik Migration
- Uygulama ilk çalıştığında database otomatik oluşur
- Migration'lar otomatik uygulanır

### 3. Seed Data
- Database boşsa örnek veriler yüklenir
- Mevcut veriler korunur

### 4. In-Memory Fallback
- SQL Server bağlanamazsa in-memory mode'a geçer
- Uygulama hata vermeden çalışmaya devam eder

## 📝 Manuel Migration Komutları

```bash
# Yeni migration ekle
dotnet ef migrations add MigrationAdi

# Migration'ı uygula
dotnet ef database update

# Belirli bir migration'a geri dön
dotnet ef database update MigrationAdi

# Migration'ı geri al (son)
dotnet ef database update 0

# Migration'ı sil
dotnet ef migrations remove

# Database'i sil
dotnet ef database drop
```

## 🔍 Database Sorgulama

SQL Server Management Studio (SSMS) veya Azure Data Studio ile:

```sql
-- Tüm kaynakları listele
SELECT * FROM Kaynaklar

-- Sadece kitapları listele
SELECT * FROM Kaynaklar WHERE KaynakTuru = 'Kitap'

-- Tüm kullanıcıları listele
SELECT * FROM Kullanicilar

-- İşlem geçmişi
SELECT * FROM IslemKayitlari ORDER BY Tarih DESC

-- Aktif ödünçler
SELECT * FROM OduncKayitlari WHERE IadeTarihi IS NULL
```

## 🐛 Sorun Giderme

### SQL Server'a Bağlanamıyor

```powershell
# SQL Server çalışıyor mu?
Get-Service MSSQLSERVER

# Başlatmak için:
Start-Service MSSQLSERVER
```

### Migration Hatası

```bash
# Migration'ları sıfırla
dotnet ef database drop --force
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Connection String Hatası

`appsettings.json`'da server adını kontrol edin:

```json
"Server=DESKTOP-C75KDDR;Database=SmartLibraryDB;..."
```

Eğer farklı bir SQL Server instance kullanıyorsanız:

```json
"Server=localhost\\SQLEXPRESS;Database=SmartLibraryDB;..."
```

## ✅ Test

Database çalışıyor mu kontrol et:

1. Uygulamayı başlat: `dotnet run`
2. Console'da şu mesajları görmeli:
   ```
   🔄 Database migration başlatılıyor...
   ✅ Database migration tamamlandı!
   ✅ Database'den yüklendi: X kaynak, Y kullanıcı
   ```

3. SQL Server'da database'i kontrol et:
   ```sql
   USE SmartLibraryDB
   SELECT COUNT(*) FROM Kaynaklar
   SELECT COUNT(*) FROM Kullanicilar
   ```

## 🎉 Sonuç

Database başarıyla entegre edildi!

- ✅ SQL Server bağlantısı
- ✅ Entity Framework Core
- ✅ Otomatik migration
- ✅ Seed data
- ✅ CRUD operasyonları
- ✅ Polimorfizm desteği (TPH)
- ✅ Fallback mechanism

**Database Name:** SmartLibraryDB
**Server:** DESKTOP-C75KDDR
**Status:** Ready! 🚀

