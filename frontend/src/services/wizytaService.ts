import axios from 'axios';
import { API_BASE_URL } from '../konfiguracja/api';

export interface Wizyta {
  idWizyty: number;
  data: string;
  status: string;
  idPacjenta: number;
  pacjentImie: string;
  pacjentNazwisko: string;
  idLekarza: number;
  lekarzImie: string;
  lekarzNazwisko: string;
  lekarzSpecjalizacja: string;
}

export interface RezerwujWizyteDto {
  idTerminu: number;
  idPacjenta: number;
}

class WizytaService {
  // Pobierz wszystkie wizyty
  async getWizyty(): Promise<Wizyta[]> {
    const response = await axios.get(`${API_BASE_URL}/wizyta`);
    return response.data;
  }

  // Pobierz wizytę po ID
  async getWizyta(id: number): Promise<Wizyta> {
    const response = await axios.get(`${API_BASE_URL}/wizyta/${id}`);
    return response.data;
  }

  // Pobierz wizyty pacjenta
  async getWizytyByPacjent(pacjentId: number): Promise<Wizyta[]> {
    const response = await axios.get(`${API_BASE_URL}/wizyta/pacjent/${pacjentId}`);
    return response.data;
  }

  // Pobierz wizyty lekarza
  async getWizytyByLekarz(lekarzId: number): Promise<Wizyta[]> {
    const response = await axios.get(`${API_BASE_URL}/wizyta/lekarz/${lekarzId}`);
    return response.data;
  }

  // Pobierz oczekujące wizyty lekarza
  async getOczekujaceWizyty(lekarzId: number): Promise<Wizyta[]> {
    const response = await axios.get(`${API_BASE_URL}/wizyta/oczekujace/lekarz/${lekarzId}`);
    return response.data;
  }

  // Rezerwuj wizytę (pacjent)
  async rezerwujWizyte(data: RezerwujWizyteDto): Promise<Wizyta> {
    const response = await axios.post(`${API_BASE_URL}/wizyta/rezerwuj`, data);
    return response.data;
  }

  // Akceptuj wizytę (lekarz)
  async akceptujWizyte(wizytaId: number): Promise<void> {
    await axios.patch(`${API_BASE_URL}/wizyta/${wizytaId}/akceptuj`);
  }

  // Odrzuć wizytę (lekarz)
  async odrzucWizyte(wizytaId: number): Promise<void> {
    await axios.patch(`${API_BASE_URL}/wizyta/${wizytaId}/odrzuc`);
  }

  // Zakończ wizytę (lekarz)
  async zakonczWizyte(wizytaId: number): Promise<void> {
    await axios.patch(`${API_BASE_URL}/wizyta/${wizytaId}/zakoncz`);
  }

  // Anuluj wizytę
  async anulujWizyte(wizytaId: number): Promise<void> {
    await axios.patch(`${API_BASE_URL}/wizyta/${wizytaId}/cancel`);
  }

  // Aktualizuj wizytę (admin)
  async updateWizyta(wizytaId: number, updateData: { status?: string; data?: string; idLekarza?: number }): Promise<void> {
    await axios.put(`${API_BASE_URL}/wizyta/${wizytaId}`, updateData);
  }

  // Usuń wizytę (admin)
  async deleteWizyta(wizytaId: number): Promise<void> {
    await axios.delete(`${API_BASE_URL}/wizyta/${wizytaId}`);
  }

  // Alias dla getWizyty() dla kompatybilności
  async getAllWizyty(): Promise<Wizyta[]> {
    return this.getWizyty();
  }
}

export default new WizytaService();
