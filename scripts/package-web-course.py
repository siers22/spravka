"""Package the tested portal and authored guide without changing other courses.

Run from repository root: python3 scripts/package-web-course.py
No downloads, PHP, MySQL or third-party Python dependencies are needed here.
"""
from pathlib import Path
from copy import deepcopy
import hashlib
import io
import json
import zipfile

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / 'examples/ConferencePortal'
SOURCE = ROOT / 'ким и учеб материалы/веб разработчик'
authored = json.loads((ROOT/'content/web-developer-guide.json').read_text())
lessons = deepcopy(authored['lessons'])
codes = {}
code_files = {}
languages = {'.php':'php', '.sql':'sql', '.css':'css', '.js':'javascript', '.json':'json', '.sh':'bash', '.py':'python'}
for lesson in lessons:
    for section in lesson['sections']:
        for index, block in enumerate(section['blocks']):
            if block['type'] != 'sourceFile':
                continue
            relative = block['path']
            file = PROJECT/relative
            key = 'file:' + relative
            codes[key] = file.read_text()
            code_files[key] = relative
            section['blocks'][index] = {'type':'code', 'key':key, 'language':languages.get(file.suffix,'plain'), 'title':relative+' — полный файл проверенного проекта'}

docs = {
    ('setup','score-map'):('Матрица опубликованных критериев','docs/requirements.md'),
    ('application','er-workbench'):('ER-диаграмма учебной БД','docs/er-diagram.svg'),
    ('design','audience'):('Обоснование дизайна','docs/design.md'),
    ('quality','http-tests'):('Сохранённый результат HTTP-проверок','docs/results/http-results.json'),
    ('quality','browser-tests'):('Сохранённый браузерный отчёт','docs/results/browser-results.json'),
}
external = {
    ('application','pdo'):[('PHP: подготовленные запросы','https://www.php.net/manual/en/pdo.prepared-statements.php')],
    ('application','passwords'):[('PHP: password_hash','https://www.php.net/manual/en/function.password-hash.php')],
    ('application','er-workbench'):[('MySQL Workbench: Reverse Engineer','https://dev.mysql.com/doc/workbench/en/wb-reverse-engineer-live.html')],
    ('design','bootstrap'):[('Bootstrap: локальные файлы','https://getbootstrap.com/docs/5.3/getting-started/contents/')],
    ('design','gimp-crop'):[('GIMP: размер изображения','https://docs.gimp.org/3.0/en/gimp-image-scale.html')],
    ('design','gimp-export'):[('GIMP: экспорт WebP','https://docs.gimp.org/3.0/en/file-webp-export.html')],
}
shots = {
    ('application','registration-fields'):['02-register-mobile.jpg'],
    ('application','login'):['01-login-mobile.jpg','08-login-desktop.jpg'],
    ('application','booking-form'):['03-booking-mobile.jpg','07-booking-desktop.jpg'],
    ('application','dashboard'):['04-dashboard-mobile.jpg'],
    ('application','admin'):['05-admin-mobile.jpg','09-admin-desktop.jpg'],
    ('quality','review-rule'):['06-review-mobile.jpg'],
}
for lesson in lessons:
    for section in lesson['sections']:
        identity = (lesson['id'],section['id'])
        if identity in docs:
            title, relative = docs[identity]
            section['blocks'].append({'type':'localLink','title':title,'path':relative})
        if identity in external:
            section['blocks'].append({'type':'links','items':[{'title':title,'url':url,'description':'Официальная документация; для экзамена используйте разрешённый офлайн-справочник.'} for title,url in external[identity]]})
        for name in shots.get(identity,[]):
            section['blocks'].append({'type':'localImage','path':'docs/screenshots/'+name,'alt':'Работающий ConferencePortal: '+section['title'],'caption':'Снимок реального запуска PHP/MySQL в Podman. Учебные данные, 13.10.2026.'})

