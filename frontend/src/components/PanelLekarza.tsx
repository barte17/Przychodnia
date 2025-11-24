import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import wizytaService, { Wizyta } from '../services/wizytaService';
import terminService, { TerminLekarza } from '../services/terminService';
import './PanelLekarza.css';

type TabType = 'oczekujace' | 'zaakceptowane' | 'historia' | 'terminy';

const PanelLekarza: React.FC = () => {
  const { userDetails } = useAuth();
  const [activeTab, setActiveTab] = useState<TabType>('oczekujace');
  const [wizyty, setWizyty] = useState<Wizyta[]>([]);
  const [terminy, setTerminy] = useState<TerminLekarza[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Stan dla dodawania terminów
  const [showAddTermin, setShowAddTermin] = useState(false);
  const [nowyTerminData, setNowyTerminData] = useState('');
  const [nowyTerminGodzinaOd, setNowyTerminGodzinaOd] = useState('08:00');
  const [nowyTerminGodzinaDo, setNowyTerminGodzinaDo] = useState('16:00');
  const [czasTrwania, setCzasTrwania] = useState(30);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (userDetails?.lekarz?.idLekarza) {
      loadData();
    }
  }, [userDetails, activeTab]);

  const loadData = async () => {
    if (!userDetails?.lekarz?.idLekarza) return;
    
    setLoading(true);
    setError(null);
    
    try {
      if (activeTab === 'terminy') {
        const data = await terminService.getTerminyByLekarz(userDetails.lekarz.idLekarza, false);
        setTerminy(data);
      } else {
        const data = await wizytaService.getWizytyByLekarz(userDetails.lekarz.idLekarza);
        setWizyty(data);
      }
    } catch (err) {
      setError('Nie udało się pobrać danych');
    } finally {
      setLoading(false);
    }
  };

  const handleAkceptuj = async (wizytaId: number) => {
    try {
      await wizytaService.akceptujWizyte(wizytaId);
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Nie udało się zaakceptować wizyty');
    }
  };

  const handleOdrzuc = async (wizytaId: number) => {
    if (!window.confirm('Czy na pewno chcesz odrzucić tę wizytę?')) return;
    
    try {
      await wizytaService.odrzucWizyte(wizytaId);
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Nie udało się odrzucić wizyty');
    }
  };

  const handleZakoncz = async (wizytaId: number) => {
    try {
      await wizytaService.zakonczWizyte(wizytaId);
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Nie udało się zakończyć wizyty');
    }
  };

  const handleDodajTerminy = async () => {
    if (!userDetails?.lekarz?.idLekarza || !nowyTerminData) return;
    
    setSubmitting(true);
    try {
      const dataOd = new Date(`${nowyTerminData}T${nowyTerminGodzinaOd}`);
      const dataDo = new Date(`${nowyTerminData}T${nowyTerminGodzinaDo}`);
      
      await terminService.createTerminyBatch({
        idLekarza: userDetails.lekarz.idLekarza,
        dataOd: dataOd.toISOString(),
        dataDo: dataDo.toISOString(),
        czasTrwaniaMinuty: czasTrwania
      });
      
      setShowAddTermin(false);
      setNowyTerminData('');
      loadData();
      alert('Terminy zostały dodane!');
    } catch (err: any) {
      alert(err.response?.data?.message || 'Nie udało się dodać terminów');
    } finally {
      setSubmitting(false);
    }
  };

  const handleUsunTermin = async (terminId: number) => {
    if (!window.confirm('Czy na pewno chcesz usunąć ten termin?')) return;
    
    try {
      await terminService.deleteTermin(terminId);
      loadData();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Nie udało się usunąć terminu');
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('pl-PL', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const getFilteredWizyty = () => {
    switch (activeTab) {
      case 'oczekujace':
        return wizyty.filter(w => w.status === 'Oczekująca');
      case 'zaakceptowane':
        return wizyty.filter(w => w.status === 'Zaakceptowana');
      case 'historia':
        return wizyty.filter(w => ['Odbyta', 'Odrzucona', 'Anulowana'].includes(w.status));
      default:
        return [];
    }
  };

  const getStatusClass = (status: string) => {
    switch (status) {
      case 'Oczekująca': return 'status-oczekujaca';
      case 'Zaakceptowana': return 'status-zaakceptowana';
      case 'Odbyta': return 'status-odbyta';
      case 'Odrzucona': return 'status-odrzucona';
      case 'Anulowana': return 'status-anulowana';
      default: return '';
    }
  };

  if (!userDetails?.lekarz) {
    return <div className="error">Ta funkcja jest dostępna tylko dla lekarzy</div>;
  }

  return (
    <div className="panel-lekarza">
      <h2>Panel lekarza</h2>
      
      <div className="tabs">
        <button 
          className={activeTab === 'oczekujace' ? 'active' : ''}
          onClick={() => setActiveTab('oczekujace')}
        >
          Oczekujące ({wizyty.filter(w => w.status === 'Oczekująca').length})
        </button>
        <button 
          className={activeTab === 'zaakceptowane' ? 'active' : ''}
          onClick={() => setActiveTab('zaakceptowane')}
        >
          Zaakceptowane
        </button>
        <button 
          className={activeTab === 'historia' ? 'active' : ''}
          onClick={() => setActiveTab('historia')}
        >
          Historia
        </button>
        <button 
          className={activeTab === 'terminy' ? 'active' : ''}
          onClick={() => setActiveTab('terminy')}
        >
          Moje terminy
        </button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}

      {loading ? (
        <div className="loading">Ładowanie...</div>
      ) : activeTab === 'terminy' ? (
        <div className="terminy-section">
          <div className="section-header">
            <h3>Twoje dostępne terminy</h3>
            <button 
              className="btn-primary"
              onClick={() => setShowAddTermin(true)}
            >
              + Dodaj terminy
            </button>
          </div>
          
          {terminy.length === 0 ? (
            <p className="no-data">Nie masz żadnych terminów</p>
          ) : (
            <div className="terminy-list">
              {terminy.map((termin) => (
                <div key={termin.idTerminu} className={`termin-card ${!termin.czyDostepny ? 'zajety' : ''}`}>
                  <div className="termin-info">
                    <span className="termin-date">{formatDate(termin.dataRozpoczecia)}</span>
                    <span className={`termin-status ${termin.czyDostepny ? 'dostepny' : 'zajety'}`}>
                      {termin.czyDostepny ? 'Dostępny' : 'Zarezerwowany'}
                    </span>
                  </div>
                  {termin.czyDostepny && (
                    <button 
                      className="btn-delete"
                      onClick={() => handleUsunTermin(termin.idTerminu)}
                    >
                      Usuń
                    </button>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      ) : (
        <div className="wizyty-section">
          {getFilteredWizyty().length === 0 ? (
            <p className="no-data">Brak wizyt w tej kategorii</p>
          ) : (
            <div className="wizyty-list">
              {getFilteredWizyty().map((wizyta) => (
                <div key={wizyta.idWizyty} className="wizyta-card">
                  <div className="wizyta-header">
                    <span className={`status ${getStatusClass(wizyta.status)}`}>
                      {wizyta.status}
                    </span>
                    <span className="data">{formatDate(wizyta.data)}</span>
                  </div>
                  
                  <div className="wizyta-body">
                    <p><strong>Pacjent:</strong> {wizyta.pacjentImie} {wizyta.pacjentNazwisko}</p>
                  </div>
                  
                  <div className="wizyta-actions">
                    {wizyta.status === 'Oczekująca' && (
                      <>
                        <button 
                          className="btn-akceptuj"
                          onClick={() => handleAkceptuj(wizyta.idWizyty)}
                        >
                          ✓ Akceptuj
                        </button>
                        <button 
                          className="btn-odrzuc"
                          onClick={() => handleOdrzuc(wizyta.idWizyty)}
                        >
                          ✗ Odrzuć
                        </button>
                      </>
                    )}
                    
                    {wizyta.status === 'Zaakceptowana' && (
                      <button 
                        className="btn-zakoncz"
                        onClick={() => handleZakoncz(wizyta.idWizyty)}
                      >
                        Zakończ wizytę
                      </button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Modal dodawania terminów */}
      {showAddTermin && (
        <div className="modal-overlay" onClick={() => setShowAddTermin(false)}>
          <div className="modal-content" onClick={e => e.stopPropagation()}>
            <h3>Dodaj terminy</h3>
            
            <div className="form-group">
              <label>Data:</label>
              <input
                type="date"
                value={nowyTerminData}
                onChange={(e) => setNowyTerminData(e.target.value)}
                min={new Date().toISOString().split('T')[0]}
              />
            </div>
            
            <div className="form-row">
              <div className="form-group">
                <label>Godzina od:</label>
                <input
                  type="time"
                  value={nowyTerminGodzinaOd}
                  onChange={(e) => setNowyTerminGodzinaOd(e.target.value)}
                />
              </div>
              
              <div className="form-group">
                <label>Godzina do:</label>
                <input
                  type="time"
                  value={nowyTerminGodzinaDo}
                  onChange={(e) => setNowyTerminGodzinaDo(e.target.value)}
                />
              </div>
            </div>
            
            <div className="form-group">
              <label>Czas trwania wizyty (minuty):</label>
              <select
                value={czasTrwania}
                onChange={(e) => setCzasTrwania(Number(e.target.value))}
              >
                <option value={15}>15 minut</option>
                <option value={30}>30 minut</option>
                <option value={45}>45 minut</option>
                <option value={60}>60 minut</option>
              </select>
            </div>
            
            <div className="modal-actions">
              <button 
                className="btn-cancel"
                onClick={() => setShowAddTermin(false)}
              >
                Anuluj
              </button>
              <button 
                className="btn-submit"
                onClick={handleDodajTerminy}
                disabled={!nowyTerminData || submitting}
              >
                {submitting ? 'Dodawanie...' : 'Dodaj terminy'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PanelLekarza;
