import { useEffect, useState } from 'react'

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

  const label = dark ? 'Switch to light mode' : 'Switch to dark mode'

  return (
    <button
      type="button"
      aria-label={label}
      title={label}
      onClick={() => setDark(v => !v)}
      className="inline-flex h-[34px] w-[34px] shrink-0 cursor-pointer items-center justify-center rounded-xs border border-ink text-ink transition-colors hover:border-accent hover:text-accent"
    >
      {/* Half-filled disc: the filled side swaps over when the theme does. */}
      <span
        aria-hidden="true"
        className={`h-3.5 w-3.5 rounded-full border-[1.5px] border-current bg-[linear-gradient(90deg,currentColor_50%,transparent_50%)] transition-transform duration-[350ms] ease-in-out ${dark ? 'rotate-180' : ''}`}
      />
    </button>
  )
}
