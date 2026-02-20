<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useApi } from '../composables/useApi.js'

const router = useRouter()
const api = useApi()

const caption = ref('')
const imageFile = ref(null)
const preview = ref(null)
const error = ref('')
const submitting = ref(false)

function onFileSelect(event) {
  const file = event.target.files[0]
  if (!file) return
  imageFile.value = file
  preview.value = URL.createObjectURL(file)
}

async function handleSubmit() {
  if (!imageFile.value) {
    error.value = 'Please select an image.'
    return
  }
  submitting.value = true
  error.value = ''
  try {
    const formData = new FormData()
    formData.append('image', imageFile.value)
    if (caption.value.trim()) {
      formData.append('caption', caption.value)
    }
    const post = await api.post('/api/posts', formData)
    router.push(`/posts/${post.id}`)
  } catch (err) {
    error.value = err.error || 'Upload failed.'
  } finally {
    submitting.value = false
  }
}
</script>

<template>
  <div class="max-w-sm mx-auto">
    <h1 class="text-2xl font-bold text-center mb-6">New Post</h1>

    <div v-if="error" class="bg-red-50 text-red-700 p-3 rounded mb-4 text-sm">
      {{ error }}
    </div>

    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div>
        <label
          class="block w-full aspect-square rounded-lg border-2 border-dashed border-gray-300 cursor-pointer hover:border-blue-400 overflow-hidden bg-gray-50"
        >
          <img
            v-if="preview"
            :src="preview"
            class="w-full h-full object-cover"
          />
          <div v-else class="flex flex-col items-center justify-center h-full text-gray-400">
            <span class="text-4xl mb-2">+</span>
            <span class="text-sm">Select a photo</span>
          </div>
          <input
            type="file"
            accept=".jpg,.jpeg,.png,.gif,.webp"
            class="hidden"
            @change="onFileSelect"
          />
        </label>
      </div>

      <div>
        <textarea
          v-model="caption"
          placeholder="Write a caption..."
          rows="3"
          class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
        ></textarea>
      </div>

      <button
        type="submit"
        :disabled="submitting || !imageFile"
        class="w-full py-2 bg-blue-500 text-white font-semibold rounded-lg hover:bg-blue-600 disabled:opacity-50"
      >
        {{ submitting ? 'Posting...' : 'Share' }}
      </button>
    </form>
  </div>
</template>
