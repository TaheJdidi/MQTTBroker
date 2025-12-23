export interface ConnectedClient {
  clientId: string;
  endpoint: string | null;
  connectedAt: Date;
  protocolVersion: string;
}
