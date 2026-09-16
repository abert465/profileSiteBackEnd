// Sets the initial theme class before the stylesheet paints, so a dark-mode
// visitor does not get a white flash on every load.
//
// This lives in a file rather than inline in index.html so that the
// Content-Security-Policy in Program.cs can keep script-src at 'self'. Inlining
// it would force either 'unsafe-inline' - which gives up most of what the policy
// buys - or a sha256 hash that silently stops matching the moment anyone edits
// these lines, with nothing but a console error to say so.
//
// Loaded as a blocking classic script in <head>: it must finish before first
// paint, which is the whole point.
(function () {
  try {
    var ls = localStorage.getItem('theme');
    var systemDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    var isDark = ls ? ls === 'dark' : systemDark;
    document.documentElement.classList.toggle('dark', isDark);
  } catch (e) { /* no-op */ }
})();
