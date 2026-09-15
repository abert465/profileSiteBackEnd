import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { getCertAdmin, addCertAdmin, updateCertAdmin } from '../../lib/adminApi'

const toInputDate = (d) => (d ? new Date(d).toISOString().slice(0,10) : '')

export default function CertForm(){
  const { id } = useParams()
  const nav = useNavigate()
  const editing = id !== undefined

  const [model, set] = useState({ name:'', issuer:'', issued:'', expires:'' })
  const [err, setErr] = useState('')

  useEffect(() => {
    if (!editing) return
    ;(async () => {
      try {
        const row = await getCertAdmin(id)
        if (!row) { setErr('Not found'); return }
        set(row)
      } catch (e) { setErr(String(e.message || e)) }
    })()
  }, [editing, id])

  async function submit(e){
    e.preventDefault()
    setErr('')
    try {
      if (editing) await updateCertAdmin(id, model)
      else await addCertAdmin(model)
      nav('/admin/certifications')
    } catch (ex) {
      setErr(String(ex.message || ex))
    }
  }

  return (
    <form onSubmit={submit} className="space-y-3 max-w-2xl">
      <h1 className="text-2xl font-bold">{editing ? 'Edit' : 'New'} Certification</h1>

      <input className="w-full border rounded p-3"
             placeholder="Name"
             value={model.name || ''}
             onChange={e=>set({...model, name:e.target.value})} />

      <input className="w-full border rounded p-3"
             placeholder="Issuer"
             value={model.issuer || ''}
             onChange={e=>set({...model, issuer:e.target.value})} />

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <label className="block">
          <span className="text-sm text-zinc-600 dark:text-zinc-300">Issued</span>
          <input type="date" className="w-full border rounded p-3"
                 value={toInputDate(model.issued)}
                 onChange={e=>set({...model, issued: e.target.value})}/>
        </label>
        <label className="block">
          <span className="text-sm text-zinc-600 dark:text-zinc-300">Expires</span>
          <input type="date" className="w-full border rounded p-3"
                 value={toInputDate(model.expires)}
                 onChange={e=>set({...model, expires: e.target.value})}/>
        </label>
      </div>

      {err && <p className="text-sm text-red-600">{err}</p>}
      <button className="px-5 py-2.5 rounded bg-blue-600 text-white">Save</button>
    </form>
  )
}
