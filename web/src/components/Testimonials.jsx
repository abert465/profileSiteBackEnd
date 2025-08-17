export default function Testimonials() {
  return (
    <section id="testimonials" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Testimonials</h2>
        <div className="mt-6 grid md:grid-cols-2 gap-6">
          <blockquote className="rounded-2xl border bg-white p-5 dark:bg-gray-900 dark:border-gray-800">
            <p>“Albert consistently delivers maintainable, high‑quality code and mentors teammates effectively.”</p>
            <footer className="mt-3 text-sm text-gray-600 dark:text-gray-400">— Former Manager</footer>
          </blockquote>
        </div>
      </div>
    </section>
  )
}