# 📚 Akıllı Kütüphane Yönetim Sistemi

Modern kütüphane yönetimi ve akıllı öneri sistemi ile donatılmış kapsamlı bir web uygulaması.

## 🎯 Proje Amacı

Kütüphanedeki kaynakları yöneten, kullanıcı profillerine göre dinamik öneri üreten nesne tabanlı bir sistem tasarlamak.

## ⚙️ Teknik Gereksinimler

### 1. Polimorfizm ✅
- **Soyut Kaynak Sınıfı**: `Kaynak` abstract class
- **Alt Sınıflar**: 
  - `Kitap` - Kitaplar için
  - `Dergi` - Dergiler için
  - `Tez` - Tezler için
- **Override Metotlar**:
  - `OzetGoster()` - Her kaynak türü kendi özetini gösterir
  - `CezaHesapla()` - Her türün farklı ceza hesaplaması (Kitap: 2₺/gün, Dergi: 1₺/gün, Tez: 3₺/gün)
  - `TeslimSuresi()` - Her türün farklı teslim süresi (Kitap: 14 gün, Dergi: 7 gün, Tez: 21 gün)

### 2. Tasarım Desenleri ✅

#### **Decorator Pattern**
- `KaynakDecorator` - Kaynaklara dinamik özellikler ekler
- `PopulerKaynakDecorator` - Popülerlik ve editör seçimi ekler
- `EtiketliKaynakDecorator` - Etiketler ekler
- `KoleksiyonKaynakDecorator` - Koleksiyon bilgisi ekler

#### **Singleton Pattern**
- `KutuphaneYoneticisi` - Thread-safe tekil instance
- Tüm kaynak ve kullanıcı yönetimi merkezi olarak yapılır
- Double-check locking implementasyonu

#### **Chain of Responsibility Pattern**
- `OneriFiltresi` - Öneri sisteminin temel sınıfı
- **Filtre Zinciri**:
  1. `KategoriFiltresi` - Kategori bazlı filtreleme
  2. `IlgiAlaniFiltresi` - İlgi alanı bazlı filtreleme
  3. `OkumaGecmisiFiltresi` - Okuma geçmişi bazlı filtreleme
  4. `YasFiltresi` - Yaş bazlı filtreleme
  5. `PopulariteFiltresi` - Popülarite bazlı filtreleme

### 3. Indexer Kullanımı ✅
```csharp
// Kutuphane sınıfı ISBN numarasına göre indexer içerir
var kitap = kutuphane["978-3-16-148410-0"];
kutuphane["978-3-16-148410-0"] = yeniKitap;
```

### 4. UML Diyagramları ✅
- **Class Diagram** - Kaynak hiyerarşisi, kullanıcı, ödünç alma, öneri bileşenleri
- **Sequence Diagram** - Ödünç alma ve iade süreci
- **Activity Diagram** - Öneri sisteminin adım adım çalışma akışı

## 🌟 Fonksiyonel Özellikler

### Kaynak Yönetimi
- ✅ Kitap, Dergi, Tez ekleme/düzenleme/silme
- ✅ Gelişmiş arama (başlık, yazar, ISBN, kategori)
- ✅ Kategori bazlı filtreleme
- ✅ Mevcut/Ödünçte durumu takibi

### Kullanıcı Yönetimi
- ✅ Kullanıcı profilleri (ad, yaş, ilgi alanları)
- ✅ Okuma geçmişi takibi
- ✅ Favori kategoriler
- ✅ Aktif ödünç listesi

### Ödünç İşlemleri
- ✅ Ödünç verme/iade alma
- ✅ Otomatik gecikme hesaplama
- ✅ Ceza hesaplama (kaynak türüne göre)
- ✅ Gecikme uyarıları

### Akıllı Öneri Sistemi
- ✅ Kullanıcıya özel öneriler (5 filtre zinciri)
- ✅ Benzer kaynaklar
- ✅ Trend kaynaklar
- ✅ Kategori bazlı öneriler
- ✅ Öneri nedenlerinin açıklanması
- ✅ Skor bazlı sıralama (0-100)

### İstatistikler ve Raporlar
- ✅ En popüler 10 kaynak listesi
- ✅ Günlük kullanım istatistikleri
- ✅ Kategori analizi
- ✅ Kullanıcı aktivite raporları
- ✅ **CSV Export** - Tüm raporlar CSV formatında dışa aktarılabilir

## 🛠️ Teknoloji Stack

### Backend (C#)
- **Framework**: ASP.NET Core 8.0
- **API**: RESTful Web API
- **Patterns**: Singleton, Decorator, Chain of Responsibility
- **Architecture**: Clean Architecture, OOP Principles

### Frontend (React)
- **Framework**: React 18.2
- **Build Tool**: Vite
- **HTTP Client**: Axios
- **Icons**: Lucide React
- **Styling**: Custom CSS (Modern gradient design)

