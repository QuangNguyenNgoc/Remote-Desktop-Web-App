// wwwroot/js/dashboardCharts.js  (ES module)
// Requires: Chart.js UMD loaded => window.Chart exists

const charts = new Map();

function cssVar(name, fallback) {
  const v = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return v || fallback;
}

function getCanvas(canvasId) {
  const el = document.getElementById(canvasId);
  if (!el) return null;
  const ctx = el.getContext("2d");
  return { el, ctx };
}

function destroy(canvasId) {
  const c = charts.get(canvasId);
  if (c) {
    c.destroy();
    charts.delete(canvasId);
  }
}

export function destroyChart(canvasId) {
  destroy(canvasId);
}

// ---------- Agents Line (Area-like) ----------
export function createOrUpdateAgentsLine(canvasId, labels, values) {
  const c = getCanvas(canvasId);
  if (!c) return;

  const gridColor = cssVar("--table-border", cssVar("--card-border", "#e5e7eb"));
  const tickColor = cssVar("--text-tertiary", "#94a3b8");
  const tooltipBg = cssVar("--card-bg", "#ffffff");
  const lineColor = "#3b82f6";

  const existing = charts.get(canvasId);
  if (existing) {
    existing.data.labels = labels;
    existing.data.datasets[0].data = values;
    existing.update("none");
    return;
  }

  const chart = new window.Chart(c.ctx, {
    type: "line",
    data: {
      labels,
      datasets: [{
        label: "agents",
        data: values,
        borderColor: lineColor,
        borderWidth: 2,
        pointRadius: 0,
        pointHoverRadius: 4,
        tension: 0.35,
        fill: true,
        backgroundColor: (context) => {
          const { chart } = context;
          const { ctx, chartArea } = chart;
          if (!chartArea) return "rgba(59,130,246,0.10)";
          const g = ctx.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
          g.addColorStop(0.05, "rgba(59,130,246,0.20)");
          g.addColorStop(0.95, "rgba(59,130,246,0.00)");
          return g;
        },
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: tooltipBg,
          borderColor: gridColor,
          borderWidth: 1,
          titleColor: cssVar("--text-secondary", "#334155"),
          bodyColor: cssVar("--text-primary", "#111827"),
          cornerRadius: 8,
        }
      },
      scales: {
        x: {
          grid: { color: gridColor, borderDash: [3,3], drawBorder: true },
          ticks: { color: tickColor, maxRotation: 0, autoSkip: true }
        },
        y: {
          grid: { color: gridColor, borderDash: [3,3], drawBorder: true },
          ticks: { color: tickColor }
        }
      }
    }
  });

  charts.set(canvasId, chart);
}

// ---------- OS Donut ----------
export function createOrUpdateOsDonut(canvasId, labels, values) {
  const c = getCanvas(canvasId);
  if (!c) return;

  const gridColor = cssVar("--table-border", cssVar("--card-border", "#e5e7eb"));
  const tooltipBg = cssVar("--card-bg", "#ffffff");
  const legendColor = cssVar("--text-secondary", "#334155");

  const total = values.reduce((a,b)=>a+b,0);
  const safeLabels = total === 0 ? ["No data"] : labels;
  const safeValues = total === 0 ? [1] : values;

  const COLORS = ["#6366f1", "#8b5cf6", "#a855f7", "#c084fc", "#d946ef", "#f0abfc"];

  const existing = charts.get(canvasId);
  if (existing) {
    existing.data.labels = safeLabels;
    existing.data.datasets[0].data = safeValues;
    existing.update("none");
    return;
  }

  const chart = new window.Chart(c.ctx, {
    type: "doughnut",
    data: {
      labels: safeLabels,
      datasets: [{
        data: safeValues,
        backgroundColor: safeValues.map((_, i) => COLORS[i % COLORS.length]),
        borderColor: cssVar("--card-bg", "#ffffff"),
        borderWidth: 3,
        hoverOffset: 6,
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      cutout: "65%",
      plugins: {
        legend: {
          position: "bottom",
          labels: {
            color: legendColor,
            usePointStyle: true,
            pointStyle: "circle",
            boxWidth: 8,
            boxHeight: 8,
            padding: 14,
            font: { size: 12 }
          }
        },
        tooltip: {
          backgroundColor: tooltipBg,
          borderColor: gridColor,
          borderWidth: 1,
          titleColor: cssVar("--text-secondary", "#334155"),
          bodyColor: cssVar("--text-primary", "#111827"),
          cornerRadius: 8,
        }
      }
    }
  });

  charts.set(canvasId, chart);
}

// ---------- Commands Bar ----------
export function createOrUpdateCommandsBar(canvasId, labels, values) {
  const c = getCanvas(canvasId);
  if (!c) return;

  const gridColor = cssVar("--table-border", cssVar("--card-border", "#e5e7eb"));
  const tickColor = cssVar("--text-tertiary", "#94a3b8");
  const tooltipBg = cssVar("--card-bg", "#ffffff");

  const existing = charts.get(canvasId);
  if (existing) {
    existing.data.labels = labels;
    existing.data.datasets[0].data = values;
    existing.update("none");
    return;
  }

  const chart = new window.Chart(c.ctx, {
    type: "bar",
    data: {
      labels,
      datasets: [{
        label: "frequency",
        data: values,
        backgroundColor: "#22c55e",
        borderRadius: 8,
        borderSkipped: false
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: {
          backgroundColor: tooltipBg,
          borderColor: gridColor,
          borderWidth: 1,
          titleColor: cssVar("--text-secondary", "#334155"),
          bodyColor: cssVar("--text-primary", "#111827"),
          cornerRadius: 8,
        }
      },
      scales: {
        x: { grid: { display: false }, ticks: { color: tickColor, maxRotation: 0, autoSkip: true } },
        y: { grid: { color: gridColor, borderDash: [3,3] }, ticks: { color: tickColor } }
      }
    }
  });

  charts.set(canvasId, chart);
}
