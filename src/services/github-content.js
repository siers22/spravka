import {validateCatalog} from '../content/catalog.js'
export const repository={owner:'siers22',name:'spravka',path:'content/catalog.json'}
export const repositoryUrl=`https://github.com/${repository.owner}/${repository.name}`
const apiRoot=`https://api.github.com/repos/${repository.owner}/${repository.name}`
export const MAX_FILE_BYTES=10*1024*1024
export function utf8Base64(text){return bytesBase64(new TextEncoder().encode(text))}
export function bytesBase64(bytes){let text='';for(let i=0;i<bytes.length;i+=8192)text+=String.fromCharCode(...bytes.subarray(i,i+8192));return btoa(text)}
export function decodeContent(content){return new TextDecoder('utf-8',{fatal:true}).decode(Uint8Array.from(atob(content.replace(/\s/g,'')),c=>c.charCodeAt(0)))}
export function assetPath(year,course,fileName,revision=crypto.randomUUID()) {
  if(!Number.isInteger(year)||year<2020||year>2100||!/^[a-z][a-z0-9-]{0,63}$/.test(course))throw new Error('Некорректный год или ID направления')
  const name=fileName.normalize('NFKC').replace(/[^a-zA-Z0-9._-]/g,'-').replace(/^\.+/,'').slice(-120)
  if(!name||!/^[-a-zA-Z0-9]+$/.test(revision))throw new Error('Некорректное имя файла')
  return `public/materials/${year}/${course}/${revision}/${name}`
}
export function validateAssets(assets){
  if(!Array.isArray(assets)||assets.length>20)throw new Error('За одну отправку можно загрузить до 20 файлов')
  const seen=new Set()
  for(const asset of assets){
    const name=asset?.path?.split('/').at(-1)||''
    if(!/^public\/materials\/\d{4}\/[a-z][a-z0-9-]{0,63}\/[a-zA-Z0-9-]+\/[-a-zA-Z0-9._]+$/.test(asset?.path||'')||!name.match(/\.(pdf|zip|docx|xlsx|xls|csv|json|sql|cs|xaml|txt|md|drawio|png|jpg|jpeg|webp)$/i)||seen.has(asset.path)||typeof asset.content!=='string'||!Number.isInteger(asset.size)||asset.size<1||asset.size>MAX_FILE_BYTES||!/^[a-zA-Z0-9+/]*={0,2}$/.test(asset.content))throw new Error('Некорректный файл для публикации')
    try{if(atob(asset.content).length!==asset.size)throw new Error()}catch{throw new Error('Размер приложения не совпадает с его содержимым')}
    seen.add(asset.path)
  }
}
export function createGitHubClient(token,fetchImpl=fetch) {
  if(typeof token!=='string'||!token.trim())throw new Error('Введите GitHub token')
  // The token lives only in this closure; never put it in URLs, storage, exported data or logs.
  async function request(path,method='GET',body){
    let response
    try{response=await fetchImpl(`${apiRoot}${path}`,{method,redirect:'error',cache:'no-store',headers:{Accept:'application/vnd.github+json',Authorization:`Bearer ${token.trim()}`,'X-GitHub-Api-Version':'2026-03-10',...(body?{'Content-Type':'application/json'}:{})},...(body?{body:JSON.stringify(body)}:{})})}catch{throw new Error('Не удалось связаться с GitHub. Проверьте соединение и повторите действие.')}
    if(!response.ok){const hints={401:'Token недействителен или истёк.',403:'Нет нужных прав, либо исчерпан лимит GitHub API.',404:'Репозиторий или каталог недоступен. Проверьте доступ token к репозиторию.',409:'В репозитории появился конфликт.',422:'GitHub отклонил данные или ветка уже существует.'};throw new Error(`GitHub ${response.status}. ${hints[response.status]||'Операция не выполнена.'}`)}
    return response.json()
  }
  async function load(){
    const repo=await request('')
    if(!repo.permissions?.push)throw new Error('У аккаунта нет права записи в этот репозиторий')
    const branch=repo.default_branch,head=await request(`/git/ref/heads/${encodeURIComponent(branch)}`)
    const baseHead=head.object.sha
    const file=await request(`/contents/${repository.path}?ref=${baseHead}`)
    const blob=file.encoding==='base64'?file:await request(`/git/blobs/${file.sha}`)
    if(blob.size>MAX_FILE_BYTES)throw new Error('Каталог слишком большой (больше 10 МБ)')
    const data=JSON.parse(decodeContent(blob.content)),errors=validateCatalog(data)
    if(errors.length)throw new Error('Каталог на GitHub не прошёл проверку: '+errors.slice(0,3).join('; '))
    return {data,baseHead,branch,fileSha:file.sha}
  }
  async function publish(data,base,assets=[],title='Обновление заданий демоэкзамена'){
    const errors=validateCatalog(data);if(errors.length)throw new Error(errors.slice(0,3).join('; '))
    const json=JSON.stringify(data,null,2)+'\n'
    if(new TextEncoder().encode(json).length>MAX_FILE_BYTES)throw new Error('Каталог больше 10 МБ. Разделите материалы перед публикацией.')
    if(!base?.baseHead||!base?.branch)throw new Error('Сначала загрузите актуальный каталог с GitHub')
    validateAssets(assets)
    const latest=await request(`/git/ref/heads/${encodeURIComponent(base.branch)}`)
    if(latest.object.sha!==base.baseHead)throw new Error('На GitHub уже есть новые изменения. Экспортируйте черновик, загрузите актуальный каталог и перенесите правки; перезапись отменена.')
    const commit=await request(`/git/commits/${base.baseHead}`)
    const files=[{path:repository.path,content:utf8Base64(json)},...assets]
    const entries=[]
    for(const file of files){const blob=await request('/git/blobs','POST',{content:file.content,encoding:'base64'});entries.push({path:file.path,mode:'100644',type:'blob',sha:blob.sha})}
    const tree=await request('/git/trees','POST',{base_tree:commit.tree.sha,tree:entries})
    const newCommit=await request('/git/commits','POST',{message:title.trim()||'Обновление заданий демоэкзамена',tree:tree.sha,parents:[base.baseHead]})
    const branch=`content/${Date.now()}-${crypto.randomUUID().slice(0,8)}`
    await request('/git/refs','POST',{ref:`refs/heads/${branch}`,sha:newCommit.sha})
    const branchUrl=`${repositoryUrl}/tree/${branch}`
    try{
      const pr=await request('/pulls','POST',{title:title.trim()||'Обновление заданий демоэкзамена',head:branch,base:base.branch,body:`Изменения каталога из редактора методички.\n\nТекущий год: ${data.currentYear}. Выпуски: ${data.editions.map(e=>e.year).sort().join(', ')}. Файлов приложено: ${assets.length}.\n\nПроверьте задания и проверки CI перед слиянием. После слияния GitHub Pages обновится автоматически.`})
      return {url:pr.html_url,branchUrl}
    }catch(error){error.branchUrl=branchUrl;error.compareUrl=`${repositoryUrl}/compare/${encodeURIComponent(base.branch)}...${branch}?expand=1`;throw error}
  }
  return {load,publish}
}
