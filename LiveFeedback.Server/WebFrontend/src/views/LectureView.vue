<script setup lang="ts">
const props = defineProps<{ id: string }>()
console.log("lecture ID: ", props.id)
</script>

<template>
  <div>Lecture view for lecture {{ id }}</div>
</template>

<style scoped>

</style>
