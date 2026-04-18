import { MapContainer, TileLayer } from 'react-leaflet'
import { useEffect, useMemo, useState } from 'react'
import axios from 'axios'
import L from 'leaflet'
import 'leaflet.markercluster'
import 'leaflet.markercluster/dist/MarkerCluster.css'
import 'leaflet.markercluster/dist/MarkerCluster.Default.css'
import { useMap } from 'react-leaflet'

function getEventIcon(eventType) {
  const iconMap = {
    stabbing: '🔪',
    shooting: '🔫',
    arrest: '⛓️',
    fight: '👊',
    robbery: '💰',
    fire: '🔥',
    unknown: '❓',
  }

  const symbol = iconMap[eventType] || '❓'

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

function ClusterLayer({ events }) {
  const map = useMap()

  useEffect(() => {
    const markers = L.markerClusterGroup()

    events.forEach((event) => {
      const marker = L.marker([event.latitude, event.longitude], {
        icon: getEventIcon(event.eventType),
      })

      marker.bindPopup(`
        <div style="min-width: 220px">
          <b>${event.title}</b><br/>
          <span>${event.sourceName}</span><br/>
          <span>Typ: ${event.eventType}</span><br/>
          <span>Datum: ${new Date(event.date).toLocaleDateString()}</span><br/><br/>
          <a href="${event.sourceUrl}" target="_blank">Otevřít zdroj</a>
        </div>
      `)

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

  useEffect(() => {
    const url =
      dataSource === 'demo'
        ? 'http://localhost:5059/api/events'
        : 'http://localhost:5059/api/sources/events'

    setLoading(true)

    axios
      .get(url)
      .then((res) => setEvents(res.data))
      .catch((err) => console.error(err))
      .finally(() => setLoading(false))
  }, [dataSource])

  const filteredEvents = useMemo(() => {
    let result = [...events]

    if (selectedType !== 'all') {
      result = result.filter((event) => event.eventType === selectedType)
    }

    if (dateFrom) {
      const fromDate = new Date(dateFrom)
      fromDate.setHours(0, 0, 0, 0)

      result = result.filter((event) => {
        const eventDate = new Date(event.date)
        return eventDate >= fromDate
      })
    }

    if (dateTo) {
      const toDate = new Date(dateTo)
      toDate.setHours(23, 59, 59, 999)

      result = result.filter((event) => {
        const eventDate = new Date(event.date)
        return eventDate <= toDate
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
            <option value="all">Vše</option>
            <option value="unknown">Unknown</option>
            <option value="fire">Požár</option>
            <option value="arrest">Zatčení</option>
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

        {filteredEvents.map((event, i) => (
          <div key={i} style={{ marginBottom: '15px' }}>
            <b>{event.title}</b>
            <div>{event.sourceName}</div>
            <div>{new Date(event.date).toLocaleDateString()}</div>
            <a href={event.sourceUrl} target="_blank">
              Otevřít zdroj
            </a>
          </div>
        ))}
      </div>
    </div>
  )
}

export default App