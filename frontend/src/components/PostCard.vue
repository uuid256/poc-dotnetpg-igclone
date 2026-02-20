<script setup>
import { useRouter } from 'vue-router'

const props = defineProps({
  post: { type: Object, required: true },
})

const router = useRouter()

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
</script>

<template>
  <div
    class="bg-white rounded-lg shadow-sm mb-4 overflow-hidden cursor-pointer"
    @click="router.push(`/posts/${post.id}`)"
  >
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
      <div class="flex gap-4 text-sm text-gray-600 mb-1">
        <span>{{ post.likeCount }} {{ post.likeCount === 1 ? 'like' : 'likes' }}</span>
        <span>{{ post.commentCount }} {{ post.commentCount === 1 ? 'comment' : 'comments' }}</span>
      </div>
      <p v-if="post.caption" class="text-sm">
        <span class="font-semibold">{{ post.username }}</span>
        {{ post.caption }}
      </p>
      <p class="text-xs text-gray-400 mt-1">{{ timeAgo(post.createdAt) }}</p>
    </div>
  </div>
</template>
