import { useState, useEffect, useCallback } from 'react'
import './App.css'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'https://aiemotionproject.onrender.com'
const API_URL = `${API_BASE_URL}/api/SyncAnalyze`
const HISTORY_API_URL = `${API_BASE_URL}/api/EmotionRecords`

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
      setError('Please fill in both username and text fields.')
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
      setError(err.message || 'An error occurred. Please try again.')
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
        return 'Positive'
      case 'negative':
        return 'Negative'
      case 'neutral':
        return 'Neutral'
      default:
        return label || 'Unknown'
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
      setHistoryError(err.message || 'An error occurred while loading history records.')
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
          <h1 className="title">Emotion Analysis</h1>
          <button
            type="button"
            onClick={handleOpenHistory}
            className="history-button"
          >
            📜 History
          </button>
        </div>
          <p className="subtitle">Analyze the emotional tone of your text</p>
          <p className="subtitle">Project is not working good in English texts. It is not a problem, it is just a limitation of the model. Model is trained on Turkish texts.</p>

        <form onSubmit={handleSubmit} className="form">
          <div className="form-group">
            <label htmlFor="username">Username</label>
            <input
              type="text"
              id="username"
              name="username"
              value={formData.username}
              onChange={handleChange}
              placeholder="Enter your username"
              disabled={loading}
              className="input"
            />
          </div>

          <div className="form-group">
            <label htmlFor="text">Text to Analyze</label>
            <textarea
              id="text"
              name="text"
              value={formData.text}
              onChange={handleChange}
              placeholder="Enter the text you want to analyze here..."
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
            {loading ? 'Analyzing...' : 'Analyze'}
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
            <h2 className="result-title">Analysis Result</h2>
            
            <div className="result-item">
              <span className="result-label">Message:</span>
              <span className="result-value">{result.message}</span>
            </div>

            <div className="result-item">
              <span className="result-label">User:</span>
              <span className="result-value">{result.username}</span>
            </div>

            <div className="result-item">
              <span className="result-label">Text:</span>
              <span className="result-value text-preview">{result.text}</span>
            </div>

            <div className="result-row">
              <div className="result-item">
                <span className="result-label">Emotion:</span>
                <span
                  className="result-badge"
                  style={{ backgroundColor: getLabelColor(result.label) }}
                >
                  {getLabelText(result.label)}
                </span>
              </div>

              <div className="result-item">
                <span className="result-label">Score:</span>
                <span className="result-value score">
                  {(result.score * 100).toFixed(2)}%
                </span>
              </div>
            </div>

            {result.createdAt && (
              <div className="result-item">
                <span className="result-label">Date:</span>
                <span className="result-value">
                  {new Date(result.createdAt).toLocaleString('en-US')}
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
              <h2 className="modal-title">Analysis History</h2>
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
                <label htmlFor="filter-label">Emotion</label>
                <select
                  id="filter-label"
                  name="label"
                  value={historyFilters.label}
                  onChange={handleHistoryFilterChange}
                  className="filter-input"
                >
                  <option value="">All</option>
                  <option value="positive">Positive</option>
                  <option value="negative">Negative</option>
                  <option value="neutral">Neutral</option>
                </select>
              </div>
              <button
                type="button"
                onClick={fetchHistory}
                className="filter-button"
                disabled={historyLoading}
              >
                {historyLoading ? 'Loading...' : 'Filter'}
              </button>
            </div>

            <div className="modal-body">
              {historyLoading && (
                <div className="loading-overlay">
                  <div className="loading-message">Loading...</div>
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
                    <span>Total: {historyData.total} records</span>
                    <span>
                      Page {historyData.page} / {Math.ceil(historyData.total / historyData.pageSize)}
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
                              Score: {(item.score * 100).toFixed(2)}%
                            </span>
                            <span className="history-date">
                              {new Date(item.createdAt).toLocaleString('en-US')}
                            </span>
                          </div>
                        </div>
                      ))}
                    </div>
                  ) : !historyLoading ? (
                    <div className="empty-message">No records found.</div>
                  ) : null}

                  {historyData.total > historyData.pageSize && (
                    <div className="pagination">
                      <button
                        type="button"
                        onClick={() => handleHistoryPageChange(historyFilters.page - 1)}
                        disabled={historyFilters.page <= 1 || historyLoading}
                        className="pagination-button"
                      >
                        ← Previous
                      </button>
                      <span className="pagination-info">
                        Page {historyFilters.page} / {Math.ceil(historyData.total / historyData.pageSize)}
                      </span>
                      <button
                        type="button"
                        onClick={() => handleHistoryPageChange(historyFilters.page + 1)}
                        disabled={historyFilters.page >= Math.ceil(historyData.total / historyData.pageSize) || historyLoading}
                        className="pagination-button"
                      >
                        Next →
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

