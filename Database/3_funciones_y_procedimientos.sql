-- =========================================================
-- FUNCIÓN 1: REGISTRAR SOLICITUD
-- =========================================================
CREATE OR REPLACE FUNCTION F_REGISTRAR_SOLICITUD (
    p_id_postulante IN NUMBER,
    p_id_oferta IN NUMBER,
    p_resumen IN CLOB
) RETURN VARCHAR2 IS
    v_dummy NUMBER;
    v_id_programa NUMBER;
    v_conteo_solicitudes NUMBER;
    v_conteo_mismo_programa NUMBER;
BEGIN
    -- 1. Validar existencia de postulante (Levanta NO_DATA_FOUND si no existe)
    SELECT id INTO v_dummy FROM POSTULANTE WHERE id = p_id_postulante;

    -- 2. Validar que la oferta exista, esté activa y dentro del plazo de postulación
    --    (fecha_limite_postulacion: hasta cuándo se puede postular; si es NULL se
    --     acepta mientras el programa no haya iniciado)
    BEGIN
        SELECT id, id_programa INTO v_dummy, v_id_programa
        FROM OFERTA
        WHERE id = p_id_oferta
          AND estado = 'Activa'
          AND SYSDATE <= NVL(fecha_limite_postulacion, fecha_inicio);
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20001, 'La oferta no existe, está cerrada o el plazo de postulación ya venció.');
    END;

    -- 3. Validar máximo de 3 solicitudes en proceso ('Pendiente' o 'Aceptada')
    SELECT COUNT(*) INTO v_conteo_solicitudes
    FROM SOLICITUD
    WHERE id_postulante = p_id_postulante
      AND estado IN ('Pendiente', 'Aceptada');

    IF v_conteo_solicitudes >= 3 THEN
        RAISE_APPLICATION_ERROR(-20002, 'El postulante ya ha alcanzado el límite máximo de 3 programas.');
    END IF;

    -- 4. Validar que no exista ya una solicitud (Pendiente/Aceptada) para el MISMO
    --    PROGRAMA, aunque sea en una oferta/sede distinta (programa único por postulante)
    SELECT COUNT(*) INTO v_conteo_mismo_programa
    FROM SOLICITUD sol
    JOIN OFERTA ofe ON sol.id_oferta = ofe.id
    WHERE sol.id_postulante = p_id_postulante
      AND ofe.id_programa = v_id_programa
      AND sol.estado IN ('Pendiente', 'Aceptada');

    IF v_conteo_mismo_programa > 0 THEN
        RAISE_APPLICATION_ERROR(-20005, 'Ya tienes una solicitud registrada para este programa (en otra sede u oferta).');
    END IF;

    -- 5. Inserción (Puede levantar DUP_VAL_ON_INDEX gracias a la restricción UNIQUE)
    INSERT INTO SOLICITUD (id_postulante, id_oferta, resumen_interes, estado)
    VALUES (p_id_postulante, p_id_oferta, p_resumen, 'Pendiente');

    COMMIT;
    RETURN 'Solicitud registrada exitosamente.';

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK;
        RETURN 'Error: El postulante especificado no existe.';
    WHEN DUP_VAL_ON_INDEX THEN
        ROLLBACK;
        RETURN 'Error: Ya has postulado a esta oferta anteriormente.';
    WHEN OTHERS THEN
        ROLLBACK;
        -- Captura de errores personalizados de RAISE_APPLICATION_ERROR y otros
        RETURN 'Error interno: ' || SQLERRM;
END F_REGISTRAR_SOLICITUD;
/

-- =========================================================
-- FUNCIÓN 2: ACEPTAR SOLICITUD
-- =========================================================
CREATE OR REPLACE FUNCTION F_ACEPTAR_SOLICITUD (
    p_id_solicitud IN NUMBER
) RETURN VARCHAR2 IS
    v_id_oferta NUMBER;
    v_estado_solicitud VARCHAR2(20);
    v_cupos NUMBER;
BEGIN
    -- 1. Bloquear la solicitud para actualización (Evita condiciones de carrera)
    SELECT id_oferta, estado
    INTO v_id_oferta, v_estado_solicitud
    FROM SOLICITUD
    WHERE id = p_id_solicitud
    FOR UPDATE;

    IF v_estado_solicitud != 'Pendiente' THEN
        RAISE_APPLICATION_ERROR(-20003, 'La solicitud no se encuentra en estado Pendiente.');
    END IF;

    -- 2. Bloquear la oferta y verificar cupos
    SELECT cupos_disponibles
    INTO v_cupos
    FROM OFERTA
    WHERE id = v_id_oferta
    FOR UPDATE;

    IF v_cupos <= 0 THEN
        RAISE_APPLICATION_ERROR(-20004, 'No hay cupos disponibles para este programa.');
    END IF;

    -- 3. Efectuar cambios
    UPDATE SOLICITUD
    SET estado = 'Aceptada'
    WHERE id = p_id_solicitud;

    UPDATE OFERTA
    SET cupos_disponibles = cupos_disponibles - 1
    WHERE id = v_id_oferta;

    COMMIT;
    RETURN 'Solicitud aceptada y cupo descontado exitosamente.';

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        ROLLBACK;
        RETURN 'Error: La solicitud especificada no existe.';
    WHEN OTHERS THEN
        ROLLBACK;
        RETURN 'Error en proceso de aceptación: ' || SQLERRM;
END F_ACEPTAR_SOLICITUD;
/

-- =========================================================
-- FUNCIÓN 4: RECHAZAR SOLICITUD
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

-- =========================================================
-- FUNCIÓN 3: CONSULTAR OFERTAS (Para la grilla en MVC)
-- =========================================================
CREATE OR REPLACE FUNCTION F_CONSULTAR_OFERTAS (
    p_area IN VARCHAR2 DEFAULT NULL,
    p_id_universidad IN NUMBER DEFAULT NULL,
    p_tipo_financiamiento IN VARCHAR2 DEFAULT NULL
) RETURN SYS_REFCURSOR IS
    p_cursor SYS_REFCURSOR;
BEGIN
    OPEN p_cursor FOR
        SELECT
            o.id as id_oferta,
            p.nombre as programa,
            p.area,
            p.tipo_financiamiento,
            u.nombre as universidad,
            s.nombre as sede,
            o.fecha_limite_postulacion,
            o.fecha_inicio,
            o.fecha_fin,
            o.cupos_disponibles
        FROM OFERTA o
        JOIN PROGRAMA p ON o.id_programa = p.id
        JOIN SEDE s ON o.id_sede = s.id
        JOIN UNIVERSIDAD u ON s.id_universidad = u.id
        WHERE o.estado = 'Activa'
          AND SYSDATE <= NVL(o.fecha_limite_postulacion, o.fecha_inicio)
          AND (p_area IS NULL OR p.area = p_area)
          AND (p_id_universidad IS NULL OR u.id = p_id_universidad)
          AND (p_tipo_financiamiento IS NULL OR p.tipo_financiamiento = p_tipo_financiamiento)
        ORDER BY o.fecha_inicio DESC;

    RETURN p_cursor;
END F_CONSULTAR_OFERTAS;
/
