export default function Background(){
  return (
    <div aria-hidden className="pointer-events-none fixed inset-0 -z-10">
      {/* Soft gradient blobs */}
      <div className="absolute -top-24 -left-24 h-72 w-72 rounded-full bg-blue-500/30 blur-3xl" />
      <div className="absolute top-1/3 -right-24 h-80 w-80 rounded-full bg-indigo-500/20 blur-3xl" />
      <div className="absolute bottom-0 left-1/2 h-64 w-64 -translate-x-1/2 rounded-full bg-sky-400/20 blur-3xl" />
      {/* Tiny dot grid overlay */}
      <div className="absolute inset-0 bg-[radial-gradient(transparent_1px,rgba(0,0,0,0.03)_1px)] [background-size:20px_20px] dark:bg-[radial-gradient(transparent_1px,rgba(255,255,255,0.04)_1px)]" />
    </div>
  )
}