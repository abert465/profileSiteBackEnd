// The three most recent roles carry full detail; anything older renders as a
// one-line entry. Index-based rather than a hardcoded cutoff year, so adding a
// job automatically demotes the oldest one instead of growing the section.
const FULL_DETAIL_COUNT = 3

export default function Experience({ experience = [] }) {
  const detailed = experience.slice(0, FULL_DETAIL_COUNT)
  const earlier = experience.slice(FULL_DETAIL_COUNT)

  return (
    <section id="experience" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Experience</h2>

        <ol className="mt-6 border-l dark:border-gray-800">
          {detailed.map((e, idx) => (
            <li key={idx} className="relative ml-6 mb-8">
              <Dot />
              <h3 className="text-base font-semibold">{e.company}</h3>
              <p className="text-sm text-gray-700 dark:text-gray-300">{e.role}</p>
              <p className="mt-0.5 text-sm text-gray-600 dark:text-gray-400">
                <Meta e={e} />
              </p>
              {e.roleNote ? (
                <p className="text-sm text-gray-500 dark:text-gray-400 italic">{e.roleNote}</p>
              ) : null}
              {/* list-outside plus padding keeps wrapped lines aligned to the
                  text edge instead of running back under the marker, and the
                  measure caps at roughly 80 characters so long highlights stay
                  readable at desktop width. */}
              <ul className="mt-3 max-w-3xl ps-5 list-disc list-outside space-y-1.5 marker:text-blue-600/70 text-gray-700 dark:text-gray-300">
                {e.highlights?.map((h, i) => <li key={i}>{h}</li>)}
              </ul>
              {e.tech?.length ? (
                <div className="mt-3 flex flex-wrap gap-2 text-xs">
                  {e.tech.map(t => (
                    <span key={t} className="px-2 py-1 rounded-full border dark:border-gray-800">{t}</span>
                  ))}
                </div>
              ) : null}
            </li>
          ))}
        </ol>

        {earlier.length ? (
          <div className="mt-2">
            <h3 className="text-sm font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">
              Earlier
            </h3>
            <ol className="mt-4 border-l dark:border-gray-800">
              {earlier.map((e, idx) => (
                <li key={idx} className="relative ml-6 mb-5">
                  <Dot muted />
                  <h4 className="text-sm font-semibold">
                    {e.company}
                    <span className="font-normal text-gray-700 dark:text-gray-300"> — {e.role}</span>
                  </h4>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    <Meta e={e} />
                  </p>
                </li>
              ))}
            </ol>
          </div>
        ) : null}
      </div>
    </section>
  )
}

// Decorative: the timeline reads from the dates, so the marker is hidden from
// screen readers. top is explicit rather than relying on static position.
function Dot({ muted = false }) {
  // li is inset 1.5rem (ml-6) from the list border and the dot is 0.75rem wide,
  // so -1.875rem centers it on the line.
  return (
    <span
      aria-hidden="true"
      className={`absolute -left-[1.875rem] top-1.5 w-3 h-3 rounded-full ${
        muted ? 'bg-gray-400 dark:bg-gray-600' : 'bg-blue-600'
      }`}
    />
  )
}

function Meta({ e }) {
  return (
    <>
      {format(e.start)} – {e.end ? format(e.end) : 'Present'}
      {e.location ? ` • ${e.location}` : ''}
    </>
  )
}

function format(d){
  try { return new Date(d).toLocaleString(undefined, { month: 'short', year: 'numeric' }) } catch { return '' }
}
