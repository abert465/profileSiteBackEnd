import { useState } from 'react'
import { sendContact } from '../lib/api'
import Footer from './Footer'

const empty = { name: '', email: '', subject: '', message: '' }

const labelClass = 'flex flex-col gap-1.5 font-mono text-[11.5px] uppercase tracking-[.07em] text-cmuted'
const fieldClass = 'w-full border-0 border-b border-cborder bg-transparent py-2.5 font-body text-lg normal-case tracking-normal text-paper outline-none transition-colors focus:border-accent2'
const outlineButton = 'rounded-xs border border-cborder px-4 py-[11px] text-[15px] text-paper transition-colors hover:border-paper'

// The band is drawn inverted - ink background, paper text - so in dark mode it
// becomes the one light block on the page. accent2 and the c* tokens are the
// band's own palette for that reason.
export default function Contact({ profile }) {
  const [form, setForm] = useState(empty)
  const [status, setStatus] = useState(null)

  async function onSubmit(e){
    e.preventDefault()
    setStatus('sending')
    try {
      await sendContact(form)
      setStatus('sent')
      setForm(empty)
    } catch {
      setStatus('error')
    }
  }

  const set = key => e => {
    setForm(f => ({ ...f, [key]: e.target.value }))
    setStatus(null)
  }

  // Resolve profile links with sensible fallbacks. The GitHub handle is
  // "abert465" - no second l. It reads like a typo and has been "corrected" into
  // a 404 more than once; leave it alone.
  const email = profile?.email || 'acampos892@gmail.com'
  const gh = profile?.github
    || profile?.links?.find(l => (l.label||'').toLowerCase().includes('github'))?.url
    || 'https://github.com/abert465'
  const li = profile?.linkedin
    || profile?.links?.find(l => (l.label||'').toLowerCase().includes('linked'))?.url
    || 'https://www.linkedin.com/in/albert-campos/'

  return (
    <section id="contact" className="bg-ink text-paper">
      <div className="mx-auto max-w-[1200px] px-[clamp(20px,4vw,48px)] pt-[clamp(64px,9vw,120px)] pb-10">
        <p className="mb-5 font-mono text-[12.5px] uppercase tracking-[.07em] text-cmuted">Contact</p>
        <div className="flex flex-wrap items-start gap-x-[clamp(40px,6vw,96px)] gap-y-14">
          <div className="min-w-0 flex-[1_1_400px]">
            <h2 className="max-w-[16ch] font-display text-[clamp(40px,5.4vw,72px)] font-normal leading-[1.02] tracking-[-0.025em] text-pretty">
              Still doing it by hand every Friday? <em className="text-accent2">Let's talk.</em>
            </h2>
            <div className="mt-11 flex flex-col items-start gap-5">
              <a href={`mailto:${email}`} className="font-display text-[clamp(24px,3vw,34px)] underline decoration-accent2 decoration-[1.5px] underline-offset-[3px] transition-colors hover:text-accent2">
                {email}
              </a>
              <div className="flex flex-wrap gap-3">
                {/* noopener: a target="_blank" link otherwise hands the opened
                    page a window.opener reference back to this one. */}
                <a href={li} target="_blank" rel="noopener noreferrer" className={outlineButton}>LinkedIn ↗</a>
                <a href={gh} target="_blank" rel="noopener noreferrer" className={outlineButton}>GitHub ↗</a>
                <a href="/resume.pdf" download type="application/pdf" className="rounded-xs bg-paper px-4 py-3 text-[15px] font-medium text-ink transition-colors hover:bg-accent2">
                  Résumé ↓
                </a>
              </div>
            </div>
          </div>

          <form onSubmit={onSubmit} className="flex min-w-0 flex-[1_1_400px] flex-col gap-[26px]">
            <p className="font-mono text-xs uppercase tracking-[.07em] text-cmuted">Or send a note</p>
            <div className="flex flex-wrap gap-x-7 gap-y-[26px]">
              <label className={`${labelClass} flex-[1_1_200px]`}>
                Name
                <input required name="name" autoComplete="name" value={form.name} onChange={set('name')} className={fieldClass} />
              </label>
              <label className={`${labelClass} flex-[1_1_200px]`}>
                Email
                <input required type="email" name="email" autoComplete="email" value={form.email} onChange={set('email')} className={fieldClass} />
              </label>
            </div>
            <label className={labelClass}>
              Subject · optional
              <input name="subject" value={form.subject} onChange={set('subject')} className={fieldClass} />
            </label>
            <label className={labelClass}>
              Message
              <textarea required name="message" rows={4} value={form.message} onChange={set('message')} className={`${fieldClass} min-h-[120px] resize-y leading-[1.5]`} />
            </label>
            <div className="flex flex-wrap items-center gap-x-6 gap-y-3.5">
              <button
                type="submit"
                disabled={status === 'sending'}
                className="cursor-pointer rounded-xs bg-accent2 px-[22px] py-3.5 text-base font-semibold text-ink transition-colors hover:bg-paper disabled:cursor-default disabled:opacity-60"
              >
                {status === 'sending' ? 'Sending…' : 'Send message →'}
              </button>
              {status === 'sent' && <p role="status" className="text-[15px] text-paper">Thanks — I'll get back to you shortly.</p>}
              {status === 'error' && <p role="alert" className="text-[15px] text-accent2">Something went wrong. Try again, or email me directly.</p>}
            </div>
          </form>
        </div>

        <Footer profile={profile} />
      </div>
    </section>
  )
}
