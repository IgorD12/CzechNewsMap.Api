import { MapContainer, TileLayer } from 'react-leaflet'
import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import L from 'leaflet'
import 'leaflet.markercluster'
import 'leaflet.markercluster/dist/MarkerCluster.css'
import 'leaflet.markercluster/dist/MarkerCluster.Default.css'
import { useMap } from 'react-leaflet'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

const EVENT_TYPES = [
  { value: 'all', label: 'Vše', icon: '🗺️' },
  { value: 'general', label: 'Obecné zprávy', icon: '📰' },
  { value: 'politics', label: 'Politika', icon: '🏛️' },
  { value: 'sport', label: 'Sport', icon: '⚽' },
  { value: 'culture', label: 'Kultura', icon: '🎭' },
  { value: 'traffic', label: 'Doprava', icon: '🚗' },
  { value: 'transport', label: 'MHD / železnice', icon: '🚆' },
  { value: 'crime', label: 'Krimi', icon: '🚓' },
  { value: 'fire', label: 'Požár', icon: '🔥' },
  { value: 'weather', label: 'Počasí', icon: '⛈️' },
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

function getEventIcon(eventType) {
  const symbol = getEventTypeMeta(eventType).icon

  return L.divIcon({
    html: `<div style="
      font-size: 18px;
      width: 30px;
      height: 30px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: white;
      border: 2px solid #333;
      border-radius: 50%;
      box-shadow: 0 2px 6px rgba(0,0,0,0.25);
    ">${symbol}</div>`,
    className: '',
    iconSize: [30, 30],
    iconAnchor: [15, 15],
  })
}

function createPopupContent(event) {
  const type = getEventTypeMeta(event.eventType)
  const wrapper = document.createElement('div')
  wrapper.style.minWidth = '220px'

  const title = document.createElement('b')
  title.textContent = event.title
  wrapper.append(title, document.createElement('br'))

  const source = document.createElement('span')
  source.textContent = event.sourceName
  wrapper.append(source, document.createElement('br'))

  const typeLine = document.createElement('span')
  typeLine.textContent = `Typ: ${type.icon} ${type.label}`
  wrapper.append(typeLine, document.createElement('br'))

  const date = document.createElement('span')
  date.textContent = `Datum: ${new Date(event.date).toLocaleDateString()}`
  wrapper.append(date, document.createElement('br'), document.createElement('br'))

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
    const markers = L.markerClusterGroup()

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

function App() {
  const [events, setEvents] = useState([])
  const [selectedType, setSelectedType] = useState('all')
  const [dataSource, setDataSource] = useState('rss')
  const [sortOrder, setSortOrder] = useState('newest')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    const url =
      dataSource === 'demo'
        ? getApiUrl('/api/events')
        : getApiUrl('/api/sources/events')

    setLoading(true)
    setError('')

    axios
      .get(url)
      .then((res) => setEvents(res.data))
      .catch((err) => {
        console.error(err)
        setEvents([])
        setError('Nepodařilo se načíst zprávy.')
      })
      .finally(() => setLoading(false))
  }, [dataSource])

  const filteredEvents = useMemo(() => {
    let result = [...events]

    if (selectedType !== 'all') {
      result = result.filter((event) => event.eventType === selectedType)
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
  }, [events, selectedType, dateFrom, dateTo, sortOrder])

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        display: 'grid',
        gridTemplateColumns: '280px 1fr 340px',
      }}
    >
      {/* LEVÝ PANEL */}
      <div
        style={{
          overflowY: 'auto',
          padding: '16px',
          borderRight: '1px solid #ddd',
        }}
      >
        <h2>Czech News Map</h2>
        <div>Počet událostí: {filteredEvents.length}</div>
        {loading && <div style={{ marginTop: '8px' }}>Načítám zprávy…</div>}
        {error && <div style={{ marginTop: '8px', color: '#d33' }}>{error}</div>}

        <div style={{ marginTop: '10px' }}>
          <label>Zdroj</label>
          <select
            value={dataSource}
            onChange={(e) => setDataSource(e.target.value)}
            style={{ width: '100%' }}
          >
            <option value="rss">Živé zdroje</option>
            <option value="demo">Demo</option>
          </select>
        </div>

        <div style={{ marginTop: '10px' }}>
          <label>Typ</label>
          <select
            value={selectedType}
            onChange={(e) => setSelectedType(e.target.value)}
            style={{ width: '100%' }}
          >
            {EVENT_TYPES.map((type) => (
              <option key={type.value} value={type.value}>
                {type.icon} {type.label}
              </option>
            ))}
          </select>
        </div>

        <div style={{ marginTop: '10px' }}>
          <label>Datum od</label>
          <input
            type="date"
            value={dateFrom}
            onChange={(e) => setDateFrom(e.target.value)}
            style={{ width: '100%' }}
          />
        </div>

        <div style={{ marginTop: '10px' }}>
          <label>Datum do</label>
          <input
            type="date"
            value={dateTo}
            onChange={(e) => setDateTo(e.target.value)}
            style={{ width: '100%' }}
          />
        </div>

        <div style={{ marginTop: '10px' }}>
          <label>Řazení</label>
          <select
            value={sortOrder}
            onChange={(e) => setSortOrder(e.target.value)}
            style={{ width: '100%' }}
          >
            <option value="newest">Od nejnovějších</option>
            <option value="oldest">Od nejstarších</option>
          </select>
        </div>

        <button
          style={{ marginTop: '10px', width: '100%' }}
          onClick={() => {
            setSelectedType('all')
            setDateFrom('')
            setDateTo('')
          }}
        >
          Resetovat filtry
        </button>
      </div>

      {/* MAPA */}
      <div>
        <MapContainer
          center={[49.8, 15.5]}
          zoom={7}
          style={{ width: '100%', height: '100%' }}
        >
          <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
          <ClusterLayer events={filteredEvents} />
        </MapContainer>
      </div>

      {/* PRAVÝ PANEL */}
      <div
        style={{
          overflowY: 'auto',
          padding: '16px',
          borderLeft: '1px solid #ddd',
        }}
      >
        <h2>Události</h2>

        {!loading && filteredEvents.length === 0 && (
          <div>Žádné zprávy neodpovídají aktuálním filtrům.</div>
        )}

        {filteredEvents.map((event, i) => {
          const type = getEventTypeMeta(event.eventType)
          const safeUrl = getSafeSourceUrl(event.sourceUrl)

          return (
            <div key={`${event.sourceUrl}-${i}`} style={{ marginBottom: '15px' }}>
              <b>{event.title}</b>
              <div>{event.sourceName}</div>
              <div>
                {type.icon} {type.label}
              </div>
              <div>{new Date(event.date).toLocaleDateString()}</div>
              {safeUrl && (
                <a href={safeUrl} target="_blank" rel="noopener noreferrer">
                  Otevřít zdroj
                </a>
              )}
            </div>
          )
        })}
      </div>
    </div>
  )
}

export default App