-- Задание 3. Сначала суммируем материалы и операции отдельно.
-- Если соединить их напрямую, строки перемножатся и стоимость завысится.
CREATE OR REPLACE VIEW order_costs AS
WITH material_cost AS (
    SELECT sm.specification_id,
           SUM(sm.quantity * m.unit_price) AS cost
    FROM specification_materials sm
    JOIN materials m ON m.id = sm.material_id
    GROUP BY sm.specification_id
), operation_cost AS (
    SELECT so.specification_id,
           SUM(so.quantity * so.time_norm * op.unit_price) AS cost
    FROM specification_operations so
    JOIN operations op ON op.id = so.operation_id
    GROUP BY so.specification_id
)
SELECT o.id AS order_id, o.number, o.order_date,
       c.name AS customer,
       ROUND(COALESCE(SUM(i.quantity * COALESCE(mc.cost, 0)), 0), 2) AS materials_cost,
       ROUND(COALESCE(SUM(i.quantity * COALESCE(oc.cost, 0)), 0), 2) AS operations_cost,
       ROUND(COALESCE(SUM(i.quantity *
           (COALESCE(mc.cost, 0) + COALESCE(oc.cost, 0))), 0), 2) AS total_cost,
       ROUND(COALESCE(SUM(i.quantity * i.sale_price - i.discount_amount), 0), 2) AS sale_total
FROM customer_orders o
JOIN counterparties c ON c.id = o.customer_id
LEFT JOIN customer_order_items i ON i.order_id = o.id
LEFT JOIN specifications s ON s.product_id = i.product_id
LEFT JOIN material_cost mc ON mc.specification_id = s.id
LEFT JOIN operation_cost oc ON oc.specification_id = s.id
GROUP BY o.id, o.number, o.order_date, c.name;

SELECT * FROM order_costs ORDER BY order_id;
