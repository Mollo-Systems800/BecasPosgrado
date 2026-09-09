-- 1. POSTULANTES
INSERT INTO POSTULANTE (nombre, apellido, email, telefono, direccion, fecha_nac, usuario, clave, rol)
VALUES ('Carlos', 'Admin', 'admin@becas.com', '12345678', 'Av. Central 1', DATE '1985-05-15', 'cadmin', 'hash123', 'ADMIN');

INSERT INTO POSTULANTE (nombre, apellido, email, telefono, direccion, fecha_nac, usuario, clave, rol)
VALUES ('Ana', 'Pérez', 'ana@gmail.com', '87654321', 'Calle Sur 2', DATE '1992-10-20', 'aperez', 'hash123', 'POSTULANTE');

INSERT INTO POSTULANTE (nombre, apellido, email, telefono, direccion, fecha_nac, usuario, clave, rol)
VALUES ('Luis', 'Gómez', 'luis@gmail.com', '11223344', 'Av. Norte 3', DATE '1990-03-10', 'lgomez', 'hash123', 'POSTULANTE');

INSERT INTO POSTULANTE (nombre, apellido, email, telefono, direccion, fecha_nac, usuario, clave, rol)
VALUES ('María', 'López', 'maria@gmail.com', '55667788', 'Plaza Este 4', DATE '1995-07-25', 'mlopez', 'hash123', 'POSTULANTE');

INSERT INTO POSTULANTE (nombre, apellido, email, telefono, direccion, fecha_nac, usuario, clave, rol)
VALUES ('Jorge', 'Díaz', 'jorge@gmail.com', '99887766', 'Pasaje Oeste 5', DATE '1991-12-05', 'jdiaz', 'hash123', 'POSTULANTE');

-- 2. UNIVERSIDADES
INSERT INTO UNIVERSIDAD (nombre, pais, ciudad, direccion, telefono)
VALUES ('Universidad Andina', 'Bolivia', 'La Paz', 'Av. Arce 123', '2112233');

INSERT INTO UNIVERSIDAD (nombre, pais, ciudad, direccion, telefono)
VALUES ('Universidad del Valle', 'Colombia', 'Cali', 'Calle 13 # 100', '3214567');

INSERT INTO UNIVERSIDAD (nombre, pais, ciudad, direccion, telefono)
VALUES ('Tech University', 'México', 'Monterrey', 'Av. Garza 456', '81123456');

-- 3. SEDES
INSERT INTO SEDE (nombre, direccion, id_universidad) VALUES ('Campus Central LP', 'Av. Arce 123', 1);
INSERT INTO SEDE (nombre, direccion, id_universidad) VALUES ('Sede Meléndez', 'Calle 13 # 100', 2);
INSERT INTO SEDE (nombre, direccion, id_universidad) VALUES ('Campus Norte MTY', 'Av. Garza 456', 3);

-- 4. PROGRAMAS (2 Maestría, 2 Doctorado, 1 Especialidad)
INSERT INTO PROGRAMA (nombre, descripcion, area, tipo_financiamiento)
VALUES ('Maestría en Ciencia de Datos', 'Enfoque en IA y Big Data', 'Maestría', 'Completo');

INSERT INTO PROGRAMA (nombre, descripcion, area, tipo_financiamiento)
VALUES ('Maestría en Finanzas', 'Finanzas corporativas y mercados', 'Maestría', 'Parcial');

INSERT INTO PROGRAMA (nombre, descripcion, area, tipo_financiamiento)
VALUES ('Doctorado en Física', 'Investigación cuántica', 'Doctorado', 'Completo');

INSERT INTO PROGRAMA (nombre, descripcion, area, tipo_financiamiento)
VALUES ('Doctorado en Educación', 'Políticas educativas públicas', 'Doctorado', 'Completo');

INSERT INTO PROGRAMA (nombre, descripcion, area, tipo_financiamiento)
VALUES ('Especialidad en Ciberseguridad', 'Seguridad en redes y hacking ético', 'Especialidad', 'Parcial');

-- 5. OFERTAS (3 Activas, 1 Cerrada)
-- Oferta 1: Activa (inició hace 10 días, termina en 30 días)
INSERT INTO OFERTA (id_programa, id_sede, fecha_inicio, fecha_fin, cupos_disponibles, estado)
VALUES (1, 1, SYSDATE - 10, SYSDATE + 30, 10, 'Activa');

-- Oferta 2: Activa
INSERT INTO OFERTA (id_programa, id_sede, fecha_inicio, fecha_fin, cupos_disponibles, estado)
VALUES (2, 2, SYSDATE - 5, SYSDATE + 45, 5, 'Activa');

-- Oferta 3: Activa (Queda 1 cupo para probar validación de límite)
INSERT INTO OFERTA (id_programa, id_sede, fecha_inicio, fecha_fin, cupos_disponibles, estado)
VALUES (5, 3, SYSDATE - 2, SYSDATE + 15, 1, 'Activa');

-- Oferta 4: Cerrada (Terminó hace 5 días)
INSERT INTO OFERTA (id_programa, id_sede, fecha_inicio, fecha_fin, cupos_disponibles, estado)
VALUES (3, 1, SYSDATE - 60, SYSDATE - 5, 2, 'Cerrada');

COMMIT;
