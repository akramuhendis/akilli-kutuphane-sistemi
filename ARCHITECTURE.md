# 🏗️ Sistem Mimarisi ve Tasarım Kararları

## Genel Mimari

```
┌─────────────────────────────────────────────────────────┐
│                   FRONTEND (React)                      │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐               │
│  │Dashboard │  │Resources │  │  Users   │               │
│  └──────────┘  └──────────┘  └──────────┘               │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐               │
│  │  Loans   │  │Recommend │  │Statistics│               │
│  └──────────┘  └──────────┘  └──────────┘               │
│                       ↕                                 │
│                  API Service                            │
└─────────────────────────────────────────────────────────┘
                        ↕ HTTP/JSON
┌─────────────────────────────────────────────────────────┐
│              BACKEND (ASP.NET Core API)                 │
│                                                         │
│  ┌──────────────────────────────────────────────────┐   │
│  │            API Controllers Layer                 │   │
│  │  Kaynak │ Kullanici │ Odunc │ Oneri │ Istatistik │   │
│  └──────────────────────────────────────────────────┘   │
│                        ↕                                │
│  ┌──────────────────────────────────────────────────┐   │
│  │            Business Logic Layer                  │   │
│  │  Kutuphane │ OneriSistemi │ IstatistikServisi    │   │
│  └──────────────────────────────────────────────────┘   │ 
│                        ↕                                │ 
│  ┌──────────────────────────────────────────────────┐   │
│  │         Design Patterns Layer                    │   │
│  │  Singleton │ Decorator │ Chain of Responsibility │   │
│  └──────────────────────────────────────────────────┘   │
│                        ↕                                │
│  ┌──────────────────────────────────────────────────┐   │
│  │              Domain Models Layer                 │   │
│  │  Kaynak (Abstract) → Kitap, Dergi, Tez           │   │
│  │  Kullanici │ OduncKaydi │ IslemKaydi             │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

## Katmanlar ve Sorumluluklar

### 1. Frontend Layer (React)

**Teknoloji:** React 18.2, Vite, Axios

**Sorumluluklar:**
- Kullanıcı arayüzü
- API çağrıları
- State yönetimi (React hooks)
- Responsive tasarım

**Bileşenler:**
```
App.jsx (Main Container)
├── Dashboard.jsx (Genel bakış)
├── KaynakYonetimi.jsx (CRUD operasyonları)
├── KullaniciYonetimi.jsx (Kullanıcı yönetimi)
├── OduncIslemleri.jsx (Ödünç/İade)
├── OneriSistemi.jsx (Akıllı öneriler)
└── Istatistikler.jsx (Raporlar ve CSV export)
```

**API Service:**
- Merkezi axios instance
- Tüm HTTP istekleri buradan yapılır
- Error handling
- Base URL yönetimi

### 2. API Controllers Layer

**Sorumluluklar:**
- HTTP request/response yönetimi
- Routing
- Input validation
- DTO dönüşümleri
- Status code yönetimi

**Controllers:**
```csharp
KaynakController      → /api/kaynak
KullaniciController   → /api/kullanici
OduncController       → /api/odunc
OneriController       → /api/oneri
IstatistikController  → /api/istatistik
```

### 3. Business Logic Layer

**Sorumluluklar:**
- İş kuralları
- Veri işleme
- Algoritma implementasyonu
- Servis orchestration

**Services:**
```csharp
Kutuphane           → Kaynak yönetimi, Indexer
OneriSistemi        → Öneri algoritması, Filtre zinciri
IstatistikServisi   → Raporlama, CSV export
```

### 4. Design Patterns Layer

#### Singleton Pattern
```csharp
KutuphaneYoneticisi
├── Thread-safe implementation
├── Double-check locking
├── Central data management
└── Single source of truth
```

**Neden Singleton?**
- Tüm sistem tek bir kütüphane yöneticisi üzerinden çalışmalı
- Data consistency
- Memory efficiency
- Global access point

#### Decorator Pattern
```csharp
KaynakDecorator (Abstract)
├── PopulerKaynakDecorator    → Popülerlik ekler
├── EtiketliKaynakDecorator   → Etiketler ekler
└── KoleksiyonKaynakDecorator → Koleksiyon bilgisi ekler
```

**Neden Decorator?**
- Runtime'da dinamik özellik ekleme
- Open/Closed principle
- Inheritance patlaması önlenir
- Flexible composition

#### Chain of Responsibility Pattern
```csharp
OneriFiltresi (Abstract)
├── KategoriFiltresi        → 1. Filtre
├── IlgiAlaniFiltresi       → 2. Filtre
├── OkumaGecmisiFiltresi    → 3. Filtre
├── YasFiltresi             → 4. Filtre
└── PopulariteFiltresi      → 5. Filtre
```

**Neden Chain of Responsibility?**
- Filtreler birbirinden bağımsız
- Yeni filtre eklemek kolay
- Single Responsibility
- Flexible ordering

### 5. Domain Models Layer

#### Inheritance Hierarchy
```csharp
Kaynak (Abstract Base Class)
├── OzetGoster()      → Abstract
├── CezaHesapla()     → Abstract
├── TeslimSuresi()    → Abstract
├── OduncVer()        → Virtual
└── IadeAl()          → Virtual

