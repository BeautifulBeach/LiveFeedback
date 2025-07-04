import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/lecture/:id',
      name: 'lecture',
      component: ()=> import('../views/LectureView.vue'),
      props: true,
    }
  ],
})

export default router
