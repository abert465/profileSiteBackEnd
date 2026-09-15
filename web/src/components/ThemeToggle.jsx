import { useEffect, useState } from 'react'
import { Sun, Moon } from 'lucide-react'

export default function ThemeToggle(){
  const [dark, setDark] = useState(() => {
    if (typeof document === 'undefined') return false
    return document.documentElement.classList.contains('dark')
  })

  useEffect(() => {
    document.documentElement.classList.toggle('dark', dark)
    // localStorage throws in private browsing and with cookies blocked; the
    // toggle still works for the session, it just will not be remembered.
    try { localStorage.setItem('theme', dark ? 'dark' : 'light') } catch { /* not persistable */ }
  }, [dark])

  return (
    <button
      type="button"
      aria-label={dark ? 'Switch to light mode' : 'Switch to dark mode'}
      onClick={() => setDark(v => !v)}
      className="inline-flex items-center gap-2 rounded-xl border px-3 py-1.5 hover:shadow dark:border-gray-800"
    >
      {dark ? <Sun className="h-4 w-4"/> : <Moon className="h-4 w-4"/>}
      <span className="hidden sm:inline">{dark ? 'Light' : 'Dark'}</span>
    </button>
  )
}
