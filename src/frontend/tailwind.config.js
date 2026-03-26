/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#EBF5FF',
          100: '#D1E9FF',
          500: '#0F62FE',
          600: '#0C50D4',
          700: '#0A3FA8',
        },
        success: {
          50: '#DCFCE7',
          600: '#16A34A',
          700: '#15803D',
          800: '#166534',
        },
        error: {
          50: '#FEE2E2',
          200: '#FECACA',
          500: '#DC2626',
          600: '#DC2626',
          700: '#B91C1C',
          800: '#991B1B',
        },
        warning: {
          50: '#FEF3C7',
          200: '#FDE68A',
          600: '#D97706',
          700: '#B45309',
          800: '#92400E',
        },
      },
      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', 'sans-serif'],
      },
      keyframes: {
        'slide-up': {
          '0%': { transform: 'translateY(100%)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        'slide-down': {
          '0%': { transform: 'translateY(-100%)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
      },
      animation: {
        'slide-up': 'slide-up 0.3s ease-out',
        'slide-down': 'slide-down 0.3s ease-out',
      },
    },
  },
  plugins: [],
}
