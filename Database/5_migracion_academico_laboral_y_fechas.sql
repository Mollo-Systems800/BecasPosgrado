-- =========================================================
-- MIGRACIÓN: Datos académicos/laborales del postulante,
--            fecha límite de postulación en OFERTA,
--            y conversión de P_CONSULTAR_OFERTAS a función.
--
-- Ejecutar UNA SOLA VEZ contra la base de datos YA POBLADA.
-- No vuelve a crear tablas ni secuencias: solo agrega columnas,
-- rellena datos de ejemplo en los registros existentes, y
-- reemplaza los objetos PL/SQL afectados.
-- =========================================================

-- ---------------------------------------------------------
-- 1. Nuevas columnas en POSTULANTE (académico + laboral)
-- ---------------------------------------------------------
ALTER TABLE POSTULANTE ADD (
    nivel_academico VARCHAR2(50),
    institucion_procedencia VARCHAR2(150),
    titulo_obtenido VARCHAR2(150),
    promedio_academico NUMBER(5,2),
    empresa_actual VARCHAR2(150),
    cargo_actual VARCHAR2(100),
    anios_experiencia NUMBER(3) DEFAULT 0
);

ALTER TABLE POSTULANTE ADD CONSTRAINT ck_postulante_promedio
    CHECK (promedio_academico IS NULL OR promedio_academico BETWEEN 0 AND 100);

ALTER TABLE POSTULANTE ADD CONSTRAINT ck_postulante_anios_exp
    CHECK (anios_experiencia >= 0);

-- ---------------------------------------------------------
-- 2. Rellenar datos de ejemplo en los postulantes existentes
-- ---------------------------------------------------------
UPDATE POSTULANTE SET
    nivel_academico = 'Licenciatura',
    institucion_procedencia = 'Universidad Mayor de San Simón',
    titulo_obtenido = 'Ingeniería de Sistemas',
    promedio_academico = 82.50,
    empresa_actual = 'BecasPosgrado',
    cargo_actual = 'Administrador del Sistema',
    anios_experiencia = 5
WHERE usuario = 'cadmin';

UPDATE POSTULANTE SET
    nivel_academico = 'Licenciatura',
    institucion_procedencia = 'Universidad Privada del Valle',
    titulo_obtenido = 'Ingeniería Comercial',
    promedio_academico = 88.30,
    empresa_actual = 'Banco Nacional',
    cargo_actual = 'Analista Financiero',
    anios_experiencia = 4
WHERE usuario = 'aperez';

UPDATE POSTULANTE SET
    nivel_academico = 'Ingeniería',
    institucion_procedencia = 'Universidad Mayor de San Andrés',
    titulo_obtenido = 'Ingeniería en Redes y Telecomunicaciones',
    promedio_academico = 79.80,
    empresa_actual = 'TechSolutions Bolivia',
    cargo_actual = 'Especialista en Ciberseguridad',
    anios_experiencia = 6
WHERE usuario = 'lgomez';

UPDATE POSTULANTE SET
    nivel_academico = 'Licenciatura',
    institucion_procedencia = 'Universidad Católica Boliviana',
    titulo_obtenido = 'Estadística',
    promedio_academico = 91.00,
    anios_experiencia = 0
WHERE usuario = 'mlopez';

UPDATE POSTULANTE SET
    nivel_academico = 'Técnico Superior',
    institucion_procedencia = 'Instituto Tecnológico Loyola',
    titulo_obtenido = 'Técnico en Informática',
    promedio_academico = 75.20,
    empresa_actual = 'Cooperativa La Merced',
    cargo_actual = 'Soporte Técnico',
    anios_experiencia = 3
WHERE usuario = 'jdiaz';

-- ---------------------------------------------------------
-- 3. Nueva columna en OFERTA: fecha límite de postulación
--    (fecha_inicio/fecha_fin pasan a representar el inicio/fin
--     del PROGRAMA; fecha_limite_postulacion es hasta cuándo se
--     puede postular, y debe ser <= fecha_inicio)
-- ---------------------------------------------------------
ALTER TABLE OFERTA ADD (fecha_limite_postulacion DATE);

-- Ajustar las 4 ofertas de ejemplo ya cargadas (ids 1-4, según 2_insertar_datos.sql)
UPDATE OFERTA SET fecha_limite_postulacion = SYSDATE + 20 WHERE id = 1; -- Activa
UPDATE OFERTA SET fecha_limite_postulacion = SYSDATE + 35 WHERE id = 2; -- Activa
UPDATE OFERTA SET fecha_limite_postulacion = SYSDATE + 10 WHERE id = 3; -- Activa, 1 cupo
UPDATE OFERTA SET fecha_limite_postulacion = SYSDATE - 10 WHERE id = 4; -- Cerrada

-- Si tienes más ofertas cargadas manualmente, dales una fecha límite razonable
-- (anterior o igual a fecha_inicio) para que la nueva validación funcione:
UPDATE OFERTA
   SET fecha_limite_postulacion = fecha_inicio
 WHERE fecha_limite_postulacion IS NULL;

COMMIT;

-- ---------------------------------------------------------
-- 4. Reemplazar el procedimiento P_CONSULTAR_OFERTAS por una
--    FUNCIÓN (el enunciado exige 3 FUNCIONES, no procedimientos)
-- ---------------------------------------------------------
DROP PROCEDURE P_CONSULTAR_OFERTAS;

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

-- ---------------------------------------------------------
-- 5. Reemplazar F_REGISTRAR_SOLICITUD:
--    - Usa fecha_limite_postulacion en vez de BETWEEN fecha_inicio/fecha_fin
--    - Agrega validación de "programa único" (no se puede postular dos
--      veces al mismo PROGRAMA aunque sea en distinta sede/oferta)
-- ---------------------------------------------------------
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
    SELECT id INTO v_dummy FROM POSTULANTE WHERE id = p_id_postulante;

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

    SELECT COUNT(*) INTO v_conteo_solicitudes
    FROM SOLICITUD
    WHERE id_postulante = p_id_postulante
      AND estado IN ('Pendiente', 'Aceptada');

    IF v_conteo_solicitudes >= 3 THEN
        RAISE_APPLICATION_ERROR(-20002, 'El postulante ya ha alcanzado el límite máximo de 3 programas.');
    END IF;

    SELECT COUNT(*) INTO v_conteo_mismo_programa
    FROM SOLICITUD sol
    JOIN OFERTA ofe ON sol.id_oferta = ofe.id
    WHERE sol.id_postulante = p_id_postulante
      AND ofe.id_programa = v_id_programa
      AND sol.estado IN ('Pendiente', 'Aceptada');

    IF v_conteo_mismo_programa > 0 THEN
        RAISE_APPLICATION_ERROR(-20005, 'Ya tienes una solicitud registrada para este programa (en otra sede u oferta).');
    END IF;

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
        RETURN 'Error interno: ' || SQLERRM;
END F_REGISTRAR_SOLICITUD;
/

-- =========================================================
-- FIN DE LA MIGRACIÓN
-- Verifica con: SELECT * FROM POSTULANTE; SELECT * FROM OFERTA;
-- y con Database/4_pruebas_unitarias.sql
-- =========================================================
