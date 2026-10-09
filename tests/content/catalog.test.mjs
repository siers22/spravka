import test from 'node:test'
import assert from 'node:assert/strict'
import fs from 'node:fs'
import {clone,copyEdition,newCourse,validateCatalog,safeUrl,editableCatalog} from '../../src/content/catalog.js'
import {parseRoute,route,progressKey} from '../../src/data/course-meta.js'
const catalog=JSON.parse(fs.readFileSync('content/catalog.json','utf8'))
test('the migrated edition contains code snapshots and existing versioned files',()=>{
 assert.deepEqual(validateCatalog(catalog),[])
 for(const course of catalog.editions[0].courses){assert.ok(Object.keys(course.codes).length);for(const url of [course.download,...course.resources.map(r=>r.url)])assert.ok(url.startsWith(`materials/2027/${course.id}/`)&&fs.statSync(`public/${url}`).size>0)}
})
test('editing a new edition cannot change the archived lessons or code',()=>{
 const draft=clone(catalog),original=JSON.stringify(draft.editions[0]);const next=copyEdition(draft,2027,2028)
 next.courses[0].lessons[0].sections[0].blocks[0].text='Новое условие';next.courses[0].codes.program='Новый код'
 assert.equal(JSON.stringify(draft.editions[0]),original);assert.ok(next.courses.every(c=>c.status==='draft'));assert.equal(draft.currentYear,2027)
 next.courses[0].status='published';draft.currentYear=2028;assert.deepEqual(validateCatalog(draft),[])
 assert.throws(()=>copyEdition(draft,2027,2028),/новый год/)
})
test('new specialties can be saved as drafts but cannot publish without KIM',()=>{
 const draft=clone(catalog),course=newCourse('network-admin');draft.editions[0].courses.push(course)
 assert.deepEqual(validateCatalog(draft),[]);course.status='published';assert.ok(validateCatalog(draft).some(e=>e.includes('/kim')))
 course.kim='09.02.06-2027';assert.deepEqual(validateCatalog(draft),[])
})
test('invalid routes, duplicate IDs and broken code references prevent publishing',()=>{
 const draft=clone(catalog),c=draft.editions[0].courses[0]
 c.lessons[0].id='overview';c.lessons[1].sections[0].blocks=[{type:'code',key:'missing'}];c.lessons[1].sections.push(clone(c.lessons[1].sections[0]));draft.currentYear=2028
 const errors=validateCatalog(draft);assert.ok(errors.some(e=>e.includes('зарезервированный')));assert.ok(errors.some(e=>e.includes('повтор')));assert.ok(errors.some(e=>e.includes('ключом')));assert.ok(errors.some(e=>e.includes('Текущий год')))
})
test('content links cannot escape Pages base or execute JavaScript',()=>{
 for(const url of ['javascript:alert(1)','data:text/html,x','//evil.test','/sources/x','../x','materials/%2e%2e/x','materials/x%2fy','https://user:password@example.com/x','a\\b','foo bar'])assert.equal(safeUrl(url),false,url)
 for(const url of ['materials/2027/systems/kim.pdf','https://example.org/a%20b.pdf','http://localhost:3000/data'])assert.equal(safeUrl(url),true,url)
})
test('malformed imports yield validation errors rather than crashing',()=>{
 for(const value of [null,{}, {schemaVersion:1,editions:[null]}, {schemaVersion:1,editions:[{year:2027,courses:[null]}]}])assert.ok(validateCatalog(value).length)
})
test('year-specific deep links and progress remain independent with legacy progress preserved',()=>{
 assert.deepEqual(parseRoute(route('network-admin','task-1','instructions',2028)),{year:2028,course:'network-admin',id:'task-1',section:'instructions'})
 assert.deepEqual(parseRoute('#network-admin/task-1',{'network-admin':{}}),{course:'network-admin',id:'task-1',section:''})
 assert.equal(progressKey('information-systems','sql',0,2027),'sql-0')
 assert.notEqual(progressKey('information-systems','sql',0,2027),progressKey('information-systems','sql',0,2028))
 assert.deepEqual(parseRoute('#archive'),{page:'archive'});assert.deepEqual(parseRoute('#admin'),{page:'admin'})
})

test('unfinished drafts stay recoverable while invalid schemas are rejected',()=>{
 const draft=clone(catalog);draft.editions[0].courses[0].kim='';draft.editions[0].courses[0].paCount=999;assert.ok(validateCatalog(draft).length);assert.equal(editableCatalog(draft),true);draft.editions[0].courses[0].lessons[0].sections=null;assert.equal(editableCatalog(draft),false)
})

