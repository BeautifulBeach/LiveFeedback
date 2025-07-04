import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import { signalRKey, signalRService } from '@/services/signalRService.ts'

const app = createApp(App)

app.use(router)
app.provide(signalRKey, signalRService)

app.mount('#app')
