import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listCertsAdmin, deleteCertAdmin } from '../../lib/adminApi'

export default function CertsList(){
  const [rows, setRows] = useState([])
  const [loading, setLoading] = useState(true)
  const [err, setErr] = useState('')

  useEffect(() => {
    (async () => {
      try { setRows(await listCertsAdmin()) }
      catch (e) { setErr(String(e.message || e)) }
      finally { setLoading(false) }
    })()
  }, [])

  async function remove(i){
    if (!confirm('Delete this certification?')) return
    await deleteCertAdmin(i)
    setRows(r => r.filter((_, idx) => idx !== i))
  }

  const fmt = (d) => d ? new Date(d).toLocaleDateString() : '—'

  return (
    <section>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Certifications</h1>
        <Link to="/admin/certifications/new" className="rounded bg-blue-600 text-white px-4 py-2">
          New
        </Link>
      </div>

      {err && <p className="mt-3 text-sm text-red-600">{err}</p>}
      {loading && <p className="mt-3 text-sm text-zinc-500">Loading…</p>}

      <div className="mt-4 overflow-x-auto rounded-xl ring-1 ring-zinc-200 dark:ring-zinc-800">
        <table className="min-w-full text-left text-sm">
          <thead className="bg-zinc-50/60 dark:bg-zinc-900/40 text-zinc-600 dark:text-zinc-300">
            <tr>
              <th className="px-4 py-3 font-semibold">Name</th>
              <th className="px-4 py-3 font-semibold">Issuer</th>
              <th className="px-4 py-3 font-semibold">Issued</th>
              <th className="px-4 py-3 font-semibold">Expires</th>
              <th className="px-4 py-3 font-semibold text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-zinc-200 dark:divide-zinc-800">
            {!loading && rows.length === 0 && !err && (
              <tr>
                <td colSpan={5} className="px-4 py-6 text-zinc-500">
                  No certifications yet. Click “New” to add your first one.
                </td>
              </tr>
            )}
            {rows.map((r, i) => (
              <tr key={`${r.name}-${r.issuer}-${i}`} className="hover:bg-zinc-50/50 dark:hover:bg-zinc-900/30">
                <td className="px-4 py-3 font-medium">{r.name || '—'}</td>
                <td className="px-4 py-3">{r.issuer || '—'}</td>
                <td className="px-4 py-3">{fmt(r.issued)}</td>
                <td className="px-4 py-3">{fmt(r.expires)}</td>
                <td className="px-4 py-3">
                  <div className="flex justify-end gap-3">
                    <Link to={`/admin/certifications/${i}`} className="underline">
                      Edit
                    </Link>
                    <button onClick={() => remove(i)} className="text-red-600">
                      Delete
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  )
}
