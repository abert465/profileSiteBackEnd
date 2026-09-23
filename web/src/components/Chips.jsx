// Tech tags, shared by project rows and experience entries.
export default function Chips({ items, className = '' }) {
  if (!items?.length) return null
  return (
    <ul className={`flex flex-wrap gap-1.5 ${className}`}>
      {items.map(t => (
        <li key={t} className="rounded-xs border border-chip px-2 py-1 font-mono text-xs text-body">{t}</li>
      ))}
    </ul>
  )
}
