import { ref, type Ref } from 'vue'
import { Lecture } from '@/models/lecture.ts'

export const lectures: Ref<Lecture[]> = ref([])