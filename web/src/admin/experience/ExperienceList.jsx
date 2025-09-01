import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  listExperiencesAdmin,
  deleteExperienceAdminByIndex,
} from "/src/lib/adminApi"; // adjust if your path differs

const toDate = (v) => (v ? new Date(v).toLocaleDateString() : null);

export default function ExperienceList() {
  const [rows, setRows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  async function load() {
    setLoading(true);
    setErr("");
    try {
      const data = await listExperiencesAdmin();
      const arr = Array.isArray(data) ? data : (Array.isArray(data?.items) ? data.items : []);
      // Keep server order so indexes line up with PUT/DELETE-by-index
      setRows(arr);
    } catch (ex) {
      setErr(String(ex.message || ex));
      setRows([]);
    } finally {
      setLoading(false);
    }
  }
  useEffect(() => { load(); }, []);

  async function remove(i) {
    if (!confirm("Delete this experience?")) return;
    try {
      await deleteExperienceAdminByIndex(i);
      await load(); // indices shift after delete
    } catch (ex) {
      setErr(String(ex.message || ex));
    }
  }

  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Experience</h1>
        <Link to="/admin/experience/new" className="rounded bg-blue-600 text-white px-4 py-2">
          New
        </Link>
      </div>

      {err && <p className="text-red-600 mt-2">{err}</p>}
      {loading && <p className="text-sm text-zinc-500 mt-3">Loading experience…</p>}

      <table className="mt-4 w-full text-left border">
        <thead className="bg-gray-50 dark:bg-gray-900">
          <tr>
            <th className="p-2 border w-8">#</th>
            <th className="p-2 border">Company</th>
            <th className="p-2 border">Role</th>
            <th className="p-2 border">Dates</th>
            <th className="p-2 border">Location</th>
            <th className="p-2 border">Tech</th>
            <th className="p-2 border">Highlights</th>
            <th className="p-2 border w-40">Actions</th>
          </tr>
        </thead>
        <tbody>
          {!loading && rows.length === 0 ? (
            <tr>
              <td className="p-2 border text-sm text-zinc-500" colSpan={8}>
                No experience yet. Click <span className="font-medium">New</span> to add one.
              </td>
            </tr>
          ) : (
            rows.map((r, i) => {
              const company = r.Company ?? r.company;
              const role = r.Role ?? r.role;
              const loc = r.Location ?? r.location;
              const start = r.Start ?? r.start;
              const end = r.End ?? r.end;
              const techArr = Array.isArray(r.Tech) ? r.Tech : (r.tech ? String(r.tech).split(",").map(s=>s.trim()) : []);
              const highlightsArr = Array.isArray(r.Highlights) ? r.Highlights : [];

              return (
                <tr key={i} className="border-t align-top">
                  <td className="p-2 border text-sm">{i}</td>
                  <td className="p-2 border">{company}</td>
                  <td className="p-2 border">{role}</td>
                  <td className="p-2 border text-sm">
                    {(toDate(start) ?? "—")} to {(toDate(end) ?? "Present")}
                  </td>
                  <td className="p-2 border">{loc}</td>
                  <td className="p-2 border text-sm">
                    {techArr.length ? techArr.join(", ") : "—"}
                  </td>
                  <td className="p-2 border">
                    {highlightsArr.length ? (
                      <ul className="list-disc pl-5 text-sm space-y-1">
                        {highlightsArr.map((h, idx) => (
                          <li key={idx}>{h}</li>
                        ))}
                      </ul>
                    ) : (
                      <span className="text-sm text-zinc-500">—</span>
                    )}
                  </td>
                  <td className="p-2 border text-sm">
                    <Link to={`/admin/experience/${i}`} className="underline mr-3">
                      Edit
                    </Link>
                    <button onClick={() => remove(i)} className="text-red-600">
                      Delete
                    </button>
                  </td>
                </tr>
              );
            })
          )}
        </tbody>
      </table>
    </div>
  );
}
