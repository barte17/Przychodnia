// Centralna konfiguracja API URL - WSZYSTKIE SERWISY UŻYWAJĄ TEGO PLIKU
const getApiBaseUrl = (): string => {
  // 1. Zmienna środowiskowa ma najwyższy priorytet
  if (import.meta.env.VITE_API_URL) {
    return import.meta.env.VITE_API_URL;
  }
  
  // 2. W rozwoju (development) - backend na localhost:5178 (HTTP)
  if (import.meta.env.DEV) {
    return 'http://localhost:5178/api';
  }
  
  // 3. W produkcji używaj tego samego hosta co frontend
  return `${window.location.origin}/api`;
};

export const API_BASE_URL = getApiBaseUrl();

export default API_BASE_URL;
