-- Задание 2. Импорт всех шести контрагентов из исходного JSON.
BEGIN;
INSERT INTO counterparties (id, name, inn, address, phone, type)
SELECT id, name, inn, addres, phone, type
FROM jsonb_to_recordset($json$[
  {
    "id": "000000001",
    "name": "ООО \"Поставка\"",
    "inn": "",
    "addres": "г.Пятигорск",
    "phone": "+79198634592",
    "type": "Поставщик"
  },
  {
    "id": "000000002",
    "name": "ООО \"Кинотеатр Квант\"",
    "inn": "26320045123",
    "addres": "г. Железноводск, ул. Мира, 123",
    "phone": "+79884581555",
    "type": "Покупатель"
  },
  {
    "id": "000000008",
    "name": "ООО \"Новый JDTO\"",
    "inn": "26320045111",
    "addres": "г. Железноводсу",
    "phone": "+79884581555",
    "type": "Покупатель"
  },
  {
    "id": "000000003",
    "name": "ООО \"Ромашка\"",
    "inn": "4140784214",
    "addres": "г. Омск, ул. Строителей, 294",
    "phone": "+79882584546",
    "type": "Поставщик"
  },
  {
    "id": "000000009",
    "name": "ООО \"Ипподром\"",
    "inn": "5874045632",
    "addres": "г. Уфа, ул. Набережная,  37",
    "phone": "+79627486389",
    "type": "Поставщик"
  },
  {
    "id": "000000010",
    "name": "ООО \"Ассоль\"",
    "inn": "2629011278",
    "addres": "г. Калуга, ул. Пушкина, 94",
    "phone": "+79184572398",
    "type": "Покупатель"
  }
]$json$::jsonb)
AS source(id text, name text, inn text, addres text, phone text, type text);

-- Дополнительные контрагенты из Excel, не заменяющие записи JSON.
INSERT INTO counterparties (id, name, type) VALUES
('EXCEL-SELLER', 'ООО ТД "Вершина"', 'Изготовитель'),
('EXCEL-BUYER', 'ИП Томилин Александр Сергеевич', 'Покупатель');
INSERT INTO units (id, name) VALUES (1, 'шт'), (2, 'тыс. шт'), (3, 'ч');
INSERT INTO products (id, code, production_code, name, unit_id) VALUES
(1, 'ФР-00000034', 'НФ-00000006', 'Стол кухонный "Самобранка"', 1);
INSERT INTO materials (id, code, name, unit_id, unit_price) VALUES
(1, 'ФР-00000009', 'Столешница круглая', 1, 3250),
(2, 'ФР-00000013', 'Мебельная деталь 500х800', 1, 95),
(3, 'ФР-00000016', 'Мебельная деталь 600х800', 1, 140),
(4, 'ФР-00000027', 'Евровинт 6,5х5', 2, 595),
(5, 'ФР-00000026', 'Опора', 1, 245);
-- Цены операций применяются к норме времени спецификации.
INSERT INTO operations (id, code, name, unit_id, unit_price) VALUES
(1, 'ФР-00000053', 'Сборка модулей', 3, 1400),
(2, 'ФР-00000052', 'Распил ДСП, МДФ и листового материала', 3, 450),
(3, 'ФР-00000494', 'Упаковка', 3, 950);
INSERT INTO specifications (id, product_id, name, manufacturer_id) VALUES
(1, 1, 'Спецификация стола Самобранка', 'EXCEL-SELLER');
INSERT INTO specification_materials VALUES
(1, 1, 1), (1, 2, 2), (1, 3, 4), (1, 4, 0.012), (1, 5, 4);
INSERT INTO specification_operations VALUES
(1, 1, 1, 0.75), (1, 2, 1, 1.5), (1, 3, 1, 0.5);
INSERT INTO departments (id, name) VALUES (1, 'Основное подразделение');
INSERT INTO users (id, login, password_hash, role) VALUES (1, 'admin', 'Aqy7bd7w5lzPrQQI6j5pRA==:EctpD6BbGaRzohlBKgC7w9EZwFCXmXtCWSdN6p01QJU=', 'Администратор');
INSERT INTO users (id, login, password_hash, role) VALUES (2, 'user25', 'CKN9qNfKgl8gSLut2rKR1Q==:3WFR6hBi89MKbvjsL6GbQcBMvL2BO/65lr8l8KBE4jY=', 'Пользователь');
INSERT INTO users (id, login, password_hash, role) VALUES (3, 'user26', 'dJJTtuS37F/Wfzh+/HKYww==:wi2oZgeG83IlicR0rfMK5pgE7lgNzpw+xZ48ZXvTUHc=', 'Пользователь');

INSERT INTO customer_orders (id, number, order_date, customer_id, executor_id) VALUES
(1, '1', '2026-04-22', 'EXCEL-BUYER', 'EXCEL-SELLER');
INSERT INTO customer_order_items (id, order_id, product_id, quantity, sale_price, discount_amount) VALUES
(1, 1, 1, 2, 14120, 1412);
INSERT INTO production_orders (id, number, order_date, start_date, customer_order_id, department_id, executor_user_id) VALUES
(1, '1', '2026-04-23', '2026-04-23', 1, 1, 1);
INSERT INTO production_order_items (id, production_order_id, specification_id, quantity) VALUES (1, 1, 1, 2);
INSERT INTO notes (id, title, content, id_user, created_at) VALUES
(1, 'Конференция ИТ', 'Содержимое заметки', 2, '2027-03-15'),
(2, 'Заказ на производство', 'Подготовить материалы для двух столов.', 1, '2027-03-16'),
(3, 'Встреча с заказчиком', 'Обсудить сроки поставки.', 2, '2027-03-17'),
(4, 'Проверка спецификации', 'Проверить нормы расхода материалов.', 3, '2027-03-18'),
(5, 'Отчёт', 'Подготовить отчёт о производстве.', 1, '2027-03-19');
SELECT setval(pg_get_serial_sequence('units', 'id'), (SELECT MAX(id) FROM units));
SELECT setval(pg_get_serial_sequence('products', 'id'), (SELECT MAX(id) FROM products));
SELECT setval(pg_get_serial_sequence('materials', 'id'), (SELECT MAX(id) FROM materials));
SELECT setval(pg_get_serial_sequence('operations', 'id'), (SELECT MAX(id) FROM operations));
SELECT setval(pg_get_serial_sequence('specifications', 'id'), (SELECT MAX(id) FROM specifications));
SELECT setval(pg_get_serial_sequence('departments', 'id'), (SELECT MAX(id) FROM departments));
SELECT setval(pg_get_serial_sequence('users', 'id'), (SELECT MAX(id) FROM users));
SELECT setval(pg_get_serial_sequence('customer_orders', 'id'), (SELECT MAX(id) FROM customer_orders));
SELECT setval(pg_get_serial_sequence('customer_order_items', 'id'), (SELECT MAX(id) FROM customer_order_items));
SELECT setval(pg_get_serial_sequence('production_orders', 'id'), (SELECT MAX(id) FROM production_orders));
SELECT setval(pg_get_serial_sequence('production_order_items', 'id'), (SELECT MAX(id) FROM production_order_items));
SELECT setval(pg_get_serial_sequence('notes', 'id'), (SELECT MAX(id) FROM notes));
COMMIT;
