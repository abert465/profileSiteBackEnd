import { motion } from 'framer-motion'

// Struck-through phrases in the headline. The rule colour and weight are what
// make them read as "replaced" rather than as a correction.
const strike = 'line-through decoration-accent decoration-[3px]'

export default function Hero({ profile }) {

  // The bundled asset ships in two widths; an uploaded photoURL is a single
  // file, so the srcset only applies when the default is in use.
  const defaultPhoto = '/images/profile.webp'
  const imgSrc = profile?.photoURL || defaultPhoto
  const imgSrcSet = imgSrc === defaultPhoto
    ? '/images/profile-480.webp 480w, /images/profile.webp 900w'
    : undefined

  // Both conditions matter: the flag is the switch, and a blank note would
  // otherwise render an empty pill with a blinking dot and no text.
  const availability = profile?.availabilityNote?.trim()
  const showAvailability = Boolean(profile?.availabilityVisible && availability)

  const name = profile?.name || 'Albert Campos'
  const email = profile?.email || 'acampos892@gmail.com'
  const eyebrow = [profile?.title, profile?.location].filter(Boolean).join(' · ')

  return (
    <section id="home" className="flex flex-wrap items-end gap-[clamp(32px,5vw,64px)] pt-[clamp(48px,8vw,104px)] pb-[clamp(40px,6vw,72px)]">
      <motion.div
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.6 }}
        className="min-w-0 flex-[1_1_560px]"
      >
        {/* Availability badge, driven by Profile.AvailabilityNote /
            AvailabilityVisible. Editable from /admin/profile, so ending the
            search is a toggle rather than a deploy. */}
        {showAvailability && (
          <p className="mb-7 inline-flex items-center gap-2.5 rounded-xs border border-gborder bg-gbg px-3 py-[7px] font-mono text-[12.5px] tracking-[.04em] text-green">
            {/* The ping runs forever, so it is the one animation on the page
                that keeps moving after the reveal settles. motion-reduce
                leaves the solid dot and drops the pulse. */}
            <span aria-hidden="true" className="relative inline-flex h-2 w-2">
              <span className="absolute inset-0 animate-ping rounded-full bg-green opacity-60 motion-reduce:hidden" />
              <span className="relative h-2 w-2 rounded-full bg-green" />
            </span>
            {availability}
          </p>
        )}
        {eyebrow && (
          <p className="mb-5 font-mono text-[13px] uppercase tracking-[.06em] text-muted">{eyebrow}</p>
        )}
        {/* The first line of Profile.Summary, with markup the plain-text field
            cannot carry. Keep the two in step if either changes. */}
        <h1 className="font-display text-[clamp(38px,5.6vw,72px)] font-normal leading-[1.04] tracking-[-0.025em] text-pretty">
          Most of what I build replaces <span className={strike}>a spreadsheet</span>,{' '}
          <span className={strike}>a paper form</span>, or{' '}
          <em className={strike}>something a person did by hand every Friday.</em>
        </h1>
        {profile?.tagline && (
          <p className="mt-7 max-w-[600px] text-[19px] leading-[1.55] text-body text-pretty">{profile.tagline}</p>
        )}
        <div className="mt-9 flex flex-wrap items-center gap-3">
          <a href="#work" className="inline-flex items-center rounded-xs bg-ink px-5 py-3.5 text-base font-medium text-paper transition-colors hover:bg-accent">
            See the work →
          </a>
          {/* `download` keeps the button honest: without it the PDF opens in
              the browser's viewer and the portfolio is gone from the tab. */}
          <a href="/resume.pdf" download type="application/pdf" className="inline-flex items-center rounded-xs border border-ink px-5 py-[13px] text-base font-medium transition-colors hover:bg-ink hover:text-paper">
            Download résumé
          </a>
          <a href={`mailto:${email}`} className="ml-2 font-mono text-[13.5px] underline decoration-accent underline-offset-[3px] hover:text-accent">
            {email}
          </a>
        </div>
      </motion.div>

      <motion.figure
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration: 0.6, delay: 0.1 }}
        className="min-w-[240px] flex-[0_1_320px]"
      >
        <img
          src={imgSrc}
          srcSet={imgSrcSet}
          // The column tops out at 320px, so the 480w file covers it at 1.5x.
          // Without this the browser assumes 100vw and pulls the 900w file.
          sizes="(min-width: 640px) 320px, 100vw"
          alt={`${name}, headshot`}
          width={900}
          height={1125}
          loading="eager"
          fetchPriority="high"
          className="block aspect-[4/5] w-full rounded-xs object-cover object-[50%_30%] [filter:var(--photo-filter)]"
        />
        <figcaption className="mt-2.5 flex justify-between font-mono text-[11.5px] uppercase tracking-[.05em] text-muted">
          <span>{name}</span>
          <span>Est. 2013</span>
        </figcaption>
      </motion.figure>
    </section>
  )
}
