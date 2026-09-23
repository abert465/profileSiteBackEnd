export default function Certifications({ certifications = [] }) {
  return (
    <div>
      <h2 className="mb-3 font-display text-[clamp(28px,3vw,38px)] font-normal tracking-[-0.02em]">Certifications</h2>
      <ul className="border-t border-ink">
        {certifications.map((c, idx) => (
          <li key={idx} className="flex flex-wrap items-baseline justify-between gap-4 border-b border-rule py-4">
            <span className="flex flex-col gap-0.5">
              <span className="text-[17px] font-medium">{c.name}</span>
              {c.issuer && <span className="text-[14.5px] text-muted">{c.issuer}</span>}
            </span>
            <span className="font-mono text-[12.5px] text-muted">{dateLabel(c)}</span>
          </li>
        ))}
      </ul>
    </div>
  )
}

// A current certification shows how long it holds; a lapsed or open-ended one
// shows when it was earned, since its expiry is no longer the useful fact.
function dateLabel(c){
  if (c.expires && new Date(c.expires) > new Date()) return `Valid to ${format(c.expires)}`
  return c.issued ? format(c.issued) : ''
}

function format(d){
  try { return new Date(d).toLocaleString('en-US', { month: 'short', year: 'numeric' }) } catch { return '' }
}
