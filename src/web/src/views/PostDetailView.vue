<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useApi } from '../composables/useApi.js'
import { useAuth } from '../composables/useAuth.js'
import CommentList from '../components/CommentList.vue'

const route = useRoute()
const router = useRouter()
const api = useApi()
const auth = useAuth()
const loggedIn = computed(() => auth.isLoggedIn())

const post = ref(null)
const comments = ref([])
const newComment = ref('')
const liked = ref(false)
const likeCount = ref(0)
const loading = ref(true)
const commentSubmitting = ref(false)

async function loadPost() {
  post.value = await api.get(`/api/posts/${route.params.id}`)
  likeCount.value = post.value.likeCount
}

async function loadComments() {
  comments.value = await api.get(`/api/posts/${route.params.id}/comments`)
}

async function addComment() {
  if (!newComment.value.trim()) return
  commentSubmitting.value = true
  try {
    const comment = await api.post(
      `/api/posts/${route.params.id}/comments`,
      { text: newComment.value },
    )
    comments.value.unshift(comment)
    newComment.value = ''
    post.value.commentCount++
  } finally {
    commentSubmitting.value = false
  }
}

async function toggleLike() {
  try {
    if (liked.value) {
      await api.del(`/api/posts/${route.params.id}/likes`)
      liked.value = false
      likeCount.value--
    } else {
      await api.post(`/api/posts/${route.params.id}/likes`)
      liked.value = true
      likeCount.value++
    }
  } catch (err) {
    if (err.status === 409) {
      liked.value = true
    }
  }
}

function timeAgo(dateString) {
  const seconds = Math.floor((Date.now() - new Date(dateString)) / 1000)
  const intervals = [
    [31536000, 'year'],
    [2592000, 'month'],
    [86400, 'day'],
    [3600, 'hour'],
    [60, 'minute'],
    [1, 'second'],
  ]
  for (const [secs, label] of intervals) {
    const count = Math.floor(seconds / secs)
    if (count >= 1) return `${count} ${label}${count > 1 ? 's' : ''} ago`
  }
  return 'just now'
}

onMounted(async () => {
  try {
    await Promise.all([loadPost(), loadComments()])
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div>
    <button @click="router.push('/')" class="text-sm text-blue-500 mb-4 inline-block">
      &larr; Back to feed
    </button>

    <div v-if="loading" class="text-center text-gray-500 py-8">Loading...</div>

    <div v-else-if="post" class="bg-white rounded-lg shadow-sm overflow-hidden">
      <div class="px-3 py-2 flex items-center">
        <span class="font-semibold text-sm">@{{ post.username }}</span>
      </div>

      <div class="aspect-square w-full bg-gray-100">
        <img
          :src="post.imageUrl"
          :alt="post.caption || 'Post image'"
          class="w-full h-full object-cover"
        />
      </div>

      <div class="px-3 py-2">
        <div class="flex items-center gap-4 mb-2">
          <button
            v-if="loggedIn"
            @click="toggleLike"
            class="text-2xl leading-none"
            :title="liked ? 'Unlike' : 'Like'"
          >
            <span v-if="liked" class="text-red-500">&#9829;</span>
            <span v-else class="text-gray-400 hover:text-red-400">&#9825;</span>
          </button>
          <span class="text-sm text-gray-600">
            {{ likeCount }} {{ likeCount === 1 ? 'like' : 'likes' }}
          </span>
        </div>

        <p v-if="post.caption" class="text-sm mb-1">
          <span class="font-semibold">{{ post.username }}</span>
          {{ post.caption }}
        </p>
        <p class="text-xs text-gray-400">{{ timeAgo(post.createdAt) }}</p>
      </div>

      <div class="border-t border-gray-100 px-3 py-3">
        <h3 class="text-sm font-semibold mb-2">
          Comments ({{ post.commentCount }})
        </h3>

        <form
          v-if="loggedIn"
          @submit.prevent="addComment"
          class="flex gap-2 mb-3"
        >
          <input
            v-model="newComment"
            type="text"
            placeholder="Add a comment..."
            class="flex-1 px-3 py-1.5 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <button
            type="submit"
            :disabled="!newComment.trim() || commentSubmitting"
            class="px-3 py-1.5 text-sm text-blue-500 font-semibold disabled:opacity-50"
          >
            Post
          </button>
        </form>

        <CommentList :comments="comments" />
      </div>
    </div>
  </div>
</template>