├─→ Kitap
│   ├── SayfaSayisi
│   ├── YayinEvi
│   ├── Dil
│   ├── TeslimSuresi() → 14 gün
│   └── CezaHesapla() → 2 TL/gün

├─→ Dergi
│   ├── SayiNo
│   ├── YayinPeriyodu
│   ├── ISSN
│   ├── TeslimSuresi() → 7 gün
│   └── CezaHesapla() → 1 TL/gün

└─→ Tez
    ├── Universite
    ├── Bolum
    ├── DanismanAdi
    ├── TezTuru
    ├── TeslimSuresi() → 21 gün
    └── CezaHesapla() → 3 TL/gün
```

## Tasarım Prensipleri

### SOLID Principles

#### 1. Single Responsibility Principle (SRP) ✅
Her sınıf tek bir sorumluluğa sahip:
- `Kaynak`: Kaynak bilgilerini tutar
- `OneriSistemi`: Sadece öneri üretir
- `IstatistikServisi`: Sadece raporlama yapar

#### 2. Open/Closed Principle (OCP) ✅
- Decorator pattern ile extension without modification
- Yeni filtre eklemek için var olan kodu değiştirmiyoruz
- Yeni kaynak türü eklemek için abstract class'ı extend ediyoruz

#### 3. Liskov Substitution Principle (LSP) ✅
- Her `Kaynak` alt sınıfı, `Kaynak` yerine kullanılabilir
- Polimorfik davranış korunuyor
```csharp
List<Kaynak> kaynaklar = new List<Kaynak>();
kaynaklar.Add(new Kitap(...));
kaynaklar.Add(new Dergi(...));
kaynaklar.Add(new Tez(...));
// Hepsi aynı şekilde kullanılır
```

#### 4. Interface Segregation Principle (ISP) ✅
- Büyük interface'ler yerine spesifik abstract metotlar
- Her kaynak türü sadece ihtiyacı olan metotları override eder

#### 5. Dependency Inversion Principle (DIP) ✅
- Controllers → Services bağımlılığı
- Concrete class'lara değil, abstraction'lara bağımlılık
- Dependency injection ready

### Design Patterns Justification

#### Neden Singleton?
**Problem:** Birden fazla kütüphane yöneticisi olursa data inconsistency
**Çözüm:** Singleton pattern ile tek instance garantisi
**Avantaj:** 
- Thread-safe
- Lazy initialization
- Memory efficient

#### Neden Decorator?
**Problem:** Kaynaklara farklı kombinasyonlarda özellik ekleme gerekiyor
**Çözüm:** Decorator pattern ile runtime composition
**Avantaj:**
- Inheritance explosion önlenir
- Dynamic composition
- Single Responsibility

#### Neden Chain of Responsibility?
**Problem:** Öneri sisteminde sıralı filtreler uygulanmalı
**Çözüm:** Chain of Responsibility pattern
**Avantaj:**
- Filtreler bağımsız
- Easy to add/remove filters
- Flexible ordering
- Single Responsibility

## Öneri Sistemi Algoritması

### Skor Hesaplama Formülü

```
TotalScore = KategoriScore + IlgiAlaniScore + PopulariteScore + YazarScore + YenilikScore

KategoriScore = (kategori eşleşirse) ? 30 : 0
IlgiAlaniScore = (ilgi alanı eşleşirse) ? 25 : 0
PopulariteScore = min(okunmaSayisi / 10, 20)
YazarScore = (aynı yazardan okumuşsa) ? 15 : 0
YenilikScore = 
    if (yaş < 1 yıl) → 10
    else if (yaş < 3 yıl) → 5
    else → 0

Maksimum Skor = 100
```

### Filtre Akışı

```
1. KategoriFiltresi
   Input: Tüm mevcut kaynaklar
   Process: Kullanıcının kategorileriyle eşleştir
   Output: Kategori eşleşen kaynaklar + bazı random

2. IlgiAlaniFiltresi
   Input: Önceki filtreden gelen kaynaklar
   Process: İlgi alanlarıyla eşleştir
   Output: İlgi alanı uyumlu kaynaklar

3. OkumaGecmisiFiltresi
   Input: Önceki filtreden gelen kaynaklar
   Process: 
   - Daha önce okunanları çıkar
   - Aynı yazarların eserlerini öne çıkar
   Output: Yeni ve ilişkili kaynaklar

4. YasFiltresi
   Input: Önceki filtreden gelen kaynaklar
   Process: Yaş grubuna göre filtrele
   Output: Yaş grubuna uygun kaynaklar

5. PopulariteFiltresi
   Input: Önceki filtreden gelen kaynaklar
   Process: %66 popüler + %33 keşif
   Output: Dengeli öneri listesi
```

## Veri Akışı

### Ödünç Verme İşlemi

```
User Action → Frontend
    ↓
API Call (POST /api/odunc/odunc-ver)
    ↓
OduncController.OduncVer()
    ↓
