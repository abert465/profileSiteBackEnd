const FALLBACK = "I'm a software developer specializing in .NET (5–9), C#, SQL optimization, and modern web frontend. I build scalable, maintainable systems and love shaving milliseconds off hot paths."

export default function About({ profile }) {
  // Summary is authored as one string with blank lines between paragraphs, so a
  // single <p> would run it all together. Split rather than add a second field.
  const paragraphs = (profile?.summary || FALLBACK)
    .split(/\n\s*\n/)
    .map((p) => p.trim())
    .filter(Boolean)

  return (
    <section id="about" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">About Me</h2>
        <div className="mt-4 max-w-3xl space-y-4 text-gray-700 dark:text-gray-300">
          {paragraphs.map((p, i) => (
            <p key={i}>{p}</p>
          ))}
        </div>
        {/* profile.links used to render here as a bare list, which repeated the
            GitHub and LinkedIn pair a third time - Contact and the footer already
            carry it - in plain underlined text that matched nothing else on the
            page. Restore this block if the profile ever holds links those two
            places do not cover. */}
      </div>
    </section>
  )
}