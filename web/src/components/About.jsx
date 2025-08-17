export default function About({ profile }) {
  return (
    <section id="about" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">About Me</h2>
        <p className="mt-4 max-w-3xl text-gray-700 dark:text-gray-300">
          {profile?.summary || "I'm a software developer specializing in .NET (5–9), C#, SQL optimization, and modern web (React/Vue). I build scalable, maintainable systems and love shaving milliseconds off hot paths."}
        </p>
        {profile?.links?.length ? (
          <ul className="mt-4 flex flex-wrap gap-3">
            {profile.links.map(l => (
              <li key={l.url}>
                <a
                  className="underline text-blue-700 hover:text-blue-800 dark:text-blue-400 dark:hover:text-blue-300"
                  href={l.url}
                  target="_blank"
                  rel="noreferrer"
                >
                  {l.label}
                </a>
              </li>
            ))}
          </ul>
        ) : null}
      </div>
    </section>
  )
}