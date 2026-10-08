import { create } from 'zustand'
import type { AuthResponse } from './api/generated/api-client'

type AuthState = {
  session: AuthResponse | null
  signIn: (session: AuthResponse) => void
  signOut: () => void
}

// Keep bearer tokens in memory. Reloading the page requires signing in again.
export const useAuth = create<AuthState>(set => ({
  session: null,
  signIn: session => set({ session }),
  signOut: () => set({ session: null }),
}))
