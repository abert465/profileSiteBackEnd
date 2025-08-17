export default function Footer(){
  return (
    <footer className="py-8 border-t mt-16 dark:border-gray-800">
      <div className="max-w-6xl mx-auto px-4 flex items-center justify-between text-sm text-gray-600 dark:text-gray-400">
        <p>© {new Date().getFullYear()} Albert Campos</p>
        <div className="flex gap-4">
          <a href="https://github.com/albert465" target="_blank">GitHub</a>
          <a href="https://www.linkedin.com/in/albertcampos" target="_blank">LinkedIn</a>
        </div>
      </div>
    </footer>
  )
}