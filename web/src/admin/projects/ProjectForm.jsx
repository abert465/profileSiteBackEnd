import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { listProjects, saveProject, updateProject } from '../../lib/adminApi'

export default function ProjectForm(){
  const { slug } = useParams()
  const nav = useNavigate()
  const editing = Boolean(slug)
  const [model, setModel] = useState({ title:'', description:'', slug:'', tech:[], repoUrl:'', liveUrl:'', highlights:[] })
  const [err, setErr] = useState('')

  useEffect(() => {
    if (!editing) return
    ;(async () => {
      const all = await listProjects()
      const p = all.find(x => x.slug === slug)
      if (p) setModel(p)
    })()
  }, [editing, slug])

  async function submit(e){
    e.preventDefault()
    try{
      if (editing) await updateProject(slug, model)
      else await saveProject(model)
      nav('/admin/projects')
    } catch { setErr('Save failed') }
  }

  return (
    <form onSubmit={submit} className="space-y-3 max-w-2xl">
      <h1 className="text-2xl font-bold">{editing ? 'Edit' : 'New'} Project</h1>
      <input className="w-full border rounded p-3" placeholder="Title" value={model.title} onChange={e=>setModel({ ...model, title:e.target.value })}/>
      <input className="w-full border rounded p-3" placeholder="Slug (optional)" value={model.slug} onChange={e=>setModel({ ...model, slug:e.target.value })}/>
      <textarea className="w-full border rounded p-3 h-32" placeholder="Description" value={model.description} onChange={e=>setModel({ ...model, description:e.target.value })}/>
      <input className="w-full border rounded p-3" placeholder="Tech (comma-separated)" value={(model.tech||[]).join(', ')} onChange={e=>setModel({ ...model, tech:e.target.value.split(',').map(s=>s.trim()).filter(Boolean) })}/>
      <input className="w-full border rounded p-3" placeholder="Repo URL" value={model.repoUrl||''} onChange={e=>setModel({ ...model, repoUrl:e.target.value })}/>
      <input className="w-full border rounded p-3" placeholder="Live URL" value={model.liveUrl||''} onChange={e=>setModel({ ...model, liveUrl:e.target.value })}/>
      <input className="w-full border rounded p-3" placeholder="Highlights (comma-separated)" value={(model.highlights||[]).join(', ')} onChange={e=>setModel({ ...model, highlights:e.target.value.split(',').map(s=>s.trim()).filter(Boolean) })}/>
      <button className="px-5 py-2.5 rounded bg-blue-600 text-white">Save</button>
      {err && <p className="text-red-600">{err}</p>}
    </form>
  )
}
