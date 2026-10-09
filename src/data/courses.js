import catalogData from '../../content/catalog.json'
import {publicUrl} from '../utils/public-url.js'
export const catalog=catalogData
export function coursesForYear(year) {
  return Object.fromEntries((catalog.editions.find(e=>e.year===year)?.courses||[]).filter(c=>c.status==='published').map(c=>[c.id,{...c,download:c.download?publicUrl(c.download):'',resources:c.resources.map(r=>({...r,url:publicUrl(r.url)}))}]))
}
export const courses=coursesForYear(catalog.currentYear)
