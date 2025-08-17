// Prefer relative "/api" during dev so Vite proxy handles CORS.
// In production, set VITE_API_BASE (e.g., https://api.yourdomain.com).
const base = (import.meta.env.VITE_API_BASE || '').replace(/\/$/, '')
const prefix = base ? base : ''

async function handle(res) {
  if (!res.ok) {
    const text = await res.text().catch(() => '')
    throw new Error(`HTTP ${res.status} ${res.statusText} — ${text}`)
  }
  // Some endpoints might return 204
  return res.status === 204 ? null : res.json()
}

// ----- Public site endpoints (no auth/cookies needed) -----
export function getProfile()       { return fetch(`${prefix}/api/profile`).then(handle) }
export function getProjects()      { return fetch(`${prefix}/api/projects`).then(handle) }
export function getPosts()         { return fetch(`${prefix}/api/blog`).then(handle) }
export function getExperience()    { return fetch(`${prefix}/api/experience`).then(handle) }
export function getEducation()     { return fetch(`${prefix}/api/education`).then(handle) }
export function getCertifications(){ return fetch(`${prefix}/api/certifications`).then(handle) }

export function sendContact(payload) {
  return fetch(`${prefix}/api/contact`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  }).then(handle)
}

// ----- Passcode gate (requires cookies) -----
export async function checkPasscode() {
  const res = await fetch(`${prefix}/api/passcode/check`, { credentials: 'include' })
  return res.ok
}

export async function loginPasscode(code) {
  const res = await fetch(`${prefix}/api/passcode/login`, {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ code }),
  })
  if (!res.ok) throw new Error('Invalid passcode')
  return true
}

export async function logoutPasscode() {
  await fetch(`${prefix}/api/passcode/logout`, { method: 'POST', credentials: 'include' })
}

// ----- Optional dev-only helper (backend endpoint not implemented yet) -----
// export function importResume(json) {
//   return fetch(`${prefix}/api/import`, {
//     method: 'POST',
//     headers: { 'Content-Type': 'application/json' },
//     body: JSON.stringify(json),
//   }).then(handle)
// }