export type RatingMessage<T> = {
  clientId: string
  lectureId: string
  rating: T
}