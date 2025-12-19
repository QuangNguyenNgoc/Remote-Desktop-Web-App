// wwwroot/js/agentMap.js
(function () {
  const maps = new Map(); // key: elementId -> { map, markers[] }

  function destroy(elementId) {
    const ctx = maps.get(elementId);
    if (!ctx) return;

    try {
      ctx.markers?.forEach(m => m.remove());
      ctx.map.remove();
    } catch { }
    maps.delete(elementId);
  }

  function init(elementId, options) {
    if (!window.L) {
      console.warn("[agentMap] Leaflet not loaded yet.");
      return;
    }

    const el = document.getElementById(elementId);
    if (!el) return;

    destroy(elementId);

    const center = options?.center ?? [14.0583, 108.2772]; // Vietnam center-ish
    const zoom = options?.zoom ?? 5;

    const map = L.map(elementId, { zoomControl: true }).setView(center, zoom);

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
      maxZoom: 19,
      attribution: "&copy; OpenStreetMap"
    }).addTo(map);

    maps.set(elementId, { map, markers: [] });
  }

  function setMarkers(elementId, markers) {
    const ctx = maps.get(elementId);
    if (!ctx) return;

    // clear old
    ctx.markers.forEach(m => m.remove());
    ctx.markers = [];

    (markers ?? []).forEach(m => {
      if (typeof m?.lat !== "number" || typeof m?.lng !== "number") return;
      const mk = L.marker([m.lat, m.lng]).addTo(ctx.map);
      if (m.label) mk.bindPopup(m.label);
      ctx.markers.push(mk);
    });
  }

  window.agentMap = { init, setMarkers, destroy };
})();
