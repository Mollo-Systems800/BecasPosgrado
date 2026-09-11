-- =========================================================
-- MIGRACIÓN 7: agrega componentes (módulos/materias) de ejemplo
-- para cada programa, así la pantalla de Componentes ya no
-- aparece vacía. Usa subconsultas por nombre de programa en vez
-- de IDs fijos, para que funcione sin importar el orden en que
-- se hayan insertado tus programas.
-- Ejecutar UNA sola vez contra la base de datos que ya tienes migrada.
-- =========================================================

-- Maestría en Ciencia de Datos
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Fundamentos de Inteligencia Artificial', id FROM PROGRAMA WHERE nombre = 'Maestría en Ciencia de Datos';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Estadística Aplicada', id FROM PROGRAMA WHERE nombre = 'Maestría en Ciencia de Datos';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Big Data y Computación Distribuida', id FROM PROGRAMA WHERE nombre = 'Maestría en Ciencia de Datos';

-- Maestría en Finanzas
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Finanzas Corporativas', id FROM PROGRAMA WHERE nombre = 'Maestría en Finanzas';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Mercados de Capitales', id FROM PROGRAMA WHERE nombre = 'Maestría en Finanzas';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Gestión de Riesgo Financiero', id FROM PROGRAMA WHERE nombre = 'Maestría en Finanzas';

-- Doctorado en Física
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Mecánica Cuántica Avanzada', id FROM PROGRAMA WHERE nombre = 'Doctorado en Física';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Métodos Computacionales en Física', id FROM PROGRAMA WHERE nombre = 'Doctorado en Física';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Seminario de Investigación I', id FROM PROGRAMA WHERE nombre = 'Doctorado en Física';

-- Doctorado en Educación
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Políticas Educativas Comparadas', id FROM PROGRAMA WHERE nombre = 'Doctorado en Educación';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Metodología de Investigación Educativa', id FROM PROGRAMA WHERE nombre = 'Doctorado en Educación';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Seminario de Tesis', id FROM PROGRAMA WHERE nombre = 'Doctorado en Educación';

-- Especialidad en Ciberseguridad
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Seguridad en Redes', id FROM PROGRAMA WHERE nombre = 'Especialidad en Ciberseguridad';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Hacking Ético', id FROM PROGRAMA WHERE nombre = 'Especialidad en Ciberseguridad';
INSERT INTO COMPONENTE (nombre, id_programa)
SELECT 'Criptografía Aplicada', id FROM PROGRAMA WHERE nombre = 'Especialidad en Ciberseguridad';

COMMIT;
