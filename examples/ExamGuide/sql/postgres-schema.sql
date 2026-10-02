-- Выполнять в уже созданной пустой базе exam_demo.
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
