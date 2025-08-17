import { useState } from 'react'
import { login } from '/src/lib/adminApi'

export default function Login(){
  const [username, setU] = useState('admin')
  const [password, setP] = useState('')
  const [err, setErr] = useState('')

  async function submit(e){
    e.preventDefault()
    setErr('')
    try{ await login(username, password); location.replace('/admin') } catch { setErr('Invalid credentials') }
  }

  return (
    <div className="min-h-screen grid place-items-center p-6">
      <form onSubmit={submit} className="w-full max-w-sm space-y-3 rounded-xl border p-6 dark:border-gray-800">
        <h1 className="font-bold text-xl">Sign in</h1>
        <input className="w-full border rounded p-3" value={username} onChange={e=>setU(e.target.value)} placeholder="Username"/>
        <input className="w-full border rounded p-3" type="password" value={password} onChange={e=>setP(e.target.value)} placeholder="Password"/>
        <button className="w-full rounded bg-blue-600 text-white py-2.5">Sign in</button>
        {err && <p className="text-red-600 text-sm">{err}</p>}
      </form>
    </div>
  )
}