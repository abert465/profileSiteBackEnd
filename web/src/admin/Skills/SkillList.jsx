import { useEffect, useState } from "react";
import {
  listSkillsAdmin,
  addSkillAdmin,
  updateSkillAdmin,
  deleteSkillAdmin,
  listSkillCategoriesAdmin,
} from "../../lib/adminApi";

export default function SkillsList() {
  const [rows, setRows] = useState([]);
  const [name, setName] = useState("");
  // Category drives the grouping on the public Skills section. Anything without
  // one renders under "Other" rather than disappearing.
  const [category, setCategory] = useState("");
  const [categories, setCategories] = useState([]);
  const [saving, setSaving] = useState(false);
  const [err, setErr] = useState("");
  const [loading, setLoading] = useState(true);

  async function load() {
    setLoading(true);
    setErr("");
    try {
      const data = await listSkillsAdmin();
      // accept array or {items:[...]}
      const arr = Array.isArray(data)
        ? data
        : Array.isArray(data?.items)
        ? data.items
        : [];
      setRows(arr);
      if (!Array.isArray(data))
        console.warn(
          "[Skills] API did not return an array. Using data.items fallback.",
          data
        );
    } catch (ex) {
      console.error("[Skills] load failed", ex);
      setErr(String(ex.message || ex));
      setRows([]);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
    listSkillCategoriesAdmin()
      .then((c) => setCategories(Array.isArray(c) ? c : []))
      .catch(() => setCategories([]));
  }, []);

  async function add(e) {
    e.preventDefault();
    if (!name.trim()) return;
    setSaving(true);
    setErr("");
    try {
      const created = await addSkillAdmin(
        name.trim(),
        true,
        rows.length,
        category || null
      );
      setRows((r) => [...r, created]);
      setName("");
    } catch (ex) {
      setErr(String(ex.message || ex));
    } finally {
      setSaving(false);
    }
  }

  async function toggle(skill) {
    const id = skill.id ?? skill.Id;

    // Normalize current values from either camelCase or PascalCase,
    // falling back to sortOrder if that's what your data uses.
    const name = skill.name ?? skill.Name ?? ""; // <- MUST be sent to avoid 400 "Name field is required"
    const currentVisible =
      (skill.isVisible ?? skill.IsVisible ?? false) === true;
    const order = Number.isFinite(skill.order ?? skill.Order)
      ? skill.order ?? skill.Order
      : typeof skill.sortOrder === "number"
      ? skill.sortOrder
      : 0;

    // Category is sent back unchanged. The API treats an omitted category as
    // "leave it alone", but sending it explicitly keeps this honest if that
    // ever changes.
    const currentCategory = skill.category ?? skill.Category ?? null;

    // Exact keys your DTO expects: Name, IsVisible, Order, Category
    const payload = {
      Name: name,
      IsVisible: !currentVisible,
      Order: order,
      Category: currentCategory,
    };

    try {
      const updated = await updateSkillAdmin(id, payload);

      // If the API returns the updated row, prefer it; otherwise apply our local changes.
      const next =
        updated && typeof updated === "object"
          ? {
              id: updated.id ?? updated.Id ?? id,
              name: updated.name ?? updated.Name ?? name,
              isVisible:
                updated.isVisible ?? updated.IsVisible ?? !currentVisible,
              order: updated.order ?? updated.Order ?? order,
              category: updated.category ?? updated.Category ?? currentCategory,
            }
          : {
              ...skill,
              name,
              isVisible: !currentVisible,
              order,
              category: currentCategory,
            };

      setRows((rows) => rows.map((r) => ((r.id ?? r.Id) === id ? next : r)));

      // Return what we sent/used (useful for debugging/tests/callers)
      return payload;
    } catch (ex) {
      setErr(String(ex.message || ex));
      throw ex;
    }
  }

  async function changeCategory(skill, next) {
    const id = skill.id ?? skill.Id;
    const payload = {
      Name: skill.name ?? skill.Name ?? "",
      IsVisible: (skill.isVisible ?? skill.IsVisible ?? true) === true,
      Order: skill.order ?? skill.Order ?? 0,
      // Empty string is the deliberate "clear it" signal; null would mean
      // "leave it alone" and the select would appear to do nothing.
      Category: next ?? "",
    };
    try {
      await updateSkillAdmin(id, payload);
      setRows((rows) =>
        rows.map((r) =>
          (r.id ?? r.Id) === id ? { ...r, category: next || null } : r
        )
      );
    } catch (ex) {
      setErr(String(ex.message || ex));
    }
  }

  async function remove(id) {
    if (!confirm("Delete this skill?")) return;
    try {
      await deleteSkillAdmin(id);
      setRows((r) => r.filter((x) => x.id !== id));
    } catch (ex) {
      setErr(String(ex.message || ex));
    }
  }
  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Skills</h1>

        {/* Inline add form (keeps flow similar to a "New" action) */}
        <form onSubmit={add} className="flex items-center gap-2">
          <input
            className="border rounded p-2"
            placeholder="Add a skill (e.g., .NET 8)"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
          <select
            className="border rounded p-2"
            value={category}
            onChange={(e) => setCategory(e.target.value)}
            aria-label="Category"
          >
            <option value="">No category</option>
            {categories.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </select>
          <button
            disabled={saving}
            className="rounded bg-blue-600 text-white px-4 py-2"
          >
            {saving ? "Adding..." : "Add"}
          </button>
        </form>
      </div>

      {err && <p className="text-red-600 mt-2">{err}</p>}
      {loading && <p className="text-sm text-zinc-500 mt-3">Loading skills…</p>}

      <table className="mt-4 w-full text-left border">
        <thead className="bg-gray-50 dark:bg-gray-900">
          <tr>
            <th className="p-2 border">Name</th>
            <th className="p-2 border w-56">Category</th>
            <th className="p-2 border w-40">Visibility</th>
            <th className="p-2 border w-40">Actions</th>
          </tr>
        </thead>
        <tbody>
          {!loading && rows.length === 0 ? (
            <tr>
              <td className="p-2 border text-sm text-zinc-500" colSpan={4}>
                No skills yet. Add your first one above.
              </td>
            </tr>
          ) : (
            rows.map((s) => (
              <tr key={s.id} className="border-t">
                <td className="p-2">{s.name}</td>
                <td className="p-2">
                  <select
                    className="border rounded p-1 text-sm w-full"
                    value={s.category ?? ""}
                    onChange={(e) => changeCategory(s, e.target.value)}
                    aria-label={`Category for ${s.name}`}
                  >
                    <option value="">Other</option>
                    {categories.map((c) => (
                      <option key={c} value={c}>
                        {c}
                      </option>
                    ))}
                  </select>
                </td>
                <td className="p-2">
                  <span
                    className={`text-xs px-2 py-0.5 rounded-full ${
                      s.isVisible
                        ? "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300"
                        : "bg-zinc-100 text-zinc-600 dark:bg-zinc-800"
                    }`}
                  >
                    {s.isVisible ? "Visible" : "Hidden"}
                  </span>
                </td>
                <td className="p-2 text-sm">
                  <button onClick={() => toggle(s)} className="underline mr-3">
                    {s.isVisible ? "Hide" : "Show"}
                  </button>
                  <button onClick={() => remove(s.id)} className="text-red-600">
                    Delete
                  </button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
