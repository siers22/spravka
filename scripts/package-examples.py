from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
out=Path('public/downloads');out.mkdir(exist_ok=True)
def add_project(z,name):
 for p in (Path('examples')/name).rglob('*'):
  if p.is_file() and not any(x in p.parts for x in ['bin','obj','.DS_Store']):z.write(p,p.relative_to('examples'))
with ZipFile(out/'dotnet-example.zip','w',ZIP_DEFLATED) as z:
 add_project(z,'ExamGuide')
 add_project(z,'ExamGuide.Core')
 add_project(z,'NotesApi')
 for name in ['er-diagram.drawio','notes.postman_collection.json','api-documentation.md']:z.write(Path('examples')/name,name)
 z.write('public/sources/api-template.docx','original-api-template.docx')
with ZipFile(out/'programmer-example.zip','w',ZIP_DEFLATED) as z:
 add_project(z,'ShoeStore')
 add_project(z,'ShoeStore.Core')
 z.write('examples/shoe-er.drawio','shoe-er.drawio')
for name in ['notes.postman_collection.json','api-documentation.md']:
 (out/name).write_bytes((Path('examples')/name).read_bytes())
print('Packaged WPF projects, shared libraries and the separate notes API')
