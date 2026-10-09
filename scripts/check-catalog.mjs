import fs from 'node:fs'
import path from 'node:path'
import assert from 'node:assert/strict'
import {validateCatalog,safeUrl} from '../src/content/catalog.js'
const data=JSON.parse(fs.readFileSync('content/catalog.json','utf8'))
const errors=validateCatalog(data)
assert.deepEqual(errors,[],errors.join('\n'))
let files=0
for(const edition of data.editions)for(const course of edition.courses){
 const lessonLinks=course.lessons.flatMap(l=>l.sections.flatMap(s=>s.blocks.flatMap(b=>b.type==='links'?b.items.map(item=>item.url):b.type==='image'?[b.url]:[])))
 for(const url of new Set([course.download,...course.resources.map(r=>r.url),...lessonLinks].filter(Boolean))){
  assert.ok(safeUrl(url),url)
  if(/^https?:\/\//i.test(url))continue
  const file=path.join('public',url);assert.ok(fs.existsSync(file)&&fs.statSync(file).isFile()&&fs.statSync(file).size>0,`Missing catalog asset: ${file}`);files++
 }
}
console.log(`OK: catalog schema, ${data.editions.length} editions, current ${data.currentYear}, ${files} versioned resource references`)
