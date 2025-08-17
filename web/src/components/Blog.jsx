export default function Blog({ posts = [] }) {
  return (
    <section id="blog" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Latest Articles</h2>
        <div className="mt-6 grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {posts.map(p => (
            <article key={p.slug} className="rounded-2xl border bg-white p-5 dark:bg-gray-900 dark:border-gray-800">
              <h3 className="font-semibold text-lg">{p.title}</h3>
              <p className="text-gray-700 mt-1 dark:text-gray-300">{p.excerpt}</p>
              <p className="text-sm text-gray-500 mt-2 dark:text-gray-400">{new Date(p.published).toLocaleDateString()}</p>
              <a className="inline-block mt-3 text-blue-600" href={`#post-${p.slug}`}>Read more →</a>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}