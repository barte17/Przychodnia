import axios from 'axios';
import { API_BASE_URL } from '../konfiguracja/api';

export interface Lekarz {
  idLekarza: number;
  imie: string;
  nazwisko: string;
  specjalizacja: string;
}

class LekarzService {
  // Pobierz wszystkich lekarzy
  async getLekarze(): Promise<Lekarz[]> {
    const response = await axios.get(`${API_BASE_URL}/lekarz`);
    return response.data;
  }

  // Pobierz lekarza po ID
  async getLekarz(id: number): Promise<Lekarz> {
    const response = await axios.get(`${API_BASE_URL}/lekarz/${id}`);
    return response.data;
  }
}

export default new LekarzService();
