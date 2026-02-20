<script setup>
defineProps({
  comments: { type: Array, required: true },
})

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
  <div>
    <div
      v-for="comment in comments"
      :key="comment.id"
      class="py-2 border-t border-gray-100"
    >
      <p class="text-sm">
        <span class="font-semibold">{{ comment.username }}</span>
        {{ comment.text }}
      </p>
      <p class="text-xs text-gray-400 mt-0.5">{{ timeAgo(comment.createdAt) }}</p>
    </div>
    <p v-if="comments.length === 0" class="text-sm text-gray-400 py-2">
      No comments yet.
    </p>
  </div>
</template>
