import React from 'react'
import { motion } from 'framer-motion'
import { Code2, Database, Atom, Cloud, Wrench, Network, Cpu } from 'lucide-react'

// One icon per category, not per skill.
//
// The old pickIcon() guessed from the label with substring matching and fell
// back to a generic </> for everything it missed, so TypeScript, Git, JIRA,
// Salesforce and Agile/Scrum all rendered identically — and AKS got </> while
// Azure Functions got a cloud, because "aks" does not contain "azure". Same
// platform, different icon, for no reason a reader could see.
//
// Keyed by the category strings in SampleData.SkillCategories.
const CATEGORY_ICONS = {
  'Backend & .NET': Cpu,
  Frontend: Atom,
  'Cloud & DevOps': Cloud,
  'Data & Reporting': Database,
  'APIs & Architecture': Network,
  'Tools & Practices': Wrench,
}

export default function Skills({ groups, skills = [] }) {
  // Fall back to a single unlabelled group if the API predates skillGroups, so
  // the section degrades to the old flat behaviour rather than rendering empty.
  const resolved =
    Array.isArray(groups) && groups.length > 0
      ? groups
      : skills.length > 0
        ? [{ category: null, items: skills }]
        : []

  if (resolved.length === 0) return null

  let index = 0

  return (
    <section id="skills" className="py-20 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Skills</h2>

        <div className="mt-8 space-y-8">
          {resolved.map((group) => {
            const Icon = CATEGORY_ICONS[group.category] ?? Code2
            return (
              <div key={group.category ?? 'all'}>
                {group.category && (
                  <h3 className="flex items-center gap-2 text-sm font-semibold uppercase tracking-wide text-gray-500 dark:text-gray-400">
                    <Icon className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                    {group.category}
                  </h3>
                )}
                <ul className="mt-3 flex flex-wrap gap-2">
                  {group.items.map((name) => {
                    // Stagger across the whole section rather than restarting per
                    // group, so the reveal reads as one pass down the page. Capped
                    // so the last chip does not wait out a long delay.
                    const delay = Math.min(index++ * 0.02, 0.6)
                    return (
                      <motion.li
                        key={name}
                        initial={{ opacity: 0, y: 8 }}
                        whileInView={{ opacity: 1, y: 0 }}
                        viewport={{ once: true }}
                        transition={{ delay }}
                        className="rounded-lg border bg-white/80 px-3 py-1.5 text-sm shadow-sm dark:bg-gray-900/80 dark:border-gray-800"
                      >
                        {name}
                      </motion.li>
                    )
                  })}
                </ul>
              </div>
            )
          })}
        </div>
      </div>
    </section>
  )
}
