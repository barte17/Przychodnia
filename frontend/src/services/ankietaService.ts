import axios from 'axios';
import { API_BASE_URL } from '../konfiguracja/api';

export interface OdpowiedzAnkiety {
  pytanie: string;
  odpowiedz: string;
  kategoria?: string;
}

export interface Ankieta {
  id?: string;
  idAnkiety: number;
  idPacjenta?: number;
  idWizyty?: number;
  idLekarza?: number;
  nazwaLekarza?: string;
  pesel?: string;
  czyAnonimowa: boolean;
  dataWypelnienia: string;
  dataWizyty?: string;
  typAnkiety: string;
  ocenaWizyty?: number;
  odpowiedzi: OdpowiedzAnkiety[];
  dodatkoweUwagi?: string;
  dataUtworzenia: string;
  ostatniaModyfikacja: string;
}

export interface CreateAnkietaAnonimowaDto {
  idWizyty: number;
  ocenaWizyty: number;
  odpowiedzi?: OdpowiedzAnkiety[];
  dodatkoweUwagi?: string;
}

export interface OcenyLekarza {
  idLekarza: number;
  sredniaOcena: number;
  liczbaOcen: number;
  oceny: Ankieta[];
}

export interface CzyOcenionaResponse {
  wizytaId: number;
  czyOceniona: boolean;
  ocena?: number;
}

class AnkietaService {
  // Pobierz wszystkie ankiety
  async getAnkiety(): Promise<Ankieta[]> {
    const response = await axios.get(`${API_BASE_URL}/ankieta`);
    return response.data;
  }

  // Pobierz ankietę po ID
  async getAnkieta(id: string): Promise<Ankieta> {
    const response = await axios.get(`${API_BASE_URL}/ankieta/${id}`);
    return response.data;
  }

  // Pobierz ankiety pacjenta
  async getAnkietyByPacjent(pacjentId: number): Promise<Ankieta[]> {
    const response = await axios.get(`${API_BASE_URL}/ankieta/pacjent/${pacjentId}`);
    return response.data;
  }

  // Pobierz ankietę wizyty
  async getAnkietaByWizyta(wizytaId: number): Promise<Ankieta | null> {
    try {
      const response = await axios.get(`${API_BASE_URL}/ankieta/wizyta/${wizytaId}`);
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      throw error;
    }
  }

  // Pobierz oceny lekarza
  async getOcenyLekarza(lekarzId: number): Promise<OcenyLekarza> {
    const response = await axios.get(`${API_BASE_URL}/ankieta/lekarz/${lekarzId}`);
    return response.data;
  }

  // Utwórz anonimową ankietę o wizycie
  async createAnkietaAnonimowa(data: CreateAnkietaAnonimowaDto): Promise<Ankieta> {
    const response = await axios.post(`${API_BASE_URL}/ankieta/anonimowa`, data);
    return response.data;
  }

  // Sprawdź czy wizyta została oceniona
  async czyWizytaOceniona(wizytaId: number): Promise<CzyOcenionaResponse> {
    const response = await axios.get(`${API_BASE_URL}/ankieta/czy-oceniona/${wizytaId}`);
    return response.data;
  }
}

export default new AnkietaService();
