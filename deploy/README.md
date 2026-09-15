# Self-hosted deploy

Runs the site on a Windows machine you own, published to the internet through a
Cloudflare Tunnel. No container runtime, no open inbound port, no hosting bill.

Two Windows services:

| Service       | Does                                                           |
| ------------- | -------------------------------------------------------------- |
| `ProfileSite` | The app. Listens on `127.0.0.1:8080` only.                      |
| `Cloudflared` | Dials out to Cloudflare, proxies requests to the app.           |

Both start at boot, so the site comes back after a reboot with nobody logged in.

State lives in `C:\ProfileSite\data` (`app.db`, `uploads`, `keys`) and is never
touched by a redeploy. `C:\ProfileSite\app` is replaced wholesale each time.

## Before you start

`tedko.dev` is registered at Namecheap and still on Namecheap's nameservers
(`dns1.registrar-servers.com`, `dns2.registrar-servers.com`). The tunnel cannot
create its DNS records until the zone is served by Cloudflare. One-time move:

1. Cloudflare dashboard → Account home → the **Add a domain** card → type
   `tedko.dev` in its box. (Same flow as **Domains** → **Onboard a domain** in
   the sidebar.) If Cloudflare offers to *transfer* the registration away from
   Namecheap, decline — only the nameservers need to change, and a transfer is
   a slower, separate operation with no benefit here.
2. Pick the **Free** plan.
3. Cloudflare offers to import the existing DNS records. Delete anything
   Namecheap parked on `tedko.dev` or `www` — those records would otherwise
   compete with the tunnel's. Leaving them is survivable (`install-tunnel.ps1`
   passes `--overwrite-dns`) but a clean zone is easier to reason about.
4. Cloudflare assigns two nameservers to this zone. For `tedko.dev` they are
   `clay.ns.cloudflare.com` and `tricia.ns.cloudflare.com`. They are specific to
   this zone — if you ever redo this, read them off the screen rather than
   reusing these.
5. Namecheap → Domain List → `tedko.dev` **Manage**:
   - **Advanced DNS** tab: confirm **DNSSEC** is off. Switching nameservers with
     DNSSEC still enabled breaks resolution until the old signatures expire.
   - **Domain** tab → Nameservers → **Custom DNS** → enter both → save (green ✓).
     The `dns*.registrar-servers.com` entries are replaced automatically.
6. Wait for the zone to read **Active** in Cloudflare. Usually minutes, up to 24
   hours. Check with `nslookup -type=NS tedko.dev 8.8.8.8` — Cloudflare's
   `*.ns.cloudflare.com` names mean it has propagated.

Only then run step 5 below.

Also note: the machine has to be on for the site to be up. That is the real cost
of this option.

### `.dev` forces HTTPS

`.dev` is on the HSTS preload list, baked into every major browser. Plain
`http://tedko.dev` is not downgraded-but-working — browsers refuse to make the
request at all. In practice this is free security and it suits the tunnel, which
is HTTPS end to end anyway. The consequence to know: a broken certificate means
the site is *unreachable*, not merely flagged. Cloudflare's Universal SSL covers
the apex and `www` automatically once the zone is Active, so the only real
requirement is not to publish before that happens.

## First run

Elevated PowerShell, from the repo root.

```powershell
# 1. build the SPA, publish the API, stage into C:\ProfileSite\app
.\deploy\publish-selfhost.ps1

# 2. fill in configuration and secrets
Copy-Item .\deploy\profilesite.env.example .\deploy\profilesite.env

# Admin__PasswordHash and Private__PasscodeHash are BCrypt hashes, not
# passwords. Run this once for each; it prompts without echoing and prints
# only the hash.
.\deploy\new-hash.ps1

notepad .\deploy\profilesite.env

# 3. register the app as a service
.\deploy\install-service.ps1
Start-Service ProfileSite

# 4. confirm it answers locally before exposing it
curl.exe -s -H 'Host: tedko.dev' -H 'X-Forwarded-Proto: https' http://127.0.0.1:8080/api/health
#  -> {"status":"ok"}

# 5. publish it
.\deploy\install-tunnel.ps1 -Hostname tedko.dev

# 6. confirm it answers publicly
curl.exe -sI https://tedko.dev/api/health
```

Step 5 opens a browser once, to authorise this machine against the zone.

`profilesite.env` is gitignored. It holds the SMTP password and the admin
password hash, so it stays on this machine — never committed, never pasted
anywhere.

## Redeploying

```powershell
.\deploy\publish-selfhost.ps1   # stops the service, swaps the app, restarts it
node deploy\smoke-test.mjs      # confirm the deploy actually works
```

The tunnel is untouched and the data directory survives. Only re-run
`install-service.ps1` after editing `profilesite.env`.

## Smoke test

`curl` is not enough to tell whether this site works. The failure that cost the
most time during setup returned `200` on every request: the SPA fallback was
serving `index.html` for `/assets/*.js`, so the page loaded an empty `<div
id="root">` and rendered blank. Only the content type gave it away.

```powershell
node deploy\smoke-test.mjs                    # https://tedko.dev
node deploy\smoke-test.mjs https://www.tedko.dev
```

Drives headless Chrome or Edge — whichever is installed, no bundled download —
and exits non-zero if the React root is empty, a script or stylesheet arrives
with the wrong content type, an image is broken, a section renders no text, or
anything lands in the console. Run it after every deploy.

It scrolls the whole page before judging, because the reveal animations are
IntersectionObserver-driven: without scrolling, Projects and Skills sit at
`opacity: 0` and look broken when they are not.

Testing the loopback origin directly is deliberately unsupported. It would need
`Host: tedko.dev`, which Chrome will not let a caller override, and mapping the
real hostname to `127.0.0.1` instead hits `.dev` being HSTS-preloaded — the
browser upgrades to HTTPS, which the origin does not speak.

## Cloudflare settings worth setting once

In the dashboard, for the zone:

- **SSL/TLS → Overview → Full (strict)**. The tunnel is already end-to-end
  encrypted; "Flexible" would be a downgrade.
- **SSL/TLS → Edge Certificates → Always Use HTTPS → on.**

## Checks

```powershell
Get-Service ProfileSite, Cloudflared
Get-EventLog -LogName Application -Source ProfileSite -Newest 20
Get-EventLog -LogName Application -Source Cloudflared -Newest 20
cloudflared tunnel list
cloudflared tunnel info profilesite     # is a connector attached
```

| Symptom                                | Usually                                                                 |
| -------------------------------------- | ----------------------------------------------------------------------- |
| `502` from Cloudflare                  | `ProfileSite` is stopped, or not listening on the port in the config.    |
| `530` / `1033`                         | `Cloudflared` is stopped, or the DNS record points at a deleted tunnel.  |
| `400 Bad Request` on every request      | Hostname missing from `AllowedHosts` in `profilesite.env`.               |
| Admin signed out after every redeploy  | `Storage__DataProtectionKeys` not pointing into the data directory.      |
| Contact form silently fails            | Gmail app password wrong, or `Email__SmtpPassword` left blank.           |

## Why the app binds loopback

`ASPNETCORE_URLS=http://127.0.0.1:8080` is doing security work, not just tidying
up. The app trusts `X-Forwarded-For` from `127.0.0.1` in order to see real
client IPs, and the contact-form rate limiter buckets on that IP. If the app
also listened on the LAN address, anyone on the network could send a forged
`X-Forwarded-For` and get a fresh rate-limit bucket per request. Loopback-only
binding means cloudflared is the only possible sender.
