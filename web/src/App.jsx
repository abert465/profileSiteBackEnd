import { useEffect, useState } from 'react'
import Header from './components/Header'
import Background from './components/Background'
import Hero from './components/Hero'
import About from './components/About'
import Skills from './components/Skills'
import Projects from './components/Projects'
import Experience from './components/Experience'
import Education from './components/Education'
import Certifications from './components/Certifications'
import Blog from './components/Blog'
import Testimonials from './components/Testimonials'
import Contact from './components/Contact'
import Footer from './components/Footer'
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
    <div>
      <Background />
      <Header profile={profile} />
      <Hero profile={profile} />
      <About profile={profile} />
      <Projects projects={projects} />
      <Skills skills={profile?.skills} />
      <Experience experience={experience} />
      <Education education={education} />
      <Certifications certifications={certifications} />
      <Testimonials />
      <Blog posts={posts} />
      <Contact profile={profile} />
      <Footer />
    </div>
  )
}