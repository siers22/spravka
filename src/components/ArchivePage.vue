<script setup>
import {computed,ref} from 'vue'
import {Archive,ArrowUpRight,CalendarDays} from 'lucide-vue-next'
import {route} from '../data/course-meta.js'
const props=defineProps({catalog:Object})
const query=ref('')
const editions=computed(()=>[...props.catalog.editions].sort((a,b)=>b.year-a.year).map(e=>({...e,courses:e.courses.filter(c=>c.status==='published'&&(`${e.year} ${c.title} ${c.kim} ${c.domain}`).toLocaleLowerCase('ru').includes(query.value.trim().toLocaleLowerCase('ru')))})).filter(e=>e.courses.length))
</script>
<template>
<div class="catalog-page"><div class="page-heading"><div class="eyebrow"><Archive :size="16"/> МАТЕРИАЛЫ ПО ГОДАМ</div><h1>Архив заданий.</h1><p class="page-subtitle">Каждый выпуск хранится отдельно. Здесь можно посмотреть задания, решения и исходные файлы прошлых лет.</p></div>
<label class="catalog-search">Найти год или направление<input v-model="query" type="search" placeholder="2027, информационные системы…"></label>
<div v-if="!catalog.editions.some(e=>e.year!==catalog.currentYear&&e.courses.some(c=>c.status==='published'))" class="catalog-notice">Сейчас опубликован первый выпуск — {{catalog.currentYear}}. Когда появится следующий год, этот выпуск останется здесь со своими заданиями и файлами.</div>
<section v-for="edition in editions" :key="edition.year" class="archive-edition"><div class="archive-year"><h2><CalendarDays :size="24"/>{{edition.year}}</h2><span class="catalog-badge">{{edition.year===catalog.currentYear?'Текущий выпуск':edition.year>catalog.currentYear?'Следующий выпуск':'Архив'}}</span></div><div class="archive-grid"><a v-for="course in edition.courses" :key="course.id" :href="route(course.id,'overview','',edition.year)" class="archive-card"><small>КИМ {{course.kim}}</small><h3>{{course.title}}</h3><p>{{course.description}}</p><span>{{course.lessons.filter(l=>l.id!=='setup').length}} заданий · {{course.resources.length}} файлов</span><b>Открыть выпуск <ArrowUpRight :size="17"/></b></a></div></section>
<p v-if="!editions.length" class="catalog-notice">По этому запросу ничего не найдено.</p>
</div>
</template>
