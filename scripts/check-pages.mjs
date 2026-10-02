import fs from 'node:fs'
import path from 'node:path'
import assert from 'node:assert/strict'
import {fileURLToPath} from 'node:url'

// Run after building with the same base, for example:
// VITE_BASE_PATH=/spravka/ npm run build && node scripts/check-pages.mjs /spravka/
const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..')
const dist = path.join(root, 'dist')
const requestedBase = process.argv[2] || process.env.VITE_BASE_PATH || '/'
assert.ok(requestedBase.startsWith('/') && !requestedBase.startsWith('//'), 'Expected a deployment path such as /spravka/, not a domain')
const base = requestedBase.endsWith('/') ? requestedBase : requestedBase + '/'
const origin = 'https://pages-verification.invalid'
const failures = []
const checkedAssets = new Set()

function check(condition, message) {
  if (!condition) failures.push(message)
}
function nonemptyFile(file, description) {
  try {
    const stat = fs.statSync(file)
    check(stat.isFile() && stat.size > 0, `${description}: empty or not a file`)
    return stat.isFile() && stat.size > 0
  } catch {
    check(false, `${description}: missing ${path.relative(root, file)}`)
    return false
  }
}
function localAsset(url, description, relativeBase = base) {
  if (!url || /^(?:data:|https?:|\/\/|#)/i.test(url)) return null
  const resolved = new URL(url, origin + relativeBase)
  if (resolved.origin !== origin) return null
  const deployedPath = decodeURIComponent(resolved.pathname)
  if (!deployedPath.startsWith(base)) {
    check(false, `${description}: ${url} escapes deployment base ${base}`)
    return null
  }
  const file = path.resolve(dist, deployedPath.slice(base.length))
  if (file !== dist && !file.startsWith(dist + path.sep)) {
    check(false, `${description}: path escapes the built site`)
    return null
  }
  if (nonemptyFile(file, description)) checkedAssets.add(file)
  return file
}

assert.ok(fs.existsSync(path.join(dist, 'index.html')), 'Build the site before checking GitHub Pages assets')
const html = fs.readFileSync(path.join(dist, 'index.html'), 'utf8')
const scriptSources = [...html.matchAll(/<script\b[^>]*\bsrc=["']([^"']+)["'][^>]*>/gi)].map(match => match[1])
const stylesheets = []
for (const match of html.matchAll(/<link\b[^>]*>/gi)) {
  const tag = match[0]
  const href = tag.match(/\bhref=["']([^"']+)["']/i)?.[1]
  if (!href) continue
  const rel = tag.match(/\brel=["']([^"']+)["']/i)?.[1] || ''
  if (/stylesheet|icon|modulepreload/.test(rel)) {
    const file = localAsset(href, `HTML ${rel}`)
    if (rel.includes('stylesheet') && file) stylesheets.push({file, href})
  }
}
check(scriptSources.length > 0, 'Built HTML has no application script')
for (const src of scriptSources) localAsset(src, 'HTML application script')
check(stylesheets.length > 0, 'Built HTML has no stylesheet')

let fontReferences = 0
for (const {file, href} of stylesheets) {
  if (!fs.existsSync(file)) continue
  const css = fs.readFileSync(file, 'utf8')
  const stylesheetPath = new URL(href, origin + base).pathname
  for (const match of css.matchAll(/url\(\s*(["']?)(.*?)\1\s*\)/gi)) {
    const url = match[2].trim()
    const asset = localAsset(url, `CSS asset in ${path.basename(file)}`, stylesheetPath)
    if (/\.woff2?(?:[?#]|$)/i.test(url)) {
      fontReferences++
      if (asset && fs.existsSync(asset)) {
        const signature = fs.readFileSync(asset).subarray(0, 4).toString('ascii')
        check(signature === 'wOF2' || signature === 'wOFF', `Invalid web font: ${url}`)
      }
    }
  }
}
check(fontReferences >= 2, 'Built CSS must retain self-hosted Latin and Cyrillic web fonts')

const requiredDownloads = [
  ['downloads/dotnet-example.zip', 'PK'],
  ['downloads/programmer-example.zip', 'PK'],
  ['sources/kim.pdf', '%PDF-'],
  ['sources/programmer/kim.pdf', '%PDF-'],
  ['sources/attachments.zip', 'PK'],
  ['sources/programmer/attachments.zip', 'PK'],
]
for (const [relative, signature] of requiredDownloads) {
  const file = localAsset(base + relative, `Course file ${relative}`)
  if (file && fs.existsSync(file)) {
    check(fs.readFileSync(file).subarray(0, signature.length).toString('ascii') === signature, `Invalid or placeholder course file: ${relative}`)
  }
}
const shoes = JSON.parse(fs.readFileSync(path.join(root, 'src/data/shoes.json'), 'utf8'))
check(shoes.length === 31, 'The shoe preview fixture must contain all 31 source models')
const shoeImages = new Set([...shoes.map(shoe => shoe.image), 'picture.png'])
for (const image of shoeImages) {
  const relative = `shoes/${image}`
  const file = localAsset(base + relative, `Shoe preview image ${image}`)
  if (file && fs.existsSync(file)) {
    const bytes = fs.readFileSync(file).subarray(0, 8)
    check(bytes.equals(Buffer.from([137, 80, 78, 71, 13, 10, 26, 10])), `Invalid PNG shoe image: ${relative}`)
  }
}
for (let tile = 1; tile <= 4; tile++) localAsset(`${base}sources/${tile}.png`, `Authentication puzzle image ${tile}`)

// Teaching snippets live in other file formats. Check actual Vue/JS/TS runtime
// sources so an otherwise valid build cannot silently ship root-relative links.
function sourceFiles(directory) {
  return fs.readdirSync(directory, {withFileTypes: true}).flatMap(entry => {
    const file = path.join(directory, entry.name)
    return entry.isDirectory() ? sourceFiles(file) : /\.(?:vue|[cm]?js|ts)$/.test(entry.name) ? [file] : []
  })
}
for (const file of sourceFiles(path.join(root, 'src'))) {
  const source = fs.readFileSync(file, 'utf8')
  for (const match of source.matchAll(/["'`]\/(?:downloads|sources|shoes|fixtures)\/[^"'`\n]*/g)) {
    const line = source.slice(0, match.index).split('\n').length
    check(false, `Root-relative runtime asset in ${path.relative(root, file)}:${line}: ${match[0]}`)
  }
}

if (failures.length) {
  console.error(`GitHub Pages asset check failed for ${base}:\n${failures.map(message => `- ${message}`).join('\n')}`)
  process.exit(1)
}
console.log(`OK: GitHub Pages base ${base}; HTML entry points, ${fontReferences} self-hosted font references, both course archives/PDFs, ${shoeImages.size} shoe images, puzzle images and runtime public URLs (${checkedAssets.size} built files checked)`)
