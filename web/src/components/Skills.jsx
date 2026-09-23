// Display order, keyed by the category strings in SampleData.SkillCategories.
// Data & Reporting sits second rather than in seed order because SQL is a core
// strength, not an afterthought. Unknown categories fall to the end.
const ORDER = [
  'Backend & .NET',
  'Data & Reporting',
  'Cloud & DevOps',
  'Frontend',
  'APIs & Architecture',
  'Tools & Practices',
]

const rank = c => {
  const i = ORDER.indexOf(c)
  return i === -1 ? ORDER.length : i
}

export default function Skills({ groups, skills = [] }) {
  // Fall back to a single unlabelled group if the API predates skillGroups, so
  // the section degrades to the old flat behaviour rather than rendering empty.
  const resolved =
    Array.isArray(groups) && groups.length > 0
      ? [...groups].sort((a, b) => rank(a.category) - rank(b.category))
      : skills.length > 0
        ? [{ category: null, items: skills }]
        : []

  if (resolved.length === 0) return null

  return (
    <section id="skills" className="pb-[clamp(56px,8vw,96px)]">
      <h2 className="mb-3 font-display text-[clamp(34px,4vw,52px)] font-normal tracking-[-0.02em]">Toolkit</h2>
      <div className="grid grid-cols-[repeat(auto-fill,minmax(min(100%,300px),1fr))] gap-x-8 border-t border-ink">
        {resolved.map(group => (
          <div key={group.category ?? 'all'} className="border-b border-rule pt-[22px] pb-6">
            {group.category && (
              <h3 className="mb-3 font-mono text-xs uppercase tracking-[.07em] text-accent">{group.category}</h3>
            )}
            <p className="font-display text-[21px] leading-[1.45] tracking-[-0.005em] text-pretty">{group.items.join(', ')}</p>
          </div>
        ))}
      </div>
    </section>
  )
}
