import { FileDown } from 'lucide-react'
import ThemeToggle from './ThemeToggle'

// GitHub and LinkedIn deliberately are not here. Eight nav items plus two social
// links plus Resume plus the theme toggle put twelve targets in one bar, which
// wraps badly and makes Resume - the one action worth taking on a portfolio -
// compete with everything else. Both links remain in Contact and the footer.
export default function Header({ profile }) {
  const nav = ['About','Projects','Skills','Experience','Education','Certifications','Blog','Contact']
  return (
    <header className="sticky top-0 z-50 bg-white/70 backdrop-blur-md border-b dark:bg-gray-950/60 dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between">
        <a href="#home" className="font-extrabold text-lg tracking-tight">{profile?.name || 'Albert Campos'}</a>
        <nav className="hidden md:flex items-center gap-5 text-sm">
          {nav.map(s => (
            <a key={s} href={`#${s.toLowerCase()}`} className="hover:text-blue-600 transition-colors">{s}</a>
          ))}
          <div className="h-5 w-px bg-gray-200 dark:bg-gray-800" />
          <a href="/resume.pdf" className="relative inline-flex items-center gap-2 rounded-xl border px-3 py-1.5 hover:shadow dark:border-gray-800">
            <FileDown className="h-4 w-4"/>Resume
          </a>
          <ThemeToggle />
        </nav>
      </div>
    </header>
  )
}