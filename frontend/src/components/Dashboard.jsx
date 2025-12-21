import { useState, useEffect } from 'react';
import { BookOpen, Users, BookCheck, AlertTriangle, TrendingUp, Clock } from 'lucide-react';
import { istatistikAPI, oduncAPI } from '../services/api';

function Dashboard() {
  const [summary, setSummary] = useState(null);
  const [delays, setDelays] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [summaryRes, delaysRes] = await Promise.all([
        istatistikAPI.getSummary(),
        oduncAPI.getDelays(),
      ]);
      setSummary(summaryRes.data);
      setDelays(delaysRes.data || []);
    } catch (error) {
      console.error('Dashboard verileri yüklenirken hata:', error);
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex-center" style={{ minHeight: '400px' }}>
        <div className="spinner"></div>
      </div>
    );
  }

  const stats = [
    {
      title: 'Toplam Kaynak',
      value: summary?.toplamKaynak || 0,
      icon: BookOpen,
      gradient: 'var(--primary-gradient)',
    },
    {
      title: 'Toplam Kullanıcı',
      value: summary?.toplamKullanici || 0,
      icon: Users,
      gradient: 'var(--secondary-gradient)',
    },
    {
      title: 'Ödünç Verilen',
      value: summary?.oduncVerilenKaynak || 0,
      icon: BookCheck,
      gradient: 'var(--success-gradient)',
    },
    {
      title: 'Gecikmeli Ödünç',
      value: summary?.gecikmeliOdunc || 0,
      icon: AlertTriangle,
      gradient: 'var(--warning-gradient)',
    },
    {
      title: 'Toplam Okunma',
      value: summary?.toplamOkunma || 0,
      icon: TrendingUp,
      gradient: 'var(--success-gradient)',
    },
    {
      title: 'Kategori Sayısı',
      value: summary?.kategoriSayisi || 0,
      icon: BookOpen,
      gradient: 'var(--primary-gradient)',
    },
  ];

  return (
    <div className="dashboard">
      {/* Stats Grid */}
      <div className="grid grid-3 mb-4">
        {stats.map((stat, index) => {
          const Icon = stat.icon;
          return (
            <div key={index} className="card fade-in" style={{ animationDelay: `${index * 0.1}s` }}>
              <div className="flex-between mb-2">
                <div>
                  <p className="text-muted" style={{ fontSize: '14px', marginBottom: '8px' }}>
                    {stat.title}
                  </p>
                  <h2 style={{ fontSize: '32px', fontWeight: 700, margin: 0 }}>
                    {stat.value.toLocaleString()}
                  </h2>
                </div>
                <div
                  style={{
                    width: '56px',
                    height: '56px',
                    borderRadius: '12px',
                    background: stat.gradient,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    boxShadow: '0 4px 15px rgba(0, 0, 0, 0.2)',
                  }}
                >
                  <Icon size={28} color="white" />
                </div>
              </div>
            </div>
          );
        })}
      </div>

      {/* Gecikme Uyarıları */}
      {delays.length > 0 && (
        <div className="card mb-4">
          <h3 className="card-title">
            <AlertTriangle size={24} style={{ display: 'inline', marginRight: '8px' }} />
            Gecikme Uyarıları
          </h3>
          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Kullanıcı</th>
                  <th>Kaynak</th>
                  <th>Gecikme Süresi</th>
                  <th>Ceza Tutarı</th>
                </tr>
              </thead>
              <tbody>
                {delays.map((delay, index) => (
                  <tr key={index}>
                    <td>{delay.kullaniciAd}</td>
                    <td>{delay.kaynakBaslik}</td>
                    <td>
                      <span className="badge badge-danger">
                        {delay.gecikmeGunSayisi} gün
                      </span>
                    </td>
                    <td>
                      <strong>{delay.ceza.toFixed(2)} TL</strong>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Özet Bilgiler */}
      <div className="grid grid-2">
        <div className="card">
          <h3 className="card-title">Sistem Durumu</h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <div className="flex-between">
              <span className="text-secondary">Mevcut Kaynaklar</span>
              <strong>{summary?.mevcutKaynak || 0}</strong>
            </div>
            <div className="flex-between">
              <span className="text-secondary">Aktif Ödünç</span>
              <strong>{summary?.oduncVerilenKaynak || 0}</strong>
            </div>
            <div className="flex-between">
              <span className="text-secondary">Toplam Ceza</span>
              <strong style={{ color: '#f5576c' }}>
                {summary?.toplamCeza?.toFixed(2) || '0.00'} TL
              </strong>
            </div>
          </div>
        </div>

        <div className="card">
          <h3 className="card-title">Hızlı Erişim</h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '12px' }}>
            <button className="btn btn-primary" style={{ width: '100%', justifyContent: 'center' }}>
              <BookOpen size={18} />
              Yeni Kaynak Ekle
            </button>
            <button className="btn btn-secondary" style={{ width: '100%', justifyContent: 'center' }}>
              <Users size={18} />
              Yeni Kullanıcı Ekle
            </button>
            <button className="btn btn-success" style={{ width: '100%', justifyContent: 'center' }}>
              <TrendingUp size={18} />
              İstatistikleri Görüntüle
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
