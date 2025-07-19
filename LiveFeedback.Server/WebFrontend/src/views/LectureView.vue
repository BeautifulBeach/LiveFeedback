<script setup lang="ts">
import { type Ref, ref, watch } from 'vue'
import { signalRService } from '@/services/signalRService.ts'

const currentRating: Ref<number> = ref(50)

watch(currentRating, async (newRating) => {
  await signalRService.sendNewRating(Math.round(newRating))
})
</script>

<template>
  <div class="slider-container">
    <input type="range" v-model.number="currentRating" class="slider" step="0.01" />
  </div>
</template>

<style lang="scss">
@mixin slider-track {
  height: 24px;
  border-radius: 1rem;
  background: linear-gradient(90deg, var(--lf-blue), var(--lf-turquoise), var(--lf-green), var(--lf-yellow), var(--lf-red));
  border: none;
}

@mixin slider-thumb {
  height: 48px;
  width: 48px;
  border-radius: 50%;
  background: white;
  box-shadow: 0 0 4px rgba(0, 0, 0, .2);
  appearance: none;
  margin-top: -12px;
}

.slider-container {
  width: 100vw;
  height: 100dvh;
  display: flex;
  justify-content: center;
  align-items: center;
}

.slider {
  width: 90%;
  max-width: 1200px;
  background: none;
  appearance: none;
  -webkit-appearance: none;
  -moz-appearance: none;

  &::-webkit-slider-runnable-track {
    @include slider-track;
  }

  &::-webkit-slider-thumb {
    @include slider-thumb;
  }

  &::-moz-range-track {
    @include slider-track;
  }

  &::-moz-range-thumb {
    @include slider-thumb;
  }
}
</style>

