<script setup lang="ts">
import { type Ref, ref, watch } from 'vue'
import { signalRService } from '@/services/signalRService.ts'

const currentRating: Ref<number> = ref(50)

watch(currentRating, async (newRating) => {
  await signalRService.sendNewRating(newRating)
})
</script>

<template>
  <input type="range" v-model.number="currentRating" />
</template>

<style scoped>

</style>
