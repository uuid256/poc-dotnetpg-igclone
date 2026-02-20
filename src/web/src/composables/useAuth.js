import { reactive, readonly } from 'vue'

const state = reactive({
  token: localStorage.getItem('token'),
  userId: localStorage.getItem('userId'),
  username: localStorage.getItem('username'),
})

export function useAuth() {
  const isLoggedIn = () => !!state.token

  function login(authResponse) {
    state.token = authResponse.token
    state.userId = authResponse.userId
    state.username = authResponse.username
    localStorage.setItem('token', authResponse.token)
    localStorage.setItem('userId', authResponse.userId)
    localStorage.setItem('username', authResponse.username)
  }

  function logout() {
    state.token = null
    state.userId = null
    state.username = null
    localStorage.removeItem('token')
    localStorage.removeItem('userId')
    localStorage.removeItem('username')
  }

  return {
    state: readonly(state),
    isLoggedIn,
    login,
    logout,
  }
}
