import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { getEducationAdmin, addEducationAdmin, updateEducationAdmin } from '../../lib/adminApi'
import { toInputDate } from '../_date'

export default function EducationForm(){
  const { id } = useParams()
  const nav = useNavigate()
  const editing = id !== undefined

  const [model, set] = useState({
    school:'', degree:'', start:'', end:'', details:[]
  })
  const [err, setErr] = useState('')

  useEffect(() => {
    if (!editing) return
    (async () => {
      try {
        const row = await getEducationAdmin(id)
        if (!row) { setErr('Not found'); return }
        set(row)
      } catch (e) {
        setErr(String(e.message || e))
      }
    })()
  }, [editing, id])

  async function submit(e){
    e.preventDefault()
    setErr('')
    try {
      if (editing) await updateEducationAdmin(id, model)
      else await addEducationAdmin(model)
      nav('/admin/education')
    } catch (ex) {
      setErr(String(ex.message || ex))
    }
  }

  return (
    <form onSubmit={submit} className="space-y-3 max-w-2xl">
      <h1 className="text-2xl font-bold">{editing ? 'Edit' : 'New'} Education</h1>

      <input className="w-full border rounded p-3" placeholder="School"
             value={model.school || ''} onChange={e=>set({...model, school:e.target.value})} />
      <input className="w-full border rounded p-3" placeholder="Degree"
             value={model.degree || ''} onChange={e=>set({...model, degree:e.target.value})} />

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <label className="block">
          <span className="text-sm text-zinc-600 dark:text-zinc-300">Start</span>
          <input type="date" className="w-full border rounded p-3"
                 value={toInputDate(model.start)}
                 onChange={e=>set({...model, start:e.target.value})}/>
        </label>
        <label className="block">
          <span className="text-sm text-zinc-600 dark:text-zinc-300">End</span>
          <input type="date" className="w-full border rounded p-3"
                 value={toInputDate(model.end)}
                 onChange={e=>set({...model, end:e.target.value})}/>
        </label>
      </div>

      <input className="w-full border rounded p-3"
             placeholder="Details (comma-separated)"
             value={(model.details || []).join(', ')}
             onChange={e=>set({
               ...model,
               details: e.target.value.split(',').map(s=>s.trim()).filter(Boolean)
             })}
      />

      {err && <p className="text-sm text-red-600">{err}</p>}
      <button className="px-5 py-2.5 rounded bg-blue-600 text-white">Save</button>
    </form>
  )
}
