// Capture the project card screenshots that ship in wwwroot/uploads/projects.
//
//   node deploy/capture-project-shots.mjs
//
// Every target below is a page any member of the public can load without signing
// in. Two of them are the sign-in pages of client systems: that is deliberate and
// it is the boundary — the login screen is exactly what an anonymous visitor sees
// at that URL, so nothing here discloses data, names, or anything behind auth.
// Do not point this at an authenticated page.
//
// Cards render the image at h-32 (128px) with object-cover, so anything taller
// than a wide band is cropped away unseen. Lay the page out at desktop width so
// it renders as intended, then clip to a 2.5:1 band and halve the raster with
// deviceScaleFactor - a full 1440x900 PNG is ~600KB of mostly discarded pixels.
// JPEG rather than PNG because every one of these is photographic.
//
// Uses puppeteer-core against installed Chrome or Edge, same as smoke-test.mjs.

import { createRequire } from 'node:module';
import { existsSync, mkdirSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, join } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const repo = dirname(here);

const require = createRequire(join(repo, 'web', 'package.json'));
let puppeteer;
try {
  puppeteer = require('puppeteer-core');
}
catch {
  console.error('puppeteer-core not installed. Run: npm --prefix web ci');
  process.exit(1);
}

const browsers = [
  'C:/Program Files/Google/Chrome/Application/chrome.exe',
  'C:/Program Files (x86)/Google/Chrome/Application/chrome.exe',
  'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
  'C:/Program Files/Microsoft/Edge/Application/msedge.exe',
];
const executablePath = browsers.find(p => existsSync(p));
if (!executablePath) {
  console.error(`No Chrome or Edge found. Looked in:\n  ${browsers.join('\n  ')}`);
  process.exit(1);
}

const outDir = join(repo, 'profileSiteBackEnd', 'wwwroot', 'uploads', 'projects');
mkdirSync(outDir, { recursive: true });

const targets = [
  { slug: 'police-overtime-scheduling', url: 'https://bostonot.extradutysolutions.com/account/login' },
  { slug: 'naas-nexus-work-orders',     url: 'https://nexus.naasllc.com/account/login' },
  { slug: 'automated-expunction-engine', url: 'https://www.easyexpunctions.com/' },
  { slug: 'developer-portfolio-platform', url: 'https://tedko.dev' },
];

const browser = await puppeteer.launch({
  executablePath,
  headless: true,
  args: ['--no-sandbox', '--disable-dev-shm-usage'],
});

const results = [];

try {
  for (const { slug, url } of targets) {
    const page = await browser.newPage();
    await page.setViewport({ width: 1440, height: 900, deviceScaleFactor: 0.5 });
    try {
      // networkidle2 rather than load: Blazor Server wires up over a circuit
      // after the document settles, and the pre-circuit paint is unstyled.
      const response = await page.goto(url, { waitUntil: 'networkidle2', timeout: 45000 });
      const status = response?.status() ?? 0;

      // Reveal animations elsewhere on these sites are IntersectionObserver
      // driven; a beat after idle avoids catching content at opacity 0.
      await new Promise(r => setTimeout(r, 1500));

      const path = join(outDir, `${slug}.jpg`);
      await page.screenshot({
        path,
        type: 'jpeg',
        quality: 82,
        clip: { x: 0, y: 0, width: 1440, height: 576 },
      });
      results.push({ slug, status, path, ok: status > 0 && status < 400 });
    }
    catch (err) {
      results.push({ slug, status: 0, ok: false, error: err.message });
    }
    finally {
      await page.close();
    }
  }
}
finally {
  await browser.close();
}

for (const r of results) {
  console.log(r.ok ? `ok   ${r.slug} (${r.status})` : `FAIL ${r.slug} — ${r.error ?? r.status}`);
}

process.exit(results.every(r => r.ok) ? 0 : 1);
