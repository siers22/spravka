const databaseName='spravka-editor-v1'
function open(){return new Promise((resolve,reject)=>{const request=indexedDB.open(databaseName,1);request.onupgradeneeded=()=>request.result.createObjectStore('drafts');request.onsuccess=()=>resolve(request.result);request.onerror=()=>reject(request.error)})}
async function transaction(mode,action){const db=await open();try{return await new Promise((resolve,reject)=>{const tx=db.transaction('drafts',mode),request=action(tx.objectStore('drafts'));let value;request.onsuccess=()=>{value=request.result};tx.oncomplete=()=>resolve(value);tx.onerror=()=>reject(tx.error);tx.onabort=()=>reject(tx.error)})}finally{db.close()}}
const key=()=>location.pathname
export const readDraft=()=>transaction('readonly',store=>store.get(key()))
export const writeDraft=value=>transaction('readwrite',store=>store.put(value,key()))
export const deleteDraft=()=>transaction('readwrite',store=>store.delete(key()))
