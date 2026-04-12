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
      font-size: 20px;
      width: 28px;
      height: 28px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: white;
      border: 2px solid #333;
      border-radius: 50%;
      box-shadow: 0 1px 4px rgba(0,0,0,0.3);
    ">${symbol}</div>`,
    className: '',
    iconSize: [28, 28],
    iconAnchor: [14, 14],
    popupAnchor: [0, -14],
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
          <span>Тип: ${event.eventType}</span><br/>
          <span>Дата: ${new Date(event.date).toLocaleDateString()}</span><br/><br/>
          <a href="${event.sourceUrl}" target="_blank" rel="noopener noreferrer">Открыть источник</a>
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
  const [dataSource, setDataSource] = useState('demo')
  const [sortOrder, setSortOrder] = useState('newest')
  const [dateFrom, setDateFrom] = useState('')
  const [dateTo, setDateTo] = useState('')
  const [loading, setLoading] = useState(false)

  useEffect(() => {
  const url =
    dataSource === 'demo'
      ? 'http://localhost:5059/api/events'
      : 'http://localhost:5059/api/sources/events'

  console.log('DATA SOURCE =', dataSource)
  console.log('REQUEST URL =', url)

  setLoading(true)

  axios
    .get(url)
    .then((res) => {
      console.log('RESPONSE DATA =', res.data)
      setEvents(res.data)
    })
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

      if (sortOrder === 'oldest') {
        return dateA - dateB
      }

      return dateB - dateA
    })

    return result
  }, [events, selectedType, dateFrom, dateTo, sortOrder])

  return (
    <div style={{ height: '100vh', width: '100%' }}>
      <div
        style={{
          position: 'absolute',
          top: 16,
          left: 16,
          zIndex: 1000,
          background: 'white',
          padding: '12px 14px',
          borderRadius: '12px',
          boxShadow: '0 2px 10px rgba(0,0,0,0.15)',
          minWidth: '260px',
        }}
      >
        <div style={{ fontWeight: 'bold', marginBottom: '8px' }}>
          Czech News Map
        </div>

        <div style={{ marginBottom: '8px' }}>
          Событий: {filteredEvents.length}
        </div>

        <div style={{ marginBottom: '10px', fontSize: '14px', color: '#444' }}>
          `Режим: ${dataSource === 'demo' ? 'Demo' : 'Live sources'}`
        </div>

        <label style={{ display: 'block', marginBottom: '6px' }}>
          Источник данных
        </label>

        <select
          value={dataSource}
          onChange={(e) => setDataSource(e.target.value)}
          style={{
            width: '100%',
            padding: '8px',
            borderRadius: '8px',
            border: '1px solid #ccc',
            marginBottom: '10px',
          }}
        >
          <option value="demo">Demo data</option>
          <option value="rss">Live sources</option>
        </select>

        <label style={{ display: 'block', marginBottom: '6px' }}>
          Тип события
        </label>

        <select
          value={selectedType}
          onChange={(e) => setSelectedType(e.target.value)}
          style={{
            width: '100%',
            padding: '8px',
            borderRadius: '8px',
            border: '1px solid #ccc',
            marginBottom: '10px',
          }}
        >
          <option value="all">Все</option>
          <option value="stabbing">Stabbing</option>
          <option value="shooting">Shooting</option>
          <option value="arrest">Arrest</option>
          <option value="fight">Fight</option>
          <option value="robbery">Robbery</option>
          <option value="fire">Fire</option>
          <option value="unknown">Unknown</option>
        </select>

        <label style={{ display: 'block', marginBottom: '6px' }}>
          Дата от
        </label>

        <input
          type="date"
          value={dateFrom}
          onChange={(e) => setDateFrom(e.target.value)}
          style={{
            width: '100%',
            padding: '8px',
            borderRadius: '8px',
            border: '1px solid #ccc',
            marginBottom: '10px',
            boxSizing: 'border-box',
          }}
        />

        <label style={{ display: 'block', marginBottom: '6px' }}>
          Дата до
        </label>

        <input
          type="date"
          value={dateTo}
          onChange={(e) => setDateTo(e.target.value)}
          style={{
            width: '100%',
            padding: '8px',
            borderRadius: '8px',
            border: '1px solid #ccc',
            marginBottom: '10px',
            boxSizing: 'border-box',
          }}
        />

        <label style={{ display: 'block', marginBottom: '6px' }}>
          Сортировка
        </label>

        <select
          value={sortOrder}
          onChange={(e) => setSortOrder(e.target.value)}
          style={{
            width: '100%',
            padding: '8px',
            borderRadius: '8px',
            border: '1px solid #ccc',
            marginBottom: '10px',
          }}
        >
          <option value="newest">Сначала новые</option>
          <option value="oldest">Сначала старые</option>
        </select>

        <button
          onClick={() => {
            setSelectedType('all')
            setDateFrom('')
            setDateTo('')
            setSortOrder('newest')
          }}
          style={{
            width: '100%',
            padding: '10px',
            borderRadius: '8px',
            border: 'none',
            background: '#222',
            color: 'white',
            cursor: 'pointer',
          }}
        >
          Сбросить фильтры
        </button>
      </div>

      <MapContainer
        center={[49.8, 15.5]}
        zoom={7}
        style={{ height: '100vh', width: '100%' }}
      >
        <TileLayer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
        <ClusterLayer events={filteredEvents} />
      </MapContainer>
    </div>
  )
}

export default App