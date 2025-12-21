import { useState, useEffect } from 'react';
import { Plus, Search, Edit, Trash2, BookOpen, BookMarked, FileText, Filter } from 'lucide-react';
import { kaynakAPI } from '../services/api';

function KaynakYonetimi() {
  const [kaynaklar, setKaynaklar] = useState([]);
  const [filteredKaynaklar, setFilteredKaynaklar] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingKaynak, setEditingKaynak] = useState(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterType, setFilterType] = useState('all');
  const [formData, setFormData] = useState({
    tur: 'kitap',
    ISBN: '',
    Baslik: '',
    Yazar: '',
    YayinTarihi: '',
    Kategori: '',
    SayfaSayisi: '',
    YayinEvi: '',
    Dil: 'Türkçe',
    SayiNo: '',
    YayinPeriyodu: 'Aylık',
    ISSN: '',
    Universite: '',
    Bolum: '',
    DanismanAdi: '',
    TezTuru: 'Yüksek Lisans',
  });

  useEffect(() => {
    loadKaynaklar();
  }, []);

  useEffect(() => {
    filterKaynaklar();
  }, [kaynaklar, searchTerm, filterType]);

  const loadKaynaklar = async () => {
    try {
      const response = await kaynakAPI.getAll();
      setKaynaklar(response.data);
    } catch (error) {
      console.error('Kaynaklar yüklenirken hata:', error);
    } finally {
      setLoading(false);
    }
  };

  const filterKaynaklar = () => {
    let filtered = kaynaklar;

    // Arama filtresi
    if (searchTerm) {
      filtered = filtered.filter(
        (k) =>
          k.baslik?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          k.yazar?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          k.isbn?.toLowerCase().includes(searchTerm.toLowerCase()) ||
          k.kategori?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Tür filtresi
    if (filterType !== 'all') {
      filtered = filtered.filter((k) => {
        const kaynakTuru = k.kaynakTuru || k.constructor?.name?.toLowerCase() || '';
        return kaynakTuru.toLowerCase() === filterType.toLowerCase();
      });
    }

    setFilteredKaynaklar(filtered);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      // Backend'e gönderilecek veriyi hazırla
      // Sayısal alanları parse et ve boş string'leri null'a çevir
      const kaynakData = {
        Tur: formData.tur, // PascalCase'e çevir
        ISBN: formData.ISBN || '',
        Baslik: formData.Baslik || '',
        Yazar: formData.Yazar || '',
        YayinTarihi: formData.YayinTarihi || new Date().toISOString().split('T')[0],
        Kategori: formData.Kategori || '',
        // Kitap özel alanları - string'leri number'a çevir
        SayfaSayisi: formData.SayfaSayisi && formData.SayfaSayisi !== '' 
          ? parseInt(formData.SayfaSayisi, 10) 
          : null,
        YayinEvi: formData.YayinEvi || null,
        Dil: formData.Dil || null,
        // Dergi özel alanları
        SayiNo: formData.SayiNo && formData.SayiNo !== '' 
          ? parseInt(formData.SayiNo, 10) 
          : null,
        YayinPeriyodu: formData.YayinPeriyodu || null,
        ISSN: formData.ISSN || null,
        // Tez özel alanları
        Universite: formData.Universite || null,
        Bolum: formData.Bolum || null,
        DanismanAdi: formData.DanismanAdi || null,
        TezTuru: formData.TezTuru || null,
      };

      // Debug: Gönderilen veriyi console'a yazdır
      console.log('Gönderilen kaynak verisi:', kaynakData);

      if (editingKaynak) {
        await kaynakAPI.update(editingKaynak.isbn || editingKaynak.ISBN, kaynakData);
      } else {
        await kaynakAPI.create(kaynakData);
      }

      await loadKaynaklar();
      setShowModal(false);
      resetForm();
    } catch (error) {
      console.error('Kaynak kaydedilirken hata:', error);
      // Hata mesajını daha detaylı göster
      const errorMessage = error.response?.data?.mesaj || error.message || 'Bilinmeyen hata';
      alert(`İşlem başarısız oldu: ${errorMessage}`);
    }
  };

  const handleDelete = async (isbn) => {
    if (!confirm('Bu kaynağı silmek istediğinizden emin misiniz?')) return;

    try {
      await kaynakAPI.delete(isbn);
      await loadKaynaklar();
    } catch (error) {
      console.error('Kaynak silinirken hata:', error);
      alert('Silme işlemi başarısız oldu!');
    }
  };

  const handleEdit = (kaynak) => {
    setEditingKaynak(kaynak);
    setFormData({
      tur: kaynak.kaynakTuru?.toLowerCase() || 'kitap',
      ISBN: kaynak.isbn || kaynak.ISBN || '',
      Baslik: kaynak.baslik || kaynak.Baslik || '',
      Yazar: kaynak.yazar || kaynak.Yazar || '',
      YayinTarihi: kaynak.yayinTarihi ? kaynak.yayinTarihi.split('T')[0] : '',
      Kategori: kaynak.kategori || kaynak.Kategori || '',
      SayfaSayisi: kaynak.sayfaSayisi || kaynak.SayfaSayisi || '',
      YayinEvi: kaynak.yayinEvi || kaynak.YayinEvi || '',
      Dil: kaynak.dil || kaynak.Dil || 'Türkçe',
      SayiNo: kaynak.sayiNo || kaynak.SayiNo || '',
      YayinPeriyodu: kaynak.yayinPeriyodu || kaynak.YayinPeriyodu || 'Aylık',
      ISSN: kaynak.issn || kaynak.ISSN || '',
      Universite: kaynak.universite || kaynak.Universite || '',
      Bolum: kaynak.bolum || kaynak.Bolum || '',
      DanismanAdi: kaynak.danismanAdi || kaynak.DanismanAdi || '',
      TezTuru: kaynak.tezTuru || kaynak.TezTuru || 'Yüksek Lisans',
    });
    setShowModal(true);
  };

  const resetForm = () => {
    setFormData({
      tur: 'kitap',
      ISBN: '',
      Baslik: '',
      Yazar: '',
      YayinTarihi: '',
      Kategori: '',
      SayfaSayisi: '',
      YayinEvi: '',
      Dil: 'Türkçe',
      SayiNo: '',
      YayinPeriyodu: 'Aylık',
      ISSN: '',
      Universite: '',
      Bolum: '',
      DanismanAdi: '',
      TezTuru: 'Yüksek Lisans',
    });
    setEditingKaynak(null);
  };

  const getKaynakIcon = (kaynak) => {
    const tur = kaynak.kaynakTuru || kaynak.constructor?.name?.toLowerCase() || '';
    if (tur.includes('kitap')) return <BookOpen size={20} />;
    if (tur.includes('dergi')) return <BookMarked size={20} />;
    if (tur.includes('tez')) return <FileText size={20} />;
    return <BookOpen size={20} />;
  };

  if (loading) {
    return (
      <div className="flex-center" style={{ minHeight: '400px' }}>
        <div className="spinner"></div>
      </div>
    );
  }

  return (
    <div className="kaynak-yonetimi">
      {/* Header Actions */}
      <div className="flex-between mb-4">
        <div className="flex gap-2" style={{ flex: 1, maxWidth: '600px' }}>
          <div className="input-group" style={{ flex: 1, marginBottom: 0 }}>
            <div style={{ position: 'relative' }}>
              <Search size={20} style={{ position: 'absolute', left: '12px', top: '12px', color: 'var(--text-muted)' }} />
              <input
                type="text"
                className="input"
                placeholder="Kaynak ara (başlık, yazar, ISBN, kategori)..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                style={{ paddingLeft: '40px' }}
              />
            </div>
          </div>
          <select
            className="input"
            value={filterType}
            onChange={(e) => setFilterType(e.target.value)}
            style={{ width: '150px' }}
          >
            <option value="all">Tümü</option>
            <option value="kitap">Kitap</option>
            <option value="dergi">Dergi</option>
            <option value="tez">Tez</option>
          </select>
        </div>
        <button className="btn btn-primary" onClick={() => { resetForm(); setShowModal(true); }}>
          <Plus size={18} />
          Yeni Kaynak
        </button>
      </div>

      {/* Kaynaklar Listesi */}
      <div className="card">
        <div className="flex-between mb-3">
          <h3 className="card-title">Kaynaklar ({filteredKaynaklar.length})</h3>
        </div>

        {filteredKaynaklar.length === 0 ? (
          <div className="text-center" style={{ padding: '40px', color: 'var(--text-muted)' }}>
            Kaynak bulunamadı
          </div>
        ) : (
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Tür</th>
                  <th>Başlık</th>
                  <th>Yazar</th>
                  <th>ISBN</th>
                  <th>Kategori</th>
                  <th>Durum</th>
                  <th>Okunma</th>
                  <th>İşlemler</th>
                </tr>
              </thead>
              <tbody>
                {filteredKaynaklar.map((kaynak, index) => (
                  <tr key={index}>
                    <td>
                      <div className="flex gap-2" style={{ alignItems: 'center' }}>
                        {getKaynakIcon(kaynak)}
                        <span style={{ textTransform: 'capitalize' }}>
                          {kaynak.kaynakTuru || 'Bilinmiyor'}
                        </span>
                      </div>
                    </td>
                    <td><strong>{kaynak.baslik || kaynak.Baslik}</strong></td>
                    <td>{kaynak.yazar || kaynak.Yazar}</td>
                    <td><code style={{ fontSize: '12px' }}>{kaynak.isbn || kaynak.ISBN}</code></td>
                    <td>{kaynak.kategori || kaynak.Kategori || '-'}</td>
                    <td>
                      <span className={`badge ${(kaynak.oduncDurumu || kaynak.OduncDurumu) ? 'badge-warning' : 'badge-success'}`}>
                        {(kaynak.oduncDurumu || kaynak.OduncDurumu) ? 'Ödünçte' : 'Mevcut'}
                      </span>
                    </td>
                    <td>{kaynak.okunmaSayisi || kaynak.OkunmaSayisi || 0}</td>
                    <td>
                      <div className="flex gap-2">
                        <button
                          className="btn btn-outline"
                          style={{ padding: '6px 12px', fontSize: '12px' }}
                          onClick={() => handleEdit(kaynak)}
                        >
                          <Edit size={14} />
                        </button>
                        <button
                          className="btn btn-danger"
                          style={{ padding: '6px 12px', fontSize: '12px' }}
                          onClick={() => handleDelete(kaynak.isbn || kaynak.ISBN)}
                        >
                          <Trash2 size={14} />
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      {/* Modal */}
      {showModal && (
        <div
          style={{
            position: 'fixed',
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: 'rgba(0, 0, 0, 0.7)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            zIndex: 2000,
            padding: '20px',
          }}
          onClick={() => setShowModal(false)}
        >
          <div
            className="card"
            style={{ maxWidth: '600px', width: '100%', maxHeight: '90vh', overflowY: 'auto' }}
            onClick={(e) => e.stopPropagation()}
          >
            <h3 className="card-title">
              {editingKaynak ? 'Kaynak Düzenle' : 'Yeni Kaynak Ekle'}
            </h3>

            <form onSubmit={handleSubmit}>
              <div className="input-group">
                <label className="input-label">Kaynak Türü</label>
                <select
                  className="input"
                  value={formData.tur}
                  onChange={(e) => setFormData({ ...formData, tur: e.target.value })}
                  required
                >
                  <option value="kitap">Kitap</option>
                  <option value="dergi">Dergi</option>
                  <option value="tez">Tez</option>
                </select>
              </div>

              <div className="input-group">
                <label className="input-label">ISBN *</label>
                <input
                  type="text"
                  className="input"
                  value={formData.ISBN}
                  onChange={(e) => setFormData({ ...formData, ISBN: e.target.value })}
                  required
                />
              </div>

              <div className="input-group">
                <label className="input-label">Başlık *</label>
                <input
                  type="text"
                  className="input"
                  value={formData.Baslik}
                  onChange={(e) => setFormData({ ...formData, Baslik: e.target.value })}
                  required
                />
              </div>

              <div className="input-group">
                <label className="input-label">Yazar *</label>
                <input
                  type="text"
                  className="input"
                  value={formData.Yazar}
                  onChange={(e) => setFormData({ ...formData, Yazar: e.target.value })}
                  required
                />
              </div>

              <div className="input-group">
                <label className="input-label">Yayın Tarihi</label>
                <input
                  type="date"
                  className="input"
                  value={formData.YayinTarihi}
                  onChange={(e) => setFormData({ ...formData, YayinTarihi: e.target.value })}
                />
              </div>

              <div className="input-group">
                <label className="input-label">Kategori</label>
                <input
                  type="text"
                  className="input"
                  value={formData.Kategori}
                  onChange={(e) => setFormData({ ...formData, Kategori: e.target.value })}
                />
              </div>

              {/* Kitap Özel Alanlar */}
              {formData.tur === 'kitap' && (
                <>
                  <div className="input-group">
                    <label className="input-label">Sayfa Sayısı</label>
                    <input
                      type="number"
                      className="input"
                      value={formData.SayfaSayisi}
                      onChange={(e) => setFormData({ ...formData, SayfaSayisi: e.target.value })}
                    />
                  </div>
                  <div className="input-group">
                    <label className="input-label">Yayın Evi</label>
                    <input
                      type="text"
                      className="input"
                      value={formData.YayinEvi}
                      onChange={(e) => setFormData({ ...formData, YayinEvi: e.target.value })}
                    />
                  </div>
                  <div className="input-group">
                    <label className="input-label">Dil</label>
                    <input
                      type="text"
                      className="input"
                      value={formData.Dil}
                      onChange={(e) => setFormData({ ...formData, Dil: e.target.value })}
                    />
                  </div>
                </>
              )}

              {/* Dergi Özel Alanlar */}
              {formData.tur === 'dergi' && (
                <>
                  <div className="input-group">
                    <label className="input-label">Sayı No</label>
                    <input
                      type="number"
                      className="input"
                      value={formData.SayiNo}
                      onChange={(e) => setFormData({ ...formData, SayiNo: e.target.value })}
                    />
                  </div>
                  <div className="input-group">
                    <label className="input-label">Yayın Periyodu</label>
                    <input
                      type="text"
                      className="input"
                      value={formData.YayinPeriyodu}
                      onChange={(e) => setFormData({ ...formData, YayinPeriyodu: e.target.value })}
                    />
                  </div>
                  <div className="input-group">
                    <label className="input-label">ISSN</label>
                    <input
                      type="text"
                      className="input"
                      value={formData.ISSN}
                      onChange={(e) => setFormData({ ...formData, ISSN: e.target.value })}
                    />
                  </div>
                </>
              )}

              {/* Tez Özel Alanlar */}
              {formData.tur === 'tez' && (
                <>
                  <div className="input-group">
                    <label className="input-label">Üniversite</label>
                    <input
                      type="text"
                      className="input"
                      value={formData.Universite}
                      onChange={(e) => setFormData({ ...formData, Universite: e.target.value })}
                    />
                  </div>
                  <div className="input-group">
                    <label className="input-label">Bölüm</label>
                    <input
                      type="text"
                      className="input"
                      value={formData.Bolum}
                      onChange={(e) => setFormData({ ...formData, Bolum: e.target.value })}
                    />
                  </div>
                  <div className="input-group">
                    <label className="input-label">Danışman Adı</label>
                    <input
                      type="text"
                      className="input"
                      value={formData.DanismanAdi}
                      onChange={(e) => setFormData({ ...formData, DanismanAdi: e.target.value })}
                    />
                  </div>
                  <div className="input-group">
                    <label className="input-label">Tez Türü</label>
                    <select
                      className="input"
                      value={formData.TezTuru}
                      onChange={(e) => setFormData({ ...formData, TezTuru: e.target.value })}
                    >
                      <option value="Yüksek Lisans">Yüksek Lisans</option>
                      <option value="Doktora">Doktora</option>
                    </select>
                  </div>
                </>
              )}

              <div className="flex gap-2" style={{ marginTop: '24px' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }}>
                  {editingKaynak ? 'Güncelle' : 'Ekle'}
                </button>
                <button
                  type="button"
                  className="btn btn-outline"
                  onClick={() => { setShowModal(false); resetForm(); }}
                >
                  İptal
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default KaynakYonetimi;
