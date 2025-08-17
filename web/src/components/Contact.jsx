import { useState } from 'react'
import { sendContact } from '../lib/api'
import { Github, Linkedin, Mail } from 'lucide-react'

export default function Contact({ profile }) {
  const [form, setForm] = useState({ name: '', email: '', subject: '', message: ''})
  const [status, setStatus] = useState(null)

  async function onSubmit(e){
    e.preventDefault()
    setStatus('sending')
    try {
      await sendContact(form)
      setStatus('sent')
      setForm({ name: '', email: '', subject: '', message: ''})
    } catch {
      setStatus('error')
    }
  }

  const field = "w-full border rounded-lg p-3 dark:bg-gray-900 dark:border-gray-800 dark:text-gray-100 placeholder:dark:text-gray-500"

  // Resolve profile links with sensible fallbacks
  const email = profile?.email || 'acampos892@gmail.com'
  const gh = profile?.github
    || profile?.links?.find(l => (l.label||'').toLowerCase().includes('github'))?.url
    || 'https://github.com/albert465'
  const li = profile?.linkedin
    || profile?.links?.find(l => (l.label||'').toLowerCase().includes('linked'))?.url
    || 'https://www.linkedin.com/in/albert-campos/'

  const IconLink = ({ href, label, children }) => (
    <a
      href={href}
      target={href.startsWith('http') ? '_blank' : undefined}
      rel={href.startsWith('http') ? 'noreferrer' : undefined}
      aria-label={label}
      title={label}
      className="inline-flex items-center justify-center h-10 w-10 rounded-full border hover:shadow transition dark:border-gray-800"
    >
      {children}
    </a>
  )

  return (
    <section id="contact" className="py-16 border-t dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4">
        <h2 className="text-2xl font-bold">Contact</h2>
        <div className="mt-6 grid md:grid-cols-2 gap-8">
          <form onSubmit={onSubmit} className="space-y-3">
            <input className={field} placeholder="Name" value={form.name} onChange={e=>setForm({...form, name:e.target.value})} />
            <input className={field} type="email" placeholder="Email" value={form.email} onChange={e=>setForm({...form, email:e.target.value})} />
            <input className={field} placeholder="Subject (optional)" value={form.subject} onChange={e=>setForm({...form, subject:e.target.value})} />
            <textarea className={`${field} h-32`} placeholder="Message" value={form.message} onChange={e=>setForm({...form, message:e.target.value})} />
            <button className="px-5 py-2.5 rounded-xl bg-blue-600 text-white disabled:opacity-50" disabled={status==='sending'}>
              {status === 'sending' ? 'Sending…' : 'Send'}
            </button>
            {status==='sent' && <p className="text-green-600">Thanks! I’ll get back to you shortly.</p>}
            {status==='error' && <p className="text-red-600">Something went wrong. Try again.</p>}
          </form>
          <div>
            <p className="text-gray-700 dark:text-gray-300">Prefer email or socials?</p>
            <div className="mt-3 flex items-center gap-3">
              <IconLink href={`mailto:${email}`} label={`Email ${email}`}>
                <Mail className="h-5 w-5" />
              </IconLink>
              <IconLink href={gh} label="GitHub">
                <Github className="h-5 w-5" />
              </IconLink>
              <IconLink href={li} label="LinkedIn">
                <Linkedin className="h-5 w-5" />
              </IconLink>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}