KutuphaneYoneticisi.OduncVer()
    ↓ (parallel)
    ├→ KullaniciGetir()
    └→ KaynakGetir()
    ↓ (validation)
    ├→ Kullanıcı var mı?
    ├→ Kaynak var mı?
    └→ Kaynak mevcut mu?
    ↓ (if valid)
    ├→ kaynak.OduncVer()      → Polimorfik
    ├→ new OduncKaydi()
    ├→ kullanici.OduncEkle()
    └→ IslemKaydet()
    ↓
Response → Frontend
    ↓
UI Update
```

### Öneri Üretme İşlemi

```
User Selection → Frontend
    ↓
API Call (GET /api/oneri/kullanici/{id})
    ↓
OneriController.GetirKullaniciOnerileri()
    ↓
OneriSistemi.OnerilerUret()
    ↓
KutuphaneYoneticisi.KullaniciGetir()
KutuphaneYoneticisi.TumKaynaklariGetir()
    ↓
Filtre Zinciri Başlat
    ↓
KategoriFiltresi.Filtrele()
    ↓
IlgiAlaniFiltresi.Filtrele()
    ↓
OkumaGecmisiFiltresi.Filtrele()
    ↓
YasFiltresi.Filtrele()
    ↓
PopulariteFiltresi.Filtrele()
    ↓ (for each kaynak)
OneriSkoruHesapla()
OneriNedenleriniBelirleme()
    ↓
Skorlara Göre Sırala
    ↓
Top N Seç
    ↓
List<OneriSonucu> → Frontend
    ↓
UI Render (skorlar + nedenler)
```

## Performance Considerations

### Singleton Pattern
- **Memory**: Single instance → memory efficient
- **Thread Safety**: Lock contention minimize edildi
- **Initialization**: Lazy loading ile performans optimizasyonu

### Chain of Responsibility
- **Early Exit**: Yeterli kaynak bulununca chain durabilir
- **Caching**: Filtre sonuçları cache'lenebilir (future enhancement)
- **Parallel Filtering**: Bağımsız filtreler paralel çalışabilir (future)

### Indexer
- **O(1) Access**: Dictionary-based ISBN lookup
- **Memory**: Minimal overhead
- **Type Safety**: Compile-time checking

## Extensibility

### Yeni Kaynak Türü Eklemek
```csharp
public class EKitap : Kaynak
{
    public string Format { get; set; }
    public long DosyaBoyutu { get; set; }
    
    public override string OzetGoster() { /* ... */ }
    public override decimal CezaHesapla(int gun) { /* ... */ }
    public override int TeslimSuresi() { return 30; }
}
```

### Yeni Decorator Eklemek
```csharp
public class DijitalKaynakDecorator : KaynakDecorator
{
    public string DownloadLink { get; set; }
    public bool CevrimiciErisim { get; set; }
    
    public override string OzetGoster()
    {
        return base.OzetGoster() + $"\n📱 Çevrimiçi Erişim: {CevrimiciErisim}";
    }
}
```

### Yeni Filtre Eklemek
```csharp
public class DilFiltresi : OneriFiltresi
{
    public override List<Kaynak> Filtrele(
        List<Kaynak> kaynaklar, 
        Kullanici kullanici, 
        int hedefSayi)
    {
        // Kullanıcının tercih ettiği dillerdeki kaynakları filtrele
        var filtrelenmis = kaynaklar
            .Where(k => kullanici.TercihEdilenDiller.Contains(k.Dil))
            .ToList();
            
        return SonrakiFiltreUygula(filtrelenmis, kullanici, hedefSayi);
    }
}
```

## Testing Strategy

### Unit Tests
```csharp
[TestClass]
public class KaynakTests
{
    [TestMethod]
    public void Kitap_CezaHesapla_DogruHesaplar()
    {
        var kitap = new Kitap(...);
        var ceza = kitap.CezaHesapla(5);
        Assert.AreEqual(10.0m, ceza); // 5 gün * 2 TL
    }
}
```

### Integration Tests
```csharp
[TestMethod]
public async Task OduncVer_GecerliIslem_BasariliDonus()
{
    // Arrange
    var client = _factory.CreateClient();
    
    // Act
    var response = await client.PostAsync("/api/odunc/odunc-ver", ...);
    
    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
}
```

## Security Considerations

### Input Validation
- DTO validation attributes
- ModelState checking
- ISBN format validation

### CORS
- Sadece frontend origin'e izin
- Credentials support
- Header restrictions

### Error Handling
- Try-catch blokları
- Meaningful error messages
- No sensitive data exposure

## Future Enhancements

1. **Database Integration**: Entity Framework Core
2. **Authentication**: JWT tokens
3. **Caching**: Redis implementation
4. **Logging**: Serilog integration
5. **Real-time Updates**: SignalR
6. **Advanced Analytics**: ML.NET for recommendations
7. **Mobile App**: React Native
8. **Notification System**: Email/SMS alerts

---

Bu mimari dokümantasyonu, sistemin tüm katmanlarını, tasarım kararlarını ve genişletilebilirlik stratejilerini açıklamaktadır.