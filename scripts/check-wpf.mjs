import fs from 'node:fs'
import assert from 'node:assert/strict'
import {lessons as systems} from '../src/data/lessons.js'
import {lessons as programmer} from '../src/data/programmer-lessons.js'
for (const [lessons, file] of [[systems,'src/data/code.js'],[programmer,'src/data/programmer-code.js']]) {
  const source=fs.readFileSync(file,'utf8')
  const names=new Set([...source.matchAll(/import\s+(\w+)\s+from/g)].map(m=>m[1]))
  for (const m of source.matchAll(/from\s+['"](.+?)\?raw['"]/g)) assert.ok(fs.existsSync(new URL(m[1],new URL('../'+file,import.meta.url))),m[1])
  for (const lesson of lessons) for(const section of lesson.sections) for(const block of section.blocks) {
    if(block.type==='code'&&block.key) assert.ok(names.has(block.key)||['schema','seed','cost','discountSql'].includes(block.key),`${lesson.id}/${section.id}: missing ${block.key}`)
  }
  assert.ok(!JSON.stringify(lessons).match(/Razor|\.cshtml|Cookie|Session|wwwroot/),'Course should teach WPF throughout')
}
for(const name of ['ExamGuide','ShoeStore']) {
  const project=fs.readFileSync(`examples/${name}/${name}.csproj`,'utf8')
  assert.ok(project.includes('<UseWPF>true</UseWPF>')&&project.includes('net10.0-windows')&&project.includes('WinExe'))
  assert.ok(project.includes('CopyToPublishDirectory'),'Images/data/config must survive Windows publish')
  assert.ok(fs.existsSync(`examples/${name}/App.xaml`)&&fs.existsSync(`examples/${name}/MainWindow.xaml`))
}
console.log('OK: WPF projects, all lesson code references and consistent Windows instructions')
