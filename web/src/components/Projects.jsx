import { useState } from 'react'
import Chips from './Chips'

// Front-end only until Project grows the fields. Keyed by slug; a project with
// no entry simply renders without an eyebrow or link note.
const DOMAINS = {
  'police-overtime-scheduling': 'Public sector · Boston Police Department',
  'automated-expunction-engine': 'Legal tech · Easy Expunctions',
  'naas-nexus-work-orders': 'Aviation MRO · NAAS',
  'enterprise-cloud-migration': 'Platform · Cloud migration',
  'developer-portfolio-platform': 'Personal · This site',
  'smartfit-nutrition-tracker': 'Personal · Mobile',
}

// Both portals link to a login wall by design. The note sits on the link so
// an unexplained gate does not look broken.
const LIVE_NOTES = {
  'police-overtime-scheduling': 'sign-in · staff only',
  'naas-nexus-work-orders': 'sign-in · staff only',
}

const pad = n => String(n).padStart(2, '0')

export default function Projects({ projects = [] }) {
  const [open, setOpen] = useState({})
  const allOpen = projects.length > 0 && projects.every(p => open[p.slug])

  const toggleAll = () => setOpen(Object.fromEntries(projects.map(p => [p.slug, !allOpen])))

  return (
    <section id="work" className="pb-[clamp(56px,8vw,96px)]">
      <div className="mb-3 flex flex-wrap items-baseline justify-between gap-4">
        <h2 className="font-display text-[clamp(34px,4vw,52px)] font-normal tracking-[-0.02em]">Selected work</h2>
        {projects.length > 0 && (
          <button
            type="button"
            onClick={toggleAll}
            className="cursor-pointer border-b border-ink pb-0.5 font-mono text-[12.5px] uppercase tracking-[.06em] transition-colors hover:border-accent hover:text-accent"
          >
            {allOpen ? 'Collapse all' : 'Expand all'}
          </button>
        )}
      </div>

      {projects.map((p, i) => (
        <Project
          key={p.slug}
          p={p}
          no={pad(i + 1)}
          open={!!open[p.slug]}
          onToggle={() => setOpen(s => ({ ...s, [p.slug]: !s[p.slug] }))}
        />
      ))}
    </section>
  )
}

function Project({ p, no, open, onToggle }) {
  const links = [
    p.repoUrl && { label: 'GitHub', href: p.repoUrl },
    p.liveUrl && { label: 'Live', href: p.liveUrl, note: LIVE_NOTES[p.slug] },
  ].filter(Boolean)
  const highlights = p.highlights ?? []
  const detailsId = `project-${p.slug}-details`

  return (
    <article className="flex flex-wrap gap-x-[clamp(24px,4vw,56px)] gap-y-6 border-t border-ink pt-8 pb-9">
      <div className="flex-[0_0_64px] font-display text-[44px] leading-none tracking-[-0.02em] text-accent">{no}</div>

      <div className="min-w-0 flex-[1_1_360px]">
        {DOMAINS[p.slug] && (
          <p className="mb-2.5 font-mono text-xs uppercase tracking-[.07em] text-muted">{DOMAINS[p.slug]}</p>
        )}
        <h3 className="font-display text-[clamp(26px,2.8vw,34px)] font-medium leading-[1.12] tracking-[-0.015em] text-pretty">{p.title}</h3>
        <p className="mt-3.5 max-w-[62ch] text-[17px] leading-[1.6] text-body text-pretty">{p.description}</p>
        <Chips items={p.tech} className="mt-[18px]" />

        <div className="mt-[22px] flex flex-wrap items-center gap-x-[22px] gap-y-2.5">
          {highlights.length > 0 && (
            <button
              type="button"
              aria-expanded={open}
              aria-controls={detailsId}
              onClick={onToggle}
              className="inline-flex cursor-pointer items-center gap-2 text-[15px] font-semibold transition-colors hover:text-accent"
            >
              <span aria-hidden="true" className="inline-flex h-5 w-5 items-center justify-center rounded-[2px] border border-current font-mono text-[13px]">
                {open ? '−' : '+'}
              </span>
              {open ? 'Hide details' : `${highlights.length} details`}
            </button>
          )}
          {links.map(l => (
            <a key={l.label} href={l.href} target="_blank" rel="noopener noreferrer" className="text-[15px] font-medium underline decoration-accent underline-offset-[3px] hover:text-accent">
              {l.label} ↗
              {l.note && <span className="ml-1.5 font-mono text-[11.5px] text-muted">{l.note}</span>}
            </a>
          ))}
          {links.length === 0 && (
            <span className="font-mono text-xs uppercase tracking-[.05em] text-accent">In progress · private repo</span>
          )}
        </div>

        {open && (
          <ol id={detailsId} className="mt-[22px] flex max-w-[66ch] flex-col border-t border-rule">
            {highlights.map((h, j) => (
              <li key={h} className="flex gap-3.5 border-b border-rule py-3 text-base leading-[1.55] text-body">
                <span className="flex-none pt-[3px] font-mono text-xs text-accent">{pad(j + 1)}</span>
                <span>{h}</span>
              </li>
            ))}
          </ol>
        )}
      </div>

      <div className="min-w-0 max-w-[440px] flex-[1_1_260px]">
        {p.imageUrl ? (
          <img
            src={p.imageUrl}
            alt={`${p.title} screenshot`}
            loading="lazy"
            className="block aspect-[5/3] w-full rounded-xs border border-chip bg-hover object-cover object-left-top"
          />
        ) : (
          <div className="flex aspect-[5/3] w-full items-center justify-center rounded-xs border border-dashed border-dash bg-[repeating-linear-gradient(135deg,var(--hover)_0_8px,var(--stripe)_8px_16px)] p-4 text-center font-mono text-xs text-muted">
            app screenshot — coming soon
          </div>
        )}
      </div>
    </article>
  )
}
