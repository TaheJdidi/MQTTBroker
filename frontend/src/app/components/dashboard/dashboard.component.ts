import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BrokerService } from '../../services/broker.service';
import { BrokerStatus, ConnectedClient } from '../../models';
import { interval, Subject } from 'rxjs';
import { takeUntil, switchMap, startWith } from 'rxjs/operators';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit, OnDestroy {
  status = signal<BrokerStatus | null>(null);
  clients = signal<ConnectedClient[]>([]);
  error = signal<string | null>(null);
  loading = signal<boolean>(false);
  
  private destroy$ = new Subject<void>();

  constructor(private brokerService: BrokerService) {}

  ngOnInit(): void {
    this.loadData();
    
    // Auto-refresh every 5 seconds
    interval(5000)
      .pipe(
        takeUntil(this.destroy$),
        startWith(0),
        switchMap(() => this.brokerService.getStatus())
      )
      .subscribe({
        next: (status) => this.status.set(status),
        error: (err) => this.error.set('Failed to fetch broker status')
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadData(): void {
    this.loading.set(true);
    this.error.set(null);

    this.brokerService.getStatus().subscribe({
      next: (status) => {
        this.status.set(status);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to fetch broker status');
        this.loading.set(false);
      }
    });

    this.brokerService.getClients().subscribe({
      next: (clients) => this.clients.set(clients),
      error: () => {}
    });
  }

  startBroker(): void {
    this.loading.set(true);
    this.brokerService.start().subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        this.error.set('Failed to start broker');
        this.loading.set(false);
      }
    });
  }

  stopBroker(): void {
    this.loading.set(true);
    this.brokerService.stop().subscribe({
      next: () => {
        this.loadData();
      },
      error: (err) => {
        this.error.set('Failed to stop broker');
        this.loading.set(false);
      }
    });
  }

  disconnectClient(clientId: string): void {
    this.brokerService.disconnectClient(clientId).subscribe({
      next: () => this.loadData(),
      error: () => this.error.set(`Failed to disconnect client ${clientId}`)
    });
  }

  formatUptime(uptime: string | null): string {
    if (!uptime) return 'N/A';
    // Parse .NET TimeSpan format (e.g., "01:23:45.6789")
    const parts = uptime.split(':');
    if (parts.length >= 3) {
      const hours = parseInt(parts[0], 10);
      const minutes = parseInt(parts[1], 10);
      const seconds = parseFloat(parts[2]);
      return `${hours}h ${minutes}m ${Math.floor(seconds)}s`;
    }
    return uptime;
  }
}
