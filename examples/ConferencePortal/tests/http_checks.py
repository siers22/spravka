"""Real HTTP checks against a fresh disposable MySQL database (stdlib only)."""
import datetime, http.cookiejar, json, os, re, time, urllib.request, urllib.parse, urllib.error
from pathlib import Path

BASE = os.environ.get('PORTAL_URL', 'http://127.0.0.1:8088/')
results = []

def check(name, condition):
    results.append({'name': name, 'passed': bool(condition)})
    if not condition:
        raise AssertionError(name)

class Client:
    def __init__(self):
        self.jar = http.cookiejar.CookieJar()
        self.http = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(self.jar))
    def request(self, page, data=None):
        url = BASE + 'index.php?' + urllib.parse.urlencode({'page': page})
        request = urllib.request.Request(url, data=urllib.parse.urlencode(data).encode() if data is not None else None)
        try:
            response = self.http.open(request, timeout=15)
        except urllib.error.HTTPError as error:
            response = error
        return response.status, response.read().decode(), dict(response.headers)
    def post(self, page, data, token=True):
        if token:
            _, html, _ = self.request(page)
            data = dict(data, csrf=re.search(r'name="csrf" value="([a-f0-9]+)"', html)[1])
        return self.request(page, data)

def run():
    guest, alice, bob, admin = (Client() for _ in range(4))
    code, html, headers = guest.request('login')
    check('login responds with UTF-8 page', code == 200 and 'Войти в аккаунт' in html)
    check('session cookie is HttpOnly and SameSite=Lax', all(c.has_nonstandard_attr('HttpOnly') and c.get_nonstandard_attr('SameSite') == 'Lax' for c in guest.jar))
    check('private responses cannot be cached', headers.get('Cache-Control') == 'no-store')
    check('guest is redirected to login', 'Войти в аккаунт' in guest.request('admin')[1])
    check('unknown page returns 404', guest.request('missing')[0] == 404)
    check('missing CSRF returns 403', guest.post('register', {}, token=False)[0] == 403)
    code, html, _ = guest.post('register', {})
    check('all five registration fields validated server-side', code == 422 and html.count('is-invalid') == 5)
    original = dict(login='WebAlice', password='GoodPass77', full_name='Иванова Алиса Сергеевна', phone='8(999)123-45-67', email='alice@example.test')
    for key, value in [('login','abc'), ('login','русский'), ('login','user_777'), ('password','1234567'), ('full_name','Alice'), ('phone','+79991234567'), ('email','wrong')]:
        code, html, _ = guest.post('register', dict(original, **{key:value}))
        check('registration rejects invalid '+key+' '+value, code == 422)
    check('valid registration creates account', 'Аккаунт создан' in alice.post('register', original)[1])
    code, html, _ = bob.post('register', original)
    check('duplicate login returns inline error and preserves fields', code == 422 and 'Этот логин уже занят' in html and 'Иванова Алиса' in html)
    code, html, _ = bob.post('register', dict(original, login='webalice'))
    check('login uniqueness is case-insensitive', code == 422)
    check('second user registration works', bob.post('register', dict(original, login='WebBobby', email='bob@example.test'))[0] == 200)
    check('incorrect password returns inline error', alice.post('login', {'login':'WebAlice','password':'bad'})[0] == 422)
    check('SQL injection cannot bypass login', guest.post('login', {'login':"' OR 1=1 --", 'password':'bad'})[0] == 422)
    before = next(c.value for c in alice.jar if c.name == 'PHPSESSID')
    code, html, _ = alice.post('login', {'login':'WebAlice','password':'GoodPass77'})
    check('valid login shows personal dashboard', code == 200 and 'Ваши встречи' in html)
    check('login rotates session identifier', next(c.value for c in alice.jar if c.name=='PHPSESSID') != before)
    check('regular user cannot access administration', alice.request('admin')[0] == 403)
    check('regular user cannot forge status POST', alice.post('admin', {'id':1,'status':'completed'}, token=False)[0] == 403)
    future = (datetime.datetime.now() + datetime.timedelta(days=7)).strftime('%Y-%m-%dT14:30')
    booking = dict(room_id='1', starts_at=future, payment='sbp')
    for changes, name in [({'room_id':'99999'},'unknown room'), ({'payment':'cash'},'unknown payment'), ({'starts_at':'2026-02-30T14:30'},'impossible date'), ({'starts_at':'2000-01-01T14:30'},'past date'), ({'room_id':'1 OR 1=1'},'injected room')]:
        check('booking rejects '+name, alice.post('booking', dict(booking, **changes))[0] == 422)
    code, html, _ = alice.post('booking', booking)
    check('valid booking starts as new', code == 200 and 'Новая' in html and 'Заявка отправлена' in html)
    booking_id = re.search(r'data-booking-id="(\d+)"', html)[1]
    check('early review rejected on server', alice.post('dashboard', {'id':booking_id,'body':'Рано'})[0] == 422)
    bob.post('login', {'login':'WebBobby','password':'GoodPass77'})
    check('other user cannot see booking', 'data-booking-id' not in bob.request('dashboard')[1])
    check('other user cannot review booking', bob.post('dashboard', {'id':booking_id,'body':'Чужой отзыв'})[0] == 422)
    code, html, _ = admin.post('login', {'login':'Conf2027','password':'Demo77'})
    check('exact KIM administrator credentials work', code == 200 and 'ПАНЕЛЬ АДМИНИСТРАТОРА' in html)
    check('administrator sees contact information', 'alice@example.test' in html and '8(999)123-45-67' in html)
    check('unknown status rejected', admin.post('admin', {'id':booking_id,'status':'hacked'})[0] == 422)
    check('assigned state is visible to owner', admin.post('admin', {'id':booking_id,'status':'assigned'})[0] == 200 and 'Мероприятие назначено' in alice.request('dashboard')[1])
    check('review remains unavailable before completion', alice.post('dashboard', {'id':booking_id,'body':'Рано'})[0] == 422)
    check('administrator completes event', admin.post('admin', {'id':booking_id,'status':'completed'})[0] == 200)
    check('empty review rejected', alice.post('dashboard', {'id':booking_id,'body':'   '})[0] == 422)
    check('oversized review rejected', alice.post('dashboard', {'id':booking_id,'body':'а'*2001})[0] == 422)
    code, html, _ = alice.post('dashboard', {'id':booking_id,'body':'Отличная встреча <script>alert(1)</script>'})
    check('owner review saved after completion', code == 200 and 'Спасибо!' in html)
    check('review XSS escaped in HTML', '&lt;script&gt;alert(1)&lt;/script&gt;' in html and '<script>alert(1)</script>' not in html)
    check('duplicate review rejected', alice.post('dashboard', {'id':booking_id,'body':'Еще один'})[0] == 422)
    check('completed event cannot be reopened', admin.post('admin', {'id':booking_id,'status':'new'})[0] == 422)
    check('review persists after a fresh login', alice.post('logout', {})[0] == 200 and 'Отличная встреча' in alice.post('login', {'login':'WebAlice','password':'GoodPass77'})[1])
    # Exercise more than one page, not only the initial empty listing.
    for _ in range(13): alice.post('booking', booking)
    code, html, _ = alice.request('dashboard')
    check('listing paginates at twelve cards', html.count('data-booking-id') == 12 and 'Страницы заявок' in html)
    response = admin.http.open(BASE+'index.php?page=admin&status=completed').read().decode()
    check('status filter narrows result set', response.count('data-booking-id') == 1)
    response = admin.http.open(BASE+'index.php?page=admin&room=2').read().decode()
    check('room filter shows empty state', 'Заявки не найдены' in response)
    response = admin.http.open(BASE+'index.php?page=admin&date='+future[:10]).read().decode()
    check('date filter selects correct day', 'Найдено: 14' in response)
    check('logout revokes administrative access', admin.post('logout', {})[0] == 200 and 'Войти в аккаунт' in admin.request('admin')[1])
    for asset in ['bootstrap.min.css','app.css','app.js','mark.svg','auditorium.webp','coworking.webp','cinema.webp']:
        check('local asset available: '+asset, guest.http.open(BASE+'assets/'+asset).status == 200)

try:
    run()
except Exception as error:
    results.append({'name':'suite completed without unhandled errors: '+str(error), 'passed':False})
    raise
finally:
    output = Path('/results/http-results.json')
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps({'checked_at':datetime.datetime.now(datetime.timezone.utc).isoformat(),'checks':results,'passed':sum(x['passed'] for x in results),'failed':sum(not x['passed'] for x in results)}, ensure_ascii=False, indent=2)+'\n')
    print(json.dumps({'passed':sum(x['passed'] for x in results),'failed':sum(not x['passed'] for x in results)}, ensure_ascii=False))
