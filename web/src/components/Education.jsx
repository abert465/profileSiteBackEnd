export default function Education({ education = [] }) {
  return (
    <section id="education" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Education</h2>
        <ul className="mt-6 space-y-6">
          {education.map((ed, idx) => (
            <li key={idx} className="rounded-xl border bg-white/80 p-5 dark:bg-gray-900/80 dark:border-gray-800">
              <h3 className="font-semibold">{ed.degree}</h3>
              <p className="text-sm text-gray-600 dark:text-gray-400">{ed.school} • {format(ed.start)} – {format(ed.end)}</p>
              {ed.details?.length ? (
                <ul className="mt-2 list-disc list-inside text-gray-700 dark:text-gray-300">
                  {ed.details.map((d,i)=> <li key={i}>{d}</li>)}
                </ul>
              ) : null}
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