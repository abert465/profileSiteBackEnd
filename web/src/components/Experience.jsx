import { useState } from 'react'
import Chips from './Chips'

export default function Experience({ experience = [] }) {
  // Keyed by index; the most recent role starts open so the section leads
  // with detail instead of five closed rows.
  const [open, setOpen] = useState({ 0: true })

  const firstYear = experience.length
    ? Math.min(...experience.map(e => new Date(e.start).getFullYear()))
    : null

  return (
    <section id="experience" className="pb-[clamp(56px,8vw,96px)]">
      <div className="mb-3 flex flex-wrap items-baseline justify-between gap-4">
        <h2 className="font-display text-[clamp(34px,4vw,52px)] font-normal tracking-[-0.02em]">Experience</h2>
        {firstYear && (
          <span className="font-mono text-[12.5px] uppercase tracking-[.06em] text-muted">{firstYear} — Present</span>
        )}
      </div>

      <ol className="border-t border-ink">
        {experience.map((e, idx) => {
          const isOpen = !!open[idx]
          const panelId = `experience-${idx}`
          return (
            <li key={idx} className="border-b border-rule">
              <button
                type="button"
                aria-expanded={isOpen}
                aria-controls={panelId}
                onClick={() => setOpen(s => ({ ...s, [idx]: !s[idx] }))}
                className="flex w-full cursor-pointer flex-wrap items-baseline gap-x-8 gap-y-1.5 py-5 text-left transition-colors hover:bg-hover"
              >
                <span className="flex-[0_0_150px] font-mono text-[13px] text-muted">
                  {format(e.start)} — {e.end ? format(e.end) : 'Present'}
                </span>
                <span className="flex min-w-0 flex-[1_1_300px] flex-col gap-[3px]">
                  <span className="font-display text-2xl leading-[1.2] tracking-[-0.01em]">{e.company}</span>
                  <span className="text-[15.5px] text-body">
                    {e.role}
                    {e.roleNote && <span className="text-muted"> · {e.roleNote}</span>}
                  </span>
                </span>
                <span className="ml-auto flex flex-none items-baseline gap-4 whitespace-nowrap font-mono text-[12.5px] text-muted">
                  <span>{duration(e.start, e.end)}</span>
                  <span aria-hidden="true" className="text-[15px] text-ink">{isOpen ? '−' : '+'}</span>
                </span>
              </button>

              {isOpen && (
                <div id={panelId} className="pb-[26px] pl-[clamp(0px,30vw_-_90px,182px)]">
                  {e.location && <p className="mb-3 font-mono text-xs text-muted">{e.location}</p>}
                  {e.highlights?.length > 0 && (
                    <ul className="flex max-w-[70ch] flex-col gap-2.5">
                      {e.highlights.map((h, i) => (
                        <li key={i} className="flex gap-3 text-base leading-[1.55] text-body">
                          <span aria-hidden="true" className="flex-none text-accent">—</span>
                          <span>{h}</span>
                        </li>
                      ))}
                    </ul>
                  )}
                  <Chips items={e.tech} className="mt-4" />
                </div>
              )}
            </li>
          )
        })}
      </ol>
    </section>
  )
}

function format(d){
  try { return new Date(d).toLocaleString('en-US', { month: 'short', year: 'numeric' }) } catch { return '' }
}

// Whole months between start and end (or now), as "2 yr 7 mo".
function duration(start, end){
  const s = new Date(start)
  const e = end ? new Date(end) : new Date()
  const months = Math.max(0, (e.getFullYear() - s.getFullYear()) * 12 + (e.getMonth() - s.getMonth()))
  const y = Math.floor(months / 12)
  const m = months % 12
  return [y ? `${y} yr` : '', m ? `${m} mo` : ''].filter(Boolean).join(' ') || '1 mo'
}
