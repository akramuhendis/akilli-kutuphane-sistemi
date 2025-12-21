import { useState, useEffect } from 'react';
import { ArrowLeftRight, BookOpen, User, Calendar, AlertTriangle, CheckCircle } from 'lucide-react';
import { oduncAPI, kaynakAPI, kullaniciAPI } from '../services/api';

function OduncIslemleri() {
  const [oduncIslemi, setOduncIslemi] = useState({ kullaniciId: '', ISBN: '' });
  const [iadeIslemi, setIadeIslemi] = useState({ kullaniciId: '', ISBN: '' });
  const [kullanicilar, setKullanicilar] = useState([]);
  const [kaynaklar, setKaynaklar] = useState([]);
  const [mevcutKaynaklar, setMevcutKaynaklar] = useState([]);
  const [delays, setDelays] = useState([]);
  const [loading, setLoading] = useState(true);
  const [message, setMessage] = useState({ type: '', text: '' });

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [kullanicilarRes, kaynaklarRes, mevcutRes, delaysRes] = await Promise.all([
        kullaniciAPI.getAll(),
        kaynakAPI.getAll(),
        kaynakAPI.getAvailable(),
        oduncAPI.getDelays(),
      ]);
      setKullanicilar(kullanicilarRes.data);
      setKaynaklar(kaynaklarRes.data);
      setMevcutKaynaklar(mevcutRes.data);
      setDelays(delaysRes.data || []);
    } catch (error) {
      console.error('Veriler yüklenirken hata:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleOduncVer = async (e) => {
    e.preventDefault();
    try {
      await oduncAPI.loan(oduncIslemi);
      setMessage({ type: 'success', text: 'Kaynak başarıyla ödünç verildi!' });
      setOduncIslemi({ kullaniciId: '', ISBN: '' });
      await loadData();
      setTimeout(() => setMessage({ type: '', text: '' }), 3000);
    } catch (error) {
      setMessage({ type: 'error', text: error.response?.data?.mesaj || 'Ödünç verme işlemi başarısız oldu!' });
      setTimeout(() => setMessage({ type: '', text: '' }), 3000);
    }
  };

  const handleIadeAl = async (e) => {
    e.preventDefault();
    try {
      await oduncAPI.return(iadeIslemi);
      setMessage({ type: 'success', text: 'Kaynak başarıyla iade alındı!' });
      setIadeIslemi({ kullaniciId: '', ISBN: '' });
      await loadData();
      setTimeout(() => setMessage({ type: '', text: '' }), 3000);
    } catch (error) {
      setMessage({ type: 'error', text: error.response?.data?.mesaj || 'İade alma işlemi başarısız oldu!' });
      setTimeout(() => setMessage({ type: '', text: '' }), 3000);
    }
  };

  if (loading) {
    return (
      <div className="flex-center" style={{ minHeight: '400px' }}>
        <div className="spinner"></div>
      </div>
    );
  }

  return (
    <div className="odunc-islemleri">
      {message.text && (
        <div className={`alert ${message.type === 'success' ? 'alert-success' : 'alert-error'}`}>
          {message.type === 'success' ? <CheckCircle size={20} /> : <AlertTriangle size={20} />}
          {message.text}
        </div>
      )}

      <div className="grid grid-2 mb-4">
        {/* Ödünç Verme */}
        <div className="card">
          <h3 className="card-title">
            <ArrowLeftRight size={24} style={{ display: 'inline', marginRight: '8px' }} />
            Ödünç Ver
          </h3>

          <form onSubmit={handleOduncVer}>
            <div className="input-group">
              <label className="input-label">Kullanıcı Seç *</label>
              <select
                className="input"
                value={oduncIslemi.kullaniciId}
                onChange={(e) => setOduncIslemi({ ...oduncIslemi, kullaniciId: e.target.value })}
                required
              >
                <option value="">Kullanıcı seçin...</option>
                {kullanicilar.map((k) => (
                  <option key={k.id || k.Id} value={k.id || k.Id}>
                    {k.ad || k.Ad} {k.soyad || k.Soyad} ({k.email || k.Email})
                  </option>
                ))}
              </select>
            </div>

            <div className="input-group">
              <label className="input-label">Mevcut Kaynak Seç *</label>
              <select
                className="input"
                value={oduncIslemi.ISBN}
                onChange={(e) => setOduncIslemi({ ...oduncIslemi, ISBN: e.target.value })}
                required
              >
                <option value="">Kaynak seçin...</option>
                {mevcutKaynaklar.map((k) => (
                  <option key={k.isbn || k.ISBN} value={k.isbn || k.ISBN}>
                    {k.baslik || k.Baslik} - {k.yazar || k.Yazar}
                  </option>
                ))}
              </select>
            </div>

            <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
              <BookOpen size={18} />
              Ödünç Ver
            </button>
          </form>
        </div>

        {/* İade Alma */}
        <div className="card">
          <h3 className="card-title">
            <CheckCircle size={24} style={{ display: 'inline', marginRight: '8px' }} />
            İade Al
          </h3>

          <form onSubmit={handleIadeAl}>
            <div className="input-group">
              <label className="input-label">Kullanıcı Seç *</label>
              <select
                className="input"
                value={iadeIslemi.kullaniciId}
                onChange={(e) => {
                  setIadeIslemi({ ...iadeIslemi, kullaniciId: e.target.value, ISBN: '' });
                }}
                required
              >
                <option value="">Kullanıcı seçin...</option>
                {kullanicilar.map((k) => (
                  <option key={k.id || k.Id} value={k.id || k.Id}>
                    {k.ad || k.Ad} {k.soyad || k.Soyad}
                  </option>
                ))}
              </select>
            </div>

            <div className="input-group">
              <label className="input-label">Ödünç Alınan Kaynak Seç *</label>
              <select
                className="input"
                value={iadeIslemi.ISBN}
                onChange={(e) => setIadeIslemi({ ...iadeIslemi, ISBN: e.target.value })}
                required
                disabled={!iadeIslemi.kullaniciId}
              >
                <option value="">Önce kullanıcı seçin...</option>
                {iadeIslemi.kullaniciId &&
                  (kullanicilar.find((k) => (k.id || k.Id) === iadeIslemi.kullaniciId)
                    ?.aktifOduncler || []).map((odunc) => (
                    <option key={odunc.isbn || odunc.ISBN} value={odunc.isbn || odunc.ISBN}>
                      {odunc.kaynakBaslik || odunc.KaynakBaslik}
                    </option>
                  ))}
              </select>
            </div>

            <button type="submit" className="btn btn-success" style={{ width: '100%' }}>
              <CheckCircle size={18} />
              İade Al
            </button>
          </form>
        </div>
      </div>

      {/* Gecikme Uyarıları */}
      {delays.length > 0 && (
        <div className="card">
          <h3 className="card-title">
            <AlertTriangle size={24} style={{ display: 'inline', marginRight: '8px', color: '#f5576c' }} />
            Gecikme Uyarıları ({delays.length})
          </h3>

          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Kullanıcı</th>
                  <th>Kaynak</th>
                  <th>Ödünç Tarihi</th>
                  <th>Gecikme Süresi</th>
                  <th>Tahmini Ceza</th>
                </tr>
              </thead>
              <tbody>
                {delays.map((delay, index) => (
                  <tr key={index}>
                    <td>
                      <div className="flex gap-2" style={{ alignItems: 'center' }}>
                        <User size={16} />
                        {delay.kullaniciAd}
                      </div>
                    </td>
                    <td>{delay.kaynakBaslik}</td>
                    <td>
                      {delay.oduncTarihi ? new Date(delay.oduncTarihi).toLocaleDateString('tr-TR') : '-'}
                    </td>
                    <td>
                      <span className="badge badge-danger">
                        {delay.gecikmeGunSayisi} gün
                      </span>
                    </td>
                    <td>
                      <strong style={{ color: '#f5576c' }}>
                        {delay.ceza?.toFixed(2) || '0.00'} TL
                      </strong>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Aktif Ödünçler Listesi */}
      <div className="card">
        <h3 className="card-title">Aktif Ödünçler</h3>

        <div className="table-container">
          <table>
            <thead>
              <tr>
                <th>Kullanıcı</th>
                <th>Kaynak</th>
                <th>Ödünç Tarihi</th>
                <th>Durum</th>
              </tr>
            </thead>
            <tbody>
              {kullanicilar.flatMap((kullanici) =>
                (kullanici.aktifOduncler || kullanici.AktifOduncler || []).map((odunc, idx) => (
                  <tr key={`${kullanici.id || kullanici.Id}-${idx}`}>
                    <td>
                      {kullanici.ad || kullanici.Ad} {kullanici.soyad || kullanici.Soyad}
                    </td>
                    <td>{odunc.kaynakBaslik || odunc.KaynakBaslik}</td>
                    <td>
                      {odunc.oduncTarihi
                        ? new Date(odunc.oduncTarihi).toLocaleDateString('tr-TR')
                        : '-'}
                    </td>
                    <td>
                      {odunc.geciktiMi && odunc.geciktiMi() ? (
                        <span className="badge badge-danger">Gecikmiş</span>
                      ) : (
                        <span className="badge badge-success">Aktif</span>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

export default OduncIslemleri;
