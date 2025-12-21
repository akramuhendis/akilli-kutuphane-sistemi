import { useState, useEffect } from 'react';
import { BarChart3, Download, TrendingUp, BookOpen, Users, AlertTriangle, FileText } from 'lucide-react';
import { istatistikAPI } from '../services/api';

function Istatistikler() {
  const [summary, setSummary] = useState(null);
  const [popular, setPopular] = useState([]);
  const [loading, setLoading] = useState(true);
  const [exportLoading, setExportLoading] = useState('');

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [summaryRes, popularRes] = await Promise.all([
        istatistikAPI.getSummary(),
        istatistikAPI.getPopular(),
      ]);
      setSummary(summaryRes.data);
      setPopular(popularRes.data || []);
    } catch (error) {
      console.error('İstatistikler yüklenirken hata:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleExport = async (type) => {
    setExportLoading(type);
    try {
      let response;
      switch (type) {
        case 'daily':
          const today = new Date().toISOString().split('T')[0];
          response = await istatistikAPI.exportDaily(today);
          break;
        case 'popular':
          response = await istatistikAPI.exportPopular();
          break;
        case 'delays':
          response = await istatistikAPI.exportDelays();
          break;
        case 'userActivity':
          response = await istatistikAPI.exportUserActivity();
          break;
        case 'categoryAnalysis':
          response = await istatistikAPI.exportCategoryAnalysis();
          break;
        default:
          return;
      }
      alert('CSV dosyası başarıyla oluşturuldu! Dosya yolu: ' + response.data.dosyaYolu);
    } catch (error) {
      console.error('Export hatası:', error);
      alert('Export işlemi başarısız oldu!');
    } finally {
      setExportLoading('');
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
    <div className="istatistikler">
      {/* Özet İstatistikler */}
      <div className="grid grid-3 mb-4">
        <div className="card">
          <div className="flex-between">
            <div>
              <p className="text-secondary" style={{ fontSize: '14px', marginBottom: '8px' }}>
                Toplam Kaynak
              </p>
              <h2 style={{ fontSize: '32px', fontWeight: 700, margin: 0 }}>
                {summary?.toplamKaynak || 0}
              </h2>
            </div>
            <BookOpen size={32} style={{ color: 'var(--text-muted)' }} />
          </div>
        </div>

        <div className="card">
          <div className="flex-between">
            <div>
              <p className="text-secondary" style={{ fontSize: '14px', marginBottom: '8px' }}>
                Toplam Kullanıcı
              </p>
              <h2 style={{ fontSize: '32px', fontWeight: 700, margin: 0 }}>
                {summary?.toplamKullanici || 0}
              </h2>
            </div>
            <Users size={32} style={{ color: 'var(--text-muted)' }} />
          </div>
        </div>

        <div className="card">
          <div className="flex-between">
            <div>
              <p className="text-secondary" style={{ fontSize: '14px', marginBottom: '8px' }}>
                Toplam Okunma
              </p>
              <h2 style={{ fontSize: '32px', fontWeight: 700, margin: 0 }}>
                {summary?.toplamOkunma || 0}
              </h2>
            </div>
            <TrendingUp size={32} style={{ color: 'var(--text-muted)' }} />
          </div>
        </div>
      </div>

      {/* Detaylı İstatistikler */}
      <div className="grid grid-2 mb-4">
        <div className="card">
          <h3 className="card-title">Kaynak Durumu</h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <div className="flex-between">
              <span className="text-secondary">Mevcut Kaynaklar</span>
              <strong>{summary?.mevcutKaynak || 0}</strong>
            </div>
            <div className="flex-between">
              <span className="text-secondary">Ödünç Verilen</span>
              <strong>{summary?.oduncVerilenKaynak || 0}</strong>
            </div>
            <div className="flex-between">
              <span className="text-secondary">Kategori Sayısı</span>
              <strong>{summary?.kategoriSayisi || 0}</strong>
            </div>
          </div>
        </div>

        <div className="card">
          <h3 className="card-title">Ödünç İstatistikleri</h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <div className="flex-between">
              <span className="text-secondary">Gecikmeli Ödünç</span>
              <span className="badge badge-danger">
                {summary?.gecikmeliOdunc || 0}
              </span>
            </div>
            <div className="flex-between">
              <span className="text-secondary">Toplam Ceza</span>
              <strong style={{ color: '#f5576c' }}>
                {summary?.toplamCeza?.toFixed(2) || '0.00'} TL
              </strong>
            </div>
          </div>
        </div>
      </div>

      {/* En Popüler 10 Kaynak */}
      <div className="card mb-4">
        <h3 className="card-title">
          <TrendingUp size={24} style={{ display: 'inline', marginRight: '8px' }} />
          En Popüler 10 Kaynak
        </h3>

        {popular.length > 0 ? (
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Sıra</th>
                  <th>Kaynak</th>
                  <th>Yazar</th>
                  <th>Kategori</th>
                  <th>Okunma Sayısı</th>
                  <th>Tür</th>
                </tr>
              </thead>
              <tbody>
                {popular.map((kaynak, index) => (
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
                    <td>
                      <span className="badge">
                        {kaynak.kaynakTuru || 'Bilinmiyor'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center" style={{ padding: '40px', color: 'var(--text-muted)' }}>
            Popüler kaynak bulunamadı
          </div>
        )}
      </div>

      {/* CSV Export Butonları */}
      <div className="card">
        <h3 className="card-title">
          <Download size={24} style={{ display: 'inline', marginRight: '8px' }} />
          Raporları Dışa Aktar (CSV)
        </h3>

        <div className="grid grid-2" style={{ gap: '16px' }}>
          <button
            className="btn btn-outline"
            onClick={() => handleExport('daily')}
            disabled={exportLoading === 'daily'}
            style={{ justifyContent: 'center', padding: '16px' }}
          >
            {exportLoading === 'daily' ? (
              <div className="spinner" style={{ width: '20px', height: '20px', margin: 0 }} />
            ) : (
              <>
                <FileText size={18} />
                Günlük İstatistikler
              </>
            )}
          </button>

          <button
            className="btn btn-outline"
            onClick={() => handleExport('popular')}
            disabled={exportLoading === 'popular'}
            style={{ justifyContent: 'center', padding: '16px' }}
          >
            {exportLoading === 'popular' ? (
              <div className="spinner" style={{ width: '20px', height: '20px', margin: 0 }} />
            ) : (
              <>
                <TrendingUp size={18} />
                Popüler Kaynaklar
              </>
            )}
          </button>

          <button
            className="btn btn-outline"
            onClick={() => handleExport('delays')}
            disabled={exportLoading === 'delays'}
            style={{ justifyContent: 'center', padding: '16px' }}
          >
            {exportLoading === 'delays' ? (
              <div className="spinner" style={{ width: '20px', height: '20px', margin: 0 }} />
            ) : (
              <>
                <AlertTriangle size={18} />
                Gecikme Raporu
              </>
            )}
          </button>

          <button
            className="btn btn-outline"
            onClick={() => handleExport('userActivity')}
            disabled={exportLoading === 'userActivity'}
            style={{ justifyContent: 'center', padding: '16px' }}
          >
            {exportLoading === 'userActivity' ? (
              <div className="spinner" style={{ width: '20px', height: '20px', margin: 0 }} />
            ) : (
              <>
                <Users size={18} />
                Kullanıcı Aktivite
              </>
            )}
          </button>

          <button
            className="btn btn-outline"
            onClick={() => handleExport('categoryAnalysis')}
            disabled={exportLoading === 'categoryAnalysis'}
            style={{ justifyContent: 'center', padding: '16px' }}
          >
            {exportLoading === 'categoryAnalysis' ? (
              <div className="spinner" style={{ width: '20px', height: '20px', margin: 0 }} />
            ) : (
              <>
                <BarChart3 size={18} />
                Kategori Analizi
              </>
            )}
          </button>
        </div>

        <div style={{ marginTop: '16px', padding: '12px', background: 'var(--bg-secondary)', borderRadius: '8px' }}>
          <p style={{ fontSize: '13px', color: 'var(--text-muted)', margin: 0 }}>
            📁 CSV dosyaları backend klasöründeki <code>exports</code> dizinine kaydedilir.
          </p>
        </div>
      </div>
    </div>
  );
}

export default Istatistikler;
