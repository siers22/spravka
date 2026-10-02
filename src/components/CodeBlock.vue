<script setup>
import { computed, ref } from 'vue'
import { Copy, Check, ChevronDown, ChevronUp } from 'lucide-vue-next'
import Prism from 'prismjs'
import 'prismjs/components/prism-csharp'
import 'prismjs/components/prism-sql'
import 'prismjs/components/prism-json'
import 'prismjs/components/prism-powershell'
import 'prismjs/components/prism-markdown'
const props=defineProps({code:String,title:String,language:{type:String,default:'csharp'}})
const expanded=ref(false), status=ref('')
const long=computed(()=>props.code.split('\n').length>26)
const highlighted=computed(()=>Prism.highlight(props.code,Prism.languages[props.language]||Prism.languages.plain,props.language))
async function copy(){try{await navigator.clipboard.writeText(props.code);status.value='Скопировано';setTimeout(()=>status.value='',2200)}catch{status.value='Выделите код и нажмите Ctrl+C'}}
</script>
<template>
<div class="code-block"><div class="code-head"><span>{{title}}</span><button @click="copy" :aria-label="'Копировать '+title"><Check v-if="status==='Скопировано'" :size="14"/><Copy v-else :size="14"/><span>{{status||'Копировать'}}</span></button></div>
<pre :class="{collapsed:long&&!expanded}" tabindex="0"><code :class="'language-'+language" v-html="highlighted"></code></pre>
<button v-if="long" class="expand-code" @click="expanded=!expanded"><ChevronUp v-if="expanded" :size="15"/><ChevronDown v-else :size="15"/>{{expanded?'Свернуть код':'Показать весь файл'}} <span>{{code.split('\n').length}} строк</span></button><span class="sr-only" role="status">{{status}}</span></div>
</template>
