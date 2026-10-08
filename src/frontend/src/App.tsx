import { useState, type FormEvent } from 'react'
import { identityApi } from './modules/identity/api/identityApi'
import { useAuth } from './modules/identity/store'

export function App() {
  const { session, signIn, signOut } = useAuth()
  const [register, setRegister] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [message, setMessage] = useState('')
  const [busy, setBusy] = useState(false)

  async function submit(event: FormEvent) {
    event.preventDefault()
    setBusy(true)
    setMessage('')
    try {
      if (register) await identityApi.register({ email, password, displayName })
      signIn(await identityApi.login({ email, password }))
      setPassword('')
    } catch {
      setMessage(register ? 'Registration failed. Use a unique email and a password of at least 12 characters with uppercase, lowercase, a number and a symbol.' : 'Sign in failed. Check your credentials or try again later.')
    } finally { setBusy(false) }
  }

  return <main className="mx-auto mt-20 max-w-md rounded-2xl bg-white p-8 shadow-sm">
    <p className="text-sm font-semibold text-indigo-600">MODULAR PROJECT STARTER</p>
    <h1 className="my-4 text-3xl font-bold">{session ? `Welcome, ${session.user.displayName}` : register ? 'Create an account' : 'Sign in'}</h1>
    {session ? <div className="space-y-4">
      <p>{session.user.email}</p>
      <button className="rounded-lg bg-indigo-600 px-4 py-3 text-white" onClick={async () => {
        try { const user = await identityApi.me(); setMessage(`Authenticated profile: ${user.displayName}`) }
        catch { setMessage('Session expired. Please sign in again.'); signOut() }
      }}>Check protected profile</button>
      <button className="ml-4 underline" onClick={() => { signOut(); setMessage('') }}>Sign out</button>
    </div> : <form onSubmit={submit} className="space-y-4">
      {register && <label className="block">Display name<input required maxLength={200} autoComplete="name" value={displayName} onChange={e => setDisplayName(e.target.value)} /></label>}
      <label className="block">Email<input type="email" required maxLength={256} autoComplete="email" value={email} onChange={e => setEmail(e.target.value)} /></label>
      <label className="block">Password<input type="password" required minLength={register ? 12 : 1} maxLength={128} autoComplete={register ? 'new-password' : 'current-password'} value={password} onChange={e => setPassword(e.target.value)} /></label>
      <button disabled={busy} className="w-full rounded-lg bg-indigo-600 p-3 font-semibold text-white">{busy ? 'Please wait...' : register ? 'Create account' : 'Sign in'}</button>
      <button type="button" className="underline" onClick={() => { setRegister(!register); setMessage('') }}>{register ? 'Already have an account? Sign in' : 'Create an account'}</button>
    </form>}
    {message && <p role="status" className="mt-5 text-sm">{message}</p>}
  </main>
}
