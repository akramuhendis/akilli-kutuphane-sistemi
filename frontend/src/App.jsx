import { useState, useEffect } from 'react';
import { BookOpen, Users, ArrowLeftRight, Sparkles, BarChart3, Home, Menu, X } from 'lucide-react';
import Dashboard from './components/Dashboard';
import KaynakYonetimi from './components/KaynakYonetimi';
import KullaniciYonetimi from './components/KullaniciYonetimi';
import OduncIslemleri from './components/OduncIslemleri';
import OneriSistemi from './components/OneriSistemi';
import Istatistikler from './components/Istatistikler';
import './App.css';

function App() {
  const [activeTab, setActiveTab] = useState('dashboard');
  const [sidebarOpen, setSidebarOpen] = useState(true);

  const tabs = [
    { id: 'dashboard', label: 'Dashboard', icon: Home },
    { id: 'kaynaklar', label: 'Kaynaklar', icon: BookOpen },
    { id: 'kullanicilar', label: 'Kullanıcılar', icon: Users },
    { id: 'odunc', label: 'Ödünç İşlemleri', icon: ArrowLeftRight },
    { id: 'oneri', label: 'Öneri Sistemi', icon: Sparkles },
    { id: 'istatistikler', label: 'İstatistikler', icon: BarChart3 },
  ];

  const renderContent = () => {
    switch (activeTab) {
      case 'dashboard':
        return <Dashboard />;
      case 'kaynaklar':
        return <KaynakYonetimi />;
      case 'kullanicilar':
        return <KullaniciYonetimi />;
      case 'odunc':
        return <OduncIslemleri />;
      case 'oneri':
        return <OneriSistemi />;
      case 'istatistikler':
        return <Istatistikler />;
      default:
        return <Dashboard />;
    }
  };

  return (
    <div className="app-container">
      {/* Sidebar */}
      <aside className={`sidebar ${sidebarOpen ? 'open' : 'closed'}`}>
        <div className="sidebar-header">
          <div className="logo">
            <BookOpen size={32} />
            <span className="logo-text">Akıllı Kütüphane</span>
          </div>
          <button 
            className="sidebar-toggle"
            onClick={() => setSidebarOpen(!sidebarOpen)}
          >
            {sidebarOpen ? <X size={24} /> : <Menu size={24} />}
          </button>
        </div>
        
        <nav className="sidebar-nav">
          {tabs.map((tab) => {
            const Icon = tab.icon;
            return (
              <button
                key={tab.id}
                className={`nav-item ${activeTab === tab.id ? 'active' : ''}`}
                onClick={() => setActiveTab(tab.id)}
              >
                <Icon size={20} />
                {sidebarOpen && <span>{tab.label}</span>}
              </button>
            );
          })}
        </nav>

        <div className="sidebar-footer">
          <div className="version-info">
            {sidebarOpen && <span>v1.0.0</span>}
          </div>
        </div>
      </aside>

      {/* Main Content */}
      <main className="main-content">
        <header className="header">
          <div className="header-content">
            <h1 className="page-title">
              {tabs.find(t => t.id === activeTab)?.label || 'Dashboard'}
            </h1>
            <div className="header-actions">
              <div className="status-indicator">
                <div className="status-dot"></div>
                <span>Sistem Aktif</span>
              </div>
            </div>
          </div>
        </header>

        <div className="content-area fade-in">
          {renderContent()}
        </div>
      </main>
    </div>
  );
}

export default App;
