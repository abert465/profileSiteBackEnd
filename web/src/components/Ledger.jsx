// Static: there is no API field for this yet. The figures match the project
// highlights and the resume; if one changes, change all three.
const ROWS = [
  {
    before: 'Boston PD’s legacy “Blue Note” overtime workflow',
    after: 'Blazor Server portal with City of Boston SSO',
    outcome: 'Sole developer · WCAG 2.1 AA',
  },
  {
    before: 'Court expunction filings processed by hand',
    after: 'Intake-to-petition automation platform',
    outcome: '~30% faster case processing · 500+ cases/mo',
  },
  {
    before: 'Self-managed IIS servers on AWS',
    after: 'Azure App Services, cut over in one window',
    outcome: '20% lower hosting cost',
  },
]

const label = 'mb-1.5 block font-mono text-[11px] uppercase tracking-[.08em] text-muted'

export default function Ledger() {
  return (
    <section aria-label="What's been replaced" className="border-t-2 border-ink pb-[clamp(56px,8vw,96px)]">
      <p className="border-b border-rule py-3 font-mono text-[11.5px] uppercase tracking-[.08em] text-muted">What's been replaced</p>
      {ROWS.map(r => (
        <div key={r.after} className="flex flex-wrap gap-x-8 gap-y-3.5 border-b border-rule py-[22px]">
          <div className="min-w-0 flex-[1_1_200px]">
            <span className={label}>Before</span>
            <span className="text-[17px] leading-[1.45] text-muted line-through decoration-accent decoration-[1.5px]">{r.before}</span>
          </div>
          <div className="min-w-0 flex-[1.2_1_220px]">
            <span className={label}>After</span>
            <span className="font-display text-[22px] leading-[1.25] tracking-[-0.01em]">{r.after}</span>
          </div>
          <div className="min-w-0 flex-[1_1_200px]">
            <span className={label}>Outcome</span>
            <span className="font-mono text-[13.5px] leading-[1.5] text-accent">{r.outcome}</span>
          </div>
        </div>
      ))}
    </section>
  )
}
