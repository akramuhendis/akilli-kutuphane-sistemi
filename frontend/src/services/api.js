import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Kaynak API
export const kaynakAPI = {
  getAll: () => api.get('/kaynak'),
  getById: (isbn) => api.get(`/kaynak/${isbn}`),
  create: (data) => api.post('/kaynak', data),
  update: (isbn, data) => api.put(`/kaynak/${isbn}`, data),
  delete: (isbn) => api.delete(`/kaynak/${isbn}`),
  search: (query) => api.get(`/kaynak/ara/${query}`),
  getByCategory: (category) => api.get(`/kaynak/kategori/${category}`),
  getAvailable: () => api.get('/kaynak/mevcut'),
  getLoaned: () => api.get('/kaynak/odunc'),
};

// Kullanıcı API
export const kullaniciAPI = {
  getAll: () => api.get('/kullanici'),
  getById: (id) => api.get(`/kullanici/${id}`),
  create: (data) => api.post('/kullanici', data),
  update: (id, data) => api.put(`/kullanici/${id}`, data),
  delete: (id) => api.delete(`/kullanici/${id}`),
  getHistory: (id) => api.get(`/kullanici/${id}/gecmis`),
  getActiveLoans: (id) => api.get(`/kullanici/${id}/aktif-oduncler`),
  getCategories: (id) => api.get(`/kullanici/${id}/kategoriler`),
};

// Ödünç API
export const oduncAPI = {
  loan: (data) => api.post('/odunc/odunc-ver', data),
  return: (data) => api.post('/odunc/iade-al', data),
  getDelays: () => api.get('/odunc/gecikme-uyarilari'),
};

// Öneri API
export const oneriAPI = {
  getUserRecommendations: (userId, count = 10) => 
    api.get(`/oneri/kullanici/${userId}?sayi=${count}`),
  getSimilar: (isbn, count = 5) => 
    api.get(`/oneri/benzer/${isbn}?sayi=${count}`),
  getTrending: (count = 10) => 
    api.get(`/oneri/trend?sayi=${count}`),
  getByCategory: (category, count = 10) => 
    api.get(`/oneri/kategori/${category}?sayi=${count}`),
};

// İstatistik API
export const istatistikAPI = {
  getSummary: () => api.get('/istatistik/ozet'),
  getPopular: () => api.get('/istatistik/populer'),
  exportDaily: (date) => api.get(`/istatistik/export/gunluk/${date}`),
  exportPopular: () => api.get('/istatistik/export/populer'),
  exportDelays: () => api.get('/istatistik/export/gecikme'),
  exportUserActivity: () => api.get('/istatistik/export/kullanici-aktivite'),
  exportCategoryAnalysis: () => api.get('/istatistik/export/kategori-analizi'),
  getTransactionHistory: () => api.get('/istatistik/islem-gecmisi'),
};

export default api;
