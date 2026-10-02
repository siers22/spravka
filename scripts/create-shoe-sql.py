from pathlib import Path
schema='''-- Выполнить один раз в пустой базе shoe_store.
CREATE TABLE categories (id int PRIMARY KEY, name varchar(100) NOT NULL UNIQUE);
CREATE TABLE subcategories (
 id int PRIMARY KEY, category_id int NOT NULL REFERENCES categories(id),
 name varchar(100) NOT NULL, UNIQUE(category_id,name)
);
CREATE TABLE manufacturers (id int PRIMARY KEY, name varchar(200) NOT NULL UNIQUE);
CREATE TABLE products (
 id int PRIMARY KEY, subcategory_id int NOT NULL REFERENCES subcategories(id),
 manufacturer_id int NOT NULL REFERENCES manufacturers(id), name varchar(300) NOT NULL,
 image_path varchar(200) NOT NULL, description varchar(2000) NOT NULL,
 composition varchar(2000) NOT NULL, base_price decimal(18,2) NOT NULL CHECK(base_price>=0),
 UNIQUE(manufacturer_id,name)
);
CREATE TABLE sizes (size decimal(3,1) PRIMARY KEY);
CREATE TABLE stock_items (
 id int PRIMARY KEY, product_id int NOT NULL REFERENCES products(id),
 size decimal(3,1) NOT NULL REFERENCES sizes(size), available_quantity int NOT NULL CHECK(available_quantity>=0),
 UNIQUE(product_id,size)
);
CREATE TABLE users (
 id int PRIMARY KEY, login varchar(100) NOT NULL UNIQUE,
 last_name varchar(100) NOT NULL, first_name varchar(100) NOT NULL, patronymic varchar(100) NOT NULL,
 role varchar(20) NOT NULL CHECK(role IN ('Admin','Manager','User'))
);
CREATE TABLE orders (
 id int PRIMARY KEY, order_date date NOT NULL, user_id int NOT NULL REFERENCES users(id)
);
CREATE TABLE order_items (
 id int PRIMARY KEY, order_id int NOT NULL REFERENCES orders(id),
 stock_item_id int NOT NULL REFERENCES stock_items(id), quantity int NOT NULL CHECK(quantity>0),
 unit_price decimal(18,2) NOT NULL CHECK(unit_price>=0), UNIQUE(order_id,stock_item_id)
);
'''
query='''-- Даты передаём параметрами: начало предыдущего и текущего месяца.
-- 25% скидки на МОДЕЛЬ, если по ней нет заказов в предыдущем месяце.
SELECT p.id,p.base_price,
 CASE WHEN NOT EXISTS (
   SELECT 1 FROM order_items oi
   JOIN orders o ON o.id=oi.order_id
   JOIN stock_items s ON s.id=oi.stock_item_id
   WHERE s.product_id=p.id
     AND o.order_date>=@previousMonthStart AND o.order_date<@currentMonthStart
 ) THEN ROUND(p.base_price*0.75,2) ELSE p.base_price END AS final_price
FROM products p;
'''
for provider in ['postgres','mssql']:
 for name,text in [('schema',schema),('discount',query)]:
  if provider=='mssql':text=text.replace('varchar(','nvarchar(')
  Path(f'examples/ShoeStore/sql/{provider}-{name}.sql').write_text(text)
