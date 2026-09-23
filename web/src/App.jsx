import { useEffect, useState } from 'react'
import { MotionConfig } from 'framer-motion'
import Header from './components/Header'
import Hero from './components/Hero'
import Ledger from './components/Ledger'
import Skills from './components/Skills'
import Projects from './components/Projects'
import Experience from './components/Experience'
import Education from './components/Education'
import Certifications from './components/Certifications'
import Blog from './components/Blog'
import Contact from './components/Contact'
import { getProfile, getProjects, getPosts, getExperience, getEducation, getCertifications } from './lib/api'

export default function App(){
  const [profile, setProfile] = useState(null)
  const [projects, setProjects] = useState([])
  const [posts, setPosts] = useState([])
  const [experience, setExperience] = useState([])
  const [education, setEducation] = useState([])
  const [certifications, setCertifications] = useState([])

  useEffect(() => {
    (async () => {
      setProfile(await getProfile())
      setProjects(await getProjects())
      setPosts(await getPosts())
      setExperience(await getExperience())
      setEducation(await getEducation())
      setCertifications(await getCertifications())
    })()
  }, [])

  return (
    // reducedMotion="user" applies every component's motion props through the
    // OS setting in one place: with "reduce" on, framer-motion drops the
    // transform and layout animation and keeps the opacity fade.
    <MotionConfig reducedMotion="user">
      {/* `site` scopes the editorial palette's selection colour and scroll
          offset, and lets <body> pick up the paper background, without
          touching the admin panel that shares this stylesheet. */}
      <div className="site min-h-screen bg-paper font-body text-ink antialiased">
        <Header profile={profile} />
        <main className="mx-auto max-w-[1200px] px-[clamp(20px,4vw,48px)]">
          <Hero profile={profile} />
          <Ledger />
          <Projects projects={projects} />
          <Experience experience={experience} />
          <Skills groups={profile?.skillGroups} skills={profile?.skills} />
          <section className="grid grid-cols-[repeat(auto-fit,minmax(min(100%,420px),1fr))] gap-x-[clamp(32px,5vw,64px)] gap-y-12 pb-[clamp(56px,8vw,96px)]">
            <Education education={education} />
            <Certifications certifications={certifications} />
          </section>
          <Blog posts={posts} />
        </main>
        <Contact profile={profile} />
      </div>
    </MotionConfig>
  )
}
