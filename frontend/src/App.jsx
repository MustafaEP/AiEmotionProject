import { useState, useEffect, useCallback } from 'react'
import './App.css'

const API_URL = 'https://aiemotionproject.onrender.com/api/SyncAnalyze'
const HISTORY_API_URL = 'https://aiemotionproject.onrender.com/api/EmotionRecords'

function App() {
  const [formData, setFormData] = useState({
    username: '',
    text: ''
  })
  const [loading, setLoading] = useState(false)
  const [result, setResult] = useState(null)
  const [error, setError] = useState(null)
  
  const [showHistory, setShowHistory] = useState(false)
  const [historyLoading, setHistoryLoading] = useState(false)
  const [historyData, setHistoryData] = useState(null)
  const [historyError, setHistoryError] = useState(null)
  const [historyFilters, setHistoryFilters] = useState({
    label: '',
    page: 1,
    pageSize: 20
  })

  const handleChange = (e) => {
    const { name, value } = e.target
    setFormData(prev => ({
      ...prev,
      [name]: value
    }))
    setError(null)
    setResult(null)
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    
    if (!formData.username.trim() || !formData.text.trim()) {
      setError('Lütfen kullanıcı adı ve metin alanlarını doldurun.')
      return
    }

    setLoading(true)
    setError(null)
    setResult(null)

    try {
      const response = await fetch(API_URL, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'accept': '*/*'
        },
        body: JSON.stringify({
          username: formData.username,
          text: formData.text
        })
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const data = await response.json()
      setResult(data)
    } catch (err) {
      setError(err.message || 'Bir hata oluştu. Lütfen tekrar deneyin.')
      console.error('API Error:', err)
    } finally {
      setLoading(false)
    }
  }

  const getLabelColor = (label) => {
    switch (label?.toLowerCase()) {
      case 'positive':
        return '#10b981'
      case 'negative':
        return '#ef4444'
      case 'neutral':
        return '#6b7280'
      default:
        return '#6b7280'
    }
  }

  const getLabelText = (label) => {
    switch (label?.toLowerCase()) {
      case 'positive':
        return 'Pozitif'
      case 'negative':
        return 'Negatif'
      case 'neutral':
        return 'Nötr'
      default:
        return label || 'Bilinmiyor'
    }
  }

  const fetchHistory = useCallback(async () => {
    setHistoryLoading(true)
    setHistoryError(null)

    try {
      const params = new URLSearchParams()
      if (historyFilters.label) params.append('label', historyFilters.label)
      params.append('page', historyFilters.page)
      params.append('pageSize', historyFilters.pageSize)

      const response = await fetch(`${HISTORY_API_URL}?${params.toString()}`, {
        method: 'GET',
        headers: {
          'accept': '*/*'
        }
      })

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`)
      }

      const data = await response.json()
      setHistoryData(data)
    } catch (err) {
      setHistoryError(err.message || 'Geçmiş kayıtlar yüklenirken bir hata oluştu.')
      console.error('History API Error:', err)
    } finally {
      setHistoryLoading(false)
    }
  }, [historyFilters])

  const handleOpenHistory = () => {
    setShowHistory(true)
    fetchHistory()
  }

  const handleCloseHistory = () => {
    setShowHistory(false)
    setHistoryData(null)
    setHistoryError(null)
  }

  const handleHistoryFilterChange = (e) => {
    const { name, value } = e.target
    setHistoryFilters(prev => ({
      ...prev,
      [name]: value,
      page: 1 // Reset to first page when filter changes
    }))
  }

  const handleHistoryPageChange = (newPage) => {
    setHistoryFilters(prev => ({
      ...prev,
      page: newPage
    }))
  }

  useEffect(() => {
    if (showHistory) {
      fetchHistory()
    }
  }, [showHistory, fetchHistory])

  return (
    <div className="app-container">
      <div className="card">
        <div className="header-actions">
          <h1 className="title">Duygu Analizi</h1>
          <button
            type="button"
            onClick={handleOpenHistory}
            className="history-button"
          >
            📜 Geçmiş
          </button>
        </div>
        <p className="subtitle">Metninizin duygusal tonunu analiz edin</p>

        <form onSubmit={handleSubmit} className="form">
          <div className="form-group">
            <label htmlFor="username">Kullanıcı Adı</label>
            <input
              type="text"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="Kullanıcı adınızı girin"
              disabled={loading}
              className="input"
            />
          </div>

          <div className="form-group">
            <label htmlFor="text">Analiz Edilecek Metin</label>
            <textarea
              id="text"
              name="text"
              value={formData.text}
              onChange={handleChange}
              placeholder="Analiz etmek istediğiniz metni buraya yazın..."
              disabled={loading}
              rows="5"
              className="textarea"
            />
          </div>

          <button
            type="submit"
            disabled={loading}
            className="submit-button"
          >
            {loading ? 'Analiz Ediliyor...' : 'Analiz Et'}
          </button>
        </form>

        {error && (
          <div className="error-message">
            <span className="error-icon">⚠️</span>
            {error}
          </div>
        )}

        {result && (
          <div className="result-card">
            <h2 className="result-title">Analiz Sonucu</h2>
            
            <div className="result-item">
              <span className="result-label">Mesaj:</span>
              <span className="result-value">{result.message}</span>
            </div>

            <div className="result-item">
              <span className="result-label">Kullanıcı:</span>
              <span className="result-value">{result.username}</span>
            </div>

            <div className="result-item">
              <span className="result-label">Metin:</span>
              <span className="result-value text-preview">{result.text}</span>
            </div>

            <div className="result-row">
              <div className="result-item">
                <span className="result-label">Duygu:</span>
                <span
                  className="result-badge"
                  style={{ backgroundColor: getLabelColor(result.label) }}
                >
                  {getLabelText(result.label)}
                </span>
              </div>

              <div className="result-item">
                <span className="result-label">Skor:</span>
                <span className="result-value score">
                  {(result.score * 100).toFixed(2)}%
                </span>
              </div>
            </div>

            {result.createdAt && (
              <div className="result-item">
                <span className="result-label">Tarih:</span>
                <span className="result-value">
                  {new Date(result.createdAt).toLocaleString('tr-TR')}
                </span>
              </div>
            )}
          </div>
        )}
      </div>

      {/* History Modal */}
      {showHistory && (
        <div className="modal-overlay" onClick={handleCloseHistory}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2 className="modal-title">Geçmiş Analizler</h2>
              <button
                type="button"
                onClick={handleCloseHistory}
                className="modal-close-button"
              >
                ✕
              </button>
            </div>

            <div className="modal-filters">
              <div className="filter-group">
                <label htmlFor="filter-label">Duygu</label>
                <select
                  id="filter-label"
                  name="label"
                  value={historyFilters.label}
                  onChange={handleHistoryFilterChange}
                  className="filter-input"
                >
                  <option value="">Tümü</option>
                  <option value="positive">Pozitif</option>
                  <option value="negative">Negatif</option>
                  <option value="neutral">Nötr</option>
                </select>
              </div>
              <button
                type="button"
                onClick={fetchHistory}
                className="filter-button"
                disabled={historyLoading}
              >
                {historyLoading ? 'Yükleniyor...' : 'Filtrele'}
              </button>
            </div>

            <div className="modal-body">
              {historyLoading && (
                <div className="loading-overlay">
                  <div className="loading-message">Yükleniyor...</div>
                </div>
              )}

              {historyError && (
                <div className="error-message">
                  <span className="error-icon">⚠️</span>
                  {historyError}
                </div>
              )}

              {historyData && (
                <>
                  <div className="history-info">
                    <span>Toplam: {historyData.total} kayıt</span>
                    <span>
                      Sayfa {historyData.page} / {Math.ceil(historyData.total / historyData.pageSize)}
                    </span>
                  </div>

                  {historyData.items && historyData.items.length > 0 ? (
                    <div className="history-list">
                      {historyData.items.map((item) => (
                        <div key={item.id} className="history-item">
                          <div className="history-item-header">
                            <span className="history-username">{item.username}</span>
                            <span
                              className="history-badge"
                              style={{ backgroundColor: getLabelColor(item.label) }}
                            >
                              {getLabelText(item.label)}
                            </span>
                          </div>
                          <div className="history-text">{item.text}</div>
                          <div className="history-item-footer">
                            <span className="history-score">
                              Skor: {(item.score * 100).toFixed(2)}%
                            </span>
                            <span className="history-date">
                              {new Date(item.createdAt).toLocaleString('tr-TR')}
                            </span>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : !historyLoading ? (
                    <div className="empty-message">Kayıt bulunamadı.</div>
                  ) : null}

                  {historyData.total > historyData.pageSize && (
                    <div className="pagination">
                      <button
                        type="button"
                        onClick={() => handleHistoryPageChange(historyFilters.page - 1)}
                        disabled={historyFilters.page <= 1 || historyLoading}
                        className="pagination-button"
                      >
                        ← Önceki
                      </button>
                      <span className="pagination-info">
                        Sayfa {historyFilters.page} / {Math.ceil(historyData.total / historyData.pageSize)}
                      </span>
                      <button
                        type="button"
                        onClick={() => handleHistoryPageChange(historyFilters.page + 1)}
                        disabled={historyFilters.page >= Math.ceil(historyData.total / historyData.pageSize) || historyLoading}
                        className="pagination-button"
                      >
                        Sonraki →
                      </button>
                    </div>
                  )}
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default App

