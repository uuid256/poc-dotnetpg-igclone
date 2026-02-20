<script setup>
import { ref, onMounted } from 'vue'
import { useApi } from '../composables/useApi.js'
import PostCard from '../components/PostCard.vue'

const api = useApi()
const posts = ref([])
const page = ref(1)
const hasMore = ref(false)
const loading = ref(false)

async function loadPosts() {
  loading.value = true
  try {
    const data = await api.get(`/api/posts/feed?page=${page.value}&pageSize=10`)
    posts.value.push(...data.posts)
    hasMore.value = data.hasMore
    page.value++
  } finally {
    loading.value = false
  }
}

onMounted(loadPosts)
</script>

<template>
  <div>
    <div v-if="loading && posts.length === 0" class="text-center text-gray-500 py-8">
      Loading...
    </div>

    <div v-else-if="posts.length === 0" class="text-center text-gray-500 py-8">
      No posts yet.
    </div>

    <template v-else>
      <PostCard v-for="post in posts" :key="post.id" :post="post" />

      <div v-if="hasMore" class="text-center py-4">
        <button
          @click="loadPosts"
          :disabled="loading"
          class="px-6 py-2 text-sm text-blue-500 font-medium hover:text-blue-600 disabled:opacity-50"
        >
          {{ loading ? 'Loading...' : 'Load more' }}
        </button>
      </div>
    </template>
  </div>
</template>
