import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BrokerConfiguration, BrokerStatus, ConnectedClient } from '../models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BrokerService {
  private readonly apiUrl = `${environment.apiUrl}/api/broker`;

  constructor(private http: HttpClient) {}

  getStatus(): Observable<BrokerStatus> {
    return this.http.get<BrokerStatus>(`${this.apiUrl}/status`);
  }

  getConfiguration(): Observable<BrokerConfiguration> {
    return this.http.get<BrokerConfiguration>(`${this.apiUrl}/configuration`);
  }

  updateConfiguration(configuration: BrokerConfiguration): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/configuration`, configuration);
  }

  getClients(): Observable<ConnectedClient[]> {
    return this.http.get<ConnectedClient[]>(`${this.apiUrl}/clients`);
  }

  start(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/start`, {});
  }

  stop(): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/stop`, {});
  }

  disconnectClient(clientId: string): Observable<{ message: string }> {
    return this.http.post<{ message: string }>(`${this.apiUrl}/clients/${encodeURIComponent(clientId)}/disconnect`, {});
  }
}
