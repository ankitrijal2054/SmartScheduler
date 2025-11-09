/**
 * SignalR Service
 * Handles real-time WebSocket connections using SignalR
 * Provides event subscription and message broadcasting
 */

export interface JobStatusUpdateEvent {
  jobId: string;
  newStatus: "Pending" | "Assigned" | "InProgress" | "Completed";
  updatedAt: string;
  contractorId?: string;
  estimatedArrivalTime?: string;
}

export interface SignalRConnectionOptions {
  url: string;
  reconnectInterval?: number;
  maxRetries?: number;
}

type EventListener = (
  data: JobStatusUpdateEvent | Record<string, unknown> | Error
) => void;
type Subscription = (event: JobStatusUpdateEvent) => void;

class SignalRService {
  private connected: boolean = false;
  private connecting: boolean = false;
  private subscriptions: Map<string, Set<Subscription>> = new Map();
  private eventListeners: Map<string, Set<EventListener>> = new Map();
  private reconnectAttempts: number = 0;
  private maxRetries: number = 5;
  private reconnectInterval: number = 5000;

  /**
   * Initialize connection (placeholder for actual SignalR implementation)
   * In production, this would establish a real SignalR connection
   */
  async connect(options: SignalRConnectionOptions): Promise<void> {
    if (this.connected || this.connecting) {
      return;
    }

    this.connecting = true;
    this.maxRetries = options.maxRetries || 5;
    this.reconnectInterval = options.reconnectInterval || 5000;

    try {
      // Simulate connection success
      // In production: establish real SignalR HubConnection
      console.log(`[SignalR] Connecting to ${options.url}`);

      this.connected = true;
      this.connecting = false;
      this.reconnectAttempts = 0;
      this.emitEvent("connected", {});
    } catch (error) {
      this.connecting = false;
      this.emitEvent("error", error);
      this.attemptReconnect(options);
    }
  }

  /**
   * Disconnect from SignalR hub
   */
  async disconnect(): Promise<void> {
    if (!this.connected) {
      return;
    }

    try {
      console.log("[SignalR] Disconnecting from hub");
      this.connected = false;
      this.subscriptions.clear();
      this.emitEvent("disconnected", {});
    } catch (error) {
      console.error("[SignalR] Disconnect error:", error);
    }
  }

  /**
   * Subscribe to job status updates
   */
  subscribe(eventName: string, callback: Subscription): () => void {
    if (!this.subscriptions.has(eventName)) {
      this.subscriptions.set(eventName, new Set());
    }

    this.subscriptions.get(eventName)!.add(callback);

    // Return unsubscribe function
    return () => {
      const subscribers = this.subscriptions.get(eventName);
      if (subscribers) {
        subscribers.delete(callback);
      }
    };
  }

  /**
   * Emit job status update event (for testing/mocking)
   */
  emitJobStatusUpdate(event: JobStatusUpdateEvent): void {
    const callbacks = this.subscriptions.get("JobStatusUpdated");
    if (callbacks) {
      callbacks.forEach((callback) => callback(event));
    }
    this.emitEvent("JobStatusUpdated", event);
  }

  /**
   * Emit custom event
   */
  private emitEvent(
    eventName: string,
    data: JobStatusUpdateEvent | Record<string, unknown>
  ): void {
    const listeners = this.eventListeners.get(eventName);
    if (listeners) {
      listeners.forEach((listener) => listener(data));
    }
  }

  /**
   * Listen for event
   */
  on(eventName: string, listener: EventListener): void {
    if (!this.eventListeners.has(eventName)) {
      this.eventListeners.set(eventName, new Set());
    }
    this.eventListeners.get(eventName)!.add(listener);
  }

  /**
   * Unlisten for event
   */
  off(eventName: string, listener: EventListener): void {
    const listeners = this.eventListeners.get(eventName);
    if (listeners) {
      listeners.delete(listener);
    }
  }

  /**
   * Check if connected
   */
  isConnected(): boolean {
    return this.connected;
  }

  /**
   * Attempt to reconnect
   */
  private attemptReconnect(options: SignalRConnectionOptions): void {
    if (this.reconnectAttempts < this.maxRetries) {
      this.reconnectAttempts++;
      console.log(
        `[SignalR] Reconnecting (attempt ${this.reconnectAttempts}/${this.maxRetries})`
      );

      setTimeout(() => {
        this.connect(options);
      }, this.reconnectInterval);
    } else {
      console.error("[SignalR] Max reconnection attempts reached. Giving up.");
      this.emitEvent("maxRetriesReached", {});
    }
  }
}

export const signalRService = new SignalRService();
