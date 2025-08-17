const base = (import.meta.env.VITE_API_BASE || '').replace(/\/$/, '')
const prefix = base ? base : ''

function getCookieValue(cookieKey) {
    const rows = document.cookie.split('; ')
    const hit = rows.find( r => r.startsWith(`${cookieKey}=`))
    return hit?.split('=')[1]
}

const csrfHeader = () => ({'X-CSRF-TOKEN' : decodeURIComponent(getCookieValue('XSRF-TOKEN') || '')})

const handle = async (res) => {
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
        return res.status === 204 ? null : res.json()
}

export const me = () => fetch(`${prefix}/api/admin/me`, { credentials:'include' }).then(handle)

export const login = (username, password) => fetch(`${prefix}/api/admin/login`, {
  method:'POST', credentials:'include', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ username, password })
}).then(handle)

export const logout = () => fetch(`${prefix}/api/admin/logout`, { method:'POST', credentials:'include' }).then(handle)

// Projects (admin)
export const listProjects = () => fetch(`${prefix}/api/admin/projects`, { credentials:'include' }).then(handle)

export const saveProject = (p) => fetch(`${prefix}/api/admin/projects`, {
  method:'POST', credentials:'include', headers:{ 'Content-Type':'application/json', ...csrfHeader() }, body: JSON.stringify(p)
}).then(handle)

export const updateProject = (slug, p) => fetch(`${prefix}/api/admin/projects/${encodeURIComponent(slug)}`, {
  method:'PUT', credentials:'include', headers:{ 'Content-Type':'application/json', ...csrfHeader() }, body: JSON.stringify(p)
}).then(handle)

export const deleteProject = (slug) => fetch(`${prefix}/api/admin/projects/${encodeURIComponent(slug)}`, {
  method:'DELETE', credentials:'include', headers:{ ...csrfHeader() }
}).then(handle)