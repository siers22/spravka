import {courseMeta,progressKey,parseRoute,route} from '../src/data/course-meta.js'
import {lessons as systems,resources as systemsResources} from '../src/data/lessons.js'
import {lessons as programmer,resources as programmerResources} from '../src/data/programmer-lessons.js'
import fs from 'node:fs'
import assert from 'node:assert/strict'
for(const [id,lessons,resources,paTime] of [['information-systems',systems,systemsResources,70],['programmer',programmer,programmerResources,90]]){
 const course=courseMeta[id]
 assert.equal(lessons.slice(1).reduce((n,l)=>n+l.time,0),210)
 assert.equal(lessons.slice(1,course.paCount+1).reduce((n,l)=>n+l.time,0),paTime)
 for(const resource of resources)assert.ok(fs.statSync('public'+resource.url).size>0,resource.url)
 assert.equal(new Set(lessons.map(l=>l.id)).size,lessons.length)
 for(const l of lessons)assert.equal(new Set(l.sections.map(s=>s.id)).size,l.sections.length)
 assert.deepEqual(parseRoute(route(id,'setup','tools')),{course:id,id:'setup',section:'tools'})
}
assert.equal(progressKey('information-systems','setup',0),'setup-0','legacy progress survives')
assert.notEqual(progressKey('information-systems','setup',0),progressKey('programmer','setup',0))
assert.equal(parseRoute('#sql/query').course,'information-systems')
assert.equal(parseRoute('#specialties').chooser,true)
assert.ok(programmerResources.every(r=>!systemsResources.some(s=>s.url===r.url)))
assert.ok(!JSON.stringify(programmer).includes('AuthDemo'))
const shoes=JSON.parse(fs.readFileSync('src/data/shoes.json'));assert.equal(shoes.length,31)
assert.ok(shoes.every(s=>fs.existsSync('public/shoes/'+s.image)))
console.log('OK: two course routes, independent progress keys, PA/GIA timings, isolated downloads and all 31 shoe images')
