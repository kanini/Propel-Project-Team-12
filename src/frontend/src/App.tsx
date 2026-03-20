import { useState } from 'react';

function App() {
  const [count, setCount] = useState(0);

  return (
    <div className="min-h-screen bg-gradient-to-br from-primary-50 to-secondary-100 flex items-center justify-center p-4">
      <div className="max-w-2xl w-full bg-white rounded-lg shadow-lg p-8">
        <h1 className="text-4xl font-bold text-primary-600 mb-4">
          Patient Access Platform
        </h1>
        <p className="text-secondary-600 mb-6">
          Welcome to the Clinical Intelligence & Appointment Booking System
        </p>
        
        <div className="bg-primary-50 border border-primary-200 rounded-md p-4 mb-6">
          <h2 className="text-lg font-semibold text-primary-700 mb-2">
            Frontend Setup Complete ✓
          </h2>
          <ul className="text-sm text-secondary-700 space-y-1">
            <li>✓ React 18 with TypeScript</li>
            <li>✓ Vite Build Tool</li>
            <li>✓ Tailwind CSS (with custom design tokens)</li>
            <li>✓ Redux Toolkit</li>
            <li>✓ React Router</li>
          </ul>
        </div>

        <div className="flex gap-4 items-center">
          <button
            onClick={() => setCount((count) => count + 1)}
            className="px-6 py-3 bg-primary-600 text-white font-medium rounded-md hover:bg-primary-700 transition-colors shadow-sm"
          >
            Count is {count}
          </button>
          <p className="text-secondary-500 text-sm">
            Edit <code className="bg-secondary-100 px-2 py-1 rounded text-xs">src/App.tsx</code> to test HMR
          </p>
        </div>
      </div>
    </div>
  );
}

export default App;