markdown = ['# Конференции.РФ — пошаговое решение 2027',
    'КИМ 09.02.07-3-2027. Цель: опубликованные требования ГИА БУ на 75 баллов. Оценку выставляет эксперт. Вариативная часть не включена.',
    'PHP 8.4 · PDO · MySQL 8.4 · Bootstrap 5.3.8 · JavaScript. Основной запуск на экзамене — обычный PHP/MySQL; Podman — тренировочная проверка.',
    '72 раздела, 360 действий. Код ниже совпадает с файлами учебного проекта. GIMP здесь не запускался; описано самостоятельное выполнение по меню.',
    '## Содержание']
for lesson in lessons:
    markdown += [f"- [{lesson['number']}. {lesson['title']}](#{lesson['id']})"]
for lesson in lessons:
    markdown += [f'<a id="{lesson["id"]}"></a>',f"## {lesson['number']}. {lesson['title']}",lesson['summary'],lesson['source']]
    for section in lesson['sections']:
        markdown += [f"### {section['title']}"]
        for block in section['blocks']:
            kind = block['type']
            if kind == 'p': markdown += [block['text']]
            elif kind == 'list': markdown += ['\n'.join(f'{i+1}. {step}' for i,step in enumerate(block['items']))]
            elif kind == 'code': markdown += [f"**{block['title']}**",'```'+block['language']+'\n'+codes[block['key']].rstrip()+'\n```']
            elif kind == 'localImage': markdown += [f"![{block['alt']}]({block['path'].removeprefix('docs/')})",block['caption']]
            elif kind == 'localLink': markdown += [f"[{block['title']}]({block['path'].removeprefix('docs/')})"]
            elif kind == 'links': markdown += ['\n'.join(f"- [{r['title']}]({r['url']})" for r in block['items'])]
    markdown += ['### Чек-лист','\n'.join('- [ ] '+item for item in lesson['checks'])]
guide = '\n\n'.join(markdown)+'\n'
(PROJECT/'docs/guide.md').write_text(guide)
guide += '\n## Материалы рядом с методичкой\n\n' + '\n'.join(
    f'- [{title}]({path})' for title,path in [
        ('Рабочий проект','ConferencePortal_PHP_MySQL_2027.zip'),
        ('Оригинальный КИМ','kim.pdf'),('Исходные приложения','attachments.zip'),
        ('Матрица критериев','requirements.md'),('ER','er-diagram.svg'),
    ])+'\n'

def project_files():
    for file in sorted(PROJECT.rglob('*')):
        if not file.is_file(): continue
        relative = file.relative_to(PROJECT).as_posix()
        if relative == 'config.local.php' or relative == '.env' or relative.startswith('tests/results/') or '__pycache__' in relative: continue
        yield relative, file.read_bytes()

