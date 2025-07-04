import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel
} from '@microsoft/signalr'
import type { InjectionKey } from 'vue'
import { messages, storageKeys } from '@/static/consts.ts'

export const signalRKey: InjectionKey<SignalRService> = Symbol('signalR')

export class SignalRService {
  private _hubConnection: HubConnection

  constructor() {
    this._hubConnection = configureHubConnection(localStorage.getItem(storageKeys.lectureId) ?? '')
  }

  async startConnectionAsync(lectureId: string): Promise<void> {
    this._hubConnection = configureHubConnection(lectureId)
    if (this._hubConnection.state == HubConnectionState.Disconnected) {
      await this._hubConnection.start()
    }
  }

  on<T>(eventName: string, handler: (value: T) => void) {
    this._hubConnection.on(eventName, handler)
  }

  async invoke<T>(method: string, params?: T) {
    await this._hubConnection.invoke(method, params)
  }

  connectionState() {
    return this._hubConnection.state
  }

}

export const signalRService = new SignalRService()

function configureHubConnection(lectureId: string): HubConnection {
  const hubConnection = new HubConnectionBuilder()
    .withUrl(`/slider-hub?group=default&clientId=${localStorage.getItem(storageKeys.clientId)}&lectureId=${lectureId}`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build()

  hubConnection.on(messages.persistClientId, id => {
    localStorage.setItem(storageKeys.clientId, id)
  })

  hubConnection.on(messages.persistLectureId, id => {
    localStorage.setItem(storageKeys.lectureId, id)
  })

  return hubConnection
}