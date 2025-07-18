import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr'
import type { InjectionKey } from 'vue'
import { messages, serverSideFunctions, storageKeys } from '@/static/consts.ts'
import type { RatingMessage } from '@/models/ratingMessage.ts'

export const signalRKey: InjectionKey<SignalRService> = Symbol('signalR')

export class SignalRService {
  private _hubConnection: HubConnection
  private _clientId: string
  private _lectureId: string

  constructor() {
    this._clientId = localStorage.getItem(storageKeys.clientId) ?? ''
    this._lectureId = localStorage.getItem(storageKeys.lectureId) ?? ''
    this._hubConnection = this.configureHubConnection(this._lectureId)
  }

  async startConnectionAsync(lectureId: string): Promise<void> {
    this._lectureId = lectureId
    this._hubConnection = this.configureHubConnection(lectureId)
    if (this._hubConnection.state == HubConnectionState.Disconnected) {
      await this._hubConnection.start()
    }
  }

  connectionState() {
    return this._hubConnection.state
  }

  async sendNewRating(rating: number) {
    const message: RatingMessage<number> = {
      clientId: this._clientId,
      lectureId: this._lectureId,
      rating: rating
    }
    await this._hubConnection.invoke(serverSideFunctions.ratingReport, message)
  }

  configureHubConnection(lectureId: string): HubConnection {
    const clientId: string = localStorage.getItem(storageKeys.clientId) ?? ''
    const hubConnection = new HubConnectionBuilder()
      .withUrl(`/slider-hub?group=default&clientId=${clientId}&lectureId=${lectureId}`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build()

    hubConnection.on(messages.persistClientId, id => {
      this._clientId = id
      localStorage.setItem(storageKeys.clientId, id)
    })

    hubConnection.on(messages.persistLectureId, id => {
      this._lectureId = id
      localStorage.setItem(storageKeys.lectureId, id)
    })

    return hubConnection
  }
}

export const signalRService = new SignalRService()