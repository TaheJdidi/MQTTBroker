import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BrokerService } from '../../services/broker.service';
import { BrokerConfiguration } from '../../models';

@Component({
  selector: 'app-configuration',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './configuration.component.html',
  styleUrls: ['./configuration.component.scss']
})
export class ConfigurationComponent implements OnInit {
  configuration = signal<BrokerConfiguration | null>(null);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  loading = signal<boolean>(false);

  // Form model
  formData: BrokerConfiguration = {
    port: 1883,
    maxPendingConnections: 100,
    enableAuthentication: false,
    enableVerboseLogging: false,
    communicationTimeout: 30
  };

  constructor(private brokerService: BrokerService) {}

  ngOnInit(): void {
    this.loadConfiguration();
  }

  loadConfiguration(): void {
    this.loading.set(true);
    this.error.set(null);

    this.brokerService.getConfiguration().subscribe({
      next: (config) => {
        this.configuration.set(config);
        this.formData = { ...config };
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load configuration');
        this.loading.set(false);
      }
    });
  }

  saveConfiguration(): void {
    this.loading.set(true);
    this.error.set(null);
    this.success.set(null);

    // Validate port
    if (this.formData.port < 1 || this.formData.port > 65535) {
      this.error.set('Port must be between 1 and 65535');
      this.loading.set(false);
      return;
    }

    this.brokerService.updateConfiguration(this.formData).subscribe({
      next: () => {
        this.success.set('Configuration saved successfully. Restart the broker to apply changes.');
        this.loading.set(false);
        this.loadConfiguration();
      },
      error: (err) => {
        this.error.set('Failed to save configuration');
        this.loading.set(false);
      }
    });
  }

  resetForm(): void {
    const config = this.configuration();
    if (config) {
      this.formData = { ...config };
    }
    this.error.set(null);
    this.success.set(null);
  }
}
