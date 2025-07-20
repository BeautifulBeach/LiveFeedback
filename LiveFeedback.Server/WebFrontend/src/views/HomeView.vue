<script setup lang="ts">
import { watch } from 'vue'
import router from '@/router'
import { lectures } from '@/services/state.ts'

watch(lectures, (newLectures) => {
  if (newLectures.length === 1) {
    router.push(`/lecture/${newLectures[0].id}`)
  }
})
</script>

<template>
  <main>
    <div class="center-frame">
      <h2 v-if="lectures.length === 0">Aktuell können keine laufenden Veranstaltungen gefunden werden</h2>
      <div v-else>
        <h1>Wähle die passende Veranstaltung:</h1>
        <div class="lectures-list">
          <RouterLink :to="`/lecture/${lecture.id}`" v-for="lecture in lectures" :key="lecture.id"
                      class="lecture">
            <div>{{ lecture.name ?? 'Unbenannt' }}</div>
            <div>Raum: {{ lecture.room ?? 'Keine Angabe' }}</div>
            <div v-if="lecture.room === null && lecture.name === null">Veranstaltungs-ID: {{ lecture.id }}</div>
          </RouterLink>
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped lang="scss">
main {
  display: flex;
  width: 100vw;
  height: 100dvh;
  justify-content: center;

  .center-frame {
    max-width: 1200px;
    display: flex;
    flex-direction: column;

    h1, h2 {
      text-align: center;
      padding: 2rem;
    }

    .lectures-list {
      display: flex;
      flex-wrap: wrap;
      gap: 1rem;
      overflow-y: scroll;
      padding: 3rem 1rem;

      .lecture {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 0.5rem;
        width: fit-content;
        flex-grow: 1;

        background-color: var(--bg-2);
        border-radius: 0.5rem;
        padding: 1rem;
        color: var(--text-color);
        text-decoration: none;
      }
    }
  }
}
</style>
