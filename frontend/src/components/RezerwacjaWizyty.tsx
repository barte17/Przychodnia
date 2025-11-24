import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import terminService, { TerminLekarza } from '../services/terminService';
import wizytaService from '../services/wizytaService';
import lekarzService, { Lekarz } from '../services/lekarzService';
import './RezerwacjaWizyty.css';

const RezerwacjaWizyty: React.FC = () => {
  const { userDetails } = useAuth();
  const [lekarze, setLekarze] = useState<Lekarz[]>([]);
  const [selectedLekarz, setSelectedLekarz] = useState<number | null>(null);
  const [terminy, setTerminy] = useState<TerminLekarza[]>([]);
  const [loading, setLoading] = useState(false);
  const [loadingTerminy, setLoadingTerminy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    loadLekarze();
  }, []);

  useEffect(() => {
    if (selectedLekarz) {
      loadTerminy(selectedLekarz);
    } else {
      setTerminy([]);
    }
  }, [selectedLekarz]);

  const loadLekarze = async () => {
    try {
      setLoading(true);
      const data = await lekarzService.getLekarze();
      setLekarze(data);
    } catch (err) {
      setError('Nie udało się pobrać listy lekarzy');
    } finally {
      setLoading(false);
    }
  };

  const loadTerminy = async (lekarzId: number) => {
    try {
      setLoadingTerminy(true);
      const data = await terminService.getTerminyByLekarz(lekarzId, true);
      setTerminy(data);
    } catch (err) {
      setError('Nie udało się pobrać terminów');
    } finally {
      setLoadingTerminy(false);
    }
  };

  const handleRezerwacja = async (terminId: number) => {
    if (!userDetails?.pacjent?.idPacjenta) {
      setError('Brak danych pacjenta');
      return;
    }

    if (!window.confirm('Czy na pewno chcesz zarezerwować ten termin?')) return;

    try {
      await wizytaService.rezerwujWizyte({
        idTerminu: terminId,
        idPacjenta: userDetails.pacjent.idPacjenta
      });
      
      setSuccess('Wizyta została zarezerwowana! Oczekuj na akceptację lekarza.');
      if (selectedLekarz) {
        loadTerminy(selectedLekarz);
      }
    } catch (err: any) {
      setError(err.response?.data?.message || 'Nie udało się zarezerwować wizyty');
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('pl-PL', {
      weekday: 'long',
      day: '2-digit',
      month: 'long',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const formatTime = (dateString: string) => {
    return new Date(dateString).toLocaleTimeString('pl-PL', {
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  // Grupuj terminy po dniach
  const groupedTerminy = terminy.reduce((groups, termin) => {
    const date = new Date(termin.dataRozpoczecia).toLocaleDateString('pl-PL');
    if (!groups[date]) {
      groups[date] = [];
    }
    groups[date].push(termin);
    return groups;
  }, {} as Record<string, TerminLekarza[]>);

  if (!userDetails?.pacjent) {
    return <div className="error">Ta funkcja jest dostępna tylko dla pacjentów</div>;
  }

  return (
    <div className="rezerwacja-wizyty">
      <h2>Zarezerwuj wizytę</h2>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="form-group">
        <label>Wybierz lekarza:</label>
        <select
          value={selectedLekarz || ''}
          onChange={(e) => {
            setSelectedLekarz(e.target.value ? Number(e.target.value) : null);
            setError(null);
            setSuccess(null);
          }}
          disabled={loading}
        >
          <option value="">-- Wybierz lekarza --</option>
          {lekarze.map((lekarz) => (
            <option key={lekarz.idLekarza} value={lekarz.idLekarza}>
              {lekarz.imie} {lekarz.nazwisko} - {lekarz.specjalizacja}
            </option>
          ))}
        </select>
      </div>

      {selectedLekarz && (
        <div className="terminy-section">
          <h3>Dostępne terminy</h3>
          
          {loadingTerminy ? (
            <div className="loading">Ładowanie terminów...</div>
          ) : terminy.length === 0 ? (
            <p className="no-data">Brak dostępnych terminów u tego lekarza</p>
          ) : (
            <div className="terminy-grid">
              {Object.entries(groupedTerminy).map(([date, dayTerminy]) => (
                <div key={date} className="day-group">
                  <h4>{date}</h4>
                  <div className="time-slots">
                    {dayTerminy.map((termin) => (
                      <button
                        key={termin.idTerminu}
                        className="time-slot"
                        onClick={() => handleRezerwacja(termin.idTerminu)}
                      >
                        {formatTime(termin.dataRozpoczecia)} - {formatTime(termin.dataZakonczenia)}
                      </button>
                    ))}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export default RezerwacjaWizyty;
