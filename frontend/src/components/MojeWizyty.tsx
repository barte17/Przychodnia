import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import wizytaService, { Wizyta } from '../services/wizytaService';
import ankietaService, { OdpowiedzAnkiety } from '../services/ankietaService';
import './MojeWizyty.css';

interface WizytaWithAnkieta extends Wizyta {
  maAnkiete?: boolean;
}

// Pytania ankiety medycznej
const PYTANIA_ANKIETY = [
  { pytanie: 'Jak oceniasz swoje samopoczucie po wizycie?', kategoria: 'Samopoczucie' },
  { pytanie: 'Czy lekarz wyjaśnił diagnozę w zrozumiały sposób?', kategoria: 'Komunikacja' },
  { pytanie: 'Czy otrzymałeś/aś wszystkie potrzebne informacje o leczeniu?', kategoria: 'Informacje' },
  { pytanie: 'Czy zalecenia lekarza są dla Ciebie jasne?', kategoria: 'Zalecenia' },
  { pytanie: 'Czy masz jakieś dodatkowe objawy, które chciałbyś/abyś zgłosić?', kategoria: 'Objawy' },
];

const MojeWizyty: React.FC = () => {
  const navigate = useNavigate();
  const { userDetails } = useAuth();
  const [wizyty, setWizyty] = useState<WizytaWithAnkieta[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [ankietaModal, setAnkietaModal] = useState<Wizyta | null>(null);
  const [odpowiedzi, setOdpowiedzi] = useState<{ [key: number]: string }>({});
  const [uwagi, setUwagi] = useState('');
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    loadWizyty();
  }, [userDetails]);

  const loadWizyty = async () => {
    if (!userDetails?.pacjent?.idPacjenta) {
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      const data = await wizytaService.getWizytyByPacjent(userDetails.pacjent.idPacjenta);

      // Sprawdź które wizyty mają już ankietę
      const wizytaWithAnkieta = await Promise.all(
        data.map(async (wizyta) => {
          if (wizyta.status === 'Odbyta') {
            const response = await ankietaService.czyWizytaOceniona(wizyta.idWizyty);
            return { ...wizyta, maAnkiete: response.czyOceniona };
          }
          return { ...wizyta, maAnkiete: false };
        })
      );

      setWizyty(wizytaWithAnkieta);
    } catch (err) {
      setError('Nie udało się pobrać wizyt');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleAnuluj = async (wizytaId: number) => {
    if (!window.confirm('Czy na pewno chcesz anulować tę wizytę?')) return;

    try {
      await wizytaService.anulujWizyte(wizytaId);
      loadWizyty();
    } catch (err) {
      alert('Nie udało się anulować wizyty');
    }
  };

  const handleOpenAnkieta = (wizyta: Wizyta) => {
    setAnkietaModal(wizyta);
    setOdpowiedzi({});
    setUwagi('');
  };

  const handleSubmitAnkieta = async () => {
    if (!ankietaModal) return;

    // Sprawdź czy wszystkie pytania mają odpowiedzi
    const nieodpowiedziane = PYTANIA_ANKIETY.filter((_, idx) => !odpowiedzi[idx]?.trim());
    if (nieodpowiedziane.length > 0) {
      alert('Proszę odpowiedzieć na wszystkie pytania');
      return;
    }

    setSubmitting(true);
    try {
      const odpowiedziAnkiety: OdpowiedzAnkiety[] = PYTANIA_ANKIETY.map((pyt, idx) => ({
        pytanie: pyt.pytanie,
        odpowiedz: odpowiedzi[idx],
        kategoria: pyt.kategoria
      }));

      await ankietaService.createAnkietaAnonimowa({
        idWizyty: ankietaModal.idWizyty,
        ocenaWizyty: 5,
        odpowiedzi: odpowiedziAnkiety,
        dodatkoweUwagi: uwagi || undefined
      });

      // Natychmiast aktualizuj stan lokalnie, żeby uniknąć wyświetlania przycisku
      setWizyty(prevWizyty =>
        prevWizyty.map(w =>
          w.idWizyty === ankietaModal.idWizyty
            ? { ...w, maAnkiete: true }
            : w
        )
      );

      setAnkietaModal(null);
      setOdpowiedzi({});
      setUwagi('');
      alert('Dziękujemy za wypełnienie ankiety!');

      // Przeładuj dane w tle dla pewności
      loadWizyty();
    } catch (err: any) {
      alert(err.response?.data?.message || 'Nie udało się zapisać ankiety');
    } finally {
      setSubmitting(false);
    }
  };

  const getStatusClass = (status: string) => {
    switch (status) {
      case 'Oczekujaca': return 'status-oczekujaca';
      case 'Zaakceptowana': return 'status-zaakceptowana';
      case 'Odbyta': return 'status-odbyta';
      case 'Odrzucona': return 'status-odrzucona';
      case 'Anulowana': return 'status-anulowana';
      default: return '';
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

  if (loading) return <div className="loading">Ładowanie...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!userDetails?.pacjent) return <div className="error">Brak danych pacjenta</div>;

  return (
    <div className="moje-wizyty">
      <button onClick={() => navigate('/dashboard')} className="btn-back">
        ← Wróć do menu głównego
      </button>
      <h2>Moje wizyty</h2>

      {wizyty.length === 0 ? (
        <p className="no-data">Nie masz żadnych wizyt</p>
      ) : (
        <div className="wizyty-list">
          {wizyty.map((wizyta) => (
            <div key={wizyta.idWizyty} className="wizyta-card">
              <div className="wizyta-header">
                <span className={`status ${getStatusClass(wizyta.status)}`}>
                  {wizyta.status}
                </span>
                <span className="data">{formatDate(wizyta.data)}</span>
              </div>

              <div className="wizyta-body">
                <p className="lekarz">
                  <strong>Lekarz:</strong> {wizyta.lekarzImie} {wizyta.lekarzNazwisko}
                </p>
                <p className="specjalizacja">
                  <strong>Specjalizacja:</strong> {wizyta.lekarzSpecjalizacja}
                </p>
              </div>

              <div className="wizyta-actions">
                {(wizyta.status === 'Oczekująca' || wizyta.status === 'Zaakceptowana') && (
                  <button
                    className="btn-anuluj"
                    onClick={() => handleAnuluj(wizyta.idWizyty)}
                  >
                    Anuluj wizytę
                  </button>
                )}

                {wizyta.status === 'Odbyta' && !wizyta.maAnkiete && (
                  <button
                    className="btn-ankieta"
                    onClick={() => handleOpenAnkieta(wizyta)}
                  >
                    📋 Wypełnij ankietę
                  </button>
                )}

                {wizyta.status === 'Odbyta' && wizyta.maAnkiete && (
                  <span className="ankieta-done">✓ Ankieta wypełniona</span>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Modal ankiety medycznej */}
      {ankietaModal && (
        <div className="modal-overlay" onClick={() => setAnkietaModal(null)}>
          <div className="modal-content ankieta-modal" onClick={e => e.stopPropagation()}>
            <h3>📋 Ankieta dotycząca wizyty</h3>
            <p className="ankieta-info">
              Wypełnij ankietę dotyczącą Twojej wizyty. Dane są przechowywane anonimowo.
            </p>
            <p className="wizyta-info">
              <strong>Lekarz:</strong> {ankietaModal.lekarzImie} {ankietaModal.lekarzNazwisko}<br />
              <strong>Data wizyty:</strong> {formatDate(ankietaModal.data)}
            </p>

            <div className="ankieta-pytania">
              {PYTANIA_ANKIETY.map((pyt, idx) => (
                <div key={idx} className="form-group">
                  <label>{idx + 1}. {pyt.pytanie}</label>
                  <textarea
                    value={odpowiedzi[idx] || ''}
                    onChange={(e) => setOdpowiedzi({ ...odpowiedzi, [idx]: e.target.value })}
                    placeholder="Wpisz swoją odpowiedź..."
                    rows={2}
                  />
                </div>
              ))}
            </div>

            <div className="form-group">
              <label>Dodatkowe uwagi (opcjonalne):</label>
              <textarea
                value={uwagi}
                onChange={(e) => setUwagi(e.target.value)}
                placeholder="Inne informacje, które chciałbyś/abyś przekazać..."
                rows={3}
              />
            </div>

            <div className="modal-actions">
              <button
                className="btn-cancel"
                onClick={() => setAnkietaModal(null)}
              >
                Anuluj
              </button>
              <button
                className="btn-submit"
                onClick={handleSubmitAnkieta}
                disabled={submitting}
              >
                {submitting ? 'Zapisywanie...' : 'Wyślij ankietę'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default MojeWizyty;
