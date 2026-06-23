import { MapContainer, TileLayer } from 'react-leaflet'
import { useCallback, useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import L from 'leaflet'
import 'leaflet.markercluster'
import 'leaflet.markercluster/dist/MarkerCluster.css'
import 'leaflet.markercluster/dist/MarkerCluster.Default.css'
import { useMap } from 'react-leaflet'
import './App.css'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

const MAP_LAYERS = {
  standard: {
    label: 'Klasická',
    url: 'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
    attribution: '&copy; OpenStreetMap contributors',
  },
  satellite: {
    label: 'Satelitní',
    url: 'https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}',
    attribution: 'Tiles &copy; Esri',
  },
}

const EVENT_TYPES = [
  { value: 'all', label: 'Vše', icon: '🗺️' },
  { value: 'general', label: 'Obecné zprávy', icon: '📰' },
  { value: 'politics', label: 'Politika', icon: '🏛️' },
  { value: 'sport', label: 'Sport', icon: '⚽' },
  { value: 'culture', label: 'Kultura', icon: '🎭' },
  { value: 'tourism', label: 'Cestování', icon: '🧭' },
  { value: 'technology', label: 'Technologie', icon: '🤖' },
  { value: 'urban', label: 'Město / stavby', icon: '🏙️' },
  { value: 'traffic', label: 'Doprava', icon: '🚗' },
  { value: 'transport', label: 'MHD / železnice', icon: '🚆' },
  { value: 'crime', label: 'Krimi', icon: '🚓' },
  { value: 'fire', label: 'Požár', icon: '🔥' },
  { value: 'weather', label: 'Počasí', icon: '⛈️' },
  { value: 'environment', label: 'Životní prostředí', icon: '🌱' },
  { value: 'business', label: 'Ekonomika', icon: '💼' },
  { value: 'health', label: 'Zdraví', icon: '🏥' },
  { value: 'education', label: 'Školství', icon: '🎓' },
  { value: 'stabbing', label: 'Napadení nožem', icon: '🔪' },
  { value: 'shooting', label: 'Střelba', icon: '🔫' },
  { value: 'arrest', label: 'Zatčení', icon: '⛓️' },
  { value: 'fight', label: 'Potyčka', icon: '👊' },
  { value: 'robbery', label: 'Loupež', icon: '💰' },
  { value: 'unknown', label: 'Neznámé', icon: '❓' },
]

const EVENT_TYPE_BY_VALUE = Object.fromEntries(EVENT_TYPES.map((type) => [type.value, type]))

function getEventTypeMeta(eventType) {
  return EVENT_TYPE_BY_VALUE[eventType] ?? EVENT_TYPE_BY_VALUE.general
}

function getApiUrl(path) {
  return `${API_BASE_URL}${path}`
}

function getSafeSourceUrl(sourceUrl) {
  try {
    const url = new URL(sourceUrl)
    return url.protocol === 'http:' || url.protocol === 'https:' ? url.toString() : null
  } catch {
    return null
  }
}

function formatDate(value) {
  return new Intl.DateTimeFormat('cs-CZ', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  }).format(new Date(value))
}

function buildCountOptions(events, getValue) {
  const counts = events.reduce((result, event) => {
    const value = getValue(event)

    if (!value) {
      return result
    }

    result[value] = (result[value] ?? 0) + 1
    return result
  }, {})

  return Object.entries(counts)
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => b.count - a.count || a.name.localeCompare(b.name, 'cs-CZ'))
}

function getEventIcon(eventType) {
  const symbol = getEventTypeMeta(eventType).icon

  return L.divIcon({
    html: `<span>${symbol}</span>`,
    className: 'news-marker',
    iconSize: [34, 34],
    iconAnchor: [17, 17],
  })
}

function createPopupContent(event) {
  const type = getEventTypeMeta(event.eventType)
  const wrapper = document.createElement('div')
  wrapper.className = 'map-popup'

  const title = document.createElement('strong')
  title.textContent = event.title
  wrapper.append(title)

  const meta = document.createElement('div')
  meta.textContent = `${event.locationName ? `${event.locationName} · ` : ''}${event.sourceName} · ${type.icon} ${type.label} · ${formatDate(event.date)}`
  wrapper.append(meta)

  const safeUrl = getSafeSourceUrl(event.sourceUrl)
  if (safeUrl) {
    const link = document.createElement('a')
    link.href = safeUrl
    link.target = '_blank'
    link.rel = 'noopener noreferrer'
    link.textContent = 'Otevřít zdroj'
    wrapper.append(link)
  }

  return wrapper
}

