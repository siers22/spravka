-- Даты передаём параметрами: начало предыдущего и текущего месяца.
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
