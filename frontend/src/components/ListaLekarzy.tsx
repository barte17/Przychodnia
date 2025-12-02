import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import lekarzService, { Lekarz } from '../services/lekarzService';
import './ListaLekarzy.css';

const ListaLekarzy: React.FC = () => {
  const navigate = useNavigate();
  const [lekarze, setLekarze] = useState<Lekarz[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadLekarze();
  }, []);

  const loadLekarze = async () => {
    try {
      setLoading(true);
      const data = await lekarzService.getLekarze();
      setLekarze(data);
    } catch (err) {
      setError('Nie udało się pobrać listy lekarzy');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="loading">Ładowanie...</div>;
  if (error) return <div className="error">{error}</div>;

  return (
    <div className="lista-lekarzy">
      <div className="header">
        <button className="btn-back" onClick={() => navigate('/dashboard')}>
          ← Wróć do menu głównego
        </button>
        <h2>Nasi lekarze</h2>
      </div>

      {lekarze.length === 0 ? (
        <p className="no-data">Brak lekarzy w systemie</p>
      ) : (
        <div className="lekarze-grid">
          {lekarze.map((lekarz) => (
            <div key={lekarz.idLekarza} className="lekarz-card">
              <div className="lekarz-avatar">
                👨‍⚕️
              </div>
              <div className="lekarz-info">
                <h3>{lekarz.imie} {lekarz.nazwisko}</h3>
                <p className="specjalizacja">{lekarz.specjalizacja}</p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ListaLekarzy;
