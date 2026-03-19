import { useEffect, useState } from 'react';
import { fetchHealthCheck, fetchNotes } from '../services/api.service';
import { Note } from '../types/note.types';
import './HomePage.css';

function HomePage() {
  const [healthStatus, setHealthStatus] = useState<string>('');
  const [loading, setLoading] = useState(true);
  const [notes, setNotes] = useState<Note[]>([]);
  const [error, setError] = useState<string>('');

  useEffect(() => {
    const loadData = async () => {
      try {
        // Fetch health status
        const healthResponse = await fetchHealthCheck();
        setHealthStatus(healthResponse.status);

        // Fetch notes
        const notesData = await fetchNotes();
        setNotes(notesData);
      } catch (err) {
        setError('Unable to connect to API');
        setHealthStatus('Unable to connect to API');
        console.error('Error loading data:', err);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  if (loading) {
    return (
      <div className="home-page">
        <div className="loading">Loading...</div>
      </div>
    );
  }

  return (
    <div className="home-page">
      <header className="header">
        <h1>Welcome to Propel IQ</h1>
        <p className="api-status">
          API Status: <span className={healthStatus === 'Healthy' ? 'status-healthy' : 'status-error'}>
            {healthStatus}
          </span>
        </p>
      </header>

      <main className="main-content">
        {error && <div className="error-message">{error}</div>}
        
        <section className="notes-section">
          <h2>Notes</h2>
          {notes.length === 0 ? (
            <p className="no-notes">No notes available. Create your first note!</p>
          ) : (
            <div className="notes-grid">
              {notes.map((note) => (
                <div key={note.id} className="note-card">
                  <h3>{note.title || 'Untitled'}</h3>
                  <p>{note.content || 'No content'}</p>
                </div>
              ))}
            </div>
          )}
        </section>
      </main>
    </div>
  );
}

export default HomePage;
