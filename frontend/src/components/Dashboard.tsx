import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

const Dashboard: React.FC = () => {
  const { user, userDetails, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = async () => {
    try {
      await logout();
    } catch (error) {
      console.error('Error during logout:', error);
    }
  };

  const getDashboardTitle = () => {
    switch (user?.role) {
      case 'Patient':
        return 'Panel Pacjenta - Przychodnia';
      case 'Doctor':
        return 'Panel Lekarza - Przychodnia';
      case 'Admin':
        return 'Panel Administratora - Przychodnia';
      default:
        return 'Panel Użytkownika - Przychodnia';
    }
  };

  const getRoleDisplayName = () => {
    switch (user?.role) {
      case 'Patient':
        return 'Pacjent';
      case 'Doctor':
        return 'Lekarz';
      case 'Admin':
        return 'Administrator';
      default:
        return user?.role;
    }
  };

  const renderPatientSections = () => (
    <div className="dashboard-sections">
      <div className="section-card">
        <h3>📅 Moje wizyty</h3>
        <p>Przeglądaj swoje zaplanowane i poprzednie wizyty oraz wypełnij ankiety</p>
        <button className="section-btn" onClick={() => navigate('/moje-wizyty')}>Zobacz wizyty</button>
      </div>

      <div className="section-card">
        <h3>➕ Umów wizytę</h3>
        <p>Zarezerwuj termin wizyty u wybranego lekarza</p>
        <button className="section-btn" onClick={() => navigate('/rezerwacja')}>Umów wizytę</button>
      </div>

      <div className="section-card">
        <h3>👨‍⚕️ Nasi lekarze</h3>
        <p>Poznaj nasz zespół lekarzy i ich specjalizacje</p>
        <button className="section-btn" onClick={() => navigate('/lekarze')}>Zobacz lekarzy</button>
      </div>
    </div>
  );

  const renderDoctorSections = () => (
    <div className="dashboard-sections">
      <div className="section-card">
        <h3>Panel Lekarza</h3>
        <p>Zarządzaj wizytami, terminami i pacjentami</p>
        <button className="section-btn" onClick={() => navigate('/panel-lekarza')}>Otwórz panel</button>
      </div>

      <div className="section-card">
        <h3>Najbliższe wizyty</h3>
        <p>Zaakceptowane wizyty na najbliższe dni</p>
        <button className="section-btn" onClick={() => navigate('/panel-lekarza')}>Najbliższe terminy</button>
      </div>

      <div className="section-card">
        <h3>Wizyty do zatwierdzenia</h3>
        <p>Wizyty wymagające akceptacji</p>
        <button className="section-btn" onClick={() => navigate('/panel-lekarza')}>Zatwierdź wizyty</button>
      </div>

      <div className="section-card">
        <h3>Zarządzanie terminami</h3>
        <p>Dodaj dostępne terminy wizyt</p>
        <button className="section-btn" onClick={() => navigate('/panel-lekarza')}>Moje terminy</button>
      </div>
    </div>
  );

  const renderAdminSections = () => (
    <div className="dashboard-sections">
      <div className="section-card">
        <h3>Zarządzanie użytkownikami</h3>
        <p>Dodaj, edytuj lub usuń użytkowników</p>
        <button 
          className="section-btn"
          onClick={() => window.location.href = '/admin/users'}
        >
          Zarządzaj użytkownikami
        </button>
      </div>

      <div className="section-card">
        <h3>Ankiety</h3>
        <p>Przeglądaj i zarządzaj ankietami pacjentów</p>
        <button className="section-btn">Zarządzaj ankietami</button>
      </div>

      <div className="section-card">
        <h3>Zarządzanie wizytami</h3>
        <p>Przeglądaj i zarządzaj wszystkimi wizytami</p>
        <button className="section-btn">Wszystkie wizyty</button>
      </div>
    </div>
  );

  const renderSections = () => {
    switch (user?.role) {
      case 'Patient':
        return renderPatientSections();
      case 'Doctor':
        return renderDoctorSections();
      case 'Admin':
        return renderAdminSections();
      default:
        return renderPatientSections();
    }
  };

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>{getDashboardTitle()}</h1>
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
            <p><strong>Rola:</strong> {getRoleDisplayName()}</p>
            {userDetails?.pacjent && (
              <p><strong>PESEL:</strong> {userDetails.pacjent.pesel}</p>
            )}
            {userDetails?.lekarz && (
              <p><strong>Specjalizacja:</strong> {userDetails.lekarz.specjalizacja}</p>
            )}
          </div>
        </div>

        {renderSections()}
      </main>
    </div>
  );
};

export default Dashboard;