## 📁 Proje Yapısı

```
akıllı-kütüphane-yönetim-sistemi/
│
├── backend/
│   ├── Models/                      # Domain models
│   │   ├── Kaynak.cs               # Abstract base class
│   │   ├── Kitap.cs                # Polymorphic implementation
│   │   ├── Dergi.cs                # Polymorphic implementation
│   │   ├── Tez.cs                  # Polymorphic implementation
│   │   └── Kullanici.cs            # User model
│   │
│   ├── Patterns/                    # Design patterns
│   │   ├── Decorator/
│   │   │   └── KaynakDecorator.cs
│   │   ├── Singleton/
│   │   │   └── KutuphaneYoneticisi.cs
│   │   └── ChainOfResponsibility/
│   │       └── OneriFiltresi.cs
│   │
│   ├── Services/                    # Business logic
│   │   ├── Kutuphane.cs            # Library service with indexer
│   │   ├── OneriSistemi.cs         # Recommendation engine
│   │   └── IstatistikServisi.cs    # Statistics & CSV export
│   │
│   ├── Controllers/                 # API endpoints
│   │   ├── KaynakController.cs
│   │   ├── KullaniciController.cs
│   │   ├── OduncController.cs
│   │   ├── OneriController.cs
│   │   └── IstatistikController.cs
│   │
│   ├── Program.cs                   # Application entry point
│   └── SmartLibrary.csproj
│
├── frontend/
│   ├── src/
│   │   ├── components/              # React components
│   │   │   ├── Dashboard.jsx
│   │   │   ├── KaynakYonetimi.jsx
│   │   │   ├── KullaniciYonetimi.jsx
│   │   │   ├── OduncIslemleri.jsx
│   │   │   ├── OneriSistemi.jsx
│   │   │   └── Istatistikler.jsx
│   │   │
│   │   ├── services/
│   │   │   └── api.js              # API service layer
│   │   │
│   │   ├── App.jsx                 # Main app component
│   │   ├── main.jsx                # Entry point
│   │   └── index.css               # Global styles
│   │
│   ├── index.html
│   ├── package.json
│   └── vite.config.js
│
├── UML/                             # UML diagrams
│   ├── ClassDiagram.md             # Class structure
│   ├── SequenceDiagram.md          # Loan/return process
│   └── ActivityDiagram.md          # Recommendation flow
│
└── README.md                        # Project documentation
```

## 🚀 Kurulum ve Çalıştırma

### Backend (C# API)

```bash
# Backend dizinine git
cd backend

# NuGet paketlerini yükle
dotnet restore

# Uygulamayı çalıştır
dotnet run

# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend (React)

```bash
# Frontend dizinine git
cd frontend

# Bağımlılıkları yükle
npm install

# Development server'ı başlat
npm run dev

# Uygulama: http://localhost:3000
```

## 📊 API Endpoints

### Kaynak API
- `GET /api/kaynak` - Tüm kaynakları getir
- `GET /api/kaynak/{isbn}` - ISBN ile kaynak getir (Indexer kullanımı)
- `POST /api/kaynak` - Yeni kaynak ekle
- `PUT /api/kaynak/{isbn}` - Kaynak güncelle
- `DELETE /api/kaynak/{isbn}` - Kaynak sil
- `GET /api/kaynak/kategori/{kategori}` - Kategoriye göre getir
- `GET /api/kaynak/ara/{aramaMetni}` - Gelişmiş arama

### Kullanıcı API
- `GET /api/kullanici` - Tüm kullanıcıları getir
- `GET /api/kullanici/{id}` - Kullanıcı getir
- `POST /api/kullanici` - Yeni kullanıcı ekle
- `GET /api/kullanici/{id}/gecmis` - Okuma geçmişi
- `GET /api/kullanici/{id}/aktif-oduncler` - Aktif ödünçler

### Ödünç API
- `POST /api/odunc/odunc-ver` - Ödünç ver
- `POST /api/odunc/iade-al` - İade al
- `GET /api/odunc/gecikme-uyarilari` - Gecikme uyarıları

### Öneri API
- `GET /api/oneri/kullanici/{kullaniciId}` - Kişiselleştirilmiş öneriler
- `GET /api/oneri/benzer/{isbn}` - Benzer kaynaklar
- `GET /api/oneri/trend` - Trend kaynaklar
- `GET /api/oneri/kategori/{kategori}` - Kategori önerileri

### İstatistik API
- `GET /api/istatistik/ozet` - Özet istatistikler
- `GET /api/istatistik/populer` - En popüler 10 kaynak
- `GET /api/istatistik/export/gunluk/{tarih}` - Günlük rapor (CSV)
- `GET /api/istatistik/export/populer` - Popüler kaynaklar (CSV)
- `GET /api/istatistik/export/gecikme` - Gecikme raporu (CSV)
- `GET /api/istatistik/export/kullanici-aktivite` - Kullanıcı aktivitesi (CSV)
- `GET /api/istatistik/export/kategori-analizi` - Kategori analizi (CSV)

## 🎨 Özellikler

### Polimorfik Davranış Örneği
```csharp
// Her kaynak türü kendi cezasını hesaplar
Kaynak kitap = new Kitap(...);
decimal ceza1 = kitap.CezaHesapla(5); // 10 TL (2 TL/gün)

