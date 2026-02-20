<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuth } from '../composables/useAuth.js'

const router = useRouter()
const auth = useAuth()
const loggedIn = computed(() => auth.isLoggedIn())

function handleLogout() {
  auth.logout()
  router.push('/')
}
</script>

<template>
  <nav class="fixed top-0 left-0 right-0 bg-white border-b border-gray-200 z-50">
    <div class="max-w-lg mx-auto px-4 h-14 flex items-center justify-between">
      <router-link to="/" class="text-xl font-bold">InstaClone</router-link>

      <div class="flex items-center gap-3">
        <template v-if="loggedIn">
          <router-link
            to="/posts/create"
            class="w-8 h-8 flex items-center justify-center rounded-full bg-blue-500 text-white text-lg leading-none hover:bg-blue-600"
          >+</router-link>
          <span class="text-sm font-medium">{{ auth.state.username }}</span>
          <button
            @click="handleLogout"
            class="text-sm text-gray-500 hover:text-gray-700"
          >Logout</button>
        </template>
        <template v-else>
          <router-link to="/login" class="text-sm text-blue-500 font-medium hover:text-blue-600">Log in</router-link>
        </template>
      </div>
    </div>
  </nav>
</template>
