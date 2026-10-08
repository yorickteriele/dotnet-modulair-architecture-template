import { IdentityApiClient } from './generated/api-client'
import { useAuth } from '../store'

export const identityApi = new IdentityApiClient('', {
  fetch: (url: RequestInfo, init?: RequestInit) => {
    const headers = new Headers(init?.headers)
    const session = useAuth.getState().session
    if (session && new Date(session.expiresAt) > new Date()) headers.set('Authorization', `Bearer ${session.accessToken}`)
    return fetch(url, { ...init, headers }).then(response => {
      if (response.status === 401) useAuth.getState().signOut()
      return response
    })
  },
})
