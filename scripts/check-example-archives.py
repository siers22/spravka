from pathlib import Path
from zipfile import ZipFile
root=Path(__file__).resolve().parent.parent
for name,projects in [('dotnet-example.zip',['ExamGuide','ExamGuide.Core','NotesApi']),('programmer-example.zip',['ShoeStore','ShoeStore.Core'])]:
 with ZipFile(root/'public/downloads'/name) as archive:
  entries=set(archive.namelist())
  assert not any('/bin/' in p or '/obj/' in p or p.endswith('.cshtml') for p in entries)
  for project in projects:
   for file in (root/'examples'/project).rglob('*'):
    if not file.is_file() or any(p in ['bin','obj','.DS_Store'] for p in file.parts):continue
    path=file.relative_to(root/'examples').as_posix()
    assert path in entries, f'{name}: missing {path}'
    assert archive.read(path)==file.read_bytes(),f'{name}: outdated {path}'
 print(f'OK: {name}, all WPF/library/API sources and assets match the repository')
