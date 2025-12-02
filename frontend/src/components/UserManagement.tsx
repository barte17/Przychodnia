import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { adminService, UserListItem, CreateUserByAdmin, UpdateUserRole } from '../services/adminService';
import { useAuth } from '../contexts/AuthContext';

const UserManagement: React.FC = () => {
  const { user, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [editingUser, setEditingUser] = useState<UserListItem | null>(null);
  const [showRoleChangeForm, setShowRoleChangeForm] = useState<string | null>(null);

  // Form states
  const [createForm, setCreateForm] = useState<CreateUserByAdmin>({
    email: '',
    password: '',
    firstName: '',
    lastName: '',
    role: 'Patient',
    pesel: '',
    specjalizacja: ''
  });

  const [roleChangeForm, setRoleChangeForm] = useState<UpdateUserRole>({
    userId: '',
    newRole: 'Patient',
    pesel: '',
    specjalizacja: ''
  });

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      setLoading(true);

      // Debug - sprawdź dane użytkownika
      console.log('Current user:', user);
      console.log('Is authenticated:', isAuthenticated);
      console.log('User role:', user?.role);
      console.log('Token from localStorage (token):', localStorage.getItem('token'));
      console.log('Token from localStorage (auth_token):', localStorage.getItem('auth_token'));

      // Debug - sprawdź co widzi backend
      try {
        const debugResponse = await fetch('http://localhost:5178/api/admin/debug-auth', {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`,
            'Content-Type': 'application/json',
          },
        });
        const debugData = await debugResponse.json();
        console.log('Backend debug auth:', debugData);
      } catch (debugError) {
        console.log('Debug auth error:', debugError);
      }

      const usersData = await adminService.getAllUsers();
      setUsers(usersData);
      setError(null);
    } catch (err: any) {
      console.error('Error loading users:', err);
      if (err.message.includes('401') || err.message.includes('Unauthorized')) {
        setError(`Brak uprawnień. Twoja rola: ${user?.role}. Wymagana: Admin`);
      } else {
        setError('Błąd podczas ładowania użytkowników: ' + err.message);
      }
    } finally {
      setLoading(false);
    }
  };

  const handleCreateUser = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await adminService.createUser(createForm);
      setShowCreateForm(false);
      setCreateForm({
        email: '',
        password: '',
        firstName: '',
        lastName: '',
        role: 'Patient',
        pesel: '',
        specjalizacja: ''
      });
      await loadUsers();
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Błąd podczas tworzenia użytkownika');
    }
  };

  const handleRoleChange = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await adminService.updateUserRole(roleChangeForm.userId, roleChangeForm);
      setShowRoleChangeForm(null);
      setRoleChangeForm({
        userId: '',
        newRole: 'Patient',
        pesel: '',
        specjalizacja: ''
      });
      await loadUsers();
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Błąd podczas zmiany roli');
    }
  };

  const handleDeleteUser = async (userId: string) => {
    if (!window.confirm('Czy na pewno chcesz usunąć tego użytkownika?')) {
      return;
    }

    try {
      await adminService.deleteUser(userId);
      await loadUsers();
      setError(null);
    } catch (err: any) {
      setError(err.message || 'Błąd podczas usuwania użytkownika');
    }
  };

  const openRoleChangeForm = (user: UserListItem) => {
    setRoleChangeForm({
      userId: user.id,
      newRole: user.role,
      pesel: user.pesel || '',
      specjalizacja: user.specjalizacja || ''
    });
    setShowRoleChangeForm(user.id);
  };

  const getRoleDisplayName = (role: string) => {
    switch (role) {
      case 'Patient': return 'Pacjent';
      case 'Doctor': return 'Lekarz';
      case 'Admin': return 'Administrator';
      default: return role;
    }
  };

  // Sprawdź czy użytkownik ma uprawnienia
  if (!isAuthenticated) {
    return (
      <div className="access-denied">
        <h2>Nie jesteś zalogowany</h2>
        <p>Musisz się zalogować, aby uzyskać dostęp do tej strony.</p>
      </div>
    );
  }

  if (user?.role !== 'Admin') {
    return (
      <div className="access-denied">
        <h2>Brak uprawnień</h2>
        <p>Nie masz uprawnień do zarządzania użytkownikami.</p>
        <p>Twoja rola: <strong>{user?.role}</strong></p>
        <p>Wymagana rola: <strong>Admin</strong></p>
        <button onClick={() => window.history.back()}>Wróć</button>
      </div>
    );
  }

  if (loading) {
    return <div className="loading">Ładowanie użytkowników...</div>;
  }

  return (
    <div className="user-management">
      <div className="user-management-header">
        <button onClick={() => navigate('/dashboard')} className="btn-back">
          ← Wróć do menu głównego
        </button>
        <h2>Zarządzanie użytkownikami</h2>
        <button
          onClick={() => setShowCreateForm(true)}
          className="btn-primary"
        >
          Dodaj nowego użytkownika
        </button>
      </div>

      {error && (
        <div className="error-message">
          {error}
          <button onClick={() => setError(null)} className="close-error">×</button>
        </div>
      )}

      {showCreateForm && (
        <div className="modal-overlay">
          <div className="modal">
            <div className="modal-header">
              <h3>Dodaj nowego użytkownika</h3>
              <button onClick={() => setShowCreateForm(false)} className="close-btn">×</button>
            </div>
            <form onSubmit={handleCreateUser} className="user-form">
              <div className="form-group">
                <label>Email:</label>
                <input
                  type="email"
                  value={createForm.email}
                  onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })}
                  required
                />
              </div>
              <div className="form-group">
                <label>Hasło:</label>
                <input
                  type="password"
                  value={createForm.password}
                  onChange={(e) => setCreateForm({ ...createForm, password: e.target.value })}
                  required
                  minLength={6}
                  placeholder="Minimum 6 znaków"
                />
              </div>
              <div className="form-row">
                <div className="form-group">
                  <label>Imię:</label>
                  <input
                    type="text"
                    value={createForm.firstName}
                    onChange={(e) => setCreateForm({ ...createForm, firstName: e.target.value })}
                    required
                  />
                </div>
                <div className="form-group">
                  <label>Nazwisko:</label>
                  <input
                    type="text"
                    value={createForm.lastName}
                    onChange={(e) => setCreateForm({ ...createForm, lastName: e.target.value })}
                    required
                  />
                </div>
              </div>
              <div className="form-group">
                <label>Rola:</label>
                <select
                  value={createForm.role}
                  onChange={(e) => setCreateForm({ ...createForm, role: e.target.value })}
                >
                  <option value="Patient">Pacjent</option>
                  <option value="Doctor">Lekarz</option>
                  <option value="Admin">Administrator</option>
                </select>
              </div>
              {createForm.role === 'Patient' && (
                <div className="form-group">
                  <label>PESEL:</label>
                  <input
                    type="text"
                    value={createForm.pesel}
                    onChange={(e) => setCreateForm({ ...createForm, pesel: e.target.value })}
                    maxLength={11}
                    required
                  />
                </div>
              )}
              {createForm.role === 'Doctor' && (
                <div className="form-group">
                  <label>Specjalizacja:</label>
                  <input
                    type="text"
                    value={createForm.specjalizacja}
                    onChange={(e) => setCreateForm({ ...createForm, specjalizacja: e.target.value })}
                    required
                  />
                </div>
              )}
              <div className="form-actions">
                <button type="button" onClick={() => setShowCreateForm(false)} className="btn-secondary">
                  Anuluj
                </button>
                <button type="submit" className="btn-primary">
                  Utwórz użytkownika
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {showRoleChangeForm && (
        <div className="modal-overlay">
          <div className="modal">
            <div className="modal-header">
              <h3>Zmień rolę użytkownika</h3>
              <button onClick={() => setShowRoleChangeForm(null)} className="close-btn">×</button>
            </div>
            <form onSubmit={handleRoleChange} className="user-form">
              <div className="form-group">
                <label>Nowa rola:</label>
                <select
                  value={roleChangeForm.newRole}
                  onChange={(e) => setRoleChangeForm({ ...roleChangeForm, newRole: e.target.value })}
                >
                  <option value="Patient">Pacjent</option>
                  <option value="Doctor">Lekarz</option>
                  <option value="Admin">Administrator</option>
                </select>
              </div>
              {roleChangeForm.newRole === 'Patient' && (
                <div className="form-group">
                  <label>PESEL:</label>
                  <input
                    type="text"
                    value={roleChangeForm.pesel}
                    onChange={(e) => setRoleChangeForm({ ...roleChangeForm, pesel: e.target.value })}
                    maxLength={11}
                    required
                  />
                </div>
              )}
              {roleChangeForm.newRole === 'Doctor' && (
                <div className="form-group">
                  <label>Specjalizacja:</label>
                  <input
                    type="text"
                    value={roleChangeForm.specjalizacja}
                    onChange={(e) => setRoleChangeForm({ ...roleChangeForm, specjalizacja: e.target.value })}
                    required
                  />
                </div>
              )}
              <div className="form-actions">
                <button type="button" onClick={() => setShowRoleChangeForm(null)} className="btn-secondary">
                  Anuluj
                </button>
                <button type="submit" className="btn-primary">
                  Zmień rolę
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      <div className="users-table-container">
        <table className="users-table">
          <thead>
            <tr>
              <th>Imię i nazwisko</th>
              <th>Email</th>
              <th>Rola</th>
              <th>PESEL</th>
              <th>Specjalizacja</th>
              <th>Data utworzenia</th>
              <th>Akcje</th>
            </tr>
          </thead>
          <tbody>
            {users.map((user) => (
              <tr key={user.id}>
                <td>{user.firstName} {user.lastName}</td>
                <td>{user.email}</td>
                <td>
                  <span className={`role-badge role-${user.role.toLowerCase()}`}>
                    {getRoleDisplayName(user.role)}
                  </span>
                </td>
                <td>{user.pesel || '-'}</td>
                <td>{user.specjalizacja || '-'}</td>
                <td>{new Date(user.createdAt).toLocaleDateString('pl-PL')}</td>
                <td className="actions-cell">
                  <button
                    onClick={() => openRoleChangeForm(user)}
                    className="btn-edit"
                    title="Zmień rolę"
                  >
                    Zmień rolę
                  </button>
                  <button
                    onClick={() => handleDeleteUser(user.id)}
                    className="btn-delete"
                    title="Usuń użytkownika"
                  >
                    Usuń
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {users.length === 0 && (
          <div className="no-users">
            Brak użytkowników w systemie
          </div>
        )}
      </div>
    </div>
  );
};

export default UserManagement;