function ClusterLayer({ events }) {
  const map = useMap()

  useEffect(() => {
    const markers = L.markerClusterGroup({
      showCoverageOnHover: false,
      maxClusterRadius: 42,
    })

    events.forEach((event) => {
      const marker = L.marker([event.latitude, event.longitude], {
        icon: getEventIcon(event.eventType),
      })

      marker.bindPopup(createPopupContent(event))
      markers.addLayer(marker)
    })

    map.addLayer(markers)

    return () => {
      map.removeLayer(markers)
    }
  }, [events, map])

  return null
}

function FilterPanel({
  categoryCounts,
  cityOptions,
  dataSource,
  dateFrom,
  dateTo,
  loading,
  mapLayer,
  onDataSourceChange,
  onDateFromChange,
  onDateToChange,
  onResetFilters,
  onMapLayerChange,
  onSearchChange,
  onSelectedCityChange,
  onSelectedSourceChange,
  onSelectedTypeChange,
  onSortOrderChange,
  onThemeModeChange,
  search,
  selectedCity,
  selectedSource,
  selectedType,
  sortOrder,
  sourceOptions,
  themeMode,
  totalEvents,
  visibleEvents,
}) {
  const activeCategoryCount = EVENT_TYPES.filter((type) => categoryCounts[type.value]).length

  return (
    <aside className="control-panel" aria-label="Filtry zpráv">
      <div className="brand-block">
        <h1>Czech News Map</h1>
        <p>Geolokované zprávy z českých médií</p>
      </div>

      <div className="metric-row" aria-live="polite">
        <div>
          <span className="metric-value">{visibleEvents}</span>
          <span className="metric-label">na mapě</span>
        </div>
        <div>
          <span className="metric-value">{totalEvents}</span>
          <span className="metric-label">načteno</span>
        </div>
      </div>

      {loading && <div className="status-pill">Načítám zprávy...</div>}

      <div className="mode-grid">
        <div className="mode-control">
          <span>Motiv</span>
          <div className="segmented-control" role="group" aria-label="Motiv aplikace">
            <button
              className={themeMode === 'dark' ? 'is-selected' : ''}
              type="button"
              onClick={() => onThemeModeChange('dark')}
            >
              Tmavý
            </button>
            <button
              className={themeMode === 'light' ? 'is-selected' : ''}
              type="button"
              onClick={() => onThemeModeChange('light')}
            >
              Světlý
            </button>
          </div>
        </div>

        <div className="mode-control">
          <span>Mapa</span>
          <div className="segmented-control" role="group" aria-label="Vrstva mapy">
            <button
              className={mapLayer === 'standard' ? 'is-selected' : ''}
              type="button"
              onClick={() => onMapLayerChange('standard')}
            >
              Klasická
            </button>
            <button
              className={mapLayer === 'satellite' ? 'is-selected' : ''}
              type="button"
              onClick={() => onMapLayerChange('satellite')}
            >
              Satelitní
            </button>
          </div>
        </div>
      </div>

      <div className="form-grid">
        <label className="field">
          <span>Data</span>
          <select value={dataSource} onChange={(e) => onDataSourceChange(e.target.value)}>
            <option value="rss">Živé zdroje</option>
            <option value="demo">Demo</option>
          </select>
        </label>

        <label className="field">
          <span>Typ</span>
          <select value={selectedType} onChange={(e) => onSelectedTypeChange(e.target.value)}>
            {EVENT_TYPES.map((type) => (
              <option key={type.value} value={type.value}>
                {type.icon} {type.label}
              </option>
            ))}
          </select>
        </label>

        <label className="field">
          <span>Médium</span>
          <select value={selectedSource} onChange={(e) => onSelectedSourceChange(e.target.value)}>
            <option value="all">Všechna média</option>
            {sourceOptions.map((source) => (
              <option key={source.name} value={source.name}>
                {source.name} ({source.count})
              </option>
            ))}
          </select>
        </label>

        <label className="field">
          <span>Město</span>
          <select value={selectedCity} onChange={(e) => onSelectedCityChange(e.target.value)}>
            <option value="all">Všechna města</option>
            {cityOptions.map((city) => (
              <option key={city.name} value={city.name}>
                {city.name} ({city.count})
              </option>
            ))}
          </select>
        </label>

        <label className="field field-wide">
          <span>Hledat</span>
          <input
            type="search"
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
            placeholder="Název, město, zdroj nebo typ"
          />
        </label>

        <label className="field">
          <span>Datum od</span>
          <input type="date" value={dateFrom} onChange={(e) => onDateFromChange(e.target.value)} />
        </label>

        <label className="field">
          <span>Datum do</span>
          <input type="date" value={dateTo} onChange={(e) => onDateToChange(e.target.value)} />
        </label>

        <label className="field field-wide">
          <span>Řazení</span>
          <select value={sortOrder} onChange={(e) => onSortOrderChange(e.target.value)}>
            <option value="newest">Od nejnovějších</option>
            <option value="oldest">Od nejstarších</option>
          </select>
        </label>
      </div>

      <button className="reset-button" type="button" onClick={onResetFilters}>
        Resetovat filtry
      </button>

      <div className="legend-block">
        <div className="section-title">Kategorie</div>
        <div className="legend-list">
          {EVENT_TYPES.filter((type) => type.value !== 'all' && categoryCounts[type.value]).map((type) => (
            <button
              className={selectedType === type.value ? 'legend-chip is-selected' : 'legend-chip'}
              key={type.value}
              type="button"
              onClick={() => onSelectedTypeChange(type.value)}
            >
              <span>{type.icon}</span>
              <span>{type.label}</span>
              <strong>{categoryCounts[type.value]}</strong>
            </button>
          ))}
        </div>
        {activeCategoryCount === 0 && <div className="empty-note">Kategorie se zobrazí po načtení dat.</div>}
      </div>
    </aside>
  )
}