def zipped(files):
    result = io.BytesIO()
    with zipfile.ZipFile(result,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as archive:
        for name, data in sorted(files):
            info=zipfile.ZipInfo(name,date_time=(2026,10,13,0,0,0))
            info.compress_type=zipfile.ZIP_DEFLATED
            info.external_attr=(0o100644 << 16)
            archive.writestr(info,data)
    return result.getvalue()

files = list(project_files())
inputs = {
    'kim.pdf':SOURCE/'КИМ 09.02.07-3-2027 Том 1.pdf',
    'attachments.zip':SOURCE/'Прил_1_ОЗ_КИМ_09.02.07-3-2027.zip',
    'recommendations.pdf':SOURCE/'Метод_рекомендации_Выпуск_09.02.07-3-2027.pdf',
}
for item in json.loads((PROJECT/'docs/sources.json').read_text()):
    assert hashlib.sha256(inputs[item['file']].read_bytes()).hexdigest()==item['sha256'], item['file']
style = None
with zipfile.ZipFile(inputs['attachments.zip']) as archive:
    for name in archive.namelist():
        if name.endswith('.pdf'): style=archive.read(name); break
assert style, 'No style guide in original attachments'
app_zip = zipped(('ConferencePortal/'+name,data) for name,data in files)
digest = hashlib.sha256(app_zip+Path(__file__).read_bytes()+json.dumps(authored,ensure_ascii=False).encode()+b''.join(path.read_bytes() for path in inputs.values())).hexdigest()[:16]
base=f'materials/2027/web-developer/versions/{digest}-conferenceportal'
destination=ROOT/'public'/base
destination.mkdir(parents=True,exist_ok=True)
outputs={'ConferencePortal_PHP_MySQL_2027.zip':app_zip,'guide.md':guide.encode(),'style-guide.pdf':style,**{name:file.read_bytes() for name,file in inputs.items()}}
for name,data in files:
    if name.startswith('docs/') and name!='docs/guide.md': outputs[name.removeprefix('docs/')]=data
outputs['schema.sql']=(PROJECT/'sql/schema.sql').read_bytes()
for name,data in files:
    if name.startswith('public/assets/'):
        outputs[name.removeprefix('public/')]=data
for name,data in list(outputs.items()):
    if name.startswith('design/') and name.endswith('.html'):
        outputs[name]=data.replace(b'../../public/assets/',b'../assets/')
outputs['source-manifest.json']=json.dumps({'sha256':{name:hashlib.sha256(data).hexdigest() for name,data in files},'codeFiles':code_files,'source':'examples/ConferencePortal','note':'Build-time snapshots; reports describe recorded runs, not automatic exam scores.'},ensure_ascii=False,indent=2).encode()+b'\n'
outputs['ConferencePortal-Guide.zip']=zipped(outputs.items())
for name,data in outputs.items():
    file=destination/name;file.parent.mkdir(parents=True,exist_ok=True);file.write_bytes(data)

for lesson in lessons:
    for section in lesson['sections']:
        for i,block in enumerate(section['blocks']):
            if block['type']=='localLink':
                section['blocks'][i]={'type':'links','items':[{'title':block['title'],'url':base+'/'+block['path'].removeprefix('docs/'),'description':'Файл из проверенного учебного проекта.'}]}
            elif block['type']=='localImage':
                section['blocks'][i]={'type':'image','url':base+'/'+block['path'].removeprefix('docs/'),'alt':block['alt'],'caption':block['caption']}

resources=[]
for name,title,description,kind in [
    ('kim.pdf','КИМ 09.02.07-3-2027','Оригинальный документ ИРПО, зеркало колледжа; 50 страниц.','PDF'),
    ('attachments.zip','Исходные приложения','Оригинальные медиа, шрифты и руководство по стилю.','ZIP'),
    ('recommendations.pdf','Рекомендации к выпуску','Дополнительное описание структуры заданий.','PDF'),
    ('style-guide.pdf','Руководство по стилю','Палитра и PT Sans из исходного приложения.','PDF'),
    ('ConferencePortal_PHP_MySQL_2027.zip','Рабочий ConferencePortal','PHP/MySQL, все ресурсы, HTML-макеты, инструкция и тесты.','ZIP'),
    ('guide.md','Полная пошаговая методичка','72 раздела, 360 действий, полные файлы решения.','MD'),
    ('ConferencePortal-Guide.zip','Автономная методичка с материалами','Markdown, снимки, документы и проект по относительным ссылкам.','ZIP'),
    ('schema.sql','Схема MySQL','4 таблицы, 3 внешних ключа и индексы.','SQL'),
    ('er-diagram.svg','ER-диаграмма','Схема учебного решения; экспорт из Workbench описан отдельно.','SVG'),
    ('requirements.md','Матрица покрытия 75 баллов','Восемь опубликованных групп, реализация и способ проверки.','MD'),
    ('design.md','Обоснование дизайна','Сценарии аудитории, стиль, графика и адаптивность.','MD'),
    ('results/http-results.json','Результат HTTP-проверок','58 проверок, 0 ошибок в сохранённом запуске; команда повторного запуска в README.','JSON'),
    ('results/browser-results.json','Браузерная проверка','Учебный пользователь, заявка, администратор, завершение и отзыв.','JSON'),
    ('source-manifest.json','Контрольные суммы файлов','Проверка совпадения каталога, архива и исходников.','JSON'),
    ('sources.json','Происхождение материалов','Ссылки, SHA-256 и дата получения оригиналов.','JSON'),
]:resources.append(dict(title=title,description=description,type=kind,url=base+'/'+name))

glossary=[
    ['Full stack','Браузерный интерфейс, серверный код и база работают как одна система.'],
    ['PHP','Серверный язык: обрабатывает формы и возвращает HTML. Исходник не должен передаваться браузеру.'],
    ['PDO','Интерфейс PHP для работы с БД через подготовленные запросы.'],
    ['ООП','Классы разделяют обязанности: контроллер, валидация и репозитории.'],
    ['Репозиторий данных','Класс, который выполняет запросы к конкретным таблицам.'],
    ['MySQL','Сервер реляционной БД; хранит пользователей, заявки и отзывы.'],
    ['PK','Первичный ключ: уникальный идентификатор строки.'],
    ['FK','Внешний ключ: связь с существующей строкой родительской таблицы.'],
    ['UNIQUE','Ограничение, запрещающее повторное значение: например, логин или отзыв одной заявки.'],
    ['JOIN','Получение связанных данных нескольких таблиц одним запросом.'],
    ['Индекс','Структура для ускорения подходящих условий поиска и сортировки.'],
    ['Сессия','Серверное состояние пользователя; браузер хранит идентификатор в cookie.'],
    ['Роль','Права аккаунта: обычный пользователь или администратор. Сервер читает роль из БД.'],
    ['CSRF','Подделка запроса от имени сессии; скрытый непредсказуемый токен помогает её отклонить.'],
    ['XSS','Выполнение чужого скрипта при небезопасном выводе. Пользовательский текст экранируется.'],
    ['Хеш пароля','Необратимое представление пароля для проверки через password_verify.'],
    ['Валидация','Проверка обязательности, формата, длины и допустимости данных до сохранения.'],
    ['Bootstrap','Локальная библиотека компонентов CSS; в этом проекте не требует npm.'],
    ['Адаптивность','Перестроение интерфейса под ширину экрана без потери функций.'],
    ['WebP','Формат изображения для уменьшения размера передаваемых фотографий.'],
    ['XCF','Рабочий формат GIMP со слоями; для сайта изображение экспортируют отдельно.'],
    ['DocumentRoot','Папка, которую HTTP-сервер отдаёт браузеру; здесь это public.'],
    ['PRG','Post/Redirect/Get: перенаправление после успешной формы предотвращает повтор при обновлении.'],
    ['Пагинация','Разделение списка на страницы; сервер ограничивает количество выбранных строк.'],
    ['Podman','Контейнерное окружение для тренировки здесь; на экзамене его наличие не предполагается.'],
]
course=dict(id='web-developer',status='published',title='Разработчик веб и мультимедийных приложений',short='Веб-разработчик',kim='09.02.07-3-2027',subtitle='От первой таблицы MySQL до работающего портала и мобильных страниц — по шагам.',domain='Портал «Конференции.РФ»',description='PHP, MySQL, регистрация, бронирование помещений, администратор, отзывы и мобильный дизайн.',verification='Живой PHP/MySQL в Podman: 58 HTTP-проверок без ошибок и браузерный сценарий. Покрыты опубликованные требования базового образца; итоговые баллы выставляет эксперт.',paTime='1 ч 30 мин',giaTime='3 ч 30 мин',paCount=1,paPoints=25,giaPoints=75,pages=50,stack='PHP · MySQL · Bootstrap · JS',dialectExamples=False,download=base+'/ConferencePortal_PHP_MySQL_2027.zip',downloadLabel='Рабочий проект',downloadType='ZIP',exampleSource='ConferencePortal_PHP_MySQL_2027.zip',codes=codes,glossary=glossary,resources=resources,lessons=lessons)
catalog_file=ROOT/'content/catalog.json'
catalog=json.loads(catalog_file.read_text())
edition=next(e for e in catalog['editions'] if e['year']==2027)
existing=next((i for i,c in enumerate(edition['courses']) if c['id']=='web-developer'),None)
if existing is None:edition['courses'].append(course)
else:edition['courses'][existing]=course
catalog_file.write_text(json.dumps(catalog,ensure_ascii=False,indent=2)+'\n')
print(f'OK: {base}; {len(codes)} exact files; 72 sections / 360 actions; project {len(app_zip)} bytes')
