CREATE OR REPLACE FUNCTION fn_get_value_total_stock()
RETURNS NUMERIC AS $$
DECLARE
    total_value NUMERIC := 0;
BEGIN
    -- Get the sum of (price * quantity) for all products
    SELECT SUM(price * stock_quantity)
    INTO total_value
    FROM products;

    -- Returns the computed value
    RETURN COALESCE(total_value, 0);
END;
$$ LANGUAGE plpgsql;

