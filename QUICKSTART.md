# 🚀 Hızlı Başlangıç Kılavuzu

## Sistem Gereksinimleri

### Backend (C#)
- **.NET 8.0 SDK** veya üzeri
- Windows, macOS veya Linux

### Frontend (React)
- **Node.js 18+** veya üzeri
- npm veya yarn

## Adım Adım Kurulum

### 1️⃣ .NET SDK Kurulumu

**.NET SDK yüklü mü kontrol edin:**
```bash
dotnet --version
```

**Yüklü değilse:**
- [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
- .NET 8.0 SDK'yı indirin ve yükleyin

### 2️⃣ Node.js Kurulumu

**Node.js yüklü mü kontrol edin:**
```bash
node --version
npm --version
```

**Yüklü değilse:**
- [https://nodejs.org](https://nodejs.org)
- LTS sürümünü indirin ve yükleyin

### 3️⃣ Backend Çalıştırma

```bash
# 1. Backend dizinine gidin
cd backend

# 2. Bağımlılıkları yükleyin
dotnet restore

# 3. Projeyi derleyin
dotnet build

# 4. Uygulamayı çalıştırın
dotnet run
```

**✅ Backend Hazır!**
- API: http://localhost:5000
- Swagger UI: http://localhost:5000/swagger
- Örnek veriler otomatik yüklendi

### 4️⃣ Frontend Çalıştırma

**Yeni bir terminal/komut satırı açın:**

```bash
# 1. Frontend dizinine gidin
cd frontend

# 2. Bağımlılıkları yükleyin
npm install

# 3. Development server'ı başlatın
npm run dev
```

**✅ Frontend Hazır!**
- Uygulama: http://localhost:3000
- Tarayıcınızda otomatik açılacak

## 🎯 İlk Kullanım

### Örnek Veriler
Sistem ilk çalıştırmada otomatik olarak örnek veriler yükler:

**Kitaplar:**
- Suç ve Ceza - Fyodor Dostoyevski
- 1984 - George Orwell
- Simyacı - Paulo Coelho
- İnce Memed - Yaşar Kemal
- Satranç - Stefan Zweig

**Dergiler:**
- Bilim ve Teknik
- National Geographic Türkiye

**Tezler:**
- Yapay Zeka ve Makine Öğrenmesi
- Sürdürülebilir Enerji Sistemleri

**Kullanıcılar:**
- Mehmet Yılmaz (mehmet.yilmaz@email.com)
- Zeynep Kaya (zeynep.kaya@email.com)
- Can Öztürk (can.ozturk@email.com)

### Test Senaryoları

#### Senaryo 1: Ödünç İşlemi
1. **Ödünç İşlemleri** sekmesine gidin
2. Bir kullanıcı seçin (örn: Mehmet Yılmaz)
3. Bir kaynak seçin (örn: 1984)
4. "Ödünç Ver" butonuna tıklayın
5. ✅ İşlem başarılı mesajını göreceksiniz

#### Senaryo 2: Öneriler
1. **Öneriler** sekmesine gidin
2. Bir kullanıcı seçin (örn: Mehmet Yılmaz)
3. Sistem otomatik olarak 10 öneri üretir
4. Her önerinin:
   - Öneri skoru (0-100)
   - Öneri nedenleri
   - Kaynak detayları gösterilir

#### Senaryo 3: İstatistikler
1. **İstatistikler** sekmesine gidin
2. Özet istatistikleri görüntüleyin
3. En popüler 10 kaynak listesini inceleyin
4. CSV raporlarını indirin:
   - Günlük İstatistikler
   - Popüler Kaynaklar
   - Gecikme Raporu
   - Kullanıcı Aktivitesi
   - Kategori Analizi

## 🔍 Özellik Testleri

### Polimorfizm Testi
```bash
# Backend terminal'inde
# Her kaynak türü kendi cezasını hesaplar:
# - Kitap: 2 TL/gün
# - Dergi: 1 TL/gün
# - Tez: 3 TL/gün
```

### Decorator Pattern Testi
Frontend'de kaynakları incelerken:
- Popülerlik seviyeleri
- Etiketler
- Koleksiyon bilgileri görünür

### Chain of Responsibility Testi
Öneriler sekmesinde:
1. Kategori filtresi
2. İlgi alanı filtresi
3. Okuma geçmişi filtresi
4. Yaş filtresi
5. Popülarite filtresi
Sırayla uygulanır ve öneri nedenleri gösterilir

### Indexer Testi
Swagger UI'da:
```
GET /api/kaynak/{isbn}
```
ISBN ile direkt kaynak erişimi test edin

## 🐛 Sorun Giderme

### Backend Sorunları

**Port zaten kullanımda:**
```bash
# appsettings.json'da port değiştirin
"Url": "http://localhost:5001"  # veya başka bir port
```

**CORS hatası:**
Backend'de CORS ayarları frontend URL'ini içeriyor. Farklı port kullanıyorsanız `Program.cs`'de güncelleyin.

### Frontend Sorunları

**Port zaten kullanımda:**
```bash
# vite.config.js'de port değiştirin
server: {
  port: 3001  # veya başka bir port
}
```

**API bağlantı hatası:**
Frontend'de `src/services/api.js` dosyasında backend URL'ini kontrol edin:
```javascript
const API_BASE_URL = 'http://localhost:5000/api';
```

## 📚 API Testi (Swagger)

1. Backend çalışırken şu adrese gidin: http://localhost:5000/swagger
2. Tüm endpoint'leri göreceksiniz
3. "Try it out" butonuyla test edebilirsiniz

### Örnek API Testleri

**Tüm kaynakları getir:**
```
GET /api/kaynak
```

**ISBN ile kaynak getir (Indexer):**
```
GET /api/kaynak/978-605-07-0456-2
```

**Kullanıcıya özel öneriler:**
```
GET /api/oneri/kullanici/{kullaniciId}?sayi=10
```

**Gecikme uyarıları:**
```
GET /api/odunc/gecikme-uyarilari
```

## 💡 İpuçları

1. **Backend'i önce başlatın**, sonra frontend'i
2. **Swagger UI** API'yi test etmek için çok kullanışlı
3. **Browser DevTools** (F12) ile network isteklerini izleyin
4. **Console'daki hataları** kontrol edin
5. **Örnek verileri** kullanarak sistemi keşfedin

## 📊 UML Diyagramlarını Görüntüleme

UML diyagramları PlantUML formatında yazılmıştır. Görüntülemek için:

### Online Görüntüleme
1. [PlantUML Online Editor](http://www.plantuml.com/plantuml/uml/)
2. UML dosyalarındaki kodu kopyalayın
3. Editor'e yapıştırın

### VS Code ile Görüntüleme
1. "PlantUML" extension'ını yükleyin
2. `.md` dosyalarını açın
3. `Alt+D` ile preview açın

## 🎓 Öğrenme Kaynakları

### Design Patterns
- **Decorator**: Kaynaklara dinamik özellikler ekleme
- **Singleton**: Tek instance yönetimi
- **Chain of Responsibility**: Filtre zinciri

### OOP Principles
- **Polimorfizm**: Kaynak hiyerarşisi
- **Encapsulation**: Private fields, public methods
- **Abstraction**: Abstract Kaynak sınıfı
- **Inheritance**: Alt sınıf implementasyonları

## 🚀 Üretim Ortamına Hazırlık

### Backend
```bash
cd backend
dotnet publish -c Release -o ./publish
```

### Frontend
```bash
cd frontend
npm run build
# dist klasöründeki dosyalar üretim için hazır
```

---

**Başarılar! 🎉**

Herhangi bir sorun yaşarsanız, console loglarını kontrol edin veya Swagger UI üzerinden API'yi test edin.

