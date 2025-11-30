/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: { 
    extend: {
      colors: {
        magenta: {
          50: '#fdf4ff',
          100: '#fae8ff',
          200: '#f5d0fe',
          300: '#f0abfc',
          400: '#e879f9',
          500: '#d946ef',  // Primary magenta
          600: '#c026d3',  // For buttons
          700: '#a21caf',
          800: '#86198f',
          900: '#701a75',
        },
      },
    } 
  },
  plugins: [],
};
