<script setup>
import { computed } from 'vue'
import {publicUrl} from '../utils/public-url.js'
import {safeUrl} from '../content/catalog.js'
import { Info,TriangleAlert } from 'lucide-vue-next'
import CodeBlock from './CodeBlock.vue'
import CostCalculator from './CostCalculator.vue'
import AuthDemo from './AuthDemo.vue'
import ErDiagram from './ErDiagram.vue'
import ShoeDemo from './ShoeDemo.vue'
const props=defineProps({blocks:Array,db:String,codeSet:Object,resources:Array})
const diagramDownload=computed(()=>{const file=props.resources?.find(r=>r.type==='DRAWIO'&&/er-diagram\.drawio$/.test(r.url));return file?(file.url.startsWith('/')?file.url:publicUrl(file.url)):''})
const databaseName=block=>block.name||'exam_demo'
const linkUrl=url=>safeUrl(url)?(/^https?:\/\//i.test(url)?url:publicUrl(url)):undefined
const sqlCreate=block=>props.db==='postgres'?`CREATE DATABASE ${databaseName(block)};`:`CREATE DATABASE ${databaseName(block)};\nGO\nUSE ${databaseName(block)};\nGO`
function getCode(block){if(typeof block.text==='string')return block.text;const value=(props.codeSet||{})[block.key];return typeof value==='string'?value:value?.[props.db]||''}
</script>
<template><template v-for="(block,index) in blocks" :key="index">
<p v-if="block.type==='p'">{{block.text}}</p>
<ul v-else-if="block.type==='links'" class="lesson-links"><li v-for="item in block.items" :key="item.url"><a :href="linkUrl(item.url)" target="_blank" rel="noopener">{{item.title}}</a><p v-if="item.description">{{item.description}}</p></li></ul>
<figure v-else-if="block.type==='image'" class="lesson-image"><a :href="linkUrl(block.url)" target="_blank" rel="noopener"><img :src="linkUrl(block.url)" :alt="block.alt" loading="lazy"></a><figcaption v-if="block.caption">{{block.caption}}</figcaption></figure>
<ol v-else-if="block.type==='list'" class="step-list"><li v-for="(item,i) in block.items" :key="i"><span>{{String(i+1).padStart(2,'0')}}</span><div>{{item}}</div></li></ol>
<aside v-else-if="block.type==='note'" :class="['callout',block.tone]"><TriangleAlert v-if="block.tone==='warning'" :size="19"/><Info v-else :size="19"/><div><strong>{{block.title}}</strong><p>{{block.text}}</p></div></aside>
<div v-else-if="block.type==='table'" class="table-scroll"><table><thead><tr><th v-for="(header,i) in block.headers" :key="i">{{header}}</th></tr></thead><tbody><tr v-for="(row,i) in block.rows" :key="i"><td v-for="(cell,j) in row" :key="j">{{cell}}</td></tr></tbody></table></div>
<CodeBlock v-else-if="block.type==='code'" :code="getCode(block)" :title="block.title" :language="block.language"/>
<ShoeDemo v-else-if="block.type==='shoeDemo'"/>
<CostCalculator v-else-if="block.type==='calculator'"/>
<AuthDemo v-else-if="block.type==='authDemo'"/>
<ErDiagram v-else-if="block.type==='diagram'" :download-url="diagramDownload"/>
<template v-else-if="block.type==='dbSetup'"><p v-if="db==='postgres'">В pgAdmin: подключение → Databases → Create → Database → {{databaseName(block)}}. Либо выполните CREATE DATABASE через Query Tool, подключённый к postgres. Затем откройте Query Tool уже у {{databaseName(block)}}: команда CREATE DATABASE не переключает текущую базу.</p><p v-else>В SSMS: подключитесь к экземпляру с Windows Authentication → New Query → выполните скрипт ниже. Затем убедитесь, что сверху выбрана {{databaseName(block)}}. Если у вас LocalDB, подключение называется (localdb)\MSSQLLocalDB.</p><CodeBlock title="Создание базы" :code="sqlCreate(block)" language="sql"/></template>
</template></template>
