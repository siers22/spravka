-- Сначала отдельно складываем материалы и операции на ОДНО изделие.
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
