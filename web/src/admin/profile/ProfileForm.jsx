import { useEffect, useState } from 'react'
import { getProfileAdmin, updateProfileAdmin } from '../../lib/adminApi'

// The profile is a single row, so this is an edit-only form: no list, no "new".
// PUT /api/admin/profile replaces every scalar and rebuilds Links from the body,
// which is why the whole loaded object is kept in state and sent back on save.
// A form that posted only the fields it displays would blank the rest.
export default function ProfileForm(){
  const [model, set] = useState(null)
  const [err, setErr] = useState('')
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    (async () => {
      try {
        const row = await getProfileAdmin()
        // A 200 with an empty body means the profile row has not been seeded.
        // Start from blanks so the panel can create it rather than dying here.
        set(row || { name:'', title:'', tagline:'', summary:'', location:'',
                     email:'', github:'', linkedin:'',
                     availabilityNote:'', availabilityVisible:false, links:[] })
      } catch (e) {
        setErr(String(e.message || e))
      }
    })()
  }, [])

  async function submit(e){
    e.preventDefault()
    setErr(''); setSaved(false)
    try {
      const fresh = await updateProfileAdmin(model)
      // Render what was persisted, so a value the server normalised or dropped
      // is visible immediately instead of at the next page load.
      if (fresh) set(fresh)
      setSaved(true)
    } catch (ex) {
      setErr(String(ex.message || ex))
    }
  }

  if (err && !model) return <p className="text-sm text-red-600">{err}</p>
  if (!model) return <p className="text-sm text-zinc-500">Loading…</p>

  const upd = (patch) => { set({ ...model, ...patch }); setSaved(false) }

  const links = model.links || []
  const updLink = (i, patch) =>
    upd({ links: links.map((l, n) => n === i ? { ...l, ...patch } : l) })

  return (
    <form onSubmit={submit} className="space-y-3 max-w-2xl">
      <h1 className="text-2xl font-bold">Profile</h1>

      <input className="w-full border rounded p-3" placeholder="Name"
             value={model.name || ''} onChange={e=>upd({ name:e.target.value })} />
      <input className="w-full border rounded p-3" placeholder="Title"
             value={model.title || ''} onChange={e=>upd({ title:e.target.value })} />
      <textarea className="w-full border rounded p-3" rows={3} placeholder="Tagline"
                value={model.tagline || ''} onChange={e=>upd({ tagline:e.target.value })} />
      <label className="block">
        <span className="text-sm text-zinc-600 dark:text-zinc-300">
          Summary — blank lines separate paragraphs on the About section
        </span>
        <textarea className="w-full border rounded p-3" rows={10}
                  value={model.summary || ''} onChange={e=>upd({ summary:e.target.value })} />
      </label>

      {/* Availability badge. Drives the green pill at the top of the Hero; the
          checkbox takes it down without clearing the wording. */}
      <fieldset className="border rounded p-3 space-y-3 dark:border-gray-800">
        <legend className="text-sm px-1 text-zinc-600 dark:text-zinc-300">Availability badge</legend>
        <input className="w-full border rounded p-3"
               placeholder="e.g. Open to full-stack .NET roles"
               value={model.availabilityNote || ''}
               onChange={e=>upd({ availabilityNote:e.target.value })} />
        <label className="flex items-center gap-2">
          <input type="checkbox" checked={Boolean(model.availabilityVisible)}
                 onChange={e=>upd({ availabilityVisible:e.target.checked })} />
          <span className="text-sm">Show the badge on the home page</span>
        </label>
        <p className="text-xs text-zinc-500">
          Hidden when unchecked or when the note is empty.
        </p>
      </fieldset>

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <input className="w-full border rounded p-3" placeholder="Location"
               value={model.location || ''} onChange={e=>upd({ location:e.target.value })} />
        <input className="w-full border rounded p-3" type="email" placeholder="Email"
               value={model.email || ''} onChange={e=>upd({ email:e.target.value })} />
        <input className="w-full border rounded p-3" placeholder="GitHub URL"
               value={model.github || ''} onChange={e=>upd({ github:e.target.value })} />
        <input className="w-full border rounded p-3" placeholder="LinkedIn URL"
               value={model.linkedin || ''} onChange={e=>upd({ linkedin:e.target.value })} />
      </div>

      {/* Links round-trip through this form because the PUT clears and re-adds
          them from the body — leaving them out of the page would delete them. */}
      <fieldset className="border rounded p-3 space-y-3 dark:border-gray-800">
        <legend className="text-sm px-1 text-zinc-600 dark:text-zinc-300">Links</legend>
        {links.map((l, i) => (
          <div key={i} className="flex gap-2">
            <input className="w-1/3 border rounded p-3" placeholder="Label"
                   value={l.label || ''} onChange={e=>updLink(i, { label:e.target.value })} />
            <input className="flex-1 border rounded p-3" placeholder="URL"
                   value={l.url || ''} onChange={e=>updLink(i, { url:e.target.value })} />
            <button type="button" className="px-3 rounded border dark:border-gray-800"
                    onClick={()=>upd({ links: links.filter((_, n) => n !== i) })}>
              Remove
            </button>
          </div>
        ))}
        <button type="button" className="px-3 py-2 rounded border dark:border-gray-800"
                onClick={()=>upd({ links: [...links, { label:'', url:'' }] })}>
          Add link
        </button>
      </fieldset>

      {err && <p className="text-sm text-red-600">{err}</p>}
      {saved && <p className="text-sm text-emerald-600">Saved.</p>}
      <button className="px-5 py-2.5 rounded bg-blue-600 text-white">Save</button>
    </form>
  )
}
