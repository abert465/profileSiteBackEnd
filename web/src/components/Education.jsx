// Education.Details is six long bullets written for the resume; this column
// has room for one paragraph. Condensed by hand from those bullets, so keep
// them in step. Used for the in-progress degree only; any other entry falls
// back to its own details.
const WGU_SUMMARY =
  'Competency-based: every course ends in a proctored exam or performance task. C#/.NET engineering core, data structures and algorithms, software security and testing, plus calculus, linear algebra and discrete math.'

export default function Education({ education = [] }) {
  return (
    <div>
      <h2 className="mb-3 font-display text-[clamp(28px,3vw,38px)] font-normal tracking-[-0.02em]">Education</h2>
      <ul className="border-t border-ink">
        {education.map((ed, idx) => {
          const inProgress = ed.end && new Date(ed.end) > new Date()
          const summary = ed.school === 'Western Governors University' ? WGU_SUMMARY : ed.details?.join(' ')
          return (
            <li key={idx} className="pt-5">
              <p className="font-mono text-[12.5px] text-muted">
                {year(ed.start)} — {inProgress ? `Expected ${year(ed.end)}` : year(ed.end)}
              </p>
              {/* The stored degree carries a parenthetical for reconciling
                  older resumes; the heading only needs the name. */}
              <h3 className="mt-2 font-display text-2xl leading-[1.2]">{shortDegree(ed.degree)}</h3>
              <p className="mt-1 text-base text-body">{ed.school}{inProgress ? ' · in progress' : ''}</p>
              {summary && <p className="mt-3.5 text-[15.5px] leading-[1.6] text-body text-pretty">{summary}</p>}
            </li>
          )
        })}
      </ul>
    </div>
  )
}

function shortDegree(d = ''){
  return d.split(/[,(]/)[0].trim()
}

function year(d){
  return d ? new Date(d).getFullYear() : ''
}
