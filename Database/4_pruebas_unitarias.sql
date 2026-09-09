SET SERVEROUTPUT ON;

DECLARE
    v_mensaje VARCHAR2(500);
    v_cursor SYS_REFCURSOR;
    -- Variables para leer el cursor
    v_id_oferta NUMBER;
    v_programa VARCHAR2(150);
    v_area VARCHAR2(50);
    v_financiamiento VARCHAR2(50);
    v_univ VARCHAR2(150);
    v_sede VARCHAR2(150);
    v_limite DATE;
    v_inicio DATE;
    v_fin DATE;
    v_cupos NUMBER;
BEGIN
    DBMS_OUTPUT.PUT_LINE('--- INICIANDO BATERÍA DE PRUEBAS ---');

    -- 1. Prueba: Postular postulante 2 a Oferta 1 (Éxito)
    v_mensaje := F_REGISTRAR_SOLICITUD(2, 1, 'Tengo mucho interés en Data Science.');
    DBMS_OUTPUT.PUT_LINE('Prueba 1 (Postulación exitosa): ' || v_mensaje);

    -- 2. Prueba: Programa único (Postular otra vez al mismo programa/oferta)
    v_mensaje := F_REGISTRAR_SOLICITUD(2, 1, 'Intento de fraude jeje.');
    DBMS_OUTPUT.PUT_LINE('Prueba 2 (Postulación duplicada - mismo programa): ' || v_mensaje);

    -- 3. Prueba: Oferta vencida (Oferta 4 está Cerrada / plazo vencido)
    v_mensaje := F_REGISTRAR_SOLICITUD(2, 4, 'Quiero el programa de Física.');
    DBMS_OUTPUT.PUT_LINE('Prueba 3 (Oferta cerrada): ' || v_mensaje);

    -- 4. Prueba: Límite de 3 solicitudes (Postulante 2 postulará a 2 y 3 para llenar el cupo, luego fallará en una extra)
    v_mensaje := F_REGISTRAR_SOLICITUD(2, 2, 'Interés 2');
    v_mensaje := F_REGISTRAR_SOLICITUD(2, 3, 'Interés 3');
    v_mensaje := F_REGISTRAR_SOLICITUD(2, 5, 'Interés 4 - No debería dejarme'); -- Oferta 5 no existe, pero saltará el límite primero
    DBMS_OUTPUT.PUT_LINE('Prueba 4 (Límite de solicitudes): ' || v_mensaje);

    -- 5. Prueba: NO_DATA_FOUND (Postulante que no existe)
    v_mensaje := F_REGISTRAR_SOLICITUD(999, 1, 'Fantasma');
    DBMS_OUTPUT.PUT_LINE('Prueba 5 (Postulante fantasma): ' || v_mensaje);

    -- 6. Prueba: Aceptar Solicitud y descontar cupo
    -- Asumimos que la solicitud de la prueba 1 obtuvo el ID 1
    v_mensaje := F_ACEPTAR_SOLICITUD(1);
    DBMS_OUTPUT.PUT_LINE('Prueba 6 (Aceptar solicitud ID 1): ' || v_mensaje);

    -- 7. Prueba: Agotar cupos de la Oferta 3 (Solo tiene 1 cupo)
    -- Asumimos que la solicitud de la Oferta 3 del postulante 2 es la ID 3
    v_mensaje := F_ACEPTAR_SOLICITUD(3);
    DBMS_OUTPUT.PUT_LINE('Prueba 7a (Aceptar solicitud ID 3 - Usa el último cupo): ' || v_mensaje);

    -- Ahora intentamos registrar y aceptar a otra persona en esa misma oferta (Oferta 3)
    v_mensaje := F_REGISTRAR_SOLICITUD(3, 3, 'Yo también quiero Ciberseguridad');
    -- Asumimos que esta nueva solicitud es la ID 4
    v_mensaje := F_ACEPTAR_SOLICITUD(4);
    DBMS_OUTPUT.PUT_LINE('Prueba 7b (Intentar aceptar sin cupos): ' || v_mensaje);

    -- 8. Prueba: Consultar Ofertas (Cursor devuelto por función)
    DBMS_OUTPUT.PUT_LINE('--- RESULTADOS DEL CURSOR (Solo Maestría) ---');
    v_cursor := F_CONSULTAR_OFERTAS(p_area => 'Maestría', p_id_universidad => NULL, p_tipo_financiamiento => NULL);

    LOOP
        FETCH v_cursor INTO v_id_oferta, v_programa, v_area, v_financiamiento, v_univ, v_sede, v_limite, v_inicio, v_fin, v_cupos;
        EXIT WHEN v_cursor%NOTFOUND;
        DBMS_OUTPUT.PUT_LINE('Oferta ID: ' || v_id_oferta || ' | Prog: ' || v_programa || ' | Univ: ' || v_univ || ' | Cupos restantes: ' || v_cupos);
    END LOOP;
    CLOSE v_cursor;

END;
/
