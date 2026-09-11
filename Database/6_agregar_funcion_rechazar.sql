-- =========================================================
-- MIGRACIÓN 6: agrega la función F_RECHAZAR_SOLICITUD
-- Ejecutar UNA sola vez contra la base de datos que ya tienes migrada
-- (no vuelve a crear tablas ni datos, solo agrega esta función nueva).
-- =========================================================
CREATE OR REPLACE FUNCTION F_RECHAZAR_SOLICITUD (
    p_id_solicitud IN NUMBER
) RETURN VARCHAR2 IS
    v_estado_solicitud VARCHAR2(20);
BEGIN
    -- 1. Bloquear la solicitud para actualización (evita condiciones de carrera)
    SELECT estado
    INTO v_estado_solicitud
    FROM SOLICITUD
    WHERE id = p_id_solicitud
    FOR UPDATE;

    IF v_estado_solicitud != 'Pendiente' THEN
        RAISE_APPLICATION_ERROR(-20006, 'La solicitud no se encuentra en estado Pendiente.');
    END IF;

    -- 2. Efectuar cambio (no toca cupos: solo se descuentan al aceptar)
    UPDATE SOLICITUD
    SET estado = 'Rechazada'
    WHERE id = p_id_solicitud;

    COMMIT;
    RETURN 'Solicitud rechazada correctamente.';

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK;
        RETURN 'Error: La solicitud especificada no existe.';
    WHEN OTHERS THEN
        ROLLBACK;
        RETURN 'Error en proceso de rechazo: ' || SQLERRM;
END F_RECHAZAR_SOLICITUD;
/
