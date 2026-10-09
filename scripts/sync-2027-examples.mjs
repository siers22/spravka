// Explicitly refresh the historical 2027 teaching examples; never silently alter future releases.
import {createServer} from 'vite'
import fs from 'node:fs'
import path from 'node:path'
import {createHash} from 'node:crypto'
import {validateCatalog} from '../src/content/catalog.js'
const data=JSON.parse(fs.readFileSync('content/catalog.json','utf8'))
if(data.currentYear!==2027)throw new Error('2027 уже в архиве. Обновите код нового года через редактор; архивные примеры автоматически не меняются.')
const server=await createServer({server:{middlewareMode:true,hmr:false,ws:false}})
try{
 const [{courseMeta},systems,programmer,{codes:systemsCodes},{codes:programmerCodes}]=await Promise.all([
  server.ssrLoadModule('/src/data/course-meta.js'),server.ssrLoadModule('/src/data/lessons.js'),server.ssrLoadModule('/src/data/programmer-lessons.js'),server.ssrLoadModule('/src/data/code.js'),server.ssrLoadModule('/src/data/programmer-code.js')
 ])
 for(const [id,source,codes] of [['information-systems',systems,systemsCodes],['programmer',programmer,programmerCodes]]){
  const target=data.editions.find(e=>e.year===2027)?.courses.find(c=>c.id===id);if(!target)continue
  // Imported solutions have their own code and assets; legacy examples must not replace them.
  if(target.exampleSource)continue
  target.codes=JSON.parse(JSON.stringify(codes))
  function copy(url){const relative=url.replace(/^\//,''),bytes=fs.readFileSync(path.join('public',relative)),hash=createHash('sha256').update(bytes).digest('hex').slice(0,16),destination=`materials/2027/${id}/versions/${hash}-${path.basename(relative)}`;fs.mkdirSync(path.dirname(path.join('public',destination)),{recursive:true});fs.writeFileSync(path.join('public',destination),bytes);return destination}
  target.download=copy(courseMeta[id].download)
  for(const resource of source.resources){const existing=target.resources.find(r=>r.title===resource.title);if(existing)existing.url=copy(resource.url)}
 }
 const errors=validateCatalog(data);if(errors.length)throw new Error(errors.join('\n'))
 fs.writeFileSync('content/catalog.json',JSON.stringify(data,null,2)+'\n')
 console.log('OK: 2027 code snapshots and versioned example files refreshed; lesson text unchanged')
}finally{await server.close()}
