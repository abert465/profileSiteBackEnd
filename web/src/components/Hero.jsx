import { motion } from 'framer-motion'
import { ArrowRight } from 'lucide-react'

export default function Hero({ profile }) {

  const imgSrc = profile?.photoURL || '/images/profile.webp'

  // Both conditions matter: the flag is the switch, and a blank note would
  // otherwise render an empty pill with a blinking dot and no text.
  const availability = profile?.availabilityNote?.trim()
  const showAvailability = Boolean(profile?.availabilityVisible && availability)

  return (
    <section id="home" className="relative">
      <div className="max-w-6xl mx-auto px-4 py-20 grid md:grid-cols-2 gap-10 items-center">
        <motion.div initial={{ opacity: 0, y: 20 }} animate={{ opacity: 1, y: 0 }} transition={{ duration: 0.6 }}>
          {/* Availability badge, driven by Profile.AvailabilityNote /
              AvailabilityVisible. Editable from /admin/profile, so ending the
              search is a toggle rather than a deploy. */}
          {showAvailability && (
            <p className="mb-5 inline-flex items-center gap-2 rounded-full border border-emerald-500/30 bg-emerald-50 px-3 py-1.5 text-sm font-medium text-emerald-700 dark:bg-emerald-500/10 dark:text-emerald-300">
              <span aria-hidden="true" className="relative flex h-2 w-2">
                <span className="absolute inline-flex h-full w-full animate-ping rounded-full bg-emerald-500 opacity-75" />
                <span className="relative inline-flex h-2 w-2 rounded-full bg-emerald-500" />
              </span>
              {availability}
            </p>
          )}
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
          {/* Profile.Skills is [JsonIgnore], so these cannot come from the
              profile payload and have to be stated here. Keep them matching the
              resume: they sit above the fold, and a recruiter reading both
              documents will notice if the range disagrees. */}
          <div className="mt-6 flex flex-wrap gap-4 text-sm">
            <span className="px-3 py-1.5 rounded-full bg-blue-50 text-blue-700 border dark:bg-blue-500/10 dark:text-blue-300 dark:border-blue-500/20">.NET 6–10</span>
            <span className="px-3 py-1.5 rounded-full bg-purple-50 text-purple-700 border dark:bg-purple-500/10 dark:text-purple-300 dark:border-purple-500/20">React & Vue</span>
            <span className="px-3 py-1.5 rounded-full bg-emerald-50 text-emerald-700 border dark:bg-emerald-500/10 dark:text-emerald-300 dark:border-emerald-500/20">Azure DevOps</span>
          </div>
        </motion.div>

        {/* Right: headshot.
            The previous version combined `absolute inset-0` with an explicit
            w-[38rem] and a 3:2 aspect, so the width overrode the inset and the
            panel escaped its grid column — the photo ran past the right edge and
            was clipped. It also stacked an empty ring/shadow div *behind* the
            image, where the ring could never show, plus a fully transparent
            bg-black/0 overlay that did nothing. Both are gone; the ring now sits
            on the image itself and the glow is a blurred layer behind it. */}
        <motion.div initial={{ opacity: 0, scale: 0.95 }} animate={{ opacity: 1, scale: 1 }} transition={{ duration: 0.6, delay: 0.1 }} className="relative mx-auto w-full max-w-sm">
          <div aria-hidden="true" className="absolute -inset-4 rounded-3xl bg-gradient-to-br from-indigo-500/20 via-fuchsia-500/10 to-slate-800/20 blur-xl" />
          <img
            src={imgSrc}
            alt={profile?.name ? `${profile.name}, headshot` : 'Profile photo'}
            width={900}
            height={900}
            loading="eager"
            fetchPriority="high"
            className="relative block w-full aspect-square rounded-3xl object-cover ring-1 ring-black/5 dark:ring-white/10 shadow-lg"
          />
        </motion.div>
      </div>
    </section>
  )
}