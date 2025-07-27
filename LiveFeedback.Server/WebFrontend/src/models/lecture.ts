export class Lecture {
  id: string
  name: string
  room: string

  public constructor(id: string, name: string, room: string) {
    this.id = id
    this.name = name ?? ''
    this.room = room ?? ''
  }
}