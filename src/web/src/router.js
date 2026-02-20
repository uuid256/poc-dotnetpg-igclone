import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  { path: '/', component: () => import('./views/FeedView.vue') },
  { path: '/login', component: () => import('./views/LoginView.vue') },
  { path: '/register', component: () => import('./views/RegisterView.vue') },
  { path: '/posts/create', component: () => import('./views/CreatePostView.vue') },
  { path: '/posts/:id', component: () => import('./views/PostDetailView.vue') },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

router.beforeEach((to) => {
  const protectedRoutes = ['/posts/create']
  if (protectedRoutes.includes(to.path) && !localStorage.getItem('token')) {
    return '/login'
  }
})

export default router
