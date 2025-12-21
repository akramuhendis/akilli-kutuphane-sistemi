# Akıllı Kütüphane Yönetim Sistemi - Frontend

Modern ve profesyonel React frontend uygulaması.

## 🚀 Hızlı Başlangıç

### Gereksinimler
- Node.js 18+ 
- npm veya yarn

### Kurulum

```bash
cd frontend
npm install
```

### Çalıştırma

```bash
npm run dev
```

Uygulama http://localhost:3000 adresinde çalışacaktır.

## 📦 Teknolojiler

- **React 18.2** - UI framework
- **Vite 5.0** - Build tool ve dev server
- **Axios** - HTTP client
- **Lucide React** - Icon library
- **CSS3** - Modern styling (gradients, animations)

## 🎨 Özellikler

- ✅ Modern ve responsive tasarım
- ✅ Dark theme
- ✅ Smooth animations
- ✅ Tüm backend endpoint'leri entegre
- ✅ Dashboard ile özet istatistikler
- ✅ Kaynak yönetimi (CRUD)
- ✅ Kullanıcı yönetimi
- ✅ Ödünç/İade işlemleri
- ✅ Akıllı öneri sistemi
- ✅ İstatistikler ve CSV export

## 📁 Proje Yapısı

```
frontend/
├── src/
│   ├── components/
│   │   ├── Dashboard.jsx
│   │   ├── KaynakYonetimi.jsx
│   │   ├── KullaniciYonetimi.jsx
│   │   ├── OduncIslemleri.jsx
│   │   ├── OneriSistemi.jsx
│   │   └── Istatistikler.jsx
│   ├── services/
│   │   └── api.js
│   ├── App.jsx
│   ├── App.css
│   ├── main.jsx
│   └── index.css
├── index.html
├── package.json
├── vite.config.js
└── README.md
```

## 🔗 Backend Bağlantısı

Backend API varsayılan olarak `http://localhost:5000` adresinde çalışmalıdır.

Vite config'de proxy ayarı mevcuttur:
- Development modunda `/api` istekleri otomatik olarak backend'e yönlendirilir.

## 🎯 Sayfalar

1. **Dashboard** - Genel bakış ve özet istatistikler
2. **Kaynaklar** - Kaynak yönetimi (Kitap, Dergi, Tez)
3. **Kullanıcılar** - Kullanıcı yönetimi
4. **Ödünç İşlemleri** - Ödünç verme, iade alma, gecikme uyarıları
5. **Öneri Sistemi** - Kullanıcıya özel öneriler ve trend kaynaklar
6. **İstatistikler** - Detaylı raporlar ve CSV export

## 🎨 Tasarım Özellikleri

- Dark theme with gradient accents
- Smooth transitions and animations
- Responsive design (mobile-friendly)
- Modern card-based layout
- Intuitive navigation

## 📝 Notlar

- Backend'in çalışır durumda olması gereklidir
- CORS ayarları backend'de yapılandırılmıştır
- Tüm API çağrıları `src/services/api.js` dosyasında merkezi olarak yönetilir
