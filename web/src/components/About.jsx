export default function About({ profile }) {
  return (
    <section id="about" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">About Me</h2>
        <p className="mt-4 max-w-3xl text-gray-700 dark:text-gray-300">
          {profile?.summary || "I'm a software developer specializing in .NET (5–9), C#, SQL optimization, and modern web frontend. I build scalable, maintainable systems and love shaving milliseconds off hot paths."}
        </p>
        {/* profile.links used to render here as a bare list, which repeated the
            GitHub and LinkedIn pair a third time - Contact and the footer already
            carry it - in plain underlined text that matched nothing else on the
            page. Restore this block if the profile ever holds links those two
            places do not cover. */}
      </div>
    </section>
  )
}