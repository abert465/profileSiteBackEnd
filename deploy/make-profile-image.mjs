// Crop a source headshot to a square and write it as WebP.
//
// There is no sharp or ImageMagick on this box, and System.Drawing has no WebP
// encoder, so this drives the same installed Chrome that smoke-test.mjs uses and
// lets the browser do the encoding. No new dependency.
//
// Usage:
//   node deploy/make-profile-image.mjs <source-image> <output.webp> [size] [--no-crop]
//
// The crop is defined below in SOURCE space and is deliberately not centered:
// it trims the bottom of the frame so a watermark in the lower-right corner is
// excluded rather than blurred over.
//
// --no-crop takes the largest centered square instead, which is what you want
// when the source is an image this script already produced - re-running the
// offset crop on an existing crop walks the subject up the frame.

import { createRequire } from 'node:module';
import { existsSync, readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, extname, join, resolve } from 'node:path';

// Resolved out of web/ the same way smoke-test.mjs does it: puppeteer-core is a
// devDependency of the front end, not of the repo root.
const here = dirname(fileURLToPath(import.meta.url));
const require = createRequire(join(here, '..', 'web', 'package.json'));
const puppeteer = require('puppeteer-core');

const args = process.argv.slice(2);
const noCrop = args.includes('--no-crop');
const [srcArg, outArg, sizeArg] = args.filter((a) => !a.startsWith('--'));
if (!srcArg || !outArg) {
  console.error('usage: node deploy/make-profile-image.mjs <source-image> <output.webp> [size] [--no-crop]');
  process.exit(2);
}

const src = resolve(srcArg);
const out = resolve(outArg);
const size = Number(sizeArg ?? 900);

if (!existsSync(src)) {
  console.error(`source not found: ${src}`);
  process.exit(1);
}

const browsers = [
  'C:/Program Files/Google/Chrome/Application/chrome.exe',
  'C:/Program Files (x86)/Google/Chrome/Application/chrome.exe',
];
const executablePath = browsers.find((p) => existsSync(p));
if (!executablePath) {
  console.error('no installed Chrome found; update the paths in this script');
  process.exit(1);
}

// The data URL's declared type has to match the bytes - Chrome will not decode
// WebP bytes announced as image/jpeg, which is the case whenever this script is
// re-run over its own output to produce a smaller variant.
const byExt = { '.png': 'image/png', '.webp': 'image/webp', '.gif': 'image/gif' };
const mime = byExt[extname(src).toLowerCase()] ?? 'image/jpeg';
const dataUrl = `data:${mime};base64,${readFileSync(src).toString('base64')}`;

const browser = await puppeteer.launch({ executablePath, headless: 'new' });
try {
  const page = await browser.newPage();
  const result = await page.evaluate(
    async (url, outSize, plain) =>
      new Promise((done, fail) => {
        const img = new Image();
        img.onerror = () => fail(new Error('image failed to decode'));
        img.onload = () => {
          // Square crop taken from the upper portion of the frame: keeps head and
          // shoulders, drops the bottom strip where the watermark sits. Under
          // --no-crop it is the largest centered square, i.e. a pure resize for
          // an already-square source.
          const side = plain
            ? Math.min(img.width, img.height)
            : Math.round(Math.min(img.width, img.height) * 0.89);
          const sx = Math.round((img.width - side) / 2);
          const sy = plain ? Math.round((img.height - side) / 2) : Math.round(img.height * 0.08);

          const canvas = document.createElement('canvas');
          canvas.width = outSize;
          canvas.height = outSize;
          const ctx = canvas.getContext('2d');
          ctx.imageSmoothingQuality = 'high';
          ctx.drawImage(img, sx, sy, side, side, 0, 0, outSize, outSize);

          done({
            webp: canvas.toDataURL('image/webp', 0.9),
            natural: { w: img.width, h: img.height },
            crop: { sx, sy, side },
          });
        };
        img.src = url;
      }),
    dataUrl,
    size,
    noCrop,
  );

  const b64 = result.webp.split(',')[1];
  writeFileSync(out, Buffer.from(b64, 'base64'));
  console.log(`source ${result.natural.w}x${result.natural.h}`);
  console.log(`crop   x=${result.crop.sx} y=${result.crop.sy} side=${result.crop.side}`);
  console.log(`wrote  ${out} (${size}x${size}, ${readFileSync(out).length} bytes)`);
} finally {
  await browser.close();
}