test('lesson links remain editable and reject unsafe destinations on publication',()=>{
 const draft=clone(catalog),section=draft.editions[0].courses[0].lessons[0].sections[0]
 section.blocks.push({type:'links',items:[{title:'Microsoft WPF',url:'https://learn.microsoft.com/en-us/dotnet/desktop/wpf/',description:'Официальная документация'}]})
 assert.deepEqual(validateCatalog(draft),[]);assert.equal(editableCatalog(draft),true)
 section.blocks.at(-1).items[0].url='javascript:alert(1)'
 assert.ok(validateCatalog(draft).some(e=>e.includes('небезопасный адрес ссылки')))
 section.blocks.at(-1).items=[null];assert.equal(editableCatalog(draft),false)
 assert.ok(validateCatalog(draft).length)
})

test('DemoExam route uses its imported source and exact runtime and API contract',()=>{
 const course=catalog.editions.find(e=>e.year===2027).courses.find(c=>c.id==='information-systems')
 assert.equal(course.exampleSource,'DemoExam_WPF_PostgreSQL_2027.zip')
 assert.equal(course.dialectExamples,false)
 assert.ok(course.download.endsWith('/DemoExam_WPF_PostgreSQL_2027.zip'))
 const codes=Object.values(course.codes).join('\n')
 assert.ok(codes.includes('<TargetFramework>net9.0-windows</TargetFramework>'))
 assert.ok(codes.includes('PackageReference Include="Npgsql" Version="9.0.3"'))
 assert.ok(codes.includes('app.MapGet("/notes"'))
 assert.ok(codes.includes('title_user = reader.GetString(1) + " - " + reader.GetString(2)'))
 assert.equal(course.lessons.filter(l=>l.id!=='setup').reduce((n,l)=>n+l.time,0),210)
 const links=course.lessons.flatMap(l=>l.sections.flatMap(s=>s.blocks.filter(b=>b.type==='links').flatMap(b=>b.items)))
 for(const {url} of links)if(!/^https?:/i.test(url))assert.ok(fs.statSync('public/'+url).size>0,url)
 assert.ok(!course.lessons.some(l=>l.sections.some(s=>s.blocks.some(b=>b.type==='diagram'))),'Do not show the legacy diagram for the imported 15-table schema')
})

test('ProgrammerExam route preserves its PostgreSQL schema, price rules and source contract',()=>{
 const course=catalog.editions.find(e=>e.year===2027).courses.find(c=>c.id==='programmer')
 assert.equal(course.exampleSource,'ProgrammerExam_WPF_PostgreSQL_2027.zip')
 assert.equal(course.dialectExamples,false)
 assert.ok(course.download.endsWith('/ProgrammerExam_WPF_PostgreSQL_2027.zip'))
 assert.equal(course.lessons.filter(l=>l.id!=='setup').reduce((n,l)=>n+l.time,0),210)
 assert.deepEqual(course.lessons.map(l=>l.id),['setup','shoe-db','catalog','checkout','orders'])
 const blocks=course.lessons.flatMap(l=>l.sections.flatMap(s=>s.blocks))
 const schema=course.codes[blocks.find(b=>b.title==='Database/01_schema.sql — точная схема из архива').key]
 assert.equal((schema.match(/CREATE TABLE /g)||[]).length,10)
 assert.equal((schema.match(/REFERENCES /g)||[]).length,9)
 const codes=Object.values(course.codes).join('\n')
 for(const text of ['net9.0-windows','Version="9.0.3"','"database.json"','monthEnd.AddMonths(-1)','o.order_date >= @p0 AND o.order_date < @p1','quantity >= @p0','transaction.Commit();'])assert.ok(codes.includes(text),text)
 assert.ok(!blocks.some(b=>['diagram','shoeDemo','authDemo'].includes(b.type)),'Legacy demos do not describe the imported schema and windows')
 assert.equal(blocks.filter(b=>b.type==='image').length,8)
 for(const block of blocks.filter(b=>b.type==='image'))assert.ok(fs.statSync('public/'+block.url).size>0,block.url)
})

test('lesson screenshots survive editing and reject unsafe image URLs',()=>{
 const draft=clone(catalog),section=draft.editions[0].courses[0].lessons[0].sections[0]
 section.blocks.push({type:'image',url:'materials/example.png',alt:'Окно примера',caption:'Снимок из архива'})
 assert.deepEqual(validateCatalog(draft),[]);assert.equal(editableCatalog(draft),true)
 section.blocks.at(-1).url='data:text/html,unsafe'
 assert.ok(validateCatalog(draft).some(e=>e.includes('адрес изображения')))
 section.blocks.at(-1).alt=null;assert.equal(editableCatalog(draft),false)
})
