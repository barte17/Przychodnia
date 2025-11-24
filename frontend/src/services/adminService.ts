import { API_BASE_URL } from '../konfiguracja/api';

export interface UserListItem {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  createdAt: string;
  pesel?: string;
  specjalizacja?: string;
}

export interface UpdateUserRole {
  userId: string;
  newRole: string;
  pesel?: string;
  specjalizacja?: string;
}

export interface CreateUserByAdmin {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: string;
  pesel?: string;
  specjalizacja?: string;
}

export interface UpdateUserByAdmin {
  email?: string;
  firstName?: string;
  lastName?: string;
  role?: string;
  pesel?: string;
  specjalizacja?: string;
}

export const adminService = {
  async getAllUsers(): Promise<UserListItem[]> {
    // Używamy tokena z localStorage (zgodnie z authService)
    const token = localStorage.getItem('auth_token');
    console.log('Admin service using token:', token?.substring(0, 20) + '...');
    
    const response = await fetch(`${API_BASE_URL}/admin/users`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch users');
    }

    return await response.json();
  },

  async getUser(userId: string): Promise<any> {
    const token = localStorage.getItem('auth_token');
    const response = await fetch(`${API_BASE_URL}/admin/users/${userId}`, {
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      throw new Error('Failed to fetch user');
    }

    return await response.json();
  },

  async createUser(userData: CreateUserByAdmin): Promise<any> {
    const token = localStorage.getItem('auth_token');
    const response = await fetch(`${API_BASE_URL}/admin/users`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to create user');
    }

    return await response.json();
  },

  async updateUserRole(userId: string, roleData: UpdateUserRole): Promise<any> {
    const token = localStorage.getItem('auth_token');
    const response = await fetch(`${API_BASE_URL}/admin/users/${userId}/role`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(roleData),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to update user role');
    }

    return await response.json();
  },

  async updateUser(userId: string, userData: UpdateUserByAdmin): Promise<any> {
    const token = localStorage.getItem('auth_token');
    const response = await fetch(`${API_BASE_URL}/admin/users/${userId}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to update user');
    }

    return await response.json();
  },

  async deleteUser(userId: string): Promise<void> {
    const token = localStorage.getItem('auth_token');
    const response = await fetch(`${API_BASE_URL}/admin/users/${userId}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || 'Failed to delete user');
    }
  },
};