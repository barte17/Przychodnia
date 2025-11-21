import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate, Link } from 'react-router-dom';

const Register: React.FC = () => {
  const [formData, setFormData] = useState({
    email: '',
    password: '',
    confirmPassword: '',
    firstName: '',
    lastName: '',
    role: 'Patient' as 'Patient' | 'Doctor' | 'Admin',
    pesel: '',
    specjalizacja: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const { register } = useAuth();
  const navigate = useNavigate();

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    // Validation
    if (formData.password !== formData.confirmPassword) {
      setError('Hasła nie są identyczne');
      setLoading(false);
      return;
    }

    if (formData.role === 'Patient' && !formData.pesel) {
      setError('PESEL jest wymagany dla pacjentów');
      setLoading(false);
      return;
    }

    if (formData.role === 'Doctor' && !formData.specjalizacja) {
      setError('Specjalizacja jest wymagana dla lekarzy');
      setLoading(false);
      return;
    }

    try {
      const registerData = {
        email: formData.email,
        password: formData.password,
        firstName: formData.firstName,
        lastName: formData.lastName,
        role: formData.role,
        ...(formData.role === 'Patient' && { pesel: formData.pesel }),
        ...(formData.role === 'Doctor' && { specjalizacja: formData.specjalizacja })
      };

      await register(registerData);
      navigate('/dashboard');
    } catch (error: any) {
      setError(
        error.response?.data?.message || 
        'Wystąpił błąd podczas rejestracji. Spróbuj ponownie.'
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="register-container">
      <div className="register-form">
        <h2>Rejestracja</h2>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label htmlFor="firstName">Imię:</label>
            <input
              type="text"
              id="firstName"
              name="firstName"
              value={formData.firstName}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="lastName">Nazwisko:</label>
            <input
              type="text"
              id="lastName"
              name="lastName"
              value={formData.lastName}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="email">Email:</label>
            <input
              type="email"
              id="email"
              name="email"
              value={formData.email}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Hasło:</label>
            <input
              type="password"
              id="password"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              required
              disabled={loading}
              minLength={6}
            />
          </div>

          <div className="form-group">
            <label htmlFor="confirmPassword">Potwierdź hasło:</label>
            <input
              type="password"
              id="confirmPassword"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleInputChange}
              required
              disabled={loading}
            />
          </div>

          {/* Rola ustawiona domyślnie na Patient - ukryta dla zwykłych użytkowników */}

          {formData.role === 'Patient' && (
            <div className="form-group">
              <label htmlFor="pesel">PESEL:</label>
              <input
                type="text"
                id="pesel"
                name="pesel"
                value={formData.pesel}
                onChange={handleInputChange}
                disabled={loading}
                pattern="[0-9]{11}"
                maxLength={11}
              />
            </div>
          )}

          {formData.role === 'Doctor' && (
            <div className="form-group">
              <label htmlFor="specjalizacja">Specjalizacja:</label>
              <input
                type="text"
                id="specjalizacja"
                name="specjalizacja"
                value={formData.specjalizacja}
                onChange={handleInputChange}
                disabled={loading}
              />
            </div>
          )}

          {error && (
            <div className="error-message">
              {error}
            </div>
          )}

          <button 
            type="submit" 
            disabled={loading}
            className="submit-button"
          >
            {loading ? 'Rejestrowanie...' : 'Zarejestruj się'}
          </button>
        </form>
        
        <div className="auth-links">
          <p>
            Masz już konto? <Link to="/login">Zaloguj się</Link>
          </p>
        </div>
      </div>
    </div>
  );
};

export default Register;