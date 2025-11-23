/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./**/*.{razor,html,cshtml}",
    "./Components/**/*.{razor,html,cshtml}",
  ],
  theme: {
    extend: {
      colors: {
        primary: "#3b82f6", // Xanh dương chủ đạo
        dark: "#111827", // Nền tối
        darker: "#0f172a", // Sidebar
        surface: "#1f2937", // Card background
      },
    },
  },
  plugins: [],
};
