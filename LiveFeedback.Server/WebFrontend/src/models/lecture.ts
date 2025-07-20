export class Lecture {
  id: string;
  name: string | null;
  room: string | null;

  public constructor(id: string, name: string | null, room: string | null) {
    this.id = id;
    this.name = name ?? null;
    this.room = room ?? null;
  }
}