function NewsCard({ event }) {
  const type = getEventTypeMeta(event.eventType)
  const safeUrl = getSafeSourceUrl(event.sourceUrl)

  return (
    <article className="news-card">
      <div className="news-card-topline">
        <span className="category-badge">{type.icon} {type.label}</span>
        <time dateTime={event.date}>{formatDate(event.date)}</time>
      </div>
      <h3>{event.title}</h3>
      <div className="news-meta">
        <span>{event.locationName ? `${event.locationName} · ` : ''}{event.sourceName}</span>
      </div>
      {safeUrl && (
        <a className="source-link" href={safeUrl} target="_blank" rel="noopener noreferrer">
          Otevřít zdroj
        </a>
      )}
    </article>
  )
}

function NewsList({ error, events, loading, onRetry }) {
  return (
    <section className="list-panel" aria-label="Seznam zpráv">
      <div className="panel-heading">
        <div>
          <h2>Události</h2>
          <p>{events.length} položek podle aktuálních filtrů</p>
        </div>
      </div>

      {error && (
        <div className="error-state">
          <strong>Nepodařilo se načíst zprávy</strong>
          <span>{error}</span>
          <button type="button" onClick={onRetry} disabled={loading}>
            Zkusit znovu
          </button>
        </div>
      )}
      {loading && <div className="empty-state">Načítám aktuální zprávy...</div>}
      {!loading && !error && events.length === 0 && (
        <div className="empty-state">Žádné zprávy neodpovídají aktuálním filtrům.</div>
      )}

      <div className="news-list">
        {events.map((event, i) => (
          <NewsCard event={event} key={`${event.sourceUrl}-${event.title}-${i}`} />
        ))}
      </div>
    </section>
  )
}

