import { Routes, Route } from 'react-router-dom'
import { useEffect, useState } from 'react'
import { me } from '../lib/adminApi'
import ProtectedRoute from './ProtectedRoute'
import AdminLayout from './AdminLayout'
import Login from '/src/admin/Login'
import Dashboard from '/src/dashboard/Dashboard.jsx'
import ProjectsList from '/src/admin/projects/ProjectsList.jsx'
import ProjectForm from '/src/admin/projects/ProjectForm.jsx'

export default function AdminApp(){
  const [authed, setAuthed] = useState(null) // null = checking, false = not authed, true = authed

  useEffect(() => {
    (async () => { try { await me(); setAuthed(true) } catch { setAuthed(false) } })()
  }, [])

  return (
    <Routes>
      {/* /admin/login */}
      <Route path="login" element={<Login/>} />

      {/* Everything else under /admin/* requires auth */}
      <Route element={<ProtectedRoute authed={authed}/> }>
        <Route element={<AdminLayout/>}>
          {/* /admin */}
          <Route index element={<Dashboard/>} />
          {/* /admin/projects */}
          <Route path="projects" element={<ProjectsList/>} />
          {/* /admin/projects/new */}
          <Route path="projects/new" element={<ProjectForm/>} />
          {/* /admin/projects/:slug */}
          <Route path="projects/:slug" element={<ProjectForm/>} />
        </Route>
      </Route>
    </Routes>
  )
}