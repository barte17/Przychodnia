import React, { useState, useEffect } from 'react';
import wizytaService from '../services/wizytaService';
import './ZarzadzanieWizytami.css';

interface Wizyta {
  idWizyty: number;
  data: string;
  status: string;
  idPacjenta: number;
  pacjentImie: string;
  pacjentNazwisko: string;
  idLekarza: number;
  lekarzImie: string;
  lekarzNazwisko: string;
  lekarzSpecjalizacja: string;
}

const ZarzadzanieWizytami: React.FC = () => {
  const [wizyty, setWizyty] = useState<Wizyta[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filtry
  const [filterStatus, setFilterStatus] = useState('');
  const [filterLekarz, setFilterLekarz] = useState('');
  const [filterPacjent, setFilterPacjent] = useState('');
  const [filterDataOd, setFilterDataOd] = useState('');
  const [filterDataDo, setFilterDataDo] = useState('');

  // Sortowanie
  const [sortBy, setSortBy] = useState<'data' | 'status' | 'lekarz' | 'pacjent'>('data');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');

  useEffect(() => {
    loadWizyty();
  }, []);

  const loadWizyty = async () => {
    try {
      setLoading(true);
      const data = await wizytaService.getAllWizyty();
      setWizyty(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Nie udało się pobrać wizyt');
    } finally {
      setLoading(false);
    }
  };

  const updateWizytaStatus = async (idWizyty: number, newStatus: string) => {
    try {
      await wizytaService.updateWizyta(idWizyty, { status: newStatus });
      await loadWizyty();
      alert(`Status wizyty został zmieniony na: ${newStatus}`);
    } catch (err: any) {
      alert(err.response?.data?.message || 'Nie udało się zmienić statusu wizyty');
    }
  };

  const deleteWizyta = async (idWizyty: number) => {
    if (!window.confirm('Czy na pewno chcesz usunąć tę wizytę?')) return;
    
    try {
      await wizytaService.deleteWizyta(idWizyty);
      await loadWizyty();
      alert('Wizyta została usunięta');
    } catch (err: any) {
      alert(err.response?.data?.message || 'Nie udało się usunąć wizyty');
    }
  };

  // Filtrowanie wizyt
  const filteredWizyty = wizyty.filter(wizyta => {
    if (filterStatus && wizyta.status !== filterStatus) return false;
    
    if (filterLekarz && 
        !`${wizyta.lekarzImie} ${wizyta.lekarzNazwisko}`.toLowerCase().includes(filterLekarz.toLowerCase()) &&
        !wizyta.lekarzSpecjalizacja.toLowerCase().includes(filterLekarz.toLowerCase())) {
      return false;
    }
    
    if (filterPacjent && 
        !`${wizyta.pacjentImie} ${wizyta.pacjentNazwisko}`.toLowerCase().includes(filterPacjent.toLowerCase())) {
      return false;
    }
    
    if (filterDataOd && new Date(wizyta.data) < new Date(filterDataOd)) return false;
    if (filterDataDo && new Date(wizyta.data) > new Date(filterDataDo + 'T23:59:59')) return false;
    
    return true;
  });

  // Sortowanie wizyt
  const sortedWizyty = [...filteredWizyty].sort((a, b) => {
    let comparison = 0;
    
    switch (sortBy) {
      case 'data':
        comparison = new Date(a.data).getTime() - new Date(b.data).getTime();
        break;
      case 'status':
        comparison = a.status.localeCompare(b.status);
        break;
      case 'lekarz':
        comparison = `${a.lekarzImie} ${a.lekarzNazwisko}`.localeCompare(`${b.lekarzImie} ${b.lekarzNazwisko}`);
        break;
      case 'pacjent':
        comparison = `${a.pacjentImie} ${a.pacjentNazwisko}`.localeCompare(`${b.pacjentImie} ${b.pacjentNazwisko}`);
        break;
    }
    
    return sortOrder === 'asc' ? comparison : -comparison;
  });

  const handleSort = (column: typeof sortBy) => {
    if (sortBy === column) {
      setSortOrder(sortOrder === 'asc' ? 'desc' : 'asc');
    } else {
      setSortBy(column);
      setSortOrder('asc');
    }
  };

  // Statystyki
  const stats = {
    total: wizyty.length,
    zaplanowane: wizyty.filter(w => w.status === 'Zaplanowana').length,
    oczekujace: wizyty.filter(w => w.status === 'Oczekująca').length,
    zaakceptowane: wizyty.filter(w => w.status === 'Zaakceptowana').length,
    odbyte: wizyty.filter(w => w.status === 'Odbyta').length,
    odrzucone: wizyty.filter(w => w.status === 'Odrzucona').length,
    anulowane: wizyty.filter(w => w.status === 'Anulowana').length
  };

  const getStatusColor = (status: string): string => {
    switch (status) {
      case 'Zaplanowana': return '#6f42c1';
      case 'Oczekująca': return '#fd7e14';
      case 'Zaakceptowana': return '#20c997';
      case 'Odbyta': return '#28a745';
      case 'Odrzucona': return '#dc3545';
      case 'Anulowana': return '#6c757d';
      default: return '#007bff';
    }
  };

  const getStatusIcon = (status: string): string => {
    switch (status) {
      case 'Zaplanowana': return '📅';
      case 'Oczekująca': return '⏳';
      case 'Zaakceptowana': return '✅';
      case 'Odbyta': return '✔️';
      case 'Odrzucona': return '❌';
      case 'Anulowana': return '🚫';
      default: return '📋';
    }
  };

  if (loading) return <div className="loading">Ładowanie wizyt...</div>;
  if (error) return <div className="error">Błąd: {error}</div>;

  return (
    <div className="zarzadzanie-wizytami">
      <h1>Zarządzanie Wizytami</h1>
      
      {/* Statystyki */}
      <div className="stats-grid">
        <div className="stat-card total">
          <h3>Łącznie</h3>
          <span className="stat-number">{stats.total}</span>
        </div>
        <div className="stat-card oczekujace">
          <h3>Oczekujące</h3>
          <span className="stat-number">{stats.oczekujace}</span>
        </div>
        <div className="stat-card zaakceptowane">
          <h3>Zaakceptowane</h3>
          <span className="stat-number">{stats.zaakceptowane}</span>
        </div>
        <div className="stat-card odbyte">
          <h3>Odbyte</h3>
          <span className="stat-number">{stats.odbyte}</span>
        </div>
      </div>

      {/* Filtry */}
      <div className="filters">
        <div className="filter-row">
          <select
            value={filterStatus}
            onChange={(e) => setFilterStatus(e.target.value)}
          >
            <option value="">Wszystkie statusy</option>
            <option value="Zaplanowana">Zaplanowana</option>
            <option value="Oczekująca">Oczekująca</option>
            <option value="Zaakceptowana">Zaakceptowana</option>
            <option value="Odbyta">Odbyta</option>
            <option value="Odrzucona">Odrzucona</option>
            <option value="Anulowana">Anulowana</option>
          </select>
          
          <input
            type="text"
            placeholder="Filtruj po lekarzu lub specjalizacji"
            value={filterLekarz}
            onChange={(e) => setFilterLekarz(e.target.value)}
          />
          
          <input
            type="text"
            placeholder="Filtruj po pacjencie"
            value={filterPacjent}
            onChange={(e) => setFilterPacjent(e.target.value)}
          />
          
          <input
            type="date"
            placeholder="Data od"
            value={filterDataOd}
            onChange={(e) => setFilterDataOd(e.target.value)}
          />
          
          <input
            type="date"
            placeholder="Data do"
            value={filterDataDo}
            onChange={(e) => setFilterDataDo(e.target.value)}
          />
          
          <button 
            onClick={() => {
              setFilterStatus('');
              setFilterLekarz('');
              setFilterPacjent('');
              setFilterDataOd('');
              setFilterDataDo('');
            }}
            className="clear-filters-btn"
          >
            Wyczyść filtry
          </button>
        </div>
      </div>

      {/* Lista wizyt */}
      <div className="wizyty-list">
        <table className="wizyty-table">
          <thead>
            <tr>
              <th>ID</th>
              <th 
                onClick={() => handleSort('data')}
                className={`sortable ${sortBy === 'data' ? `sorted-${sortOrder}` : ''}`}
              >
                Data i godzina {sortBy === 'data' && (sortOrder === 'asc' ? '↑' : '↓')}
              </th>
              <th 
                onClick={() => handleSort('status')}
                className={`sortable ${sortBy === 'status' ? `sorted-${sortOrder}` : ''}`}
              >
                Status {sortBy === 'status' && (sortOrder === 'asc' ? '↑' : '↓')}
              </th>
              <th 
                onClick={() => handleSort('pacjent')}
                className={`sortable ${sortBy === 'pacjent' ? `sorted-${sortOrder}` : ''}`}
              >
                Pacjent {sortBy === 'pacjent' && (sortOrder === 'asc' ? '↑' : '↓')}
              </th>
              <th 
                onClick={() => handleSort('lekarz')}
                className={`sortable ${sortBy === 'lekarz' ? `sorted-${sortOrder}` : ''}`}
              >
                Lekarz {sortBy === 'lekarz' && (sortOrder === 'asc' ? '↑' : '↓')}
              </th>
              <th>Akcje</th>
            </tr>
          </thead>
          <tbody>
            {sortedWizyty.map((wizyta) => (
              <tr key={wizyta.idWizyty}>
                <td>{wizyta.idWizyty}</td>
                <td>{new Date(wizyta.data).toLocaleString('pl-PL')}</td>
                <td>
                  <span 
                    className="status-badge"
                    style={{ backgroundColor: getStatusColor(wizyta.status) }}
                  >
                    {getStatusIcon(wizyta.status)} {wizyta.status}
                  </span>
                </td>
                <td>{wizyta.pacjentImie} {wizyta.pacjentNazwisko}</td>
                <td>
                  <div className="lekarz-info">
                    <div className="lekarz-name">
                      {wizyta.lekarzImie} {wizyta.lekarzNazwisko}
                    </div>
                    <div className="lekarz-spec">{wizyta.lekarzSpecjalizacja}</div>
                  </div>
                </td>
                <td className="actions">
                  <select
                    value={wizyta.status}
                    onChange={(e) => updateWizytaStatus(wizyta.idWizyty, e.target.value)}
                    className="status-select"
                  >
                    <option value="Zaplanowana">Zaplanowana</option>
                    <option value="Oczekująca">Oczekująca</option>
                    <option value="Zaakceptowana">Zaakceptowana</option>
                    <option value="Odbyta">Odbyta</option>
                    <option value="Odrzucona">Odrzucona</option>
                    <option value="Anulowana">Anulowana</option>
                  </select>
                  <button 
                    onClick={() => deleteWizyta(wizyta.idWizyty)}
                    className="delete-btn"
                  >
                    🗑️
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {sortedWizyty.length === 0 && (
          <div className="no-data">
            {wizyty.length === 0 ? 'Brak wizyt w systemie' : 'Brak wizyt spełniających kryteria filtrowania'}
          </div>
        )}
      </div>

      <div className="summary">
        <p>Pokazano {sortedWizyty.length} z {wizyty.length} wizyt</p>
      </div>
    </div>
  );
};

export default ZarzadzanieWizytami;