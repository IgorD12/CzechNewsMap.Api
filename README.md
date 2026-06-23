# Czech News Map

Czech News Map je webová aplikace pro vizualizaci aktuálních zpráv na mapě České republiky.

Projekt agreguje data z různých zpravodajských zdrojů (např. iDNES, Novinky) a zobrazuje jednotlivé události jako body na mapě. Uživatel může filtrovat data podle typu události, data a řazení.

## Funkce

- zobrazení událostí na mapě (Leaflet)
- filtrování podle typu události
- filtrování podle data (od / do)
- řazení (od nejnovějších / od nejstarších)
- přepínání mezi demo daty a reálnými RSS zdroji
- seznam událostí v pravém panelu

## Technologie

- Frontend: React + Leaflet
- Backend: ASP.NET Core (C#)
- Data: RSS feedy (iDNES, Novinky)

## Stav projektu

Tento projekt je funkční (working build), ale není finální verze.

Aplikace je stále ve vývoji a některé části nejsou ještě plně optimalizované nebo dokončené (např. rozšíření zdrojů, přesnější klasifikace událostí, práce s historickými daty).

## Spuštění projektu

### Backend
``bash
dotnet run


English:
# Czech News Map

Czech News Map is a web application that visualizes current news on a map of the Czech Republic.

The project aggregates data from various news sources (e.g., iDNES, Novinky) and displays individual events as points on the map. Users can filter the data by event type, date, and sorting order.

## Features

- Display of events on a map (Leaflet)
- Filtering by event type
- Filtering by date (from / to)
- Sorting (from newest / from oldest)
- Switching between demo data and real RSS feeds
- List of events in the right panel

## Technology

- Frontend: React + Leaflet
- Backend: ASP.NET Core (C#)
- Data: RSS feeds (iDNES, Novinky)

## Project Status

This project is a working build, but it is not the final version.

The application is still under development, and some parts are not yet fully optimized or completed (e.g., expanding sources, more precise event classification, working with historical data).

## Running the Project

### Backend

``bash
dotnet run

## Production hosting notes

Backend:
- `/health` returns a simple API health response for hosting checks.
- `Frontend:AllowedOrigins` should contain the exact public frontend origins, for example `https://news-map.example.com`.
- `SourceCache:DurationMinutes` controls how long aggregated RSS events stay cached in memory.
- `SourceCache:BrowserCacheSeconds` controls the short browser/proxy cache header for `/api/sources/events`.
- Environment variable example for ASP.NET Core: `Frontend__AllowedOrigins__0=https://news-map.example.com`.

Frontend:
- Use `frontend/.env.production.example` as a template.
- Set `VITE_API_BASE_URL` to the public backend URL when API and frontend are hosted on different domains.
- Leave `VITE_API_BASE_URL` empty when both are served from the same origin.