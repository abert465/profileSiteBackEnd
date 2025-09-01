import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { addExperienceAdmin } from "../../lib/adminApi"; // adjust path if needed

const toDateOnly = (v) => (v ? String(v).slice(0, 10) : null);

export default function ExperienceForm() {
  const nav = useNavigate();
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

  async function onSubmit(e) {
    e.preventDefault();
    setErr("");
    if (!form.company.trim() || !form.role.trim()) {
      setErr("Company and Role are required.");
      return;
    }
    setSaving(true);
    try {
      const payload = {
        Company: form.company.trim(),
        Role: form.role.trim(),
        Location: form.location.trim(),
        Start: toDateOnly(form.start),
        End: form.end ? toDateOnly(form.end) : null,
        Tech: form.techText.split(",").map(s => s.trim()).filter(Boolean),
        Highlights: form.highlightsText.split(/\r?\n/).map(s => s.trim()).filter(Boolean),
      };
      await addExperienceAdmin(payload);
      nav("/admin/experience");
    } catch (ex) {
      setErr(String(ex.message || ex));
    } finally {
      setSaving(false);
    }
  }

  return (
    <div>
      <h1 className="text-2xl font-bold">New Experience</h1>
      {err && <p className="text-red-600 mt-2">{err}</p>}

      <form onSubmit={onSubmit} className="mt-4 space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
          <input className="border rounded p-2" placeholder="Company"
                 value={form.company}
                 onChange={(e)=>setForm(f=>({...f,company:e.target.value}))}/>
          <input className="border rounded p-2" placeholder="Role"
                 value={form.role}
                 onChange={(e)=>setForm(f=>({...f,role:e.target.value}))}/>
          <input className="border rounded p-2" placeholder="Location"
                 value={form.location}
                 onChange={(e)=>setForm(f=>({...f,location:e.target.value}))}/>
          <div className="flex gap-2">
            <input type="date" className="border rounded p-2 flex-1"
                   value={form.start}
                   onChange={(e)=>setForm(f=>({...f,start:e.target.value}))}/>
            <input type="date" className="border rounded p-2 flex-1"
                   value={form.end}
                   onChange={(e)=>setForm(f=>({...f,end:e.target.value}))}/>
          </div>
          <input className="border rounded p-2 md:col-span-2" placeholder="Tech (comma separated)"
                 value={form.techText}
                 onChange={(e)=>setForm(f=>({...f,techText:e.target.value}))}/>
        </div>

        <div>
          <label className="text-xs text-zinc-500 block">Highlights (one per line)</label>
          <textarea className="border rounded p-2 w-full mt-1" rows={6}
                    placeholder={`Led migration to .NET 8\nCut Azure spend by 20%\nBuilt CI/CD with Azure Pipelines`}
                    value={form.highlightsText}
                    onChange={(e)=>setForm(f=>({...f,highlightsText:e.target.value}))}/>
        </div>

        <div className="flex gap-2">
          <button disabled={saving} className="rounded bg-blue-600 text-white px-4 py-2">
            {saving ? "Saving..." : "Save"}
          </button>
          <button type="button" onClick={()=>nav("/admin/experience")} className="px-4 py-2 border rounded">
            Cancel
          </button>
        </div>
      </form>
    </div>
  );
}
