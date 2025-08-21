import { Link, Outlet, useLocation } from 'react-router-dom'

export default function AdminLayout(){
  const items = [
  ['Dashboard','/admin'],
  // ['Profile','/admin/profile'],
  ['Projects','/admin/projects'],
  // ['Experience','/admin/experience'],
  // ['Education','/admin/education'],
  // ['Certifications','/admin/certifications'],
  // ['Posts','/admin/posts'],
  ['Skills','/admin/skills'],
  ]
  const { pathname } = useLocation()
  return (
    <div className="min-h-screen grid grid-cols-[220px_1fr] dark:bg-gray-950">
      <aside className="border-r p-4 dark:border-gray-800">
        <div className="font-bold mb-4">Admin</div>
        <nav className="space-y-1">
          {items.map(([label, href]) => (
            <Link key={href} to={href} className={`block rounded px-3 py-2 hover:bg-gray-100 dark:hover:bg-gray-900 ${pathname===href?'bg-gray-100 dark:bg-gray-900':''}`}>{label}</Link>
          ))}
        </nav>
      </aside>
      <main className="p-6">
        <Outlet/>
      </main>
    </div>
  )
}