Kaynak dergi = new Dergi(...);
decimal ceza2 = dergi.CezaHesapla(5); // 5 TL (1 TL/gün)

Kaynak tez = new Tez(...);
decimal ceza3 = tez.CezaHesapla(5); // 15 TL (3 TL/gün)
```

### Decorator Pattern Örneği
```csharp
Kaynak kitap = new Kitap(...);
kitap = new PopulerKaynakDecorator(kitap, populeriteSeviyesi: 85, editorSecimi: true);
kitap = new EtiketliKaynakDecorator(kitap, new List<string> { "Klasik", "Edebiyat" });
string ozet = kitap.OzetGoster(); // Tüm ekstra özellikler dahil
```

### Chain of Responsibility Örneği
```csharp
// Filtre zinciri otomatik olarak uygulanır
var oneriler = oneriSistemi.OnerilerUret(kullaniciId, 10);
// Kategori → İlgi Alanı → Okuma Geçmişi → Yaş → Popülarite
```

### Indexer Kullanımı
```csharp
var kutuphane = new Kutuphane();
// Get - Kaynak okuma
var kitap = kutuphane["978-3-16-148410-0"];
// Set - Kaynak ekleme/güncelleme
kutuphane["978-3-16-148410-0"] = yeniKitap;
```

## 📈 Öneri Sistemi Detayları

### Skor Hesaplama (0-100)
1. **Kategori Uyumu** (0-30): Kullanıcının okuduğu/favori kategoriler
2. **İlgi Alanı Uyumu** (0-25): İlgi alanlarıyla eşleşme
3. **Popülarite** (0-20): Okunma sayısı
4. **Yazar Tanıdıklığı** (0-15): Daha önce okunan yazarlar
5. **Yenilik** (0-10): Son yıllarda yayınlananlar

### Filtre Zinciri Akışı
```
Tüm Kaynaklar
    ↓
[Kategori Filtresi] → Kullanıcının kategorileriyle eşleşenler
    ↓
[İlgi Alanı Filtresi] → İlgi alanlarıyla eşleşenler
    ↓
[Okuma Geçmişi Filtresi] → Daha önce okunmayanlar + benzer yazarlar
    ↓
[Yaş Filtresi] → Yaş grubuna uygun kaynaklar
    ↓
[Popülarite Filtresi] → Popüler + keşif karışımı
    ↓
Skor Hesaplama → Sıralama → Top N Öneri
```

## 📝 CSV Export Örnekleri

### Günlük İstatistikler
```csv
GÜNLÜK KÜTÜPHANE İSTATİSTİKLERİ
Tarih: 16.12.2025

İşlem Türü,Sayı
ODUNC_VERILDI,15
IADE_ALINDI,12
KAYNAK_EKLENDI,3

Saat,İşlem Türü,Açıklama
09:15:30,ODUNC_VERILDI,Mehmet Yılmaz - Suç ve Ceza
...
```

### Popüler Kaynaklar
```csv
EN POPÜLER KAYNAKLAR
Oluşturma Tarihi: 16.12.2025 14:30

Sıra,ISBN,Başlık,Yazar,Kategori,Okunma Sayısı,Tür
1,978-605-07-0456-2,Simyacı,Paulo Coelho,Felsefe,89,Kitap
2,978-605-375-125-4,1984,George Orwell,Distopya,67,Kitap
...
```

## 🎯 Geliştirme Notları

### Zorunlu Gereksinimlerin Karşılanması

✅ **Polimorfizm**: Kaynak hiyerarşisi ile tam implementasyon
✅ **Decorator Pattern**: Kaynaklara dinamik özellik ekleme
✅ **Singleton Pattern**: KutuphaneYoneticisi thread-safe implementasyon
✅ **Chain of Responsibility**: 5 aşamalı filtre zinciri
✅ **Indexer**: ISBN bazlı kaynak erişimi
✅ **UML**: Class, Sequence ve Activity diyagramları
✅ **CSV Export**: Tüm raporlar için CSV desteği
✅ **Okuma Geçmişi**: Kullanıcı profili takibi
✅ **Popüler Liste**: Dinamik top 10 listesi
✅ **Gecikme Uyarıları**: Otomatik gecikme kontrolü

## 👨‍💻 Geliştirici

Bu proje dönem projesi kapsamında geliştirilmiştir.

---

**Not**: Sistem örnek verilerle birlikte gelir. İlk çalıştırmada otomatik olarak kitap, dergi, tez ve kullanıcı verileri yüklenir.

