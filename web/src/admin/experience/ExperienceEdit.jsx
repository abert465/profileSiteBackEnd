import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams, Link } from "react-router-dom";
import {
  listExperiencesAdmin,
  updateExperienceAdminByIndex,
  deleteExperienceAdminByIndex,
} from "../../lib/adminApi";

// helpers
const toInputDate = (v) => (v ? String(v).slice(0, 10) : "");
const toDateOnly = (v) => (v ? String(v).slice(0, 10) : null);
const linesToArray = (text) => text.split(/\r?\n/).map(s => s.trim()).filter(Boolean);
const commaToArray = (text) => text.split(",").map(s => s.trim()).filter(Boolean);

export default function ExperienceEdit() {
  const { index } = useParams();
  const i = useMemo(() => {
    const n = Number.parseInt(index, 10);
    return Number.isFinite(n) && n >= 0 ? n : null;
  }, [index]);

  const nav = useNavigate();
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");
  const [saving, setSaving] = useState(false);

  const [form, setForm] = useState({
    company: "",
    role: "",
    location: "",
    start: "",
    end: "",
    techText: "",
    highlightsText: "",
  });

  async function load() {
    if (i === null) {
      setErr("Invalid experience index.");
      setLoading(false);
      return;
    }
    setLoading(true);
    setErr("");
    try {
      const data = await listExperiencesAdmin();
      const arr = Array.isArray(data) ? data : (Array.isArray(data?.items) ? data.items : []);
      const row = arr[i];
      if (!row) {
        setErr(`No experience found at index ${i}.`);
      } else {
        setForm({
          company: row.Company ?? row.company ?? "",
          role: row.Role ?? row.role ?? "",
          location: row.Location ?? row.location ?? "",
          start: toInputDate(row.Start ?? row.start),
          end: toInputDate(row.End ?? row.end),
          techText: Array.isArray(row.Tech) ? row.Tech.join(", ") : (row.tech ?? ""),
          highlightsText: Array.isArray(row.Highlights) ? row.Highlights.join("\n") : (row.highlights ?? ""),
        });
      }
    } catch (ex) {
      setErr(String(ex.message || ex));
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { load(); /* eslint-disable-next-line */ }, [i]);

  async function onSave(e) {
    e.preventDefault();
    if (!form.company.trim() || !form.role.trim()) {
      setErr("Company and Role are required.");
      return;
    }
    setSaving(true);
    setErr("");
    try {
      const payload = {
        Company: form.company.trim(),
        Role: form.role.trim(),
        Location: form.location.trim(),
        Start: toDateOnly(form.start),
        End: form.end ? toDateOnly(form.end) : null,
        Tech: commaToArray(form.techText),
        Highlights: linesToArray(form.highlightsText),
      };
      await updateExperienceAdminByIndex(i, payload);
      nav("/admin/experience");
    } catch (ex) {
      setErr(String(ex.message || ex));
    } finally {
      setSaving(false);
    }
  }

  async function onDelete() {
    if (!confirm("Delete this experience?")) return;
    try {
      await deleteExperienceAdminByIndex(i);
      nav("/admin/experience");
    } catch (ex) {
      setErr(String(ex.message || ex));
    }
  }

  if (loading) {
    return (
      <div>
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold">Edit Experience</h1>
          <Link to="/admin/experience" className="underline">Back to list</Link>
        </div>
        <p className="text-sm text-zinc-500 mt-3">Loading…</p>
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Edit Experience</h1>
        <Link to="/admin/experience" className="underline">Back to list</Link>
      </div>

      {err && <p className="text-red-600 mt-2">{err}</p>}

      {!err && (
        <form onSubmit={onSave} className="mt-4 space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
            <input
              className="border rounded p-2"
              placeholder="Company"
              value={form.company}
              onChange={(e) => setForm((f) => ({ ...f, company: e.target.value }))}
            />
            <input
              className="border rounded p-2"
              placeholder="Role"
              value={form.role}
              onChange={(e) => setForm((f) => ({ ...f, role: e.target.value }))}
            />
            <input
              className="border rounded p-2"
              placeholder="Location"
              value={form.location}
              onChange={(e) => setForm((f) => ({ ...f, location: e.target.value }))}
            />
            <div className="flex gap-2">
              <input
                type="date"
                className="border rounded p-2 flex-1"
                value={form.start}
                onChange={(e) => setForm((f) => ({ ...f, start: e.target.value }))}
              />
              <input
                type="date"
                className="border rounded p-2 flex-1"
                value={form.end}
                onChange={(e) => setForm((f) => ({ ...f, end: e.target.value }))}
              />
            </div>
            <input
              className="border rounded p-2 md:col-span-2"
              placeholder="Tech (comma separated)"
              value={form.techText}
              onChange={(e) => setForm((f) => ({ ...f, techText: e.target.value }))}
            />
          </div>

          <div>
            <label className="text-xs text-zinc-500 block">Highlights (one per line)</label>
            <textarea
              className="border rounded p-2 w-full mt-1"
              rows={6}
              value={form.highlightsText}
              onChange={(e) => setForm((f) => ({ ...f, highlightsText: e.target.value }))}
            />
          </div>

          <div className="flex gap-2">
            <button disabled={saving} className="rounded bg-blue-600 text-white px-4 py-2">
              {saving ? "Saving..." : "Save"}
            </button>
            <button
              type="button"
              onClick={() => nav("/admin/experience")}
              className="px-4 py-2 border rounded"
            >
              Cancel
            </button>
            <button
              type="button"
              onClick={onDelete}
              className="px-4 py-2 border rounded text-red-600"
            >
              Delete
            </button>
          </div>
        </form>
      )}
    </div>
  );
}
