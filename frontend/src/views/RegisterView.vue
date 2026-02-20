<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '../composables/useApi.js'
import { useAuth } from '../composables/useAuth.js'

const router = useRouter()
const api = useApi()
const auth = useAuth()

const username = ref('')
const email = ref('')
const password = ref('')
const displayName = ref('')
const error = ref('')
const submitting = ref(false)

async function handleSubmit() {
  error.value = ''
  submitting.value = true
  try {
    const data = await api.post('/api/auth/register', {
      username: username.value,
      email: email.value,
      password: password.value,
      displayName: displayName.value || undefined,
    })
    auth.login(data)
    router.push('/')
  } catch (err) {
    error.value = err.error || 'Registration failed.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="max-w-sm mx-auto">
    <h1 class="text-2xl font-bold text-center mb-6">Sign up</h1>

    <div v-if="error" class="bg-red-50 text-red-700 p-3 rounded mb-4 text-sm">
      {{ error }}
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div>
        <input
          v-model="username"
          type="text"
          placeholder="Username"
          required
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
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
          placeholder="Password (min 6 characters)"
          required
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
      <div>
        <input
          v-model="displayName"
          type="text"
          placeholder="Display name (optional)"
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
        />
      </div>
      <button
        type="submit"
        :disabled="submitting"
        class="w-full py-2 bg-blue-500 text-white font-semibold rounded-lg hover:bg-blue-600 disabled:opacity-50"
      >
        {{ submitting ? 'Signing up...' : 'Sign up' }}
      </button>
    </form>

    <p class="text-center text-sm text-gray-500 mt-6">
      Already have an account?
      <router-link to="/login" class="text-blue-500 font-medium">Log in</router-link>
    </p>
  </div>
</template>
