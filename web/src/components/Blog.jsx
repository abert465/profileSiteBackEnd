// Post has no topic field yet; keyed by slug until it does. A post without an
// entry shows its read time alone.
const TOPICS = {
  'optimizing-tsql': 'SQL Server',
  'ci-cd-azure-devops': 'Azure DevOps',
}

export default function Blog({ posts = [] }) {
  if (posts.length === 0) return null

  return (
    <section id="writing" className="pb-[clamp(56px,8vw,96px)]">
      <h2 className="mb-3 font-display text-[clamp(34px,4vw,52px)] font-normal tracking-[-0.02em]">Writing</h2>
      <ul className="border-t border-ink">
        {posts.map(p => (
          <li key={p.slug}>
            <a
              href={`#post-${p.slug}`}
              className="grid grid-cols-[repeat(auto-fit,minmax(min(100%,240px),1fr))] items-baseline gap-x-8 gap-y-1.5 border-b border-rule py-6 transition-colors hover:bg-hover"
            >
              <span className="font-mono text-[12.5px] text-muted">
                {[TOPICS[p.slug], `${readMinutes(p.content)} min`].filter(Boolean).join(' · ')}
              </span>
              <span className="col-span-2 font-display text-[clamp(22px,2.4vw,28px)] leading-[1.2] tracking-[-0.01em] text-pretty max-sm:col-span-1">
                {p.title}
              </span>
              <span className="flex justify-between gap-3 text-[15.5px] text-body">
                <span>{p.excerpt}</span>
                <span aria-hidden="true" className="text-accent">→</span>
              </span>
            </a>
          </li>
        ))}
      </ul>
    </section>
  )
}

// 200 words a minute, never under one.
function readMinutes(content = ''){
  const words = content.trim().split(/\s+/).filter(Boolean).length
  return Math.max(1, Math.round(words / 200))
}
