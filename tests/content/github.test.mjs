import test from 'node:test'
import assert from 'node:assert/strict'
import fs from 'node:fs'
import {createGitHubClient,utf8Base64,decodeContent,assetPath,validateAssets} from '../../src/services/github-content.js'
const data=JSON.parse(fs.readFileSync('content/catalog.json','utf8'))
const base={baseHead:'base-head',branch:'main',fileSha:'catalog-sha'}
function mock(responses){const calls=[];return {calls,fetch:async(url,options)=>{calls.push({url,options});assert.ok(url.startsWith('https://api.github.com/repos/siers22/spravka'));assert.equal(options.redirect,'error');const response=responses.shift();if(response instanceof Error)throw response;if(!response)throw new Error('Unexpected call');return {ok:response.status?response.status<400:true,status:response.status||200,json:async()=>response.body||response}}}}
test('UTF-8 catalog encoding preserves Russian text and emoji',()=>assert.equal(decodeContent(utf8Base64('КИМ 2028 · 💾\nSQL')),'КИМ 2028 · 💾\nSQL'))
test('load pins the catalog to the default branch commit and supports large Git blobs',async()=>{
 const transport=mock([{permissions:{push:true},default_branch:'main'},{object:{sha:base.baseHead}},{sha:base.fileSha,encoding:'none'},{content:utf8Base64(JSON.stringify(data)),size:259632}])
 const loaded=await createGitHubClient('test-token',transport.fetch).load()
 assert.equal(loaded.baseHead,base.baseHead);assert.equal(loaded.fileSha,base.fileSha);assert.deepEqual(loaded.data,data)
 assert.ok(transport.calls[2].url.endsWith('?ref=base-head'));assert.ok(transport.calls[3].url.endsWith('/git/blobs/catalog-sha'))
})
test('reader accounts cannot enable GitHub publishing',async()=>{
 const transport=mock([{permissions:{push:false},default_branch:'main'}]);await assert.rejects(createGitHubClient('test-token',transport.fetch).load(),/нет права записи/);assert.equal(transport.calls.length,1)
})
test('a concurrent commit blocks all writes instead of overwriting other editors',async()=>{
 const transport=mock([{object:{sha:'new-head'}}]);await assert.rejects(createGitHubClient('test-token',transport.fetch).publish(data,base),/новые изменения/);assert.equal(transport.calls.length,1);assert.equal(transport.calls[0].options.method,'GET')
})
test('catalog and uploaded files publish as one commit in a separate branch and PR',async()=>{
 const asset={path:assetPath(2028,'network-admin','kim.pdf','revision-1'),size:4,content:utf8Base64('test')}
 const transport=mock([{object:{sha:base.baseHead}},{tree:{sha:'old-tree'}},{sha:'catalog-blob'},{sha:'asset-blob'},{sha:'new-tree'},{sha:'new-commit'},{ref:'created'},{html_url:'https://github.com/siers22/spravka/pull/123'}])
 const result=await createGitHubClient('test-token',transport.fetch).publish(data,base,[asset],'Задания 2028')
 assert.equal(result.url,'https://github.com/siers22/spravka/pull/123')
 const bodies=transport.calls.filter(c=>c.options.body).map(c=>JSON.parse(c.options.body))
 assert.equal(decodeContent(bodies[0].content),JSON.stringify(data,null,2)+'\n');assert.equal(bodies[2].base_tree,'old-tree');assert.deepEqual(bodies[2].tree.map(f=>f.path),['content/catalog.json',asset.path]);assert.deepEqual(bodies[3].parents,['base-head']);assert.ok(bodies[4].ref.startsWith('refs/heads/content/'));assert.equal(bodies[5].base,'main');assert.ok(!bodies.some(b=>JSON.stringify(b).includes('test-token')))
})
test('failed PR creation keeps a recovery link to the committed branch',async()=>{
 const transport=mock([{object:{sha:base.baseHead}},{tree:{sha:'old-tree'}},{sha:'blob'},{sha:'tree'},{sha:'commit'},{ref:'created'},{status:403,body:{message:'Forbidden'}}])
 await assert.rejects(createGitHubClient('test-token',transport.fetch).publish(data,base),error=>error.message.includes('403')&&error.branchUrl.startsWith('https://github.com/siers22/spravka/tree/content/')&&error.compareUrl.includes('expand=1'))
})
test('unsafe imports and oversized or mislabeled assets are rejected before network writes',async()=>{
 for(const asset of [{path:'../x',size:1,content:'eA=='},{path:'public/materials/2028/test/rev/evil.html',size:1,content:'eA=='},{path:'public/materials/2028/test/rev/test.pdf',size:5,content:'eA=='}])assert.throws(()=>validateAssets([asset]))
 const transport=mock([]);const invalid=structuredClone(data);invalid.currentYear=1900;await assert.rejects(createGitHubClient('test-token',transport.fetch).publish(invalid,base));assert.equal(transport.calls.length,0)
 assert.ok(!assetPath(2028,'test','../../КИМ.pdf','r-1').includes('/../'))
})
test('API failures report a bounded message without exposing credentials',async()=>{
 const transport=mock([{status:401,body:{message:'test-token'}}]);await assert.rejects(createGitHubClient('test-token',transport.fetch).load(),error=>error.message.includes('401')&&!error.message.includes('test-token'))
})
