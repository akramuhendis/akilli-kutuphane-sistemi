using System;
using System.Collections.Generic;
using System.Linq;
using SmartLibrary.Models;
using SmartLibrary.Patterns.Singleton;

namespace SmartLibrary.Services
{
    /// <summary>
    /// Kütüphane Servis Sınıfı
    /// 
    /// Bu sınıf, Indexer Pattern kullanarak kaynaklara erişim sağlar
    /// ve çeşitli arama/filtreleme operasyonları sunar.
    /// 
    /// Indexer Pattern:
    /// - Kaynaklara array-like syntax ile erişim: kutuphane["ISBN"]
    /// - GET: ISBN ile kaynak getirme
    /// - SET: Kaynak ekleme/güncelleme
    /// - Daha okunabilir ve sezgisel API
    /// 
    /// Özellikler:
    /// - ISBN bazlı kaynak erişimi (Indexer)
    /// - Kategori bazlı filtreleme
    /// - Gelişmiş arama (başlık, yazar, ISBN, kategori)
    /// - Durum bazlı sorgular (mevcut, ödünçte)
    /// - Yazar ve başlık bazlı arama
    /// 
    /// Singleton Dependency:
    /// - KutuphaneYoneticisi singleton'ını kullanır
    /// - Tüm işlemler merkezi yönetici üzerinden yapılır
    /// </summary>
    public class Kutuphane
    {
        /// <summary>
        /// Singleton kütüphane yöneticisi referansı
        /// Tüm kaynak işlemleri bu yönetici üzerinden yapılır
        /// </summary>
        private readonly KutuphaneYoneticisi _yonetici;

        /// <summary>
        /// Constructor
        /// Singleton instance'ını alır
        /// </summary>
        public Kutuphane()
        {
            _yonetici = KutuphaneYoneticisi.Instance;
        }

        // ==================== INDEXER PATTERN ====================

        /// <summary>
        /// ISBN ile kaynak erişimi için Indexer
        /// 
        /// Indexer Pattern Kullanımı:
        /// - GET: var kitap = kutuphane["978-3-16-148410-0"];
        /// - SET: kutuphane["978-3-16-148410-0"] = yeniKitap;
        /// 
        /// Avantajlar:
        /// - Array-like syntax (daha okunabilir)
        /// - Type-safe erişim
        /// - Exception handling
        /// - Kolay kullanım
        /// 
        /// Exception Handling:
        /// - GET: KeyNotFoundException (kaynak bulunamazsa)
        /// - SET: ArgumentNullException (null kaynak), ArgumentException (ISBN uyuşmazlığı)
        /// </summary>
        /// <param name="isbn">Kaynağın ISBN numarası</param>
        /// <returns>Kaynak nesnesi</returns>
        /// <exception cref="KeyNotFoundException">Kaynak bulunamadığında</exception>
        public Kaynak this[string isbn]
        {
            /// <summary>
            /// GET Accessor - ISBN ile kaynak getirme
            /// 
            /// İşlem:
            /// 1. Singleton yönetici üzerinden kaynak getirilir
            /// 2. Kaynak null ise KeyNotFoundException fırlatılır
            /// 3. Kaynak bulunursa döndürülür
            /// 
            /// Performans: O(1) Dictionary lookup
            /// </summary>
            get
            {
                var kaynak = _yonetici.KaynakGetir(isbn);
                
                // Kaynak bulunamazsa açıklayıcı hata mesajı ile exception fırlat
                if (kaynak == null)
                {
                    throw new KeyNotFoundException($"ISBN {isbn} numaralı kaynak bulunamadı.");
                }
                
                return kaynak;
            }

            /// <summary>
            /// SET Accessor - Kaynak ekleme/güncelleme
            /// 
            /// İşlem Adımları:
            /// 1. Null kontrolü (null kaynak eklenemez)
            /// 2. ISBN uyumluluk kontrolü (index ISBN'i ile kaynak ISBN'i eşleşmeli)
            /// 3. Mevcut kaynak kontrolü (varsa güncelleme, yoksa ekleme)
            /// 4. Singleton yönetici üzerinden işlem yapılır
            /// 
            /// Güncelleme Stratejisi:
            /// - Mevcut kaynak silinir
            /// - Yeni kaynak eklenir
            /// - Bu sayede tüm özellikler güncellenir (tür değişikliği de mümkün)
            /// </summary>
            /// <param name="value">Eklenecek/güncellenecek kaynak nesnesi</param>
            /// <exception cref="ArgumentNullException">Kaynak null ise</exception>
            /// <exception cref="ArgumentException">ISBN uyuşmazlığı varsa</exception>
            set
            {
                // Null kontrolü
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value), "Kaynak null olamaz.");
                }
                
