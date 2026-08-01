-- Genera asientos para un vuelo según la capacidad del avión asignado.
-- Uso: psql ... -v myflight=754272 -f generar_asientos.sql
\set ON_ERROR_STOP on
SELECT set_config('myapp.flight', :'myflight', false);

DO $$
DECLARE
    v_flight_id int := current_setting('myapp.flight')::int;
    v_capacity  int;
    v_rows      int;
    v_class     text;
    v_row       int;
    v_letter    text;
    v_seat_no   text;
BEGIN
    SELECT a.capacity INTO v_capacity
    FROM flight f
    JOIN airplane a ON a.airplane_id = f.airplane_id
    WHERE f.flight_id = v_flight_id;

    IF v_capacity IS NULL THEN
        RAISE NOTICE 'Vuelo % sin aeronave asignada', v_flight_id;
        RETURN;
    END IF;

    IF EXISTS (SELECT 1 FROM "Seats" WHERE "FlightId" = v_flight_id) THEN
        RAISE NOTICE 'Vuelo % ya tiene asientos', v_flight_id;
        RETURN;
    END IF;

    v_rows := ceil(v_capacity / 6.0);

    FOR v_row IN 1..v_rows LOOP
        IF v_row <= 2 THEN
            v_class := 'First';
        ELSIF v_row <= 5 THEN
            v_class := 'Business';
        ELSE
            v_class := 'Economy';
        END IF;

        FOR v_letter IN (SELECT chr(64 + n) FROM generate_series(1, 6) AS n) LOOP
            IF (v_row - 1) * 6 + ascii(v_letter) - 64 <= v_capacity THEN
                v_seat_no := v_row::text || v_letter;

                INSERT INTO "Seats" ("FlightId", "SeatNo", "SeatClass", "IsOccupied", "Price")
                VALUES (
                    v_flight_id,
                    v_seat_no,
                    v_class,
                    false,
                    CASE v_class
                        WHEN 'First' THEN 850.00
                        WHEN 'Business' THEN 450.00
                        ELSE 180.00
                    END
                );
            END IF;
        END LOOP;
    END LOOP;

    RAISE NOTICE 'Generados asientos para vuelo %', v_flight_id;
END $$;
