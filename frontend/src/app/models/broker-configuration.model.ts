export interface BrokerConfiguration {
  port: number;
  maxPendingConnections: number;
  enableAuthentication: boolean;
  enableVerboseLogging: boolean;
  communicationTimeout: number;
}
