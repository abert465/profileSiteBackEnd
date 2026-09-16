// Links come from the profile, not from literals. The hardcoded pair that used
// to live here pointed at github.com/albert465 and linkedin.com/in/albertcampos
// - neither of which is Albert's - so the footer sent visitors to a 404 while
// the correct URLs sat in the profile data the rest of the page already used.
export default function Footer({ profile }) {
  const links = [
    { label: 'GitHub', url: profile?.github },
    { label: 'LinkedIn', url: profile?.linkedin },
  ].filter(l => l.url)

  return (
    <footer className="py-8 border-t mt-16 dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4 flex items-center justify-between text-sm text-gray-600 dark:text-gray-400">
        <p>© {new Date().getFullYear()} {profile?.name ?? 'Albert Campos'}</p>
        <div className="flex gap-4">
          {links.map(l => (
            // noopener: a target="_blank" link otherwise hands the opened page a
            // window.opener reference back to this one.
            <a key={l.label} href={l.url} target="_blank" rel="noopener noreferrer">
              {l.label}
            </a>
          ))}
        </div>
      </div>
    </footer>
  )
}
