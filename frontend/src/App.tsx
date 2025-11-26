import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import Login from './components/Login';
import Register from './components/Register';
import Dashboard from './components/Dashboard';
import ProtectedRoute from './components/ProtectedRoute';
import AdminRoute from './components/AdminRoute';
import UserManagement from './components/UserManagement';
import MojeWizyty from './components/MojeWizyty';
import RezerwacjaWizyty from './components/RezerwacjaWizyty';
import PanelLekarza from './components/PanelLekarza';
import ListaLekarzy from './components/ListaLekarzy';
import ZarzadzanieAnkietami from './components/ZarzadzanieAnkietami';
import ZarzadzanieWizytami from './components/ZarzadzanieWizytami';
import './App.css'

const AppRoutes: React.FC = () => {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner">Ładowanie aplikacji...</div>
      </div>
    );
  }

  return (
    <Routes>
      {/* Publiczne trasy */}
      <Route 
        path="/login" 
        element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <Login />} 
      />
      <Route 
        path="/register" 
        element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <Register />} 
      />
      
      {/* Chronione trasy */}
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        }
      />
      
      {/* Trasy administracyjne */}
      <Route
        path="/admin/users"
        element={
          <AdminRoute>
            <UserManagement />
          </AdminRoute>
        }
      />
      <Route
        path="/admin/ankiety"
        element={
          <AdminRoute>
            <ZarzadzanieAnkietami />
          </AdminRoute>
        }
      />
      <Route
        path="/admin/wizyty"
        element={
          <AdminRoute>
            <ZarzadzanieWizytami />
          </AdminRoute>
        }
      />

      {/* Trasy pacjenta */}
      <Route
        path="/moje-wizyty"
        element={
          <ProtectedRoute>
            <MojeWizyty />
          </ProtectedRoute>
        }
      />
      <Route
        path="/rezerwacja"
        element={
          <ProtectedRoute>
            <RezerwacjaWizyty />
          </ProtectedRoute>
        }
      />
      <Route
        path="/lekarze"
        element={
          <ProtectedRoute>
            <ListaLekarzy />
          </ProtectedRoute>
        }
      />

      {/* Trasy lekarza */}
      <Route
        path="/panel-lekarza"
        element={
          <ProtectedRoute>
            <PanelLekarza />
          </ProtectedRoute>
        }
      />
      
      {/* Przekierowanie domyślne */}
      <Route
        path="/"
        element={
          isAuthenticated ? (
            <Navigate to="/dashboard" replace />
          ) : (
            <Navigate to="/login" replace />
          )
        }
      />
      
      {/* 404 - przekieruj do głównej */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

function App() {
  return (
    <Router>
      <AuthProvider>
        <div className="app">
          <AppRoutes />
        </div>
      </AuthProvider>
    </Router>
  );
}

export default App
