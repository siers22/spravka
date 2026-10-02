from pathlib import Path
import json,re,shutil
sheets={s['file']:s['rows'][1:] for s in json.loads(Path('tmp/programmers/sheets.json').read_text())}
def norm(s):return re.sub(r'\s+',' ',str(s).replace('\u00a0',' ')).strip()
def name(s):
 s=norm(s)
 return 'Черные туфли в классическом стиле — база для деловых образов' if s=='Черные туфли в классическом стиле' else s
rows={t:[] for t in ['categories','subcategories','manufacturers','products','sizes','stock_items','users','orders','order_items']}
cat={};sub={};maker={};products={};users={};sku={};orders={}
for r in sheets['Users_import.xlsx']:
 ident=len(users)+1;ln,fn,pn,login,role=map(norm,r);users[f'{ln} {fn} {pn}']=ident
 rows['users'].append([ident,login,ln,fn,pn,{'Администратор':'Admin','Менеджер':'Manager','Авторизованный пользователь':'User'}[role]])
for r in sheets['Sizes_import.xlsx']:rows['sizes'].append([r[0]])
for r in sheets['Products_import.xlsx']:
 c,s,img,n,m,description,composition,price=r;c,s,m=map(norm,[c,s,m]);n=name(n)
 if c not in cat:cat[c]=len(cat)+1;rows['categories'].append([cat[c],c])
 if (cat[c],s) not in sub:sub[cat[c],s]=len(sub)+1;rows['subcategories'].append([sub[cat[c],s],cat[c],s])
 if m not in maker:maker[m]=len(maker)+1;rows['manufacturers'].append([maker[m],m])
 ident=len(products)+1;products[n,m]=ident
 rows['products'].append([ident,sub[cat[c],s],maker[m],n,norm(img),description,composition,price])
for n,m,size,qty in sheets['Stock_Items_import.xlsx']:
 size=float(size);pid=products[name(n),norm(m)];ident=len(sku)+1;sku[pid,size]=ident;rows['stock_items'].append([ident,pid,size,qty])
for oid,date,fio,c,n,m,size,qty,price in sheets['Orders_import.xlsx']:
 if oid not in orders:orders[oid]=True;rows['orders'].append([oid,date[:10],users[norm(fio)]])
 rows['order_items'].append([len(rows['order_items'])+1,oid,sku[products[name(n),norm(m)],float(size)],qty,price])
for provider in ['postgres','mssql']:
 def literal(v):
  if isinstance(v,str):return ('N' if provider=='mssql' else '')+"'"+v.replace("'","''")+"'"
  return str(v)
 statements=['-- Исходные XLSX: нормализованы пробелы и сопоставлено сокращённое название модели.','-- Stock_Items уже содержит доступные остатки. Старые заказы повторно не списываются.','BEGIN TRANSACTION;' if provider=='mssql' else 'BEGIN;']
 for table,values in rows.items():
  for row in values:statements.append(f'INSERT INTO {table} VALUES('+','.join(literal(v) for v in row)+');')
 statements.append('COMMIT;')
 Path(f'examples/ShoeStore/sql/{provider}-data.sql').write_text('\n'.join(statements)+'\n')
 Path(f'examples/ShoeStore/sql/{provider}-complete.sql').write_text(Path(f'examples/ShoeStore/sql/{provider}-schema.sql').read_text()+'\n'+'\n'.join(statements)+'\n')
# Browser training fixture, derived from the same normalized source; no backend needed to read the guide.
preview=[]
for ident,sid,mid,n,img,desc,composition,price in rows['products']:
 category=next(c for (cid,s),value in sub.items() if value==sid for c,cvalue in cat.items() if cvalue==cid)
 sold=sorted({date[:10] for oid,date,uid in rows['orders'] if any(line[1]==oid and next(sr[1] for sr in rows['stock_items'] if sr[0]==line[2])==ident for line in rows['order_items'])})
 preview.append(dict(id=ident,name=n,category=category,manufacturer=next(m for m,v in maker.items() if v==mid),image=img,description=desc,price=price,available=sum(sr[3] for sr in rows['stock_items'] if sr[1]==ident),orderedDates=sold))
Path('src/data/shoes.json').write_text(json.dumps(preview,ensure_ascii=False,indent=2))
print({k:len(v) for k,v in rows.items()})
