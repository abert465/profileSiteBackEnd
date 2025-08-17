import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import './styles.css'
import App from './App.jsx'
import AdminApp from './admin/AdminApp.jsx'

createRoot(document.getElementById('root')).render(
  <StrictMode>
    <BrowserRouter>
      <Routes>
        <Route path="/*" element={<App/>} />
        <Route path="/admin/*" element={<AdminApp/>} />
      </Routes>
    </BrowserRouter>
  </StrictMode>
)
