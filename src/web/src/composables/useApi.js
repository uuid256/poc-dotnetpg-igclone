export function useApi() {
  const getToken = () => localStorage.getItem('token')

  async function request(url, options = {}) {
    const token = getToken()
    const headers = { ...options.headers }

    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }

    if (options.body && !(options.body instanceof FormData)) {
      headers['Content-Type'] = 'application/json'
    }

    const response = await fetch(url, { ...options, headers })

    if (!response.ok) {
      const error = await response.json().catch(() => ({ error: response.statusText }))
      throw { status: response.status, ...error }
    }

    const text = await response.text()
    return text ? JSON.parse(text) : null
  }

  return {
    get: (url) => request(url),
    post: (url, body) => request(url, {
      method: 'POST',
      body: body instanceof FormData ? body : JSON.stringify(body),
    }),
    del: (url) => request(url, { method: 'DELETE' }),
  }
}
