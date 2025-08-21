import { useEffect, useState } from 'react'
import { listSkillsAdmin, addSkillAdmin, updateSkillAdmin, deleteSkillAdmin } from '../../lib/adminApi'

export default function SkillsList(){
  const [rows, setRows] = useState([])
  const [name, setName] = useState('')
  const [saving, setSaving] = useState(false)
  const [err, setErr] = useState('')

  async function load(){ setRows(await listSkillsAdmin()) }
  useEffect(()=>{ load() }, [])

  async function add(e){
    e.preventDefault()
    if(!name.trim()) return
    setSaving(true); setErr('')
    try{
      const created = await addSkillAdmin(name.trim(), true, null)
      setRows(r => [...r, created])
      setName('')
    }catch(ex){ setErr(String(ex.message||ex)) } finally{ setSaving(false) }
  }

  async function toggle(id, isVisible){
    await updateSkillAdmin(id, { isVisible: !isVisible })
    setRows(r => r.map(x => x.id === id ? { ...x, isVisible: !isVisible } : x))
  }

  async function remove(id){
    if(!confirm('Delete this skill?')) return
    await deleteSkillAdmin(id)
    setRows(r => r.filter(x => x.id !== id))
  }

  return (
    <section>
      <h1 className="text-2xl font-bold">Skills</h1>

      <form onSubmit={add} className="mt-4 flex gap-2">
        <input className="border rounded p-2 flex-1" placeholder="Add a skill (e.g., .NET 8)"
               value={name} onChange={e=>setName(e.target.value)} />
        <button disabled={saving} className="rounded bg-blue-600 text-white px-4 py-2">
          {saving ? 'Adding...' : 'Add'}
        </button>
      </form>
      {err && <p className="text-sm text-red-600 mt-2">{err}</p>}

      <ul className="mt-6 divide-y">
        {rows.map(s => (
          <li key={s.id} className="py-3 flex items-center justify-between">
            <div className="flex items-center gap-3">
              <span className="font-medium">{s.name}</span>
              <span className={`text-xs px-2 py-0.5 rounded-full ${s.isVisible ? 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300' : 'bg-zinc-100 text-zinc-600 dark:bg-zinc-800'}`}>
                {s.isVisible ? 'Visible' : 'Hidden'}
              </span>
            </div>
            <div className="text-sm flex items-center gap-3">
              <button onClick={()=>toggle(s.id, s.isVisible)} className="underline">
                {s.isVisible ? 'Hide' : 'Show'}
              </button>
              <button onClick={()=>remove(s.id)} className="text-red-600">Delete</button>
            </div>
          </li>
        ))}
      </ul>
    </section>
  )
}
