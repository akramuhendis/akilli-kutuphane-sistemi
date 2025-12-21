import { useState, useEffect } from 'react';
import { Sparkles, Star, TrendingUp, BookOpen, Users, Filter } from 'lucide-react';
import { oneriAPI, kullaniciAPI } from '../services/api';

function OneriSistemi() {
  const [kullanicilar, setKullanicilar] = useState([]);
  const [selectedUserId, setSelectedUserId] = useState('');
  const [oneriler, setOneriler] = useState([]);
  const [trending, setTrending] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    loadKullanicilar();
    loadTrending();
  }, []);

  useEffect(() => {
    if (selectedUserId) {
      loadOneriler();
    } else {
      setOneriler([]);
    }
  }, [selectedUserId]);

  const loadKullanicilar = async () => {
    try {
      const response = await kullaniciAPI.getAll();
      setKullanicilar(response.data);
    } catch (error) {
      console.error('Kullanıcılar yüklenirken hata:', error);
    }
  };

  const loadOneriler = async () => {
    if (!selectedUserId) return;

    setLoading(true);
    try {
      const response = await oneriAPI.getUserRecommendations(selectedUserId, 10);
      setOneriler(response.data || []);
    } catch (error) {
      console.error('Öneriler yüklenirken hata:', error);
    } finally {
      setLoading(false);
    }
  };

  const loadTrending = async () => {
    try {
      const response = await oneriAPI.getTrending(10);
      setTrending(response.data || []);
    } catch (error) {
      console.error('Trend kaynaklar yüklenirken hata:', error);
    }
  };

  const selectedKullanici = kullanicilar.find((k) => (k.id || k.Id) === selectedUserId);

  return (
    <div className="oneri-sistemi">
      {/* Kullanıcı Seçimi */}
      <div className="card mb-4">
        <h3 className="card-title">
          <Filter size={24} style={{ display: 'inline', marginRight: '8px' }} />
          Kullanıcıya Özel Öneriler
        </h3>

        <div className="input-group">
          <label className="input-label">Kullanıcı Seç</label>
          <select
            className="input"
            value={selectedUserId}
            onChange={(e) => setSelectedUserId(e.target.value)}
          >
            <option value="">Kullanıcı seçin...</option>
            {kullanicilar.map((k) => (
              <option key={k.id || k.Id} value={k.id || k.Id}>
                {k.ad || k.Ad} {k.soyad || k.Soyad} - {k.email || k.Email}
              </option>
            ))}
          </select>
        </div>

        {selectedKullanici && (
          <div style={{ marginTop: '16px', padding: '16px', background: 'var(--bg-secondary)', borderRadius: '8px' }}>
            <div className="flex-between">
              <div>
                <strong>{selectedKullanici.ad || selectedKullanici.Ad} {selectedKullanici.soyad || selectedKullanici.Soyad}</strong>
                <p style={{ fontSize: '14px', color: 'var(--text-muted)', margin: '4px 0 0 0' }}>
                  Yaş: {selectedKullanici.yas || selectedKullanici.Yas} | 
                  İlgi Alanları: {(selectedKullanici.ilgiAlanlari || selectedKullanici.IlgiAlanlari || []).join(', ') || 'Yok'}
                </p>
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Öneriler */}
      {loading && (
        <div className="flex-center" style={{ minHeight: '200px' }}>
          <div className="spinner"></div>
        </div>
      )}

      {!loading && selectedUserId && oneriler.length > 0 && (
        <div className="card mb-4">
          <h3 className="card-title">
            <Sparkles size={24} style={{ display: 'inline', marginRight: '8px' }} />
            Size Özel Öneriler ({oneriler.length})
          </h3>

          <div className="grid grid-2">
            {oneriler.map((oneri, index) => {
              const kaynak = oneri.kaynak || oneri;
              return (
                <div key={index} className="card fade-in" style={{ background: 'var(--bg-secondary)' }}>
                  <div className="flex-between mb-3">
                    <div style={{ flex: 1 }}>
                      <div className="flex gap-2" style={{ alignItems: 'center', marginBottom: '8px' }}>
                        <span className="badge badge-primary">#{oneri.sira || index + 1}</span>
                        {kaynak.kaynakTuru && (
                          <span className="badge" style={{ textTransform: 'capitalize' }}>
                            {kaynak.kaynakTuru}
                          </span>
                        )}
                      </div>
                      <h4 style={{ fontSize: '18px', fontWeight: 600, margin: '8px 0' }}>
                        {kaynak.baslik || kaynak.Baslik}
                      </h4>
                      <p style={{ fontSize: '14px', color: 'var(--text-secondary)', margin: '4px 0' }}>
                        {kaynak.yazar || kaynak.Yazar}
                      </p>
                    </div>
                    <div
                      style={{
                        width: '64px',
                        height: '64px',
                        borderRadius: '12px',
                        background: 'var(--primary-gradient)',
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        flexShrink: 0,
                      }}
                    >
                      <Star size={32} color="white" fill="white" />
                    </div>
                  </div>

                  {/* Öneri Skoru */}
                  {oneri.oneriSkoru !== undefined && (
                    <div style={{ marginBottom: '12px' }}>
                      <div className="flex-between" style={{ marginBottom: '4px' }}>
                        <span className="text-secondary" style={{ fontSize: '13px' }}>Öneri Skoru</span>
                        <strong style={{ fontSize: '18px' }}>{oneri.oneriSkoru.toFixed(1)}/100</strong>
                      </div>
                      <div
                        style={{
                          height: '8px',
                          background: 'var(--bg-primary)',
                          borderRadius: '4px',
                          overflow: 'hidden',
                        }}
                      >
                        <div
                          style={{
                            height: '100%',
                            width: `${Math.min(oneri.oneriSkoru, 100)}%`,
                            background: 'var(--primary-gradient)',
                            transition: 'width 0.5s ease',
                          }}
                        />
                      </div>
                    </div>
                  )}

                  {/* Öneri Nedenleri */}
                  {oneri.oneriNedenleri && oneri.oneriNedenleri.length > 0 && (
                    <div style={{ marginTop: '12px', paddingTop: '12px', borderTop: '1px solid var(--border-color)' }}>
                      <span className="text-secondary" style={{ fontSize: '12px', display: 'block', marginBottom: '8px' }}>
                        Öneri Nedenleri:
                      </span>
                      <ul style={{ margin: 0, paddingLeft: '20px', fontSize: '13px', color: 'var(--text-secondary)' }}>
                        {oneri.oneriNedenleri.map((neden, i) => (
                          <li key={i} style={{ marginBottom: '4px' }}>{neden}</li>
                        ))}
                      </ul>
                    </div>
                  )}

                  {/* Kaynak Detayları */}
                  <div style={{ marginTop: '12px', fontSize: '12px', color: 'var(--text-muted)' }}>
                    <div className="flex-between">
                      <span>ISBN: {kaynak.isbn || kaynak.ISBN}</span>
                      <span>Okunma: {kaynak.okunmaSayisi || kaynak.OkunmaSayisi || 0}</span>
                    </div>
                    {kaynak.kategori && (
                      <div style={{ marginTop: '4px' }}>
                        <span className="badge badge-success" style={{ fontSize: '11px' }}>
                          {kaynak.kategori || kaynak.Kategori}
                        </span>
                      </div>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {!loading && selectedUserId && oneriler.length === 0 && (
        <div className="card">
          <div className="text-center" style={{ padding: '40px', color: 'var(--text-muted)' }}>
            <Sparkles size={48} style={{ margin: '0 auto 16px', opacity: 0.5 }} />
            <p>Bu kullanıcı için henüz öneri bulunamadı.</p>
          </div>
        </div>
      )}

      {/* Trend Kaynaklar */}
      <div className="card">
        <h3 className="card-title">
          <TrendingUp size={24} style={{ display: 'inline', marginRight: '8px' }} />
          Trend Kaynaklar
        </h3>

        {trending.length > 0 ? (
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Sıra</th>
                  <th>Kaynak</th>
                  <th>Yazar</th>
                  <th>Kategori</th>
                  <th>Okunma Sayısı</th>
                </tr>
              </thead>
              <tbody>
                {trending.map((kaynak, index) => (
                  <tr key={index}>
                    <td>
                      <div
                        style={{
                          width: '32px',
                          height: '32px',
                          borderRadius: '8px',
                          background: index < 3 ? 'var(--primary-gradient)' : 'var(--bg-secondary)',
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'center',
                          color: 'white',
                          fontWeight: 700,
                        }}
                      >
                        {index + 1}
                      </div>
                    </td>
                    <td>
                      <strong>{kaynak.baslik || kaynak.Baslik}</strong>
                    </td>
                    <td>{kaynak.yazar || kaynak.Yazar}</td>
                    <td>{kaynak.kategori || kaynak.Kategori || '-'}</td>
                    <td>
                      <span className="badge badge-primary">
                        {kaynak.okunmaSayisi || kaynak.OkunmaSayisi || 0}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center" style={{ padding: '40px', color: 'var(--text-muted)' }}>
            Trend kaynak bulunamadı
          </div>
        )}
      </div>
    </div>
  );
}

export default OneriSistemi;
