import React, { useState, useEffect } from 'react';
import ankietaService from '../services/ankietaService';
import './ZarzadzanieAnkietami.css';

interface Ankieta {
  id: string;
  idAnkiety: number;
  idWizyty: number;
  idPacjenta?: number;
  idLekarza: number;
  nazwaLekarza: string;
  pesel?: string;
  czyAnonimowa: boolean;
  dataWypelnienia: string;
  dataWizyty: string;
  typAnkiety: string;
  ocenaWizyty: number;
  odpowiedzi: Array<{
    pytanie: string;
    odpowiedz: string;
    kategoria: string;
  }>;
  dodatkoweUwagi?: string;
  dataUtworzenia: string;
}

const ZarzadzanieAnkietami: React.FC = () => {
  const [ankiety, setAnkiety] = useState<Ankieta[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedAnkieta, setSelectedAnkieta] = useState<Ankieta | null>(null);
  const [showModal, setShowModal] = useState(false);

  // Filtry
  const [filterLekarz, setFilterLekarz] = useState('');
  const [filterOcena, setFilterOcena] = useState('');
  const [filterDataOd, setFilterDataOd] = useState('');
  const [filterDataDo, setFilterDataDo] = useState('');

  useEffect(() => {
    loadAnkiety();
  }, []);

  const loadAnkiety = async () => {
    try {
      setLoading(true);
      const data = await ankietaService.getAllAnkiety();
      setAnkiety(data);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Nie udało się pobrać ankiet');
    } finally {
      setLoading(false);
    }
  };

  const deleteAnkieta = async (id: string) => {
    if (!window.confirm('Czy na pewno chcesz usunąć tę ankietę?')) return;
    
    try {
      await ankietaService.deleteAnkieta(id);
      await loadAnkiety();
      alert('Ankieta została usunięta');
    } catch (err: any) {
      alert(err.response?.data?.message || 'Nie udało się usunąć ankiety');
    }
  };

  const openAnkietaModal = (ankieta: Ankieta) => {
    setSelectedAnkieta(ankieta);
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setSelectedAnkieta(null);
  };

  // Filtrowanie ankiet
  const filteredAnkiety = ankiety.filter(ankieta => {
    if (filterLekarz && (!ankieta.nazwaLekarza || !ankieta.nazwaLekarza.toLowerCase().includes(filterLekarz.toLowerCase()))) {
      return false;
    }
    
    if (filterOcena && (!ankieta.ocenaWizyty || ankieta.ocenaWizyty.toString() !== filterOcena)) {
      return false;
    }
    
    if (filterDataOd && new Date(ankieta.dataWypelnienia) < new Date(filterDataOd)) {
      return false;
    }
    
    if (filterDataDo && new Date(ankieta.dataWypelnienia) > new Date(filterDataDo + 'T23:59:59')) {
      return false;
    }
    
    return true;
  });

  // Statystyki
  const ankietyZOcenami = ankiety.filter(a => a.ocenaWizyty != null);
  const stats = {
    total: ankiety.length,
    anonimowe: ankiety.filter(a => a.czyAnonimowa).length,
    sredniaOcena: ankietyZOcenami.length > 0 ? (ankietyZOcenami.reduce((sum, a) => sum + a.ocenaWizyty, 0) / ankietyZOcenami.length).toFixed(1) : '0',
    najlepszaOcena: ankietyZOcenami.length > 0 ? Math.max(...ankietyZOcenami.map(a => a.ocenaWizyty)) : 0,
    najgorszaOcena: ankietyZOcenami.length > 0 ? Math.min(...ankietyZOcenami.map(a => a.ocenaWizyty)) : 0
  };

  if (loading) return <div className="loading">Ładowanie ankiet...</div>;
  if (error) return <div className="error">Błąd: {error}</div>;

  return (
    <div className="zarzadzanie-ankietami">
      <h1>Zarządzanie Ankietami</h1>
      
      {/* Statystyki */}
      <div className="stats-grid">
        <div className="stat-card">
          <h3>Łączna liczba</h3>
          <span className="stat-number">{stats.total}</span>
        </div>
        <div className="stat-card">
          <h3>Anonimowe</h3>
          <span className="stat-number">{stats.anonimowe}</span>
        </div>
        <div className="stat-card">
          <h3>Średnia ocena</h3>
          <span className="stat-number">{stats.sredniaOcena}</span>
        </div>
        <div className="stat-card">
          <h3>Najlepsza ocena</h3>
          <span className="stat-number">{stats.najlepszaOcena}</span>
        </div>
      </div>

      {/* Filtry */}
      <div className="filters">
        <div className="filter-row">
          <input
            type="text"
            placeholder="Filtruj po lekarzu"
            value={filterLekarz}
            onChange={(e) => setFilterLekarz(e.target.value)}
          />
          <select
            value={filterOcena}
            onChange={(e) => setFilterOcena(e.target.value)}
          >
            <option value="">Wszystkie oceny</option>
            <option value="5">5 ⭐</option>
            <option value="4">4 ⭐</option>
            <option value="3">3 ⭐</option>
            <option value="2">2 ⭐</option>
            <option value="1">1 ⭐</option>
          </select>
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
              setFilterLekarz('');
              setFilterOcena('');
              setFilterDataOd('');
              setFilterDataDo('');
            }}
            className="clear-filters-btn"
          >
            Wyczyść filtry
          </button>
        </div>
      </div>

      {/* Lista ankiet */}
      <div className="ankiety-list">
        <table className="ankiety-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Lekarz</th>
              <th>Ocena</th>
              <th>Data wypełnienia</th>
              <th>Typ</th>
              <th>Anonimowa</th>
              <th>Akcje</th>
            </tr>
          </thead>
          <tbody>
            {filteredAnkiety.map((ankieta) => (
              <tr key={ankieta.id}>
                <td>{ankieta.idAnkiety}</td>
                <td>{ankieta.nazwaLekarza || 'Nieznany lekarz'}</td>
                <td>
                  {ankieta.ocenaWizyty ? (
                    <span className={`ocena ocena-${ankieta.ocenaWizyty}`}>
                      {ankieta.ocenaWizyty} ⭐
                    </span>
                  ) : (
                    <span className="brak-oceny">Brak oceny</span>
                  )}
                </td>
                <td>{new Date(ankieta.dataWypelnienia).toLocaleString('pl-PL')}</td>
                <td>{ankieta.typAnkiety}</td>
                <td>
                  <span className={`status ${ankieta.czyAnonimowa ? 'anonimowa' : 'zwykla'}`}>
                    {ankieta.czyAnonimowa ? '✓ Tak' : '✗ Nie'}
                  </span>
                </td>
                <td className="actions">
                  <button 
                    onClick={() => openAnkietaModal(ankieta)}
                    className="view-btn"
                  >
                    👁️ Zobacz
                  </button>
                  <button 
                    onClick={() => deleteAnkieta(ankieta.id)}
                    className="delete-btn"
                  >
                    🗑️ Usuń
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {filteredAnkiety.length === 0 && (
          <div className="no-data">
            {ankiety.length === 0 ? 'Brak ankiet w systemie' : 'Brak ankiet spełniających kryteria filtrowania'}
          </div>
        )}
      </div>

      {/* Modal z detalami ankiety */}
      {showModal && selectedAnkieta && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal-content" onClick={(e) => e.stopPropagation()}>
            <div className="modal-header">
              <h2>Szczegóły ankiety #{selectedAnkieta.idAnkiety}</h2>
              <button onClick={closeModal} className="close-btn">✕</button>
            </div>
            
            <div className="modal-body">
              <div className="ankieta-details">
                <div className="detail-section">
                  <h3>Informacje podstawowe</h3>
                  <p><strong>Lekarz:</strong> {selectedAnkieta.nazwaLekarza || 'Nieznany lekarz'}</p>
                  <p><strong>Data wizyty:</strong> {new Date(selectedAnkieta.dataWizyty).toLocaleString('pl-PL')}</p>
                  <p><strong>Data wypełnienia:</strong> {new Date(selectedAnkieta.dataWypelnienia).toLocaleString('pl-PL')}</p>
                  <p><strong>Ocena:</strong> 
                    {selectedAnkieta.ocenaWizyty ? (
                      <span className={`ocena ocena-${selectedAnkieta.ocenaWizyty}`}>
                        {selectedAnkieta.ocenaWizyty} ⭐
                      </span>
                    ) : (
                      <span className="brak-oceny">Brak oceny</span>
                    )}
                  </p>
                  <p><strong>Typ:</strong> {selectedAnkieta.typAnkiety}</p>
                  <p><strong>Anonimowa:</strong> {selectedAnkieta.czyAnonimowa ? 'Tak' : 'Nie'}</p>
                </div>

                <div className="detail-section">
                  <h3>Odpowiedzi</h3>
                  <div className="odpowiedzi-list">
                    {selectedAnkieta.odpowiedzi.map((odpowiedz, index) => (
                      <div key={index} className="odpowiedz-item">
                        <p className="pytanie"><strong>Q:</strong> {odpowiedz.pytanie}</p>
                        <p className="odpowiedz"><strong>A:</strong> {odpowiedz.odpowiedz}</p>
                        <span className="kategoria">{odpowiedz.kategoria}</span>
                      </div>
                    ))}
                  </div>
                </div>

                {selectedAnkieta.dodatkoweUwagi && (
                  <div className="detail-section">
                    <h3>Dodatkowe uwagi</h3>
                    <p className="uwagi">{selectedAnkieta.dodatkoweUwagi}</p>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default ZarzadzanieAnkietami;