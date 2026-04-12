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
  const [dateFilter, setDateFilter] = useState('all')

  useEffect(() => {
    axios
      .get('http://localhost:5059/api/events/from-rss')
      .then((res) => setEvents(res.data))
      .catch((err) => console.error(err))
  }, [])

  const filteredEvents = useMemo(() => {
    let result = events
    const now = new Date()

    if (selectedType !== 'all') {
      result = result.filter((event) => event.eventType === selectedType)
    }

    if (dateFilter === 'today') {
      result = result.filter((event) => {
        const eventDate = new Date(event.date)
        return eventDate.toDateString() === now.toDateString()
      })
    }

    if (dateFilter === '7days') {
      result = result.filter((event) => {
        const eventDate = new Date(event.date)
        const diffInDays = (now - eventDate) / (1000 * 60 * 60 * 24)
        return diffInDays <= 7
      })
    }

    return result
  }, [events, selectedType, dateFilter])

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
          minWidth: '220px',
        }}
      >
        <div style={{ fontWeight: 'bold', marginBottom: '8px' }}>
          Czech News Map
        </div>

        <div style={{ marginBottom: '8px' }}>
          Событий: {filteredEvents.length}
        </div>

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
          Дата
        </label>

        <select
          value={dateFilter}
          onChange={(e) => setDateFilter(e.target.value)}
          style={{
            width: '100%',
            padding: '8px',
            borderRadius: '8px',
            border: '1px solid #ccc',
          }}
        >
          <option value="all">Все</option>
          <option value="today">Сегодня</option>
          <option value="7days">Последние 7 дней</option>
        </select>
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