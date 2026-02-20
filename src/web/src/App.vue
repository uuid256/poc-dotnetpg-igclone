<script setup>
import NavBar from './components/NavBar.vue'
</script>

<template>
  <div class="min-h-screen bg-gray-50">
    <NavBar />
    <main class="max-w-lg mx-auto px-4 py-6 pt-20">
      <router-view />
    </main>
  </div>
</template>
