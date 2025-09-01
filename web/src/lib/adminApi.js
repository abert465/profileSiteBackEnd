const base = (import.meta.env.VITE_API_BASE || "").replace(/\/$/, "");
const prefix = base ? base : "";

function getCookieValue(cookieKey) {
  const rows = document.cookie.split("; ");
  const hit = rows.find((r) => r.startsWith(`${cookieKey}=`));
  return hit?.split("=")[1];
}

const csrfHeader = () => ({
  "X-CSRF-TOKEN": decodeURIComponent(getCookieValue("XSRF-TOKEN") || ""),
});

const handle = async (res) => {
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  return res.status === 204 ? null : res.json();
};

export const me = () =>
  fetch(`${prefix}/api/admin/auth/me`, { credentials: "include" }).then(handle);

export const login = (username, password) =>
  fetch(`${prefix}/api/admin/auth/login`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  }).then(handle);

export const logout = () =>
  fetch(`${prefix}/api/admin/auth/logout`, {
    method: "POST",
    credentials: "include",
  }).then(handle);

// Projects (admin)
export const listProjects = () =>
  fetch(`${prefix}/api/admin/projects`, { credentials: "include" }).then(
    handle
  );

export const saveProject = (p) =>
  fetch(`${prefix}/api/admin/projects`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json", ...csrfHeader() },
    body: JSON.stringify(p),
  }).then(handle);

export const updateProject = (slug, p) =>
  fetch(`${prefix}/api/admin/projects/${encodeURIComponent(slug)}`, {
    method: "PUT",
    credentials: "include",
    headers: { "Content-Type": "application/json", ...csrfHeader() },
    body: JSON.stringify(p),
  }).then(handle);

export const deleteProject = (slug) =>
  fetch(`${prefix}/api/admin/projects/${encodeURIComponent(slug)}`, {
    method: "DELETE",
    credentials: "include",
    headers: { ...csrfHeader() },
  }).then(handle);

export const listSkillsAdmin = () =>
  fetch(`${prefix}/api/admin/skills`, { credentials: "include" }).then(handle);

export const addSkillAdmin = (name, isVisible = true, order = null) =>
  fetch(`${prefix}/api/admin/skills`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json", ...csrfHeader() },
    body: JSON.stringify({ name, isVisible, order }),
  }).then(handle);

export const updateSkillAdmin = (id, patch) =>
  fetch(`${prefix}/api/admin/skills/${id}`, {
    method: "PUT",
    credentials: "include",
    headers: { "Content-Type": "application/json", ...csrfHeader() },
    body: JSON.stringify(patch),
  }).then(handle);

export const deleteSkillAdmin = (id) =>
  fetch(`${prefix}/api/admin/skills/${id}`, {
    method: "DELETE",
    credentials: "include",
    headers: { ...csrfHeader() },
  }).then(handle);

  export const listExperiencesAdmin = () =>
  fetch(`${prefix}/api/admin/experience`, {
    credentials: "include",
    headers: { ...csrfHeader() },
  }).then(handle);

// Body: full Experience object { Company, Role, Location, Start, End, Highlights[], Tech[] }
export const addExperienceAdmin = (payload) =>
  fetch(`${prefix}/api/admin/experience`, {
    method: "POST",
    credentials: "include",
    headers: { "Content-Type": "application/json", ...csrfHeader() },
    body: JSON.stringify(payload),
  }).then(handle);

// UPDATE *by index* (i), not by id
export const updateExperienceAdminByIndex = (i, payload) =>
  fetch(`${prefix}/api/admin/experience/${i}`, {
    method: "PUT",
    credentials: "include",
    headers: { "Content-Type": "application/json", ...csrfHeader() },
    body: JSON.stringify(payload),
  }).then(handle);

// DELETE *by index* (i), not by id
export const deleteExperienceAdminByIndex = (i) =>
  fetch(`${prefix}/api/admin/experience/${i}`, {
    method: "DELETE",
    credentials: "include",
    headers: { ...csrfHeader() },
  }).then(handle);