                // ISBN uyumluluk kontrolü
                // Index'teki ISBN ile kaynak nesnesindeki ISBN eşleşmeli
                if (value.ISBN != isbn)
                {
                    throw new ArgumentException("ISBN numarası uyuşmuyor.");
                }

                // Mevcut kaynak var mı kontrol et
                var mevcutKaynak = _yonetici.KaynakGetir(isbn);
                
                if (mevcutKaynak != null)
                {
                    // Güncelleme: Önce eski kaynağı sil
                    _yonetici.KaynakSil(isbn);
                }
                
                // Yeni kaynağı ekle (veya güncellenmiş kaynağı)
                _yonetici.KaynakEkle(value);
            }
        }

        // ==================== YARDIMCI METOTLAR ====================

        /// <summary>
        /// ISBN ile kaynak var mı kontrolü
        /// 
        /// Indexer'ın tersine exception fırlatmaz, sadece bool döner.
        /// 
        /// Kullanım:
        /// - Kaynak eklemeden önce varlık kontrolü
        /// - Conditional işlemler
        /// </summary>
        /// <param name="isbn">Kontrol edilecek ISBN numarası</param>
        /// <returns>true: Kaynak var, false: Kaynak yok</returns>
        public bool KaynakVarMi(string isbn)
        {
            return _yonetici.KaynakGetir(isbn) != null;
        }

        // ==================== LİSTELEME VE FİLTRELEME ====================

        /// <summary>
        /// Tüm kaynakları listeler
        /// 
        /// Dönen Liste:
        /// - Kitap, Dergi ve Tez türündeki tüm kaynaklar
        /// - Polimorfik olarak Kaynak base class üzerinden erişilir
        /// </summary>
        /// <returns>Tüm kaynakların listesi</returns>
        public List<Kaynak> TumKaynaklar()
        {
            return _yonetici.TumKaynaklariGetir();
        }

        /// <summary>
        /// Kategoriye göre kaynakları filtreler
        /// 
        /// Filtreleme:
        /// - Case-insensitive karşılaştırma (büyük/küçük harf duyarsız)
        /// - Exact match (tam eşleşme)
        /// 
        /// Kullanım:
        /// - Kategori bazlı kaynak listeleme
        /// - Öneri sistemi için kategori filtreleme
        /// </summary>
        /// <param name="kategori">Filtrelenecek kategori adı</param>
        /// <returns>Kategoriye uygun kaynakların listesi</returns>
        public List<Kaynak> KategoriyeGoreFiltrele(string kategori)
        {
            return _yonetici.TumKaynaklariGetir()
                .Where(k => k.Kategori != null && k.Kategori.Equals(kategori, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Mevcut (ödünç verilmemiş) kaynakları getirir
        /// 
        /// Filtreleme Kriteri:
        /// - OduncDurumu = false olan kaynaklar
        /// 
        /// Kullanım:
        /// - Ödünç verme işlemlerinde mevcut kaynak listesi
        /// - Dashboard istatistikleri
        /// </summary>
        /// <returns>Mevcut kaynakların listesi</returns>
        public List<Kaynak> MevcutKaynaklar()
        {
            return _yonetici.TumKaynaklariGetir()
                .Where(k => !k.OduncDurumu)
                .ToList();
        }

        /// <summary>
        /// Şu anda ödünç verilmiş kaynakları getirir
        /// 
        /// Filtreleme Kriteri:
        /// - OduncDurumu = true olan kaynaklar
        /// 
        /// Kullanım:
        /// - Ödünç durumu takibi
        /// - İstatistik hesaplamaları
        /// - Gecikme kontrolü
        /// </summary>
        /// <returns>Ödünç verilmiş kaynakların listesi</returns>
        public List<Kaynak> OduncVerilenKaynaklar()
        {
            return _yonetici.TumKaynaklariGetir()
                .Where(k => k.OduncDurumu)
                .ToList();
        }

        // ==================== ARAMA METOTLARI ====================

        /// <summary>
        /// Yazar adına göre kaynak arar
        /// 
        /// Arama Özellikleri:
        /// - Partial match (kısmi eşleşme)
        /// - Case-insensitive (büyük/küçük harf duyarsız)
        /// - Contains kontrolü (içeriyor mu?)
        /// 
        /// Örnek:
        /// - Arama: "Orwell" → "George Orwell" yazarlı kitapları bulur
        /// - Arama: "dostoyevski" → "Fyodor Dostoyevski" yazarlı kitapları bulur
        /// </summary>
        /// <param name="yazar">Aranacak yazar adı (kısmi eşleşme yeterli)</param>
        /// <returns>Yazar adı içeren kaynakların listesi</returns>
        public List<Kaynak> YazaraGoreAra(string yazar)
        {
            return _yonetici.TumKaynaklariGetir()
                .Where(k => k.Yazar.Contains(yazar, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Başlığa göre kaynak arar
        /// 
        /// Arama Özellikleri:
        /// - Partial match (kısmi eşleşme)
        /// - Case-insensitive (büyük/küçük harf duyarsız)
        /// - Contains kontrolü
        /// 
        /// Örnek:
        /// - Arama: "1984" → "1984" başlıklı kitabı bulur
        /// - Arama: "suç" → "Suç ve Ceza" başlıklı kitabı bulur
        /// </summary>
        /// <param name="baslik">Aranacak başlık (kısmi eşleşme yeterli)</param>
        /// <returns>Başlığı içeren kaynakların listesi</returns>
        public List<Kaynak> BasligaGoreAra(string baslik)
        {
            return _yonetici.TumKaynaklariGetir()
                .Where(k => k.Baslik.Contains(baslik, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Gelişmiş çok kriterli arama
        /// 
        /// Arama Kriterleri (OR mantığı):
        /// - Başlık içinde arar
        /// - Yazar adı içinde arar
        /// - ISBN numarası içinde arar
        /// - Kategori içinde arar
        /// 
        /// Arama Özellikleri:
        /// - Multi-field search (çoklu alan araması)
        /// - Partial match (kısmi eşleşme)
        /// - Case-insensitive (büyük/küçük harf duyarsız)
        /// - OR mantığı (herhangi bir alanda eşleşme yeterli)
        /// 
        /// Kullanım Senaryoları:
        /// - Genel arama kutusu için
        /// - Kullanıcı arayüzünde tek bir arama alanı
        /// - Hızlı kaynak bulma
        /// 
        /// Örnek:
        /// - Arama: "1984" → "1984" başlıklı veya ISBN'inde "1984" geçen kaynaklar
        /// - Arama: "orwell" → Yazarı "Orwell" olan veya başlığında "orwell" geçen kaynaklar
        /// </summary>
        /// <param name="aramaMetni">Aranacak metin</param>
        /// <returns>Herhangi bir alanda eşleşen kaynakların listesi</returns>
        public List<Kaynak> GelismisArama(string aramaMetni)
        {
            return _yonetici.TumKaynaklariGetir()
                .Where(k => 
                    // Başlık içinde ara
                    k.Baslik.Contains(aramaMetni, StringComparison.OrdinalIgnoreCase) ||
                    // Yazar adı içinde ara
                    k.Yazar.Contains(aramaMetni, StringComparison.OrdinalIgnoreCase) ||
                    // ISBN numarası içinde ara
                    k.ISBN.Contains(aramaMetni, StringComparison.OrdinalIgnoreCase) ||
                    // Kategori içinde ara (null kontrolü ile)
                    (k.Kategori != null && k.Kategori.Contains(aramaMetni, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
    }
}
