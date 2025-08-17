import { motion } from 'framer-motion'
import { Github, ExternalLink } from 'lucide-react'

export default function Projects({ projects = [] }) {
  return (
    <section id="projects" className="py-20 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Projects</h2>
        <div className="mt-6 grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {projects.map((p, i) => (
            <motion.article
              key={p.slug}
              initial={{ opacity: 0, y: 15 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ delay: i * 0.05 }}
              className="rounded-2xl"
            >
              <div className="relative rounded-2xl gradient-border p-[1px]">
                <div className="rounded-2xl bg-white/80 p-5 shadow-sm dark:bg-gray-900/80 dark:border-gray-800">
                  <div className="h-32 rounded-xl bg-gray-200 mb-4 dark:bg-gray-800" />
                  <h3 className="font-semibold text-lg">{p.title}</h3>
                  <p className="text-gray-700 mt-1 dark:text-gray-300">{p.description}</p>
                  <div className="mt-3 flex flex-wrap gap-2 text-xs">
                    {p.tech?.map(t => <span key={t} className="px-2 py-1 rounded-full border dark:border-gray-800">{t}</span>)}
                  </div>
                  <div className="mt-4 flex gap-3 text-sm">
                    {p.liveUrl && (
                      <a className="inline-flex items-center gap-2 px-3 py-1.5 rounded-lg bg-blue-600 text-white" href={p.liveUrl} target="_blank" rel="noreferrer">
                        <ExternalLink className="h-4 w-4"/>Live
                      </a>
                    )}
                    {p.repoUrl && (
                      <a className="inline-flex items-center gap-2 px-3 py-1.5 rounded-lg border dark:border-gray-800" href={p.repoUrl} target="_blank" rel="noreferrer">
                        <Github className="h-4 w-4"/>GitHub
                      </a>
                    )}
                  </div>
                  {p.highlights?.length ? (
                    <ul className="mt-3 list-disc list-inside text-sm text-gray-700 dark:text-gray-300">
                      {p.highlights.map(h => <li key={h}>{h}</li>)}
                    </ul>
                  ) : null}
                </div>
              </div>
            </motion.article>
          ))}
        </div>
      </div>
    </section>
  )
}