// Shared by the browser editor and the build checks. Content is data, never HTML or executable JS.
export const slugPattern = /^[a-z][a-z0-9-]{0,63}$/
export const blockTypes = ['p','list','note','table','code','links','image','dbSetup','diagram','calculator','authDemo','shoeDemo']
const reservedLessons = new Set(['overview','glossary','resources','cheatsheet'])
export const clone = value => JSON.parse(JSON.stringify(value))
export function safeUrl(value) {
  if (typeof value !== 'string' || !value.trim() || /[\\\s\u0000-\u001f]/.test(value)) return false
  if (/^https?:\/\//i.test(value)) { try { const url=new URL(value);return !url.username&&!url.password } catch { return false } }
  return !value.startsWith('/') && !/[?#:%]/.test(value) && value.split('/').every(part=>part&&part!=='.'&&part!=='..')
}
// Local drafts may contain unfinished values. Recover them as long as the editor structure is intact.
export function editableCatalog(data) {
  const every=(value,predicate)=>Array.isArray(value)&&value.every(predicate)
  const block=b=>b&&typeof b.type==='string'&&(b.type!=='image'||typeof b.url==='string'&&typeof b.alt==='string')&&(b.type!=='list'||Array.isArray(b.items))&&(b.type!=='links'||every(b.items,v=>v&&typeof v.title==='string'&&typeof v.url==='string'))&&(b.type!=='table'||every(b.headers,v=>typeof v==='string')&&every(b.rows,Array.isArray))
  const section=s=>s&&every(s.blocks,block)
  const lesson=l=>l&&Array.isArray(l.checks)&&every(l.sections,section)
  const course=c=>c&&typeof c.id==='string'&&c.codes&&typeof c.codes==='object'&&Array.isArray(c.glossary)&&Array.isArray(c.resources)&&every(c.lessons,lesson)
  return !!(data?.schemaVersion===1&&Array.isArray(data.editions)&&data.editions.some(e=>e?.year===data.currentYear)&&data.editions.every(e=>e&&every(e.courses,course)))
}
export function validateCatalog(data) {
  const errors=[]
  const fail=(path,message)=>errors.push(`${path}: ${message}`)
  const text=(value,path,required=false)=>{if(typeof value!=='string'||value.length>500000||(required&&!value.trim()))fail(path,'нужен текст'+(required?' (не пустой)':''))}
  const list=(value,path)=>{if(!Array.isArray(value)){fail(path,'нужен список');return []}return value}
  const unique=(items,path,key)=>{const seen=new Set();items.forEach(item=>{const id=item?.[key];if(seen.has(id))fail(path,`повтор ${id}`);seen.add(id)})}
  const slug=(value,path)=>{if(!slugPattern.test(value||''))fail(path,'ID: латинские буквы, цифры и дефис; первая — буква')}
  if(!data||data.schemaVersion!==1)return ['Неизвестная версия каталога (нужна schemaVersion: 1)']
  const editions=list(data.editions,'Выпуски');unique(editions,'Выпуски','year')
  if(!editions.length)fail('Выпуски','нужен хотя бы один год')
  for(const edition of editions){
    if(!edition||!Number.isInteger(edition.year)||edition.year<2020||edition.year>2100){fail('Год','допустимо 2020–2100');continue}
    const path=String(edition.year),courses=list(edition.courses,path);unique(courses,path,'id')
    for(const c of courses){
      if(!c||typeof c!=='object'){fail(path,'направление должно быть объектом');continue}
      const p=path+'/'+(c.id||'?'),published=c.status==='published';slug(c.id,p)
      if(['admin','archive','specialties','year'].includes(c.id))fail(p,'зарезервированный ID')
      if(!['draft','published'].includes(c.status))fail(p,'статус draft или published')
      for(const field of ['title','short','kim','subtitle','domain','description','verification','paTime','giaTime','stack'])text(c[field],p+'/'+field,published&&['title','short','kim'].includes(field))
      for(const field of ['paCount','giaPoints','paPoints','pages'])if(!Number.isInteger(c[field])||c[field]<0)fail(p+'/'+field,'нужно целое неотрицательное число')
      if(c.download&&!safeUrl(c.download))fail(p+'/download','небезопасный адрес')
      if(c.dialectExamples!==undefined&&typeof c.dialectExamples!=='boolean')fail(p+'/dialectExamples','нужно логическое значение');
      if(typeof c.download!=='string')fail(p+'/download','нужна строка')
      for(const field of ['downloadLabel','downloadType'])if(c[field]!==undefined)text(c[field],p+'/'+field)
      const lessons=list(c.lessons,p+'/задания');unique(lessons,p+'/задания','id')
      const taskCount=lessons.filter(l=>l?.id!=='setup').length
      if(published&&!taskCount)fail(p,'для публикации нужно хотя бы одно задание')
      if(c.paCount>taskCount)fail(p+'/paCount','больше количества заданий')
      const codes=c.codes&&typeof c.codes==='object'&&!Array.isArray(c.codes)?c.codes:{}
      if(c.codes!==codes)fail(p+'/codes','нужен объект примеров кода')
      for(const [key,value] of Object.entries(codes))if(typeof value!=='string'&&!(value&&typeof value.postgres==='string'&&typeof value.mssql==='string'))fail(p+'/codes/'+key,'нужен текст либо варианты postgres/mssql')
      for(const l of lessons){
        if(!l){fail(p,'пустое задание');continue}const lp=p+'/'+(l.id||'?');slug(l.id,lp)
        if(reservedLessons.has(l.id))fail(lp,'зарезервированный ID')
        for(const field of ['title','short','number','icon','tag','summary','deliverable','source'])text(l[field],lp+'/'+field,published&&['title','number'].includes(field))
        if(!Number.isFinite(l.time)||l.time<0)fail(lp+'/time','минуты должны быть неотрицательным числом')
        const checks=list(l.checks,lp+'/чек-лист');checks.forEach(v=>text(v,lp+'/чек-лист',published));if(published&&!checks.length)fail(lp,'нужен чек-лист')
        const sections=list(l.sections,lp+'/разделы');unique(sections,lp+'/разделы','id');if(published&&!sections.length)fail(lp,'нужен раздел')
        for(const s of sections){if(!s){fail(lp,'пустой раздел');continue}const sp=lp+'/'+(s.id||'?');slug(s.id,sp);if(s.id==='checklist')fail(sp,'ID занят чек-листом');text(s.title,sp,published)
          const blocks=list(s.blocks,sp+'/блоки');if(published&&!blocks.length)fail(sp,'нужен блок содержимого')
          for(const b of blocks){
            if(!b||!blockTypes.includes(b.type)){fail(sp,'неизвестный тип блока');continue}
            if(['p','note'].includes(b.type))text(b.text,sp,published)
            if(b.type==='note'){text(b.title,sp,published);if(!['info','warning','success'].includes(b.tone))fail(sp,'неизвестный стиль примечания')}
            if(b.type==='list')list(b.items,sp).forEach(v=>text(v,sp,published))
            if(b.type==='links')list(b.items,sp).forEach(v=>{if(!v||!safeUrl(v.url))fail(sp,'небезопасный адрес ссылки');if(v){text(v.title,sp,published);if(v.description!==undefined)text(v.description,sp)}})
            if(b.type==='image'){if(!safeUrl(b.url))fail(sp,'небезопасный адрес изображения');text(b.alt,sp,published);if(b.caption!==undefined)text(b.caption,sp)}
            if(b.type==='table'){const headers=list(b.headers,sp);headers.forEach(v=>text(v,sp));list(b.rows,sp).forEach(row=>{if(!Array.isArray(row)||row.length!==headers.length)fail(sp,'в строке таблицы неверное число ячеек');else row.forEach(v=>text(v,sp))})}
            if(b.type==='code'){if(typeof b.text!=='string'&&!(typeof b.key==='string'&&Object.hasOwn(codes,b.key)))fail(sp,'нет текста или примера кода с указанным ключом');if(b.text!==undefined)text(b.text,sp);if(b.title!==undefined)text(b.title,sp);if(b.language!==undefined)text(b.language,sp)}
            if(b.type==='dbSetup'&&b.name!==undefined&&!/^[a-z_][a-z0-9_]*$/i.test(b.name))fail(sp,'имя БД: латинские буквы, цифры и подчёркивание')
          }
        }
      }
      list(c.glossary,p+'/словарь').forEach(row=>{if(!Array.isArray(row)||row.length!==2)fail(p,'термин должен содержать название и определение');else row.forEach(v=>text(v,p))})
      list(c.resources,p+'/файлы').forEach(r=>{if(!r||!safeUrl(r.url))fail(p,'небезопасный адрес файла');if(r)for(const field of ['title','description','type'])text(r[field],p,published&&field==='title')})
    }
  }
  const current=editions.find(e=>e?.year===data.currentYear)
  if(!current||!current.courses?.some(c=>c?.status==='published'))fail('Текущий год','выберите выпуск с опубликованным направлением')
  return errors
}
export function newLesson(id='task-1',number='01') {
  return {id,number,title:'Новое задание',short:'',icon:'FileText',time:0,tag:'Задание',summary:'',deliverable:'',source:'',sections:[{id:'instructions',title:'Условие и порядок работы',blocks:[{type:'p',text:'Добавьте условие задания.'}]}],checks:['Проверить результат работы']}
}
export function newCourse(id='new-specialty') {
  return {id,status:'draft',title:'Новое направление',short:'Новое направление',kim:'',subtitle:'',domain:'',description:'',verification:'',pages:0,paCount:1,paTime:'',giaTime:'',paPoints:0,giaPoints:0,stack:'',download:'',downloadLabel:'Основной файл',downloadType:'',codes:{},glossary:[],resources:[],lessons:[newLesson()]}
}
export function copyEdition(data,fromYear,toYear) {
  if(!Number.isInteger(toYear)||toYear<2020||toYear>2100||data.editions.some(e=>e.year===toYear))throw new Error('Укажите новый год от 2020 до 2100, которого ещё нет в каталоге')
  const source=data.editions.find(e=>e.year===fromYear);if(!source)throw new Error('Исходный выпуск не найден')
  const edition=clone(source);edition.year=toYear
  edition.courses.forEach(c=>{c.status='draft';c.kim=c.kim.replace(String(fromYear),String(toYear))})
  data.editions.push(edition);return edition
}
