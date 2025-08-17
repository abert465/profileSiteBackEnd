export default function Experience({ experience = [] }) {
  return (
    <section id="experience" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Experience</h2>
        <ol className="mt-6 relative border-l dark:border-gray-800">
          {experience.map((e, idx) => (
            <li key={idx} className="ml-6 mb-8">
              <div className="absolute -left-1.5 w-3 h-3 bg-blue-600 rounded-full" />
              <h3 className="font-semibold">{e.role} — {e.company}</h3>
              <p className="text-sm text-gray-600 dark:text-gray-400">{format(e.start)} – {e.end ? format(e.end) : 'Present'}{e.location ? ` • ${e.location}` : ''}</p>
              <ul className="mt-2 list-disc list-inside text-gray-700 dark:text-gray-300">
                {e.highlights?.map((h,i)=> <li key={i}>{h}</li>)}
              </ul>
              {e.tech?.length ? (
                <div className="mt-2 flex flex-wrap gap-2 text-xs">
                  {e.tech.map(t => <span key={t} className="px-2 py-1 rounded-full border dark:border-gray-800">{t}</span>)}
                </div>
              ) : null}
            </li>
          ))}
        </ol>
      </div>
    </section>
  )
}

function format(d){
  try { return new Date(d).toLocaleString(undefined, { month: 'short', year: 'numeric' }) } catch { return '' }
}