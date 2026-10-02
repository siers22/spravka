from pathlib import Path
from xml.etree.ElementTree import Element, SubElement, ElementTree
import re,html
schema=Path('examples/ExamGuide/sql/postgres-schema.sql').read_text()
tables=re.findall(r'CREATE TABLE (\w+) \((.*?)\n\);',schema,re.S)
root=Element('mxfile',host='app.diagrams.net',type='device');diagram=SubElement(root,'diagram',name='Полная ER-схема',id='exam-er');model=SubElement(diagram,'mxGraphModel',dx='1400',dy='1400',grid='1',gridSize='10',page='1',pageScale='1',pageWidth='1654',pageHeight='2339');graph=SubElement(model,'root');SubElement(graph,'mxCell',id='0');SubElement(graph,'mxCell',id='1',parent='0')
for i,(name,body) in enumerate(tables):
 fields=[]
 for field in body.strip().splitlines():
  field=field.strip().rstrip(',')
  if field.startswith('PRIMARY KEY('):fields.append(field)
  else:
   for part in re.split(r',\s+(?=\w+\s+(?:varchar|int|date|decimal))',field): fields.append(part)
 value=f'<b>{name}</b><hr>'+'<br>'.join(html.escape(x) for x in fields)
 cell=SubElement(graph,'mxCell',id=name,value=value,style='rounded=1;whiteSpace=wrap;html=1;fillColor=#edf3e4;strokeColor=#adc596;fontColor=#294633;align=left;verticalAlign=top;spacing=12;fontSize=12;',vertex='1',parent='1')
 SubElement(cell,'mxGeometry',x=str(40+(i%3)*515),y=str(40+(i//3)*390),width='455',height=str(max(170,len(fields)*25+45)),attrib={'as':'geometry'})
edge_id=0
for name,body in tables:
 for field,target in re.findall(r'(\w+)\s+[^\n,]+?REFERENCES (\w+)\(id\)',body):
  edge_id+=1
  cell=SubElement(graph,'mxCell',id=f'edge{edge_id}',value=f'{field} · N:1',style='edgeStyle=orthogonalEdgeStyle;rounded=0;html=1;endArrow=ERone;startArrow=ERmany;strokeColor=#718d5b;fontSize=10;fontColor=#547041;',edge='1',parent='1',source=name,target=target)
  SubElement(cell,'mxGeometry',relative='1',attrib={'as':'geometry'})
ElementTree(root).write('examples/er-diagram.drawio',encoding='utf-8',xml_declaration=True)
Path('public/downloads/er-diagram.drawio').write_bytes(Path('examples/er-diagram.drawio').read_bytes())
print(len(tables),'tables',edge_id,'relations')
