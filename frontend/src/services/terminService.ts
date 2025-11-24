import axios from 'axios';
import { API_BASE_URL } from '../konfiguracja/api';

export interface TerminLekarza {
  idTerminu: number;
  dataRozpoczecia: string;
  dataZakonczenia: string;
  czyDostepny: boolean;
  idLekarza: number;
  lekarzImie: string;
  lekarzNazwisko: string;
  lekarzSpecjalizacja: string;
}

export interface CreateTerminDto {
  dataRozpoczecia: string;
  dataZakonczenia: string;
  idLekarza: number;
}

export interface CreateTerminyBatchDto {
  idLekarza: number;
  dataOd: string;
  dataDo: string;
  czasTrwaniaMinuty: number;
}

class TerminService {
  // Pobierz wszystkie dostępne terminy
  async getTerminy(): Promise<TerminLekarza[]> {
    const response = await axios.get(`${API_BASE_URL}/termin`);
    return response.data;
  }

  // Pobierz terminy konkretnego lekarza
  async getTerminyByLekarz(lekarzId: number, tylkoDostepne: boolean = true): Promise<TerminLekarza[]> {
    const response = await axios.get(`${API_BASE_URL}/termin/lekarz/${lekarzId}?tylkoDostepne=${tylkoDostepne}`);
    return response.data;
  }

  // Dodaj pojedynczy termin (lekarz)
  async createTermin(data: CreateTerminDto): Promise<TerminLekarza> {
    const response = await axios.post(`${API_BASE_URL}/termin`, data);
    return response.data;
  }

  // Dodaj wiele terminów naraz (lekarz)
  async createTerminyBatch(data: CreateTerminyBatchDto): Promise<{ message: string; terminy: TerminLekarza[] }> {
    const response = await axios.post(`${API_BASE_URL}/termin/batch`, data);
    return response.data;
  }

  // Usuń termin (lekarz)
  async deleteTermin(terminId: number): Promise<void> {
    await axios.delete(`${API_BASE_URL}/termin/${terminId}`);
  }
}

export default new TerminService();
