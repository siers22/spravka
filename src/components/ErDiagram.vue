<script setup>
defineProps({downloadUrl:String})
const entities=[
{name:'customers',label:'Заказчики',fields:['PK id','name · inn · address','phone · customer_type']},
{name:'customer_orders',label:'Заказы покупателя',fields:['PK id','FK customer_id → customers','order_number · order_date','executor']},
{name:'customer_order_items',label:'Строки заказа',fields:['PK id','FK order_id → customer_orders','FK product_id → products','quantity · sale_price','discount_amount']},
{name:'products',label:'Изделия',fields:['PK id','code · name · unit']},
{name:'specification_materials',label:'Материалы изделия',fields:['PK, FK product_id → products','PK, FK material_id → materials','consumption']},
{name:'materials',label:'Справочник материалов',fields:['PK id','code · name · unit · price']},
{name:'specification_operations',label:'Операции изделия',fields:['PK, FK product_id → products','PK, FK operation_id → operations','time_norm · quantity']},
{name:'operations',label:'Справочник операций',fields:['PK id','code · name · unit · price']}
]
</script>
<template><div class="er-diagram"><div class="lab-heading"><span class="mini-tag">ОСНОВА СХЕМЫ</span><span>PK — ключ · FK — ссылка</span></div><div class="er-grid"><div v-for="(entity,index) in entities" :key="entity.name" :class="['entity','entity-'+index]"><div class="entity-title"><strong>{{entity.name}}</strong><small>{{entity.label}}</small></div><div class="entity-fields"><p v-for="field in entity.fields" :key="field" :class="{'key-field':field.includes('PK')||field.includes('FK')}">{{field}}</p></div></div></div><div class="relations"><span>customers <b>1 → N</b> customer_orders</span><span>customer_orders <b>1 → N</b> customer_order_items</span><span>products <b>1 → N</b> customer_order_items</span><span>products / materials <b>1 → N</b> specification_materials</span><span>products / operations <b>1 → N</b> specification_operations</span></div><p class="diagram-caption">Компактная учебная схема. Производственные таблицы и их связи перечислены ниже; полная модель — в SQL.</p><a v-if="downloadUrl" class="diagram-download" :href="downloadUrl" download>Скачать полную ER-схему · .drawio →</a></div></template>
