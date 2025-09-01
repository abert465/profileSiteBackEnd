import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { listProjects, deleteProject } from "../../lib/adminApi";

export default function ProjectsList() {
  const [rows, setRows] = useState([]);
  const [error, setError] = useState("");

  async function load() {
    try {
      setRows(await listProjects());
    } catch {
      setError("Failed to load");
    }
  }
  useEffect(() => {
    load();
  }, []);

  async function remove(slug) {
    if (!confirm("Delete this project?")) return;
    await deleteProject(slug);
    setRows((rows) => rows.filter((r) => r.slug !== slug));
  }

  return (
    <div>
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Projects</h1>
        <Link
          to="/admin/projects/new"
          className="rounded bg-blue-600 text-white px-4 py-2"
        >
          New
        </Link>
      </div>
      {error && <p className="text-red-600 mt-2">{error}</p>}
      <table className="mt-4 w-full text-left border">
        <thead className="bg-gray-50 dark:bg-gray-900">
          <tr>
            <th className="p-2 border">Title</th>
            <th className="p-2 border">Slug</th>
            <th className="p-2 border w-32">Actions</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.slug} className="border-t">
              <td className="p-2">{r.title}</td>
              <td className="p-2 text-xs text-gray-500">{r.slug}</td>
              <td className="p-2 text-sm">
                <Link
                  to={`/admin/projects/${r.slug}`}
                  className="underline mr-3"
                >
                  Edit
                </Link>
                <button onClick={() => remove(r.slug)} className="text-red-600">
                  Delete
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
