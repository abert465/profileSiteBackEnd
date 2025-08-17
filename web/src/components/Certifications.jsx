export default function Certifications({ certifications = [] }) {
  return (
    <section id="certifications" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Certifications</h2>
        <ul className="mt-6 grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
          {certifications.map((c, idx) => (
            <li key={idx} className="rounded-xl border bg-white/80 p-4 dark:bg-gray-900/80 dark:border-gray-800">
              <div className="font-semibold">{c.name}</div>
              {c.issuer && <div className="text-sm text-gray-600 dark:text-gray-400">{c.issuer}</div>}
              <div className="text-xs text-gray-500 mt-1 dark:text-gray-400">
                {c.issued ? `Issued ${format(c.issued)}` : ''}{c.expires ? ` • Expires ${format(c.expires)}` : ''}
              </div>
            </li>
          ))}
        </ul>
      </div>
    </section>
  )
}

function format(d){
  try { return new Date(d).toLocaleString(undefined, { month: 'short', year: 'numeric' }) } catch { return '' }
}