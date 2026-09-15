import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listEducationAdmin, deleteEducationAdmin } from '../../lib/adminApi'

export default function EducationList(){
  const [rows, setRows] = useState([])
  const [loading, setLoading] = useState(true)
  const [err, setErr] = useState('')

  useEffect(() => {
    (async () => {
      try { setRows(await listEducationAdmin()) }
      catch (e) { setErr(String(e.message || e)) }
      finally { setLoading(false) }
    })()
  }, [])

  async function remove(i){
    if (!confirm('Delete this education entry?')) return
    await deleteEducationAdmin(i)
    setRows(r => r.filter((_, idx) => idx !== i))
  }

  return (
    <section>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Education</h1>
        <Link to="/admin/education/new" className="rounded bg-blue-600 text-white px-4 py-2">New</Link>
      </div>

      {err && <p className="mt-3 text-sm text-red-600">{err}</p>}
      {loading && <p className="mt-3 text-sm text-zinc-500">Loading…</p>}

      <ul className="mt-4 divide-y">
        {rows.map((r, i) => (
          <li key={`${r.school}-${r.degree}-${i}`} className="py-3 flex items-center justify-between">
            <div>
              <div className="font-medium">{r.degree} — {r.school}</div>
              <div className="text-sm text-gray-500">
                {new Date(r.start).toLocaleDateString()} – {new Date(r.end).toLocaleDateString()}
              </div>
            </div>
            <div className="text-sm">
              <Link to={`/admin/education/${i}`} className="underline mr-3">Edit</Link>
              <button onClick={() => remove(i)} className="text-red-600">Delete</button>
            </div>
          </li>
        ))}
        {!loading && rows.length === 0 && !err && (
          <li className="py-6 text-sm text-zinc-500">No education entries yet. Click “New”.</li>
        )}
      </ul>
    </section>
  )
}
