-- =========================================================
-- FUNCIÓN 1: REGISTRAR SOLICITUD
-- =========================================================
CREATE OR REPLACE FUNCTION F_REGISTRAR_SOLICITUD (
    p_id_postulante IN NUMBER,
    p_id_oferta IN NUMBER,
    p_resumen IN CLOB
) RETURN VARCHAR2 IS
    v_dummy NUMBER;
    v_estado_oferta VARCHAR2(20);
    v_conteo_solicitudes NUMBER;
BEGIN
    -- 1. Validar existencia de postulante (Levanta NO_DATA_FOUND si no existe)
    SELECT id INTO v_dummy FROM POSTULANTE WHERE id = p_id_postulante;

    -- 2. Validar que la oferta exista y esté activa (en fechas)
    BEGIN
        SELECT id INTO v_dummy 
        FROM OFERTA 
        WHERE id = p_id_oferta 
          AND SYSDATE BETWEEN fecha_inicio AND fecha_fin 
          AND estado = 'Activa';
    EXCEPTION
        WHEN NO_DATA_FOUND THEN
            RAISE_APPLICATION_ERROR(-20001, 'La oferta no existe o no se encuentra activa en este momento.');
    END;

    -- 3. Validar máximo de 3 solicitudes en proceso ('Pendiente' o 'Aceptada')
    SELECT COUNT(*) INTO v_conteo_solicitudes 
    FROM SOLICITUD 
    WHERE id_postulante = p_id_postulante 
      AND estado IN ('Pendiente', 'Aceptada');
      
    IF v_conteo_solicitudes >= 3 THEN
        RAISE_APPLICATION_ERROR(-20002, 'El postulante ya ha alcanzado el límite máximo de 3 programas.');
    END IF;

    -- 4. Inserción (Puede levantar DUP_VAL_ON_INDEX gracias a la restricción UNIQUE)
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
-- PROCEDIMIENTO 3: CONSULTAR OFERTAS (Para la grilla en MVC)
-- =========================================================
CREATE OR REPLACE PROCEDURE P_CONSULTAR_OFERTAS (
    p_area IN VARCHAR2 DEFAULT NULL,
    p_id_universidad IN NUMBER DEFAULT NULL,
    p_cursor OUT SYS_REFCURSOR
) IS
BEGIN
    OPEN p_cursor FOR
        SELECT 
            o.id as id_oferta,
            p.nombre as programa,
            p.area,
            u.nombre as universidad,
            s.nombre as sede,
            o.fecha_inicio,
            o.fecha_fin,
            o.cupos_disponibles
        FROM OFERTA o
        JOIN PROGRAMA p ON o.id_programa = p.id
        JOIN SEDE s ON o.id_sede = s.id
        JOIN UNIVERSIDAD u ON s.id_universidad = u.id
        WHERE SYSDATE BETWEEN o.fecha_inicio AND o.fecha_fin
          AND o.estado = 'Activa'
          AND (p_area IS NULL OR p.area = p_area)
          AND (p_id_universidad IS NULL OR u.id = p_id_universidad)
        ORDER BY o.fecha_inicio DESC;
END P_CONSULTAR_OFERTAS;
/