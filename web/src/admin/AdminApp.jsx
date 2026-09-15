import { Routes, Route } from "react-router-dom";
import { useEffect, useState } from "react";
import { me } from "../lib/adminApi";
import ProtectedRoute from "./ProtectedRoute";
import AdminLayout from "./AdminLayout";
import Login from "/src/admin/Login";
import Dashboard from "/src/dashboard/Dashboard.jsx";
import ProjectsList from "/src/admin/projects/ProjectsList.jsx";
import ProjectForm from "/src/admin/projects/ProjectForm.jsx";
import SkillsList from "/src/admin/skills/SkillList.jsx";
import ExperienceList from "/src/admin/experience/ExperienceList.jsx";
import ExperienceForm from "/src/admin/experience/ExperienceForm.jsx";
import ExperienceEdit from "/src/admin/experience/ExperienceEdit.jsx";
import EducationList from "/src/admin/education/EducationList.jsx";
import EducationForm from "/src/admin/education/EducationForm.jsx";
import CertsList from "/src/admin/cert/CertsList.jsx";
import CertForm from "/src/admin/cert/CertForm.jsx";

export default function AdminApp() {
  const [authed, setAuthed] = useState(null); // null = checking, false = not authed, true = authed

  useEffect(() => {
    (async () => {
      try {
        await me();
        setAuthed(true);
      } catch {
        setAuthed(false);
      }
    })();
  }, []);

  return (
    <Routes>
      {/* /admin/login */}
      <Route path="login" element={<Login />} />

      {/* Everything else under /admin/* requires auth */}
      <Route element={<ProtectedRoute authed={authed} />}>
        <Route element={<AdminLayout />}>
          <Route index element={<Dashboard />} />
          <Route path="projects" element={<ProjectsList />} />
          <Route path="projects/new" element={<ProjectForm />} />
          <Route path="projects/:slug" element={<ProjectForm />} />
          <Route path="experience" element={<ExperienceList />} />
          <Route path="experience/new" element={<ExperienceForm />} />
          <Route path="experience/:index" element={<ExperienceEdit />} />
          <Route path="skills" element={<SkillsList />} />
          <Route path="education" element={<EducationList />} />
          <Route path="education/new" element={<EducationForm />} />
          <Route path="education/:i" element={<EducationForm />} />
          <Route path="certifications" element={<CertsList />} />
          <Route path="certifications/new" element={<CertForm />} />
          <Route path="certifications/:i" element={<CertForm />} />
        </Route>
      </Route>
    </Routes>
  );
}
