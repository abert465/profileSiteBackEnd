import { Github, Linkedin, FileDown } from 'lucide-react'
import ThemeToggle from './ThemeToggle'

export default function Header({ profile }) {
  const nav = ['About','Projects','Skills','Experience','Education','Certifications','Blog','Contact']
  const gh = profile?.links?.find(l => l.label?.toLowerCase().includes('github'))?.url || 'https://github.com/abert465'
  const li = profile?.links?.find(l => l.label?.toLowerCase().includes('linked'))?.url || 'https://www.linkedin.com/in/albert-campos/'
  return (
    <header className="sticky top-0 z-50 bg-white/70 backdrop-blur-md border-b dark:bg-gray-950/60 dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4 py-3 flex items-center justify-between">
        <a href="#home" className="font-extrabold text-lg tracking-tight">{profile?.name || 'Albert Campos'}</a>
        <nav className="hidden md:flex items-center gap-5 text-sm">
          {nav.map(s => (
            <a key={s} href={`#${s.toLowerCase()}`} className="hover:text-blue-600 transition-colors">{s}</a>
          ))}
          <div className="h-5 w-px bg-gray-200 dark:bg-gray-800" />
          <a href={gh} target="_blank" className="hover:text-blue-600 flex items-center gap-2"><Github className="h-4 w-4"/>GitHub</a>
          <a href={li} target="_blank" className="hover:text-blue-600 flex items-center gap-2"><Linkedin className="h-4 w-4"/>LinkedIn</a>
          <a href="/resume.pdf" className="relative inline-flex items-center gap-2 rounded-xl border px-3 py-1.5 hover:shadow dark:border-gray-800">
            <FileDown className="h-4 w-4"/>Resume
          </a>
          <ThemeToggle />
        </nav>
      </div>
    </header>
  )
}