import axios from 'axios';

const API_BASE_URL = 'https://localhost:5178/api';

export interface RegisterData {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: 'Patient' | 'Doctor' | 'Admin';
  pesel?: string;
  specjalizacja?: string;
}

export interface LoginData {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  expires: string;
}

export interface UserInfo {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  createdAt: string;
  pacjent?: {
    idPacjenta: number;
    imie: string;
    nazwisko: string;
    pesel: string;
  };
  lekarz?: {
    idLekarza: number;
    imie: string;
    nazwisko: string;
    specjalizacja: string;
  };
}

class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'user_info';

  constructor() {
    // Set up axios interceptor for authentication
    axios.interceptors.request.use((config) => {
      const token = this.getToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
      return config;
    });

    // Set up response interceptor to handle token expiration
    axios.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          this.logout();
          window.location.href = '/login';
        }
        return Promise.reject(error);
      }
    );
  }

  async register(data: RegisterData): Promise<AuthResponse> {
    const response = await axios.post(`${API_BASE_URL}/auth/register`, data);
    const authData = response.data;
    
    this.setToken(authData.token);
    this.setUserInfo(authData);
    
    return authData;
  }

  async login(data: LoginData): Promise<AuthResponse> {
    const response = await axios.post(`${API_BASE_URL}/auth/login`, data);
    const authData = response.data;
    
    this.setToken(authData.token);
    this.setUserInfo(authData);
    
    return authData;
  }

  async getCurrentUser(): Promise<UserInfo> {
    const response = await axios.get(`${API_BASE_URL}/auth/me`);
    return response.data;
  }

  async logout(): Promise<void> {
    try {
      await axios.post(`${API_BASE_URL}/auth/logout`);
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      this.clearAuthData();
    }
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getUserInfo(): AuthResponse | null {
    const userInfo = localStorage.getItem(this.USER_KEY);
    return userInfo ? JSON.parse(userInfo) : null;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    const userInfo = this.getUserInfo();
    
    if (!token || !userInfo) {
      return false;
    }

    // Check if token is expired
    const expirationDate = new Date(userInfo.expires);
    return expirationDate > new Date();
  }

  hasRole(requiredRole: string): boolean {
    const userInfo = this.getUserInfo();
    return userInfo?.role === requiredRole;
  }

  private setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  private setUserInfo(userInfo: AuthResponse): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(userInfo));
  }

  private clearAuthData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }
}

export default new AuthService();