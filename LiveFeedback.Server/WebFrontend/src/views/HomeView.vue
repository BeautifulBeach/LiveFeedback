<script setup lang="ts">
import { inject } from 'vue'
import { signalRKey, SignalRService } from '@/services/signalRService.ts'

const signalR = inject<SignalRService>(signalRKey)
</script>

<template>
  <main>
    Hallo Welt
    <RouterLink to="/about">Über uns</RouterLink>
    <div>Verbunden: {{ signalR?.connectionState() }}</div>
  </main>
</template>
