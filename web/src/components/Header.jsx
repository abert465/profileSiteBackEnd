import { useState } from 'react'
import ThemeToggle from './ThemeToggle'

// GitHub and LinkedIn deliberately are not here. Social links in the bar make
// Résumé - the one action worth taking on a portfolio - compete with
// everything else. Both links remain in the contact band.
const NAV = [
  { label: 'Work', href: '#work' },
  { label: 'Experience', href: '#experience' },
  { label: 'Skills', href: '#skills' },
  { label: 'Writing', href: '#writing' },
  { label: 'Contact', href: '#contact' },
]

export default function Header({ profile }) {
  const [open, setOpen] = useState(false)

  return (
    <header className="sticky top-0 z-20 border-b border-rule bg-bar backdrop-blur-[10px]">
      <div className="mx-auto flex max-w-[1200px] items-center justify-between gap-5 px-[clamp(20px,4vw,48px)] py-3.5">
        <a href="#home" className="font-display text-[21px] font-medium tracking-[-0.01em]">
          {profile?.name || 'Albert Campos'}
        </a>

        <div className="flex items-center gap-[clamp(12px,1.8vw,26px)] whitespace-nowrap font-mono text-[12.5px] uppercase tracking-[.06em]">
          <nav aria-label="Primary" className="hidden items-center gap-[clamp(12px,1.8vw,26px)] md:flex">
            {NAV.map(n => (
              <a key={n.href} href={n.href} className="transition-colors hover:text-accent">{n.label}</a>
            ))}
          </nav>
          {/* `download` keeps the button honest: without it the PDF opens in
              the browser's viewer and the portfolio is gone from the tab. */}
          <a href="/resume.pdf" download type="application/pdf" className="rounded-xs bg-ink px-3.5 py-[9px] text-paper transition-colors hover:bg-accent">Résumé ↓</a>
          <ThemeToggle />
          {/* Below md the five links collapse behind a menu; Résumé and the
              theme toggle stay in the bar because they are the actions. */}
          <button
            type="button"
            aria-expanded={open}
            aria-controls="mobile-nav"
            onClick={() => setOpen(v => !v)}
            className="cursor-pointer border-b border-ink pb-0.5 uppercase transition-colors hover:border-accent hover:text-accent md:hidden"
          >
            {open ? 'Close' : 'Menu'}
          </button>
        </div>
      </div>

      {open && (
        <nav id="mobile-nav" aria-label="Primary" className="border-t border-rule md:hidden">
          <ul className="mx-auto max-w-[1200px] px-[clamp(20px,4vw,48px)] font-mono text-[12.5px] uppercase tracking-[.06em]">
            {NAV.map(n => (
              <li key={n.href} className="border-b border-rule last:border-b-0">
                <a href={n.href} onClick={() => setOpen(false)} className="block py-3 hover:text-accent">{n.label}</a>
              </li>
            ))}
          </ul>
        </nav>
      )}
    </header>
  )
}
