export interface BrokerStatus {
  isRunning: boolean;
  startedAt: Date | null;
  connectedClients: number;
  port: number;
  uptime: string | null;
}
