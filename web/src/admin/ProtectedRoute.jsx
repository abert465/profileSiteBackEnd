import { Navigate, Outlet } from 'react-router-dom'
export default function ProtectedRoute({ authed }){
  if (authed === null) return null // splash/spinner if you want
  return authed ? <Outlet/> : <Navigate to="/admin/login" replace />
}