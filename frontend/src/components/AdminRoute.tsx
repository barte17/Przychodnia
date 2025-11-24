import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';

interface AdminRouteProps {
  children: React.ReactNode;
}

const AdminRoute: React.FC<AdminRouteProps> = ({ children }) => {
  const { isAuthenticated, user, loading } = useAuth();

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner">Ładowanie...</div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (user?.role !== 'Admin') {
    return (
      <div className="access-denied">
        <h2>Brak dostępu</h2>
        <p>Nie masz uprawnień do przeglądania tej strony.</p>
        <button onClick={() => window.history.back()}>Wróć</button>
      </div>
    );
  }

  return <>{children}</>;
};

export default AdminRoute;