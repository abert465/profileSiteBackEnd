import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { listProjects, saveProject, updateProject, uploadProjectImage, deleteProjectImage } from '../../lib/adminApi'

export default function ProjectForm(){
  const { slug } = useParams()
  const nav = useNavigate()
  const editing = Boolean(slug)
  const [model, setModel] = useState({ title:'', description:'', slug:'', tech:[], repoUrl:'', liveUrl:'', highlights:[], imageUrl:'' })
  const [techInput, setTechInput] = useState('')
  const [highlightsInput, setHighlightsInput] = useState('')
  const [imageFile, setImageFile] = useState(null)
  const [imagePreview, setImagePreview] = useState('')
  const [uploading, setUploading] = useState(false)
  const [err, setErr] = useState('')

  useEffect(() => {
    if (!editing) return
    ;(async () => {
      const all = await listProjects()
      const p = all.find(x => x.slug === slug)
      if (p) {
        setModel(p)
        if (p.imageUrl) setImagePreview(p.imageUrl)
      }
    })()
  }, [editing, slug])

  function handleImageChange(e) {
    const file = e.target.files?.[0]
    if (!file) return

    if (file.size > 5 * 1024 * 1024) {
      setErr('Image must be less than 5MB')
      return
    }

    const validTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/webp', 'image/gif']
    if (!validTypes.includes(file.type)) {
      setErr('Image must be JPG, PNG, WebP, or GIF')
      return
    }

    setImageFile(file)
    setErr('')

    const reader = new FileReader()
    reader.onloadend = () => {
      setImagePreview(reader.result)
    }
    reader.readAsDataURL(file)
  }

  async function handleDeleteImage() {
    if (!editing || !model.slug) return
    try {
      setUploading(true)
      await deleteProjectImage(model.slug)
      setModel({ ...model, imageUrl: '' })
      setImagePreview('')
      setImageFile(null)
      setErr('')
    } catch (error) {
      setErr('Failed to delete image')
    } finally {
      setUploading(false)
    }
  }

  async function submit(e){
    e.preventDefault()
    setErr('')
    try{
      let projectSlug = slug

      if (editing) {
        await updateProject(slug, model)
      } else {
        const result = await saveProject(model)
        projectSlug = result.slug
      }

      console.log('About to upload image:', { imageFile, projectSlug, hasFile: !!imageFile })

      if (imageFile && projectSlug) {
        setUploading(true)
        try {
          console.log('Uploading image to:', projectSlug)
          const result = await uploadProjectImage(projectSlug, imageFile)
          console.log('Upload result:', result)
        } catch (error) {
          console.error('Upload error:', error)
          setErr('Project saved but image upload failed')
          setUploading(false)
          return
        }
        setUploading(false)
      } else {
        console.log('Skipping image upload - missing imageFile or projectSlug')
      }

      nav('/admin/projects')
    } catch (error) {
      setErr('Save failed')
    }
  }

  return (
    <form onSubmit={submit} className="space-y-3 max-w-2xl">
      <h1 className="text-2xl font-bold">{editing ? 'Edit' : 'New'} Project</h1>
      <input className="w-full border rounded p-3" placeholder="Title" value={model.title} onChange={e=>setModel({ ...model, title:e.target.value })}/>
      <input className="w-full border rounded p-3" placeholder="Slug (optional)" value={model.slug} onChange={e=>setModel({ ...model, slug:e.target.value })}/>
      <textarea className="w-full border rounded p-3 h-32" placeholder="Description" value={model.description} onChange={e=>setModel({ ...model, description:e.target.value })}/>

      {/* Tech Stack with Tags */}
      <div>
        <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">Tech Stack</label>
        <input
          type="text"
          className="w-full border rounded p-3"
          placeholder="Type tech and press Enter or comma to add"
          value={techInput}
          onChange={e=>setTechInput(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter' || e.key === ',') {
              e.preventDefault()
              const trimmed = techInput.trim()
              if (trimmed && !(model.tech||[]).includes(trimmed)) {
                setModel({ ...model, tech: [...(model.tech||[]), trimmed] })
                setTechInput('')
              }
            } else if (e.key === 'Backspace' && techInput === '' && (model.tech||[]).length > 0) {
              setModel({ ...model, tech: model.tech.slice(0, -1) })
            }
          }}
          onBlur={() => {
            const trimmed = techInput.trim()
            if (trimmed && !(model.tech||[]).includes(trimmed)) {
              setModel({ ...model, tech: [...(model.tech||[]), trimmed] })
              setTechInput('')
            }
          }}
        />
        <div className="flex flex-wrap gap-2 mt-2">
          {(model.tech||[]).map((t, idx) => (
            <span key={idx} className="inline-flex items-center gap-1 px-3 py-1 bg-blue-100 dark:bg-blue-900 text-blue-800 dark:text-blue-100 rounded-full text-sm">
              {t}
              <button type="button" onClick={() => setModel({ ...model, tech: model.tech.filter((_, i) => i !== idx) })} className="hover:text-red-600 font-bold">×</button>
            </span>
          ))}
        </div>
      </div>

      <input className="w-full border rounded p-3" placeholder="Repo URL" value={model.repoUrl||''} onChange={e=>setModel({ ...model, repoUrl:e.target.value })}/>
      <input className="w-full border rounded p-3" placeholder="Live URL" value={model.liveUrl||''} onChange={e=>setModel({ ...model, liveUrl:e.target.value })}/>

      {/* Project Image with File Upload */}
      <div className="space-y-2">
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Project Image</label>
        <input
          type="file"
          name="projectImage"
          accept="image/jpeg,image/jpg,image/png,image/webp,image/gif"
          onChange={handleImageChange}
          className="w-full border rounded p-3 dark:bg-gray-800 dark:text-gray-200"
          disabled={uploading}
        />
        <p className="text-sm text-gray-500 dark:text-gray-400">Max 5MB. Formats: JPG, PNG, WebP, GIF. Image will be automatically resized to 1200x800px.</p>

        {imagePreview && (
          <div className="mt-3 space-y-2">
            <img
              src={imagePreview}
              alt="Preview"
              className="max-w-md border rounded shadow-sm"
            />
            {editing && model.imageUrl && (
              <button
                type="button"
                onClick={handleDeleteImage}
                disabled={uploading}
                className="px-4 py-2 rounded bg-red-600 text-white text-sm disabled:opacity-50"
              >
                {uploading ? 'Deleting...' : 'Delete Image'}
              </button>
            )}
          </div>
        )}
      </div>

      {/* Highlights with Tags */}
      <div>
        <label className="block text-sm font-medium mb-1 text-gray-700 dark:text-gray-300">Highlights</label>
        <input
          type="text"
          className="w-full border rounded p-3"
          placeholder="Type highlight and press Enter to add"
          value={highlightsInput}
          onChange={e=>setHighlightsInput(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') {
              e.preventDefault()
              const trimmed = highlightsInput.trim()
              if (trimmed && !(model.highlights||[]).includes(trimmed)) {
                setModel({ ...model, highlights: [...(model.highlights||[]), trimmed] })
                setHighlightsInput('')
              }
            } else if (e.key === 'Backspace' && highlightsInput === '' && (model.highlights||[]).length > 0) {
              setModel({ ...model, highlights: model.highlights.slice(0, -1) })
            }
          }}
          onBlur={() => {
            const trimmed = highlightsInput.trim()
            if (trimmed && !(model.highlights||[]).includes(trimmed)) {
              setModel({ ...model, highlights: [...(model.highlights||[]), trimmed] })
              setHighlightsInput('')
            }
          }}
        />
        <div className="flex flex-col gap-2 mt-2">
          {(model.highlights||[]).map((h, idx) => (
            <div key={idx} className="flex items-center gap-2 p-2 bg-gray-100 dark:bg-gray-800 rounded">
              <span className="flex-1">{h}</span>
              <button type="button" onClick={() => setModel({ ...model, highlights: model.highlights.filter((_, i) => i !== idx) })} className="text-red-600 hover:text-red-700 font-bold">×</button>
            </div>
          ))}
        </div>
      </div>

      <button
        type="submit"
        disabled={uploading}
        className="px-5 py-2.5 rounded bg-blue-600 text-white disabled:opacity-50"
      >
        {uploading ? 'Uploading...' : 'Save'}
      </button>
      {err && <p className="text-red-600">{err}</p>}
    </form>
  )
}
