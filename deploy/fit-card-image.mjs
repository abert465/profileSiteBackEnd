// Fit an image to the project card band and write it as JPEG.
//
// Cards render at h-32 (128px) with object-cover, and capture-project-shots.mjs
// produces 1440x576 — a 2.5:1 band. An uploaded image that is not that shape
// gets centre-cropped by the browser, unseen. This does the crop deliberately,
// at full source resolution, so the framing is a decision rather than a side
// effect.
//
// No sharp or ImageMagick on this box, so this drives installed Chrome the same
// way make-profile-image.mjs and smoke-test.mjs do. No new dependency.
//
//   node deploy/fit-card-image.mjs <source> <output.jpg> [width] [height]

import { createRequire } from 'node:module';
import { existsSync, readFileSync, writeFileSync } from 'node:fs';
import { fileURLToPath } from 'node:url';
import { dirname, extname, join, resolve } from 'node:path';

const here = dirname(fileURLToPath(import.meta.url));
const require = createRequire(join(here, '..', 'web', 'package.json'));
const puppeteer = require('puppeteer-core');

const [, , srcArg, outArg, wArg, hArg] = process.argv;
if (!srcArg || !outArg) {
  console.error('usage: node deploy/fit-card-image.mjs <source> <output.jpg> [width] [height]');
  process.exit(2);
}

const src = resolve(srcArg);
const out = resolve(outArg);
const outW = Number(wArg ?? 1440);
const outH = Number(hArg ?? 576);

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

const ext = extname(src).toLowerCase();
const mime = ext === '.png' ? 'image/png' : ext === '.webp' ? 'image/webp' : 'image/jpeg';
const dataUrl = `data:${mime};base64,${readFileSync(src).toString('base64')}`;

const browser = await puppeteer.launch({ executablePath, headless: 'new' });
try {
  const page = await browser.newPage();
  const result = await page.evaluate(
    async (url, w, h) =>
      new Promise((done, fail) => {
        const img = new Image();
        img.onerror = () => fail(new Error('image failed to decode'));
        img.onload = () => {
          // Centre crop to the target aspect, taking as much of the source as
          // that ratio allows. A source already at the target ratio is untouched
          // apart from the resample.
          const target = w / h;
          const natural = img.width / img.height;

          let sw = img.width;
          let sh = img.height;
          if (natural > target) sw = Math.round(img.height * target);
          else sh = Math.round(img.width / target);

          const sx = Math.round((img.width - sw) / 2);
          const sy = Math.round((img.height - sh) / 2);

          const canvas = document.createElement('canvas');
          canvas.width = w;
          canvas.height = h;
          const ctx = canvas.getContext('2d');
          ctx.imageSmoothingQuality = 'high';
          ctx.drawImage(img, sx, sy, sw, sh, 0, 0, w, h);

          done({
            jpeg: canvas.toDataURL('image/jpeg', 0.86),
            natural: { w: img.width, h: img.height, ratio: +natural.toFixed(3) },
            crop: { sx, sy, sw, sh },
          });
        };
        img.src = url;
      }),
    dataUrl,
    outW,
    outH,
  );

  writeFileSync(out, Buffer.from(result.jpeg.split(',')[1], 'base64'));
  console.log(`source ${result.natural.w}x${result.natural.h} (${result.natural.ratio}:1)`);
  console.log(`crop   x=${result.crop.sx} y=${result.crop.sy} ${result.crop.sw}x${result.crop.sh}`);
  console.log(`wrote  ${out} (${outW}x${outH}, ${readFileSync(out).length} bytes)`);
} finally {
  await browser.close();
}
