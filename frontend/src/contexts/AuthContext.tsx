import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import authService, { AuthResponse, UserInfo, LoginData, RegisterData } from '../services/authService';

interface AuthContextType {
  user: AuthResponse | null;
  userDetails: UserInfo | null;
  loading: boolean;
  login: (data: LoginData) => Promise<void>;
  register: (data: RegisterData) => Promise<void>;
  logout: () => Promise<void>;
  isAuthenticated: boolean;
  hasRole: (role: string) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<AuthResponse | null>(null);
  const [userDetails, setUserDetails] = useState<UserInfo | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initializeAuth = async () => {
      try {
        if (authService.isAuthenticated()) {
          const userInfo = authService.getUserInfo();
          setUser(userInfo);
          
          // Fetch detailed user information
          try {
            const details = await authService.getCurrentUser();
            setUserDetails(details);
          } catch (error) {
            console.error('Error fetching user details:', error);
            // If we can't fetch details, clear auth data
            await authService.logout();
            setUser(null);
            setUserDetails(null);
          }
        }
      } catch (error) {
        console.error('Error initializing auth:', error);
        setUser(null);
        setUserDetails(null);
      } finally {
        setLoading(false);
      }
    };

    initializeAuth();
  }, []);

  const login = async (data: LoginData): Promise<void> => {
    try {
      const authData = await authService.login(data);
      setUser(authData);
      
      // Fetch detailed user information
      const details = await authService.getCurrentUser();
      setUserDetails(details);
    } catch (error) {
      setUser(null);
      setUserDetails(null);
      throw error;
    }
  };

  const register = async (data: RegisterData): Promise<void> => {
    try {
      const authData = await authService.register(data);
      setUser(authData);
      
      // Fetch detailed user information
      const details = await authService.getCurrentUser();
      setUserDetails(details);
    } catch (error) {
      setUser(null);
      setUserDetails(null);
      throw error;
    }
  };

  const logout = async (): Promise<void> => {
    try {
      await authService.logout();
    } finally {
      setUser(null);
      setUserDetails(null);
    }
  };

  const isAuthenticated = authService.isAuthenticated();
  
  const hasRole = (role: string): boolean => {
    return authService.hasRole(role);
  };

  const value: AuthContextType = {
    user,
    userDetails,
    loading,
    login,
    register,
    logout,
    isAuthenticated,
    hasRole,
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};