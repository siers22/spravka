<script setup>
import { computed,ref } from 'vue'
import { Minus,Plus,Equal } from 'lucide-vue-next'
const quantity=ref(2)
const materials=[['Столешница круглая',1,3250,'шт'],['Деталь 500 × 800',2,95,'шт'],['Деталь 600 × 800',4,140,'шт'],['Евровинт 6,5 × 5',0.012,595,'тыс. шт'],['Опора',4,245,'шт']]
const operations=[['Сборка модулей',0.75,1400],['Распил',1.5,450],['Упаковка',0.5,950]]
const valid=computed(()=>Number.isInteger(Number(quantity.value))&&Number(quantity.value)>=1&&Number(quantity.value)<=10000)
const costMaterials=materials.reduce((sum,item)=>sum+item[1]*item[2],0), costOperations=operations.reduce((sum,item)=>sum+item[1]*item[2],0)
const money=value=>new Intl.NumberFormat('ru-RU',{minimumFractionDigits:2,maximumFractionDigits:2}).format(value)
function adjust(delta){quantity.value=Math.min(10000,Math.max(1,(Number(quantity.value)||1)+delta))}
</script>
<template><div class="cost-lab"><div class="lab-heading"><span class="mini-tag">РАЗБЕРЁМ НА ПРИМЕРЕ</span><span>Стол «Самобранка»</span></div>
<div class="cost-tables"><div><h4>Материалы на 1 стол</h4><div v-for="row in materials" :key="row[0]" class="cost-row"><span>{{row[0]}}<small>{{row[1]}} {{row[3]}} × {{row[2]}} ₽</small></span><b>{{money(row[1]*row[2])}}</b></div><div class="cost-subtotal">Итого <b>{{money(costMaterials)}} ₽</b></div></div>
<div><h4>Операции на 1 стол</h4><div v-for="row in operations" :key="row[0]" class="cost-row"><span>{{row[0]}}<small>норма {{row[1]}} × {{row[2]}} ₽ × 1</small></span><b>{{money(row[1]*row[2])}}</b></div><div class="cost-subtotal">Итого <b>{{money(costOperations)}} ₽</b></div><p class="quiet">Сначала сумма на одно изделие.<br>Потом умножение на количество.</p></div></div>
<div class="cost-total"><div><label for="quantity">Столов в заказе</label><div class="quantity"><button @click="adjust(-1)" aria-label="Уменьшить количество" :disabled="Number(quantity)<=1"><Minus :size="14"/></button><input id="quantity" v-model="quantity" type="number" min="1" max="10000" step="1"><button @click="adjust(1)" aria-label="Увеличить количество"><Plus :size="14"/></button></div></div><Equal :size="20"/><div class="cost-answer" aria-live="polite"><small>Стоимость заказа</small><strong>{{valid?money((costMaterials+costOperations)*Number(quantity))+' ₽':'Введите 1–10 000'}}</strong><span v-if="valid">{{money(costMaterials+costOperations)}} × {{quantity}}</span></div></div></div></template>
