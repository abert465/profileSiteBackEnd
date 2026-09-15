// Renders whatever /api/testimonials returns. The endpoint already filters on
// IsVisible and orders the results, so nothing is decided here.
//
// The section hides itself when there is nothing to show. An empty "Testimonials"
// heading reads worse than no section at all, and this is the one section that
// can legitimately be empty.
export default function Testimonials({ testimonials = [] }) {
  if (testimonials.length === 0) return null

  return (
    <section id="testimonials" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Testimonials</h2>
        <div className="mt-6 grid md:grid-cols-2 gap-6">
          {testimonials.map(t => (
            <blockquote
              key={t.id}
              className="rounded-2xl border bg-white p-5 dark:bg-gray-900 dark:border-gray-800"
            >
              <p>{`“${t.content}”`}</p>
              <footer className="mt-3 text-sm text-gray-600 dark:text-gray-400">
                {`— ${t.name}`}
                {(t.title || t.company) && (
                  <span className="block text-xs mt-0.5">
                    {[t.title, t.company].filter(Boolean).join(', ')}
                  </span>
                )}
              </footer>
            </blockquote>
          ))}
        </div>
      </div>
    </section>
  )
}
