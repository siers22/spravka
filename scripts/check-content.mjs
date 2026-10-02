import { lessons,resources } from '../src/data/lessons.js'
import fs from 'node:fs'
import assert from 'node:assert/strict'
assert.equal(lessons.slice(1).reduce((n,l)=>n+l.time,0),210)
assert.equal(lessons.slice(1,4).reduce((n,l)=>n+l.time,0),70)
for(const resource of resources){assert.ok(fs.statSync('public'+resource.url).size>0,resource.url)}
const collection=JSON.parse(fs.readFileSync('public/downloads/notes.postman_collection.json'))
assert.equal(collection.item.length,2)
assert.ok(collection.item[1].name.includes('500'))
const customers=JSON.parse(fs.readFileSync('examples/ExamGuide/Data/customers.json','utf8').replace(/^\uFEFF/,''))
assert.equal(customers.length,6)
assert.ok(customers.every(c=>typeof c.id==='string'&&c.id.length===9))
console.log('OK: время ПА/ГИА, все скачиваемые материалы, коллекция и 6 исходных контрагентов')
