import { useState, useEffect } from 'react';
import { Plus, Edit, Trash2, User, Mail, Calendar, BookOpen } from 'lucide-react';
import { kullaniciAPI } from '../services/api';

function KullaniciYonetimi() {
  const [kullanicilar, setKullanicilar] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editingKullanici, setEditingKullanici] = useState(null);
  const [formData, setFormData] = useState({
    Ad: '',
    Soyad: '',
    Email: '',
    Yas: '',
    IlgiAlanlari: '',
    FavoriKategoriler: '',
  });

  useEffect(() => {
    loadKullanicilar();
  }, []);

  const loadKullanicilar = async () => {
    try {
      const response = await kullaniciAPI.getAll();
      setKullanicilar(response.data);
    } catch (error) {
      console.error('Kullanıcılar yüklenirken hata:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const kullaniciData = {
        ...formData,
        Yas: parseInt(formData.Yas),
        IlgiAlanlari: formData.IlgiAlanlari
          ? formData.IlgiAlanlari.split(',').map((s) => s.trim()).filter(Boolean)
          : [],
        FavoriKategoriler: formData.FavoriKategoriler
          ? formData.FavoriKategoriler.split(',').map((s) => s.trim()).filter(Boolean)
          : [],
      };

      if (editingKullanici) {
        await kullaniciAPI.update(editingKullanici.id || editingKullanici.Id, kullaniciData);
      } else {
        await kullaniciAPI.create(kullaniciData);
      }
      
      await loadKullanicilar();
      setShowModal(false);
      resetForm();
    } catch (error) {
      const errorMessage = error.response?.data?.mesaj || 'İşlem başarısız oldu!';
      alert(errorMessage);
      console.error('Kullanıcı kaydedilirken hata:', error);
    }
  };

  const handleDelete = async (id) => {
    if (!confirm('Bu kullanıcıyı silmek istediğinizden emin misiniz?')) return;

    try {
      await kullaniciAPI.delete(id);
      await loadKullanicilar();
      alert('Kullanıcı başarıyla silindi!');
    } catch (error) {
      const errorMessage = error.response?.data?.mesaj || 'Kullanıcı silinirken bir hata oluştu!';
      alert(errorMessage);
      console.error('Kullanıcı silinirken hata:', error);
    }
  };

  const resetForm = () => {
    setFormData({
      Ad: '',
      Soyad: '',
      Email: '',
      Yas: '',
      IlgiAlanlari: '',
      FavoriKategoriler: '',
    });
    setEditingKullanici(null);
  };

  if (loading) {
    return (
      <div className="flex-center" style={{ minHeight: '400px' }}>
        <div className="spinner"></div>
      </div>
    );
  }

  return (
    <div className="kullanici-yonetimi">
      {/* Header */}
      <div className="flex-between mb-4">
        <h2 style={{ fontSize: '24px', fontWeight: 700 }}>Kullanıcı Yönetimi</h2>
        <button className="btn btn-primary" onClick={() => { resetForm(); setShowModal(true); }}>
          <Plus size={18} />
          Yeni Kullanıcı
        </button>
      </div>

      {/* Kullanıcılar Grid */}
      <div className="grid grid-3">
        {kullanicilar.map((kullanici, index) => (
          <div key={kullanici.id || index} className="card fade-in">
            <div className="flex-between mb-3">
              <div className="flex gap-3" style={{ alignItems: 'center' }}>
                <div
                  style={{
                    width: '48px',
                    height: '48px',
                    borderRadius: '12px',
                    background: 'var(--primary-gradient)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                  }}
                >
                  <User size={24} color="white" />
                </div>
                <div>
                  <h3 style={{ fontSize: '18px', fontWeight: 600, margin: 0 }}>
                    {kullanici.ad || kullanici.Ad} {kullanici.soyad || kullanici.Soyad}
                  </h3>
                  <p style={{ fontSize: '14px', color: 'var(--text-muted)', margin: '4px 0 0 0' }}>
                    <Mail size={14} style={{ display: 'inline', marginRight: '4px' }} />
                    {kullanici.email || kullanici.Email}
                  </p>
                </div>
              </div>
            </div>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '12px', marginTop: '16px' }}>
              <div className="flex-between">
                <span className="text-secondary">Yaş</span>
                <strong>{kullanici.yas || kullanici.Yas}</strong>
              </div>

              {(kullanici.aktifOduncler || kullanici.AktifOduncler || []).length > 0 && (
                <div className="flex-between">
                  <span className="text-secondary">Aktif Ödünç</span>
                  <span className="badge badge-warning">
                    {(kullanici.aktifOduncler || kullanici.AktifOduncler || []).length} kaynak
                  </span>
                </div>
              )}

              {(kullanici.oduncGecmisi || kullanici.OduncGecmisi || []).length > 0 && (
                <div className="flex-between">
                  <span className="text-secondary">Toplam Ödünç</span>
                  <strong>{(kullanici.oduncGecmisi || kullanici.OduncGecmisi || []).length}</strong>
                </div>
              )}

              {(kullanici.ilgiAlanlari || kullanici.IlgiAlanlari || []).length > 0 && (
                <div>
                  <span className="text-secondary" style={{ fontSize: '12px', display: 'block', marginBottom: '4px' }}>
                    İlgi Alanları
                  </span>
                  <div className="flex gap-1" style={{ flexWrap: 'wrap' }}>
                    {(kullanici.ilgiAlanlari || kullanici.IlgiAlanlari || []).slice(0, 3).map((ilgi, i) => (
                      <span key={i} className="badge badge-primary" style={{ fontSize: '11px' }}>
                        {ilgi}
                      </span>
                    ))}
                  </div>
                </div>
              )}

              {(kullanici.favoriKategoriler || kullanici.FavoriKategoriler || []).length > 0 && (
                <div>
                  <span className="text-secondary" style={{ fontSize: '12px', display: 'block', marginBottom: '4px' }}>
                    Favori Kategoriler
                  </span>
                  <div className="flex gap-1" style={{ flexWrap: 'wrap' }}>
                    {(kullanici.favoriKategoriler || kullanici.FavoriKategoriler || []).slice(0, 2).map((kat, i) => (
                      <span key={i} className="badge badge-success" style={{ fontSize: '11px' }}>
                        {kat}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>

            <div className="flex gap-2" style={{ marginTop: '16px', paddingTop: '16px', borderTop: '1px solid var(--border-color)' }}>
              <button
                className="btn btn-outline"
                style={{ flex: 1, padding: '8px', fontSize: '13px' }}
                onClick={() => {
                  setEditingKullanici(kullanici);
                  setFormData({
                    Ad: kullanici.ad || kullanici.Ad || '',
                    Soyad: kullanici.soyad || kullanici.Soyad || '',
                    Email: kullanici.email || kullanici.Email || '',
                    Yas: kullanici.yas || kullanici.Yas || '',
                    IlgiAlanlari: (kullanici.ilgiAlanlari || kullanici.IlgiAlanlari || []).join(', '),
                    FavoriKategoriler: (kullanici.favoriKategoriler || kullanici.FavoriKategoriler || []).join(', '),
                  });
                  setShowModal(true);
                }}
              >
                <Edit size={14} />
                Düzenle
              </button>
              <button
                className="btn btn-danger"
                style={{ flex: 1, padding: '8px', fontSize: '13px' }}
                onClick={() => handleDelete(kullanici.id || kullanici.Id)}
              >
                <Trash2 size={14} />
                Sil
              </button>
            </div>
          </div>
        ))}
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
            style={{ maxWidth: '500px', width: '100%' }}
            onClick={(e) => e.stopPropagation()}
          >
            <h3 className="card-title">
              {editingKullanici ? 'Kullanıcı Düzenle' : 'Yeni Kullanıcı Ekle'}
            </h3>

            <form onSubmit={handleSubmit}>
              <div className="input-group">
                <label className="input-label">Ad *</label>
                <input
                  type="text"
                  className="input"
                  value={formData.Ad}
                  onChange={(e) => setFormData({ ...formData, Ad: e.target.value })}
                  required
                />
              </div>

              <div className="input-group">
                <label className="input-label">Soyad *</label>
                <input
                  type="text"
                  className="input"
                  value={formData.Soyad}
                  onChange={(e) => setFormData({ ...formData, Soyad: e.target.value })}
                  required
                />
              </div>

              <div className="input-group">
                <label className="input-label">Email *</label>
                <input
                  type="email"
                  className="input"
                  value={formData.Email}
                  onChange={(e) => setFormData({ ...formData, Email: e.target.value })}
                  required
                />
              </div>

              <div className="input-group">
                <label className="input-label">Yaş *</label>
                <input
                  type="number"
                  className="input"
                  value={formData.Yas}
                  onChange={(e) => setFormData({ ...formData, Yas: e.target.value })}
                  required
                  min="1"
                />
              </div>

              <div className="input-group">
                <label className="input-label">İlgi Alanları (virgülle ayırın)</label>
                <input
                  type="text"
                  className="input"
                  value={formData.IlgiAlanlari}
                  onChange={(e) => setFormData({ ...formData, IlgiAlanlari: e.target.value })}
                  placeholder="Örn: Bilim, Teknoloji, Edebiyat"
                />
              </div>

              <div className="input-group">
                <label className="input-label">Favori Kategoriler (virgülle ayırın)</label>
                <input
                  type="text"
                  className="input"
                  value={formData.FavoriKategoriler}
                  onChange={(e) => setFormData({ ...formData, FavoriKategoriler: e.target.value })}
                  placeholder="Örn: Klasik Edebiyat, Roman"
                />
              </div>

              <div className="flex gap-2" style={{ marginTop: '24px' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }}>
                  {editingKullanici ? 'Güncelle' : 'Ekle'}
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

export default KullaniciYonetimi;
