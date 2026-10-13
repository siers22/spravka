import test from 'node:test'
import assert from 'node:assert/strict'
import fs from 'node:fs'
import {execFileSync} from 'node:child_process'
import {parseRoute, route, progressKey} from '../../src/data/course-meta.js'

const catalog=JSON.parse(fs.readFileSync('content/catalog.json','utf8'))
const web=catalog.editions.find(e=>e.year===2027).courses.find(c=>c.id==='web-developer')

test('the published web project can be reconstructed from Git alone', {skip:!fs.existsSync('.git')}, ()=>{
 const tracked=new Set(execFileSync('git',['ls-files','-z','--','examples/ConferencePortal'],{encoding:'utf8'}).split('\0'))
 const base=web.download.slice(0,web.download.lastIndexOf('/'))
 const manifest=JSON.parse(fs.readFileSync('public/'+base+'/source-manifest.json','utf8'))
 for(const file of Object.keys(manifest.sha256)) {
   assert.ok(tracked.has('examples/ConferencePortal/'+file),`Project file missing from Git: ${file}`)
 }
})

test('web course covers the 75-point base route with one PA task and actual source files',()=>{
 assert.ok(web)
 assert.equal(web.status,'published')
 assert.equal(web.kim,'09.02.07-3-2027')
 assert.equal(web.giaPoints,75)
 assert.equal(web.paPoints,25)
 assert.equal(web.paCount,1)
 assert.equal(web.dialectExamples,false)
 assert.deepEqual(web.lessons.map(l=>l.time),[0,90,60,60])
 assert.equal(web.lessons.flatMap(l=>l.sections).length,72)
 const blocks=web.lessons.flatMap(l=>l.sections.flatMap(s=>s.blocks))
 assert.equal(blocks.filter(b=>b.type==='list').reduce((n,b)=>n+b.items.length,0),360)
 assert.equal(blocks.filter(b=>b.type==='image').length,9)
 assert.ok(!blocks.some(b=>['dbSetup','diagram','shoeDemo','authDemo','calculator','sourceFile'].includes(b.type)))
 for(const [key,value] of Object.entries(web.codes)) {
   assert.equal(value,fs.readFileSync('examples/ConferencePortal/'+key.replace(/^file:/,''),'utf8'),key)
 }
 const schema=web.codes['file:sql/schema.sql']
 assert.equal((schema.match(/CREATE TABLE IF NOT EXISTS/g)||[]).length,4)
 assert.equal((schema.match(/FOREIGN KEY/g)||[]).length,3)
 assert.ok(web.codes['file:bin/init.php'].includes("'Conf2027', password_hash('Demo77'"))
})

test('web routes and progress are independent from both previous specialties',()=>{
 assert.deepEqual(parseRoute('#web-developer/application',{'web-developer':web}),{course:'web-developer',id:'application',section:''})
 assert.deepEqual(parseRoute(route('web-developer','design','gimp-export',2027)),{year:2027,course:'web-developer',id:'design',section:'gimp-export'})
 const own=progressKey('web-developer','setup',0,2027)
 assert.notEqual(own,progressKey('programmer','setup',0,2027))
 assert.notEqual(own,progressKey('information-systems','setup',0,2027))
 assert.notEqual(own,progressKey('web-developer','setup',0,2028))
})

test('web materials contain real recorded checks and five safe local design pages',()=>{
 const report=JSON.parse(fs.readFileSync('examples/ConferencePortal/docs/results/http-results.json','utf8'))
 assert.equal(report.failed,0)
 assert.equal(report.passed,report.checks.length)
 assert.ok(report.passed>=58)
 for(const page of ['login','register','dashboard','booking','admin']) {
   const html=fs.readFileSync(`examples/ConferencePortal/docs/design/${page}.html`,'utf8')
   assert.ok(html.includes('../../public/assets/app.css'))
   assert.ok(!html.includes('name="csrf"'))
   assert.ok(!/<form[^>]+action=/.test(html))
 }
 const media=JSON.parse(fs.readFileSync('examples/ConferencePortal/public/assets/media-manifest.json','utf8'))
 assert.equal(media.length,3)
 for(const asset of media) {
   assert.ok(asset.afterBytes<asset.beforeBytes)
   assert.equal(fs.statSync('examples/ConferencePortal/public/assets/'+asset.output).size,asset.afterBytes)
 }
})
