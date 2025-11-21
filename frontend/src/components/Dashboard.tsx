import React from 'react';
import { useAuth } from '../contexts/AuthContext';

const Dashboard: React.FC = () => {
  const { user, userDetails, logout } = useAuth();

  const handleLogout = async () => {
    try {
      await logout();
    } catch (error) {
      console.error('Error during logout:', error);
    }
  };

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>Panel Pacjenta - Przychodnia</h1>
        <div className="user-info">
          <span>Witaj, {user?.firstName} {user?.lastName}</span>
          <button onClick={handleLogout} className="logout-btn">
            Wyloguj się
          </button>
        </div>
      </header>

      <main className="dashboard-content">
        <div className="user-details">
          <h2>Twoje dane</h2>
          <div className="info-card">
            <p><strong>Email:</strong> {user?.email}</p>
            <p><strong>Imię:</strong> {user?.firstName}</p>
            <p><strong>Nazwisko:</strong> {user?.lastName}</p>
            <p><strong>Rola:</strong> {user?.role === 'Patient' ? 'Pacjent' : user?.role}</p>
            {userDetails?.pacjent && (
              <p><strong>PESEL:</strong> {userDetails.pacjent.pesel}</p>
            )}
          </div>
        </div>

        <div className="dashboard-sections">
          <div className="section-card">
            <h3>Moje wizyty</h3>
            <p>Przeglądaj swoje zaplanowane i poprzednie wizyty</p>
            <button className="section-btn">Zobacz wizyty</button>
          </div>

          <div className="section-card">
            <h3>Umów wizytę</h3>
            <p>Zarezerwuj termin wizyty u wybranego lekarza</p>
            <button className="section-btn">Umów wizytę</button>
          </div>

          <div className="section-card">
            <h3>Lekarze</h3>
            <p>Przeglądaj dostępnych lekarzy i ich specjalizacje</p>
            <button className="section-btn">Zobacz lekarzy</button>
          </div>

          <div className="section-card">
            <h3>Historia medyczna</h3>
            <p>Przeglądaj swoją historię medyczną i diagnozy</p>
            <button className="section-btn">Historia</button>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Dashboard;