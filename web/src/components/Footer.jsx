// Sits inside the contact band rather than below it, so it shares the band's
// inverted colours instead of dropping back to the page's.
export default function Footer({ profile }) {
  return (
    <footer className="mt-[clamp(64px,8vw,104px)] flex flex-wrap justify-between gap-3 border-t border-crule pt-5 font-mono text-xs text-cmuted">
      <span>© {new Date().getFullYear()} {profile?.name ?? 'Albert Campos'}{profile?.location ? ` · ${profile.location}` : ''}</span>
      <span>.NET 10 API · React SPA · one box behind Cloudflare Tunnel. Broke it twice getting here.</span>
    </footer>
  )
}
