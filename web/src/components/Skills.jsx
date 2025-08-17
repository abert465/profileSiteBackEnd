import React from 'react'
import { motion } from 'framer-motion'
import { Code2, Database, Atom, Cloud, GitBranch, Gauge, Cpu, GitCommit } from 'lucide-react'

function pickIcon(label){
  const l = (label || '').toLowerCase()
  if (l.includes('sql') || l.includes('entity')) return Database
  if (l.includes('react')) return Atom
  if (l.includes('azure')) return Cloud
  if (l.includes('devops')) return GitBranch
  if (l.includes('ci/cd') || l.includes('cicd')) return GitCommit
  if (l.includes('optimiz')) return Gauge
  if (l.includes('.net') || l.includes('c#') || l.includes('ef')) return Cpu
  return Code2
}

export default function Skills({ skills = [] }) {
  return (
    <section id="skills" className="py-20 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Skills</h2>
        <ul className="mt-6 grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
          {skills.map((s, i) => {
            const Icon = pickIcon(s)
            return (
              <motion.li
                key={s}
                initial={{ opacity: 0, y: 10 }}
                whileInView={{ opacity: 1, y: 0 }}
                viewport={{ once: true }}
                transition={{ delay: i * 0.03 }}
                className="group rounded-xl border bg-white/80 p-3 shadow-sm hover:shadow-md dark:bg-gray-900/80 dark:border-gray-800"
              >
                <div className="flex items-center gap-2">
                  <Icon className="h-4 w-4 text-blue-600" />
                  <span>{s}</span>
                </div>
              </motion.li>
            )
          })}
        </ul>
      </div>
    </section>
  )
}