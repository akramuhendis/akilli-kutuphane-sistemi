# 🚀 SQL Server Migration Talimatları

## Database Bilgileri
- **Server**: DESKTOP-C75KDDR
- **Database**: SmartLibraryDB
- **Authentication**: Windows Authentication

## ⚡ Hızlı Kurulum

### Adım 1: Backend Dizinine Git
```powershell
cd "C:\Users\user\Desktop\akıllı kütüphane yönetim sistemi\backend"
```

### Adım 2: Paketleri Yükle
```powershell
dotnet restore
```

### Adım 3: Database Oluştur
```powershell
dotnet ef database update
```

### Adım 4: Uygulamayı Çalıştır
```powershell
dotnet run
```

## ✅ Sonuç

Uygulama çalıştığında:
1. Database otomatik oluşturulur (**SmartLibraryDB**)
2. Tablolar oluşturulur (Kaynaklar, Kullanicilar, OduncKayitlari, IslemKayitlari)
3. Örnek veriler yüklenir
4. Sistem hazır!

## 📊 Database Tabloları

### 1. Kaynaklar (Table Per Hierarchy)
- ISBN (PK)
- Baslik, Yazar, YayinTarihi
- OduncDurumu, OduncTarihi
- OkunmaSayisi, Kategori
- **KaynakTuru** (Discriminator: "Kitap", "Dergi", "Tez")
- Kitap özellikleri: SayfaSayisi, YayinEvi, Dil
- Dergi özellikleri: SayiNo, YayinPeriyodu, ISSN
- Tez özellikleri: Universite, Bolum, DanismanAdi, TezTuru

### 2. Kullanicilar
- Id (PK)
- Ad, Soyad, Email, Yas
- IlgiAlanlari (CSV)
- FavoriKategoriler (CSV)
- KayitTarihi

### 3. OduncKayitlari
- ISBN, OduncTarihi (Composite PK)
- KaynakBaslik, Kategori
- IadeTarihi, TeslimSuresi
- Foreign Keys: KullaniciId, KullaniciIdAktif

### 4. IslemKayitlari
- Id (PK)
- IslemTuru, Aciklama, Tarih

## 🔧 Alternatif Migration Komutları

### Entity Framework Tools Yükle
```powershell
dotnet tool install --global dotnet-ef
```

### Migration Oluştur (manuel)
```powershell
cd backend
dotnet ef migrations add InitialCreate
```

### Database Güncelle
```powershell
dotnet ef database update
```

### Database Sil (Yeniden Başlat)
```powershell
dotnet ef database drop --force
dotnet ef database update
```

## 🔍 SQL Server Kontrol

### SSMS (SQL Server Management Studio) ile:
```sql
-- Database'i kontrol et
USE SmartLibraryDB;

-- Tabloları listele
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;

-- Kaynakları görüntüle
SELECT * FROM Kaynaklar;

-- Kullanıcıları görüntüle
SELECT * FROM Kullanicilar;
```

## ⚠️ Sorun Giderme

### SQL Server çalışmıyor mu?
```powershell
# Servisi başlat
Start-Service MSSQLSERVER

# Durumu kontrol et
Get-Service MSSQLSERVER
```

### Connection hatası?
`appsettings.json` dosyasında connection string'i kontrol edin:
```json
"Server=DESKTOP-C75KDDR;Database=SmartLibraryDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### Migration zaten var hatası?
```powershell
# Migrations klasörünü sil ve tekrar oluştur
Remove-Item -Recurse -Force .\Migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 🎉 Başarı Mesajları

Uygulama başlatıldığında console'da göreceksiniz:

```
🔄 Database migration başlatılıyor...
✅ Database migration tamamlandı!
✅ Database'den yüklendi: X kaynak, Y kullanıcı
📚 Örnek veriler yükleniyor... (eğer database boşsa)
✅ Örnek veriler yüklendi:
   - 9 kaynak
   - 3 kullanıcı
🚀 Akıllı Kütüphane Yönetim Sistemi başlatıldı!
📚 API: http://localhost:5000
📖 Swagger: http://localhost:5000/swagger
```

## 💡 Önemli Notlar

1. **Otomatik Migration**: Uygulama ilk çalıştığında database otomatik oluşur
2. **Seed Data**: Database boşsa örnek veriler otomatik yüklenir
3. **Fallback Mode**: SQL Server'a bağlanamazsa in-memory mode'da çalışır
4. **Data Persistence**: Tüm veriler database'de kalıcı olarak saklanır
5. **Polimorfizm**: Table Per Hierarchy (TPH) stratejisi ile Kaynak hiyerarşisi desteklenir

---

**Database Adı**: SmartLibraryDB  
**Server**: DESKTOP-C75KDDR  
**Durum**: Hazır! 🚀