function App() {
  const [events, setEvents] = useState([])
  const [selectedType, setSelectedType] = useState('all')
  const [selectedSource, setSelectedSource] = useState('all')
  const [selectedCity, setSelectedCity] = useState('all')
  const [dataSource, setDataSource] = useState('rss')
  const [sortOrder, setSortOrder] = useState('newest')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [search, setSearch] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [activeMobileView, setActiveMobileView] = useState('map')
  const [themeMode, setThemeMode] = useState('dark')
  const [mapLayer, setMapLayer] = useState('standard')

  useEffect(() => {
    document.documentElement.dataset.theme = themeMode

    return () => {
      delete document.documentElement.dataset.theme
    }
  }, [themeMode])

  const loadEvents = useCallback((options = {}) => {
    const url = dataSource === 'demo' ? getApiUrl('/api/events') : getApiUrl('/api/sources/events')
    const requestConfig = options.refresh ? { params: { refresh: true } } : undefined

    setLoading(true)
    setError('')

    axios
      .get(url, requestConfig)
      .then((res) => setEvents(res.data))
      .catch((err) => {
        console.error(err)
        setEvents([])
        setError('Zkontrolujte připojení k API nebo to zkuste znovu.')
      })
      .finally(() => setLoading(false))
  }, [dataSource])

  useEffect(() => {
    loadEvents()
  }, [loadEvents])

  const categoryCounts = useMemo(() => {
    return events.reduce((counts, event) => {
      counts[event.eventType] = (counts[event.eventType] ?? 0) + 1
      return counts
    }, {})
  }, [events])

  const sourceOptions = useMemo(() => buildCountOptions(events, (event) => event.sourceName), [events])
  const cityOptions = useMemo(() => buildCountOptions(events, (event) => event.locationName), [events])

  const filteredEvents = useMemo(() => {
    let result = [...events]
    const normalizedSearch = search.trim().toLowerCase()

    if (selectedType !== 'all') {
      result = result.filter((event) => event.eventType === selectedType)
    }

    if (selectedSource !== 'all') {
      result = result.filter((event) => event.sourceName === selectedSource)
    }

    if (selectedCity !== 'all') {
      result = result.filter((event) => event.locationName === selectedCity)
    }

    if (normalizedSearch) {
      result = result.filter((event) => {
        const type = getEventTypeMeta(event.eventType)
        const searchable = `${event.title} ${event.locationName ?? ''} ${event.sourceName} ${type.label}`.toLowerCase()
        return searchable.includes(normalizedSearch)
      })
    }

    if (dateFrom) {
      result = result.filter((event) => {
        const eventDay = new Date(event.date).toISOString().slice(0, 10)
        return eventDay >= dateFrom
      })
    }

    if (dateTo) {
      result = result.filter((event) => {
        const eventDay = new Date(event.date).toISOString().slice(0, 10)
        return eventDay <= dateTo
      })
    }

    result.sort((a, b) => {
      const dateA = new Date(a.date)
      const dateB = new Date(b.date)

      return sortOrder === 'oldest' ? dateA - dateB : dateB - dateA
    })

    return result
  }, [dateFrom, dateTo, events, search, selectedCity, selectedSource, selectedType, sortOrder])

  const tileLayer = MAP_LAYERS[mapLayer] ?? MAP_LAYERS.standard

  return (
    <div className="app-shell" data-theme={themeMode}>
      <FilterPanel
        categoryCounts={categoryCounts}
        cityOptions={cityOptions}
        dataSource={dataSource}
        dateFrom={dateFrom}
        dateTo={dateTo}
        loading={loading}
        mapLayer={mapLayer}
        onDataSourceChange={(value) => {
          setDataSource(value)
          setSelectedSource('all')
          setSelectedCity('all')
        }}
        onDateFromChange={setDateFrom}
        onDateToChange={setDateTo}
        onMapLayerChange={setMapLayer}
        onResetFilters={() => {
          setSelectedType('all')
          setDateFrom('')
          setDateTo('')
          setSearch('')
          setSelectedSource('all')
          setSelectedCity('all')
        }}
        onSearchChange={setSearch}
        onSelectedCityChange={setSelectedCity}
        onSelectedSourceChange={setSelectedSource}
        onSelectedTypeChange={setSelectedType}
        onSortOrderChange={setSortOrder}
        onThemeModeChange={setThemeMode}
        search={search}
        selectedCity={selectedCity}
        selectedSource={selectedSource}
        selectedType={selectedType}
        sortOrder={sortOrder}
        sourceOptions={sourceOptions}
        themeMode={themeMode}
        totalEvents={events.length}
        visibleEvents={filteredEvents.length}
      />

      <nav className="mobile-tabs" aria-label="Mobilní zobrazení">
        <button
          className={activeMobileView === 'map' ? 'is-active' : ''}
          type="button"
          onClick={() => setActiveMobileView('map')}
        >
          Mapa
        </button>
        <button
          className={activeMobileView === 'list' ? 'is-active' : ''}
          type="button"
          onClick={() => setActiveMobileView('list')}
        >
          Zprávy
        </button>
      </nav>

      <main className={activeMobileView === 'map' ? 'map-panel is-active' : 'map-panel'}>
        <MapContainer center={[49.8, 15.5]} zoom={7} className="map-canvas">
          <TileLayer key={mapLayer} attribution={tileLayer.attribution} url={tileLayer.url} />
          <ClusterLayer events={filteredEvents} />
        </MapContainer>
      </main>

      <NewsList error={error} events={filteredEvents} loading={loading} onRetry={() => loadEvents({ refresh: true })} />
    </div>
  )
}

export default App