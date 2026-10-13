"""Verify real project snapshots and local Markdown/HTML links after packaging."""
from pathlib import Path
from urllib.parse import urlsplit, unquote
import hashlib
import json
import posixpath
import re
import zipfile

root=Path(__file__).resolve().parents[1]
data=json.loads((root/'content/catalog.json').read_text())
course=next(c for e in data['editions'] if e['year']==2027 for c in e['courses'] if c['id']=='web-developer')
folder=root/'public'/Path(course['download']).parent
manifest=json.loads((folder/'source-manifest.json').read_text())
with zipfile.ZipFile(root/'public'/course['download']) as archive:
    for name,sha in manifest['sha256'].items():
        local=(root/'examples/ConferencePortal'/name).read_bytes()
        assert hashlib.sha256(local).hexdigest()==sha, f'Stale source: {name}'
        assert archive.read('ConferencePortal/'+name)==local, f'Stale ZIP file: {name}'
    assert 'ConferencePortal/config.local.php' not in archive.namelist()
    assert 'ConferencePortal/.env' not in archive.namelist()
for key,name in manifest['codeFiles'].items():
    assert course['codes'][key]==(root/'examples/ConferencePortal'/name).read_text(), f'Stale code: {key}'

with zipfile.ZipFile(folder/'ConferencePortal-Guide.zip') as archive:
    names=set(archive.namelist())
    for name in names:
        if name.endswith('.md'):
            text=archive.read(name).decode()
            text=re.sub(r'```[^\n]*\n.*?```','',text,flags=re.S)
            refs=re.findall(r'!?\[[^\]]*\]\(([^)]+)\)',text)
        elif name.endswith('.html'):
            refs=re.findall(r'(?:src|href)="([^"]+)"',archive.read(name).decode())
        elif name.endswith('app.css'):
            refs=[v.strip("'\"") for v in re.findall(r'url\(([^)]+)\)',archive.read(name).decode())]
        else: continue
        for ref in refs:
            parts=urlsplit(ref)
            if parts.scheme or parts.netloc or not parts.path: continue
            target=posixpath.normpath(posixpath.join(posixpath.dirname(name),unquote(parts.path)))
            assert target in names, f'Broken standalone link: {name} → {ref}'
print(f'OK: web ZIP, {len(manifest["sha256"])} exact project files, {len(manifest["codeFiles"])} catalog code snapshots and standalone links')
