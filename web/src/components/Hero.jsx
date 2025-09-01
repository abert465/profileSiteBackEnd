import { motion } from 'framer-motion'
import { ArrowRight } from 'lucide-react'

export default function Hero({ profile }) {

  const imgSrc = profile?.photoURL || '/images/profile.webp'

  return (
    <section id="home" className="relative">
      <div className="max-w-6xl mx-auto px-4 py-20 grid md:grid-cols-2 gap-10 items-center">
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.6 }}>
          <h1 className="text-4xl md:text-6xl font-extrabold leading-tight">
            <span className="bg-gradient-to-r from-blue-600 via-sky-500 to-indigo-600 bg-clip-text text-transparent">
              {profile?.name}
            </span>
            <span className="block text-gray-900 mt-2 dark:text-white">{profile?.title}</span>
          </h1>
          <p className="mt-4 text-lg text-gray-700 dark:text-gray-300">{profile?.tagline}</p>
          <div className="mt-8 flex flex-wrap gap-3">
            <a href="#projects" className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl text-white bg-gradient-to-r from-blue-600 to-indigo-600 shadow">
              View My Work <ArrowRight className="h-4 w-4"/>
            </a>
            <a href="/resume.pdf" className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl border hover:bg-gray-100 dark:border-gray-800 dark:hover:bg-gray-900">Download Resume</a>
          </div>
          <div className="mt-6 flex gap-4 text-sm">
            <span className="px-3 py-1.5 rounded-full bg-blue-50 text-blue-700 border dark:bg-blue-500/10 dark:text-blue-300 dark:border-blue-500/20">.NET 8–9</span>
            <span className="px-3 py-1.5 rounded-full bg-purple-50 text-purple-700 border dark:bg-purple-500/10 dark:text-purple-300 dark:border-purple-500/20">React & Vue</span>
            <span className="px-3 py-1.5 rounded-full bg-emerald-50 text-emerald-700 border dark:bg-emerald-500/10 dark:text-emerald-300 dark:border-emerald-500/20">Azure DevOps</span>
          </div>
        </motion.div>

        {/* Right: image panel (was empty)*/}
        <motion.div initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} transition={{ duration: 0.6, delay: 0.1 }} className="relative h-64 md:h-80">
          <div className=" absolute inset-0 rounded-3xl  w-[26rem] md:w-[32rem] lg:w-[38rem] aspect-[300/200] bg-gradient-to-br from-indigo-500/20 via-fuchsia-500/10 to-slate-800/20 p-6">
            <div className="absolute inset-0 rounded-3xl overflow-hidden ring-1 ring-black/5 dark:ring-white/10 shadow-lg" />
            <img src={imgSrc} alt={profile?.name ? `${profile.name} portrait` : 'Profile photo'} loading='eager' fetchPriority='high' className="block h-full w-full object-cover rounded-3xl" />
          </div>
          <div className="pointer-events-none absolute inset-0 bg-black/0" />
        </motion.div>
      </div>
    </section>
  )
}