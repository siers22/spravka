-- Учебные данные: нормы и цены взяты из одноимённых XLSX.
INSERT INTO products VALUES (1, N'ФР-00000034', N'Стол кухонный "Самобранка"', N'шт');
INSERT INTO materials VALUES
 (1,N'ФР-00000009',N'Столешница круглая',N'шт',3250),
 (2,N'ФР-00000013',N'Мебельная деталь 500х800',N'шт',95),
 (3,N'ФР-00000016',N'Мебельная деталь 600х800',N'шт',140),
 (4,N'ФР-00000027',N'Евровинт 6,5х5',N'тыс. шт',595),
 (5,N'ФР-00000026',N'Опора',N'шт',245);
INSERT INTO operations VALUES
 (1,N'ФР-00000053',N'Сборка модулей',N'ч',1400),
 (2,N'ФР-00000052',N'Распил ДСП, МДФ и листового материала',N'ч',450),
 (3,N'ФР-00000494',N'Упаковка',N'нормоед.',950);
INSERT INTO specification_materials VALUES (1,1,1),(1,2,2),(1,3,4),(1,4,0.012),(1,5,4);
INSERT INTO specification_operations VALUES (1,1,0.75,1),(1,2,1.5,1),(1,3,0.5,1);
-- Томилина нет в JSON. Это дополнительная учебная запись из заказа, не часть импорта.
INSERT INTO customers VALUES ('DEMO00001',N'ИП Томилин Александр Сергеевич','','','',N'Покупатель');
INSERT INTO customer_orders VALUES (1,'1','2026-04-22','DEMO00001',N'ООО ТД "Вершина"');
INSERT INTO customer_order_items VALUES (1,1,1,2,14120,1412);
INSERT INTO production_orders VALUES (1,'1','2026-04-23','2026-04-23',N'Основное подразделение',N'Администратор',1);
INSERT INTO production_order_items VALUES (1,1,1,2,N'НФ-00000006');
INSERT INTO production_materials VALUES (1,1,2,N'шт'),(1,2,4,N'шт'),(1,3,8,N'шт'),(1,4,0.024,N'шт'),(1,5,8,N'тыс. шт');
INSERT INTO production_operations VALUES (1,1,2,N'ч'),(1,2,2,N'ч'),(1,3,2,N'шт');
-- Users и notes создаёт dotnet run -- --seed, чтобы хранить хеши паролей.
