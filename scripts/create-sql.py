from pathlib import Path
schema='''-- Выполнять в уже созданной пустой базе exam_demo.
CREATE TABLE customers (
 id varchar(9) PRIMARY KEY, name varchar(200) NOT NULL,
 inn varchar(20) NOT NULL, address varchar(300) NOT NULL,
 phone varchar(30) NOT NULL, customer_type varchar(30) NOT NULL
);
CREATE TABLE products (
 id int PRIMARY KEY, code varchar(30) UNIQUE NOT NULL,
 name varchar(200) NOT NULL, unit varchar(20) NOT NULL
);
CREATE TABLE materials (
 id int PRIMARY KEY, code varchar(30) UNIQUE NOT NULL,
 name varchar(200) NOT NULL, unit varchar(20) NOT NULL,
 price decimal(18,4) NOT NULL CHECK (price >= 0)
);
CREATE TABLE operations (
 id int PRIMARY KEY, code varchar(30) UNIQUE NOT NULL,
 name varchar(200) NOT NULL, unit varchar(20) NOT NULL,
 price decimal(18,4) NOT NULL CHECK (price >= 0)
);
CREATE TABLE specification_materials (
 product_id int NOT NULL REFERENCES products(id),
 material_id int NOT NULL REFERENCES materials(id),
 consumption decimal(18,6) NOT NULL CHECK (consumption > 0),
 PRIMARY KEY(product_id, material_id)
);
CREATE TABLE specification_operations (
 product_id int NOT NULL REFERENCES products(id),
 operation_id int NOT NULL REFERENCES operations(id),
 time_norm decimal(18,6) NOT NULL CHECK (time_norm > 0),
 quantity decimal(18,6) NOT NULL CHECK (quantity > 0),
 PRIMARY KEY(product_id, operation_id)
);
CREATE TABLE customer_orders (
 id int PRIMARY KEY, order_number varchar(30) NOT NULL,
 order_date date NOT NULL, customer_id varchar(9) NOT NULL REFERENCES customers(id),
 executor varchar(200) NOT NULL
);
CREATE TABLE customer_order_items (
 id int PRIMARY KEY, order_id int NOT NULL REFERENCES customer_orders(id),
 product_id int NOT NULL REFERENCES products(id),
 quantity decimal(18,6) NOT NULL CHECK (quantity > 0),
 sale_price decimal(18,4) NOT NULL CHECK (sale_price >= 0),
 discount_amount decimal(18,4) NOT NULL CHECK (discount_amount >= 0)
);
CREATE TABLE production_orders (
 id int PRIMARY KEY, order_number varchar(30) NOT NULL, order_date date NOT NULL,
 launch_date date NOT NULL, department varchar(100) NOT NULL, executor varchar(200) NOT NULL,
 customer_order_id int NULL REFERENCES customer_orders(id)
);
CREATE TABLE production_order_items (
 id int PRIMARY KEY, production_order_id int NOT NULL REFERENCES production_orders(id),
 product_id int NOT NULL REFERENCES products(id), quantity decimal(18,6) NOT NULL CHECK (quantity > 0),
 source_product_code varchar(30) NOT NULL
);
-- Потребность конкретного заказа не подменяет нормы спецификации.
CREATE TABLE production_materials (
 production_order_id int NOT NULL REFERENCES production_orders(id),
 material_id int NOT NULL REFERENCES materials(id), quantity decimal(18,6) NOT NULL CHECK (quantity > 0),
 source_unit varchar(20) NOT NULL, PRIMARY KEY(production_order_id, material_id)
);
CREATE TABLE production_operations (
 production_order_id int NOT NULL REFERENCES production_orders(id),
 operation_id int NOT NULL REFERENCES operations(id), quantity decimal(18,6) NOT NULL CHECK (quantity > 0),
 source_unit varchar(20) NOT NULL, PRIMARY KEY(production_order_id, operation_id)
);
CREATE TABLE users (
 id int PRIMARY KEY, login varchar(100) NOT NULL UNIQUE,
 password_hash varchar(500) NOT NULL, role varchar(30) NOT NULL CHECK (role IN ('Администратор','Пользователь')),
 failed_attempts int NOT NULL DEFAULT 0 CHECK (failed_attempts >= 0),
 is_blocked int NOT NULL DEFAULT 0 CHECK (is_blocked IN (0,1))
);
CREATE TABLE notes (
 id int PRIMARY KEY, title varchar(200) NOT NULL, content varchar(2000) NOT NULL,
 id_user int NOT NULL REFERENCES users(id), created_at date NOT NULL
);
'''
seed='''-- Учебные данные: нормы и цены взяты из одноимённых XLSX.
INSERT INTO products VALUES (1, 'ФР-00000034', 'Стол кухонный "Самобранка"', 'шт');
INSERT INTO materials VALUES
 (1,'ФР-00000009','Столешница круглая','шт',3250),
 (2,'ФР-00000013','Мебельная деталь 500х800','шт',95),
 (3,'ФР-00000016','Мебельная деталь 600х800','шт',140),
 (4,'ФР-00000027','Евровинт 6,5х5','тыс. шт',595),
 (5,'ФР-00000026','Опора','шт',245);
INSERT INTO operations VALUES
 (1,'ФР-00000053','Сборка модулей','ч',1400),
 (2,'ФР-00000052','Распил ДСП, МДФ и листового материала','ч',450),
 (3,'ФР-00000494','Упаковка','нормоед.',950);
INSERT INTO specification_materials VALUES (1,1,1),(1,2,2),(1,3,4),(1,4,0.012),(1,5,4);
INSERT INTO specification_operations VALUES (1,1,0.75,1),(1,2,1.5,1),(1,3,0.5,1);
-- Томилина нет в JSON. Это дополнительная учебная запись из заказа, не часть импорта.
INSERT INTO customers VALUES ('DEMO00001','ИП Томилин Александр Сергеевич','','','','Покупатель');
INSERT INTO customer_orders VALUES (1,'1','2026-04-22','DEMO00001','ООО ТД "Вершина"');
INSERT INTO customer_order_items VALUES (1,1,1,2,14120,1412);
INSERT INTO production_orders VALUES (1,'1','2026-04-23','2026-04-23','Основное подразделение','Администратор',1);
INSERT INTO production_order_items VALUES (1,1,1,2,'НФ-00000006');
INSERT INTO production_materials VALUES (1,1,2,'шт'),(1,2,4,'шт'),(1,3,8,'шт'),(1,4,0.024,'шт'),(1,5,8,'тыс. шт');
INSERT INTO production_operations VALUES (1,1,2,'ч'),(1,2,2,'ч'),(1,3,2,'шт');
-- Users и notes создаёт dotnet run -- --seed, чтобы хранить хеши паролей.
'''
query='''-- Сначала отдельно складываем материалы и операции на ОДНО изделие.
-- Так два JOIN не размножают строки 5 материалов × 3 операции.
WITH material_cost AS (
 SELECT sm.product_id, SUM(sm.consumption * m.price) AS cost
 FROM specification_materials sm JOIN materials m ON m.id = sm.material_id
 GROUP BY sm.product_id
), operation_cost AS (
 SELECT so.product_id, SUM(so.time_norm * so.quantity * o.price) AS cost
 FROM specification_operations so JOIN operations o ON o.id = so.operation_id
 GROUP BY so.product_id
)
SELECT co.id AS order_id,
 ROUND(COALESCE(SUM(oi.quantity *
   (COALESCE(mc.cost,0) + COALESCE(oc.cost,0))),0),2) AS total_cost
FROM customer_orders co
LEFT JOIN customer_order_items oi ON oi.order_id = co.id
LEFT JOIN material_cost mc ON mc.product_id = oi.product_id
LEFT JOIN operation_cost oc ON oc.product_id = oi.product_id
WHERE co.id = 1
GROUP BY co.id;
-- Ожидается 14374.28 по нормам Спецификации и ценам Цены.xlsx.
-- Отсутствие строки нормы здесь означает, что этот вид затрат не применяется.
-- Для реальных данных проверяйте полноту спецификации отдельно.
'''
for provider in ['postgres','mssql']:
 for name,text in [('schema',schema),('seed',seed),('cost',query)]:
  if provider=='mssql':
   import re
   text=text.replace('varchar(', 'nvarchar(')
   text=re.sub(r"(?<![\w])'([^']*)'",lambda m:"N'"+m.group(1)+"'" if any(ord(c)>127 for c in m.group(1)) else m.group(0),text)
  Path(f'examples/ExamGuide/sql/{provider}-{name}.sql').write_text(text)
