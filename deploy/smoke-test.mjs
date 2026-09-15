// Render the site in a real browser and fail on anything a curl check cannot see:
// JavaScript errors, assets served with the wrong content type, broken images, an
// empty React root.
//
//   node deploy/smoke-test.mjs                 # https://tedko.dev
//   node deploy/smoke-test.mjs https://www.tedko.dev
//
// Public URL only, not the loopback origin. Reaching the origin directly would
// mean sending Host: tedko.dev, which Chrome refuses to let a caller override,
// and routing the real hostname to 127.0.0.1 instead runs into .dev being
// HSTS-preloaded: the browser upgrades to HTTPS, which the origin does not
// speak. Testing through the tunnel covers more of the path anyway.
//
// Exit code 0 means every check passed, 1 means at least one failed, so this can
// gate a deploy.
//
// Uses puppeteer-core against a Chrome or Edge already installed on the machine -
// no bundled browser download. puppeteer-core is a devDependency of web/, which
// publish-selfhost.ps1 already runs `npm ci` against.

import { createRequire } from 'node:module';
import { existsSync } from 'node:fs';
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

const target = process.argv[2] ?? 'https://tedko.dev';

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

const failures = [];
const consoleErrors = [];
const pageErrors = [];
const badResponses = [];

const browser = await puppeteer.launch({
  executablePath,
  headless: true,
  args: ['--no-sandbox', '--disable-dev-shm-usage'],
});

try {
  const page = await browser.newPage();
  await page.setViewport({ width: 1440, height: 900 });

  // Otherwise a cached copy can report success for a deploy that is broken.
  await page.setCacheEnabled(false);

  page.on('console', m => { if (m.type() === 'error') consoleErrors.push(m.text()); });
  page.on('pageerror', e => pageErrors.push(e.message));
  page.on('requestfailed', r => badResponses.push(`${r.url()} — ${r.failure()?.errorText}`));
  page.on('response', r => {
    if (r.status() >= 400) badResponses.push(`${r.status()} ${r.url()}`);

    // The failure that started all this: assets resolving to the SPA fallback.
    // Every status stays 200, so only the content type gives it away.
    const url = r.url();
    const type = r.headers()['content-type'] ?? '';
    if (/\.js(\?|$)/.test(url) && !/javascript|ecmascript/i.test(type)) {
      failures.push(`script served as "${type}": ${url}`);
    }
    if (/\.css(\?|$)/.test(url) && !/text\/css/i.test(type)) {
      failures.push(`stylesheet served as "${type}": ${url}`);
    }
  });

  const response = await page.goto(target, { waitUntil: 'networkidle2', timeout: 45000 });
  if (response.status() !== 200) failures.push(`GET ${target} returned ${response.status()}`);

  // Scroll the whole page before judging anything visual: the reveal animations
  // are IntersectionObserver-driven, so content stays at opacity 0 until it has
  // been scrolled into view at least once. Without this every animated section
  // looks empty and the test reports a failure that no real visitor would see.
  await page.evaluate(async () => {
    const step = window.innerHeight * 0.8;
    for (let y = 0; y < document.body.scrollHeight; y += step) {
      window.scrollTo(0, y);
      await new Promise(r => setTimeout(r, 250));
    }
    window.scrollTo(0, 0);
    await new Promise(r => setTimeout(r, 600));
  });

  const root = await page.evaluate(() => {
    const el = document.getElementById('root');
    return { exists: !!el, children: el?.children.length ?? 0, text: document.body.innerText.trim().length };
  });
  if (!root.exists) failures.push('no #root element');
  if (root.children === 0) failures.push('#root is empty — React did not mount');
  if (root.text < 500) failures.push(`only ${root.text} chars of visible text`);

  const images = await page.evaluate(() =>
    [...document.images].map(i => ({ src: i.currentSrc || i.src, ok: i.complete && i.naturalWidth > 0 })));
  for (const img of images.filter(i => !i.ok)) failures.push(`broken image: ${img.src}`);

  const invisible = await page.evaluate(() =>
    [...document.querySelectorAll('section')]
      .filter(s => s.innerText.trim().length === 0 && s.getBoundingClientRect().height > 200)
      .map(s => s.id || s.className.toString().slice(0, 40)));
  for (const s of invisible) failures.push(`section renders no text: ${s}`);

  for (const e of consoleErrors) failures.push(`console error: ${e}`);
  for (const e of pageErrors) failures.push(`page error: ${e}`);
  for (const e of badResponses) failures.push(`request failed: ${e}`);

  console.log(`target:   ${target}`);
  console.log(`#root:    ${root.children} children, ${root.text} chars of text`);
  console.log(`images:   ${images.length} loaded, ${images.filter(i => !i.ok).length} broken`);
}
finally {
  await browser.close();
}

if (failures.length) {
  console.error(`\nFAILED (${failures.length}):`);
  for (const f of [...new Set(failures)]) console.error(`  ${f}`);
  process.exit(1);
}

console.log('\nall checks passed.');
