<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '../composables/useApi.js'
import { useAuth } from '../composables/useAuth.js'

const router = useRouter()
const api = useApi()
const auth = useAuth()

const email = ref('')
const password = ref('')
const error = ref('')
const submitting = ref(false)

async function handleSubmit() {
  error.value = ''
  submitting.value = true
  try {
    const data = await api.post('/api/auth/login', {
      email: email.value,
      password: password.value,
    })
    auth.login(data)
    router.push('/')
  } catch (err) {
    error.value = err.error || 'Invalid credentials.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="max-w-sm mx-auto">
    <h1 class="text-2xl font-bold text-center mb-6">Log in</h1>

    <div v-if="error" class="bg-red-50 text-red-700 p-3 rounded mb-4 text-sm">
      {{ error }}
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div>
        <input
          v-model="email"
          type="email"
          placeholder="Email"
          required
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
      <div>
        <input
          v-model="password"
          type="password"
          placeholder="Password"
          required
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
      <button
        type="submit"
        :disabled="submitting"
        class="w-full py-2 bg-blue-500 text-white font-semibold rounded-lg hover:bg-blue-600 disabled:opacity-50"
      >
        {{ submitting ? 'Logging in...' : 'Log in' }}
      </button>
    </form>

    <p class="text-center text-sm text-gray-500 mt-6">
      Don't have an account?
      <router-link to="/register" class="text-blue-500 font-medium">Sign up</router-link>
    </p>
  </div>
</template>
