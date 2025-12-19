// wwwroot/js/dashboardCharts.js
(function () {
  const charts = new Map();

  function cssVar(name, fallback) {
    const v = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
    return v || fallback;
  }

  function ensureChartJs() {
    if (!window.Chart) {
      console.warn("[dashboardCharts] Chart.js not loaded yet.");
      return false;
    }
    return true;
  }

  function destroy(id) {
    const c = charts.get(id);
    if (c) {
      c.destroy();
      charts.delete(id);
    }
  }

  function commonOptions() {
    const text = cssVar("--text-tertiary", "#6b7280");
    const grid = cssVar("--card-border", cssVar("--table-border", "#e5e7eb"));
    const primary = cssVar("--text-primary", "#111827");

    return {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { labels: { color: text } },
        tooltip: {
          enabled: true,
          backgroundColor: cssVar("--card-bg", "#ffffff"),
          titleColor: primary,
          bodyColor: primary,
          borderColor: grid,
          borderWidth: 1,
          displayColors: false
        }
      },
      scales: {
        x: {
          ticks: { color: text, font: { size: 12 } },
          grid: { color: grid, borderDash: [3, 3], drawBorder: false },
          border: { color: grid }
        },
        y: {
          ticks: { color: text, font: { size: 12 } },
          grid: { color: grid, borderDash: [3, 3], drawBorder: false },
          border: { color: grid }
        }
      },
      elements: {
        point: { radius: 0, hoverRadius: 3 },
        line: { tension: 0.35, borderWidth: 2 }
      }
    };
  }

  function renderLine(id, labels, values) {
    if (!ensureChartJs()) return;
    const el = document.getElementById(id);
    if (!el) return;

    destroy(id);

    const blue = "#3b82f6"; // giống chart Figma
    const chart = new Chart(el, {
      type: "line",
      data: {
        labels,
        datasets: [
          {
            label: "Agents",
            data: values,
            borderColor: blue,
            fill: true,
            backgroundColor: (ctx) => {
              const chart = ctx.chart;
              const { chartArea } = chart;
              if (!chartArea) return "rgba(59,130,246,0.12)";
              const g = chart.ctx.createLinearGradient(0, chartArea.top, 0, chartArea.bottom);
              g.addColorStop(0, "rgba(59,130,246,0.20)");
              g.addColorStop(1, "rgba(59,130,246,0)");
              return g;
            }
          }
        ]
      },
      options: commonOptions()
    });

    charts.set(id, chart);
  }

  function renderPie(id, labels, values) {
    if (!ensureChartJs()) return;
    const el = document.getElementById(id);
    if (!el) return;

    destroy(id);

    // giống tông donut Figma (indigo/purple)
    const colors = ["#6366f1", "#8b5cf6", "#a855f7", "#c084fc"];

    const chart = new Chart(el, {
      type: "doughnut",
      data: {
        labels,
        datasets: [
          {
            data: values,
            backgroundColor: colors,
            borderWidth: 0
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        cutout: "70%",
        plugins: {
          legend: {
            position: "bottom",
            labels: {
              color: cssVar("--text-secondary", "#374151"),
              usePointStyle: true,
              pointStyle: "circle",
              boxWidth: 8,
              font: { size: 12 }
            }
          },
          tooltip: {
            backgroundColor: cssVar("--card-bg", "#ffffff"),
            titleColor: cssVar("--text-primary", "#111827"),
            bodyColor: cssVar("--text-primary", "#111827"),
            borderColor: cssVar("--card-border", "#e5e7eb"),
            borderWidth: 1,
            displayColors: false
          }
        }
      }
    });

    charts.set(id, chart);
  }

  function renderBar(id, labels, values) {
    if (!ensureChartJs()) return;
    const el = document.getElementById(id);
    if (!el) return;

    destroy(id);

    const green = "#10b981"; // giống Figma
    const chart = new Chart(el, {
      type: "bar",
      data: {
        labels,
        datasets: [
          {
            label: "Commands",
            data: values,
            backgroundColor: green,
            borderRadius: 8,
            borderSkipped: false
          }
        ]
      },
      options: commonOptions()
    });

    charts.set(id, chart);
  }

  window.dashboardCharts = { renderLine, renderPie, renderBar, destroy };
})();
