import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import { signalRService } from '@/services/signalRService.ts'
import { storageKeys } from '@/static/consts.ts'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView
    },
    {
      path: '/lecture/:id',
      name: 'lecture',
      component: () => import('../views/LectureView.vue'),
      props: true
    }
  ]
})

router.beforeEach(async (to, from, next) => {
  if (to.name === 'lecture') {
    await signalRService.startConnectionAsync(to.params.id as string)
  } else {
    await signalRService.startConnectionAsync(localStorage.getItem(storageKeys.lectureId) ?? '')
  }
  next()
})

export default router
