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
    this._hubConnection = new HubConnectionBuilder()
      .withUrl(`/slider-hub?group=default&clientId=${localStorage.getItem(storageKeys.clientId)}&lectureId=${localStorage.getItem(storageKeys.lectureId)}`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build()

    this._hubConnection.on(messages.persistClientId, id => {
      localStorage.setItem(storageKeys.clientId, id)
    })

    this._hubConnection.on(messages.persistLectureId, id => {
      localStorage.setItem(storageKeys.lectureId, id)
    })
  }

  async startConnectionAsync(): Promise<void> {
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
