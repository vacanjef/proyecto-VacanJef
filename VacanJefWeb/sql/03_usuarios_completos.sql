-- ============================================================
-- VACANJEF · Script 03 — Correcciones y usuarios completos
-- Ejecutar sobre la BD VacanJef DESPUÉS de 01_vacanjef_corregido.sql
--
-- Qué hace este script:
--   1. Agrega el cargo faltante (id=5 Auxiliar de servicios)
--      que referenciaban Andrés Forero y Viviana Pinto.
--      Sin este paso el INSERT de esos empleados falla por FK.
--   2. Crea cuentas en Usuarios para los 13 clientes restantes
--      (ya existían Carlos Ramírez y Lucía Gómez).
--   3. Crea cuentas en Usuarios para los 5 empleados restantes
--      (ya existían admin, jorge.salinas y patricia.luna).
--
-- Contraseñas:
--   Clientes  → cliente123
--   Empleados → emp2026
-- ============================================================

USE VacanJef;
GO

-- ============================================================
-- 1. CARGO FALTANTE (id_cargo = 5)
--    Andrés Forero y Viviana Pinto lo necesitan.
--    SET IDENTITY_INSERT fuerza el id exacto.
-- ============================================================
SET IDENTITY_INSERT Cargos ON;
INSERT INTO Cargos (id_cargo, nombre_cargo)
SELECT 5, 'Auxiliar de servicios'
WHERE NOT EXISTS (SELECT 1 FROM Cargos WHERE id_cargo = 5);
SET IDENTITY_INSERT Cargos OFF;
GO

-- ============================================================
-- 2. USUARIOS PARA LOS 13 CLIENTES RESTANTES (cliente123)
-- ============================================================
INSERT INTO Usuarios (correo, password_hash, rol, id_cliente, id_empleado)
SELECT correo, hash, 'Cliente',
       (SELECT id_cliente FROM Clientes WHERE Clientes.correo = src.correo),
       NULL
FROM (VALUES
    ('andres.torres@gmail.com',    '$2b$11$Gn.YQqbHp5jwLnLo02NlFubglweOV7SO3bTh4J9YFMH0.mcvH8.FO'),
    ('vale.herrera@outlook.com',   '$2b$11$.x5NV3yO625jnJXqG5SayOkRlPEOBC3we4e/bhiSRkEdVxjOr0l/O'),
    ('sebas.morales@gmail.com',    '$2b$11$1RyM389lMPbLB5I.oWVcl.sNAA2HYkdzPmKka92L1w8NQ39mhixFS'),
    ('dani.vargas@gmail.com',      '$2b$11$8VjHe3pAy7.lIL.d6bWjhe4j6Y7057vx89RFOXvOr/9/ndEsQO4.u'),
    ('felipe.castillo@yahoo.com',  '$2b$11$.RLcJQZerGYCl3PMeBjUAeMAonqOHGS8UCeqKBg7IvtuLtJQ4x5iu'),
    ('mariana.rios@gmail.com',     '$2b$11$ETsuFz8UE/c3Hgr.lnF2JernJTT2l7Y.x1b/DDviJlQ7IUoIxG68a'),
    ('camilo.pedraza@gmail.com',   '$2b$11$ZCFYxhqTRjSXUslno4kj5.WnMPtR0wmTEsLCdjdXauHJLoaKLZV6O'),
    ('sara.mendoza@hotmail.com',   '$2b$11$kEEq5ykYZcwOoz1VRZXlpeJ27jwL6rMv.rPom3vC3rCcIVZCO1QSq'),
    ('miguel.sanchez@gmail.com',   '$2b$11$GhVgUtKRi/EjlSULL2ou/OO3G4hlpedD2mAIJqdbhWb3kymsFkQwe'),
    ('natalia.castro@outlook.com', '$2b$11$.rk9NMjZsN7vgHHLSbpxKOooSXkqCI0/OO.HFvvGSF7fu7MppaGBK'),
    ('julian.parra@gmail.com',     '$2b$11$EzryJ69V708ta5.bwfudi.YAyBUENBVkHpgIPlvt1xeFIEi7oCTWa'),
    ('isabella.ruiz@hotmail.com',  '$2b$11$/1hOWHkU47JkfBb2G6GRkO7NcNU0fJxF1AXTiST7vZftLQIvTG062'),
    ('esteban.molina@gmail.com',   '$2b$11$Etz9QOyGuknHtpBZaFDxKecWot8ixQTeJ/8vhAmjuOr59ixg.N8Q2')
) AS src(correo, hash)
WHERE NOT EXISTS (
    SELECT 1 FROM Usuarios u WHERE u.correo = src.correo
);
GO

-- ============================================================
-- 3. USUARIOS PARA LOS 5 EMPLEADOS RESTANTES (emp2026)
-- ============================================================
INSERT INTO Usuarios (correo, password_hash, rol, id_cliente, id_empleado)
SELECT correo, hash, 'Empleado', NULL,
       (SELECT id_empleado FROM Empleados WHERE Empleados.correo = src.correo)
FROM (VALUES
    ('diego.ospina@vacanjef.com',   '$2b$11$nJWn5WVkkNDTsicysl3l7uhtW/175IMMuDjcLXEGSGK/9KZ0XeQY.'),
    ('natalia.reyes@vacanjef.com',  '$2b$11$XhmPZshu65IhQtHjanQFAu.3zZLU7K8.URWHjZfwMvT4wzhodH.cC'),
    ('andres.forero@vacanjef.com',  '$2b$11$LB4ycSq1N2nTzcPKSyYH/e1LjcRFX13iZClC0xRP2GlC/vw6pSLtO'),
    ('hernan.quiroz@vacanjef.com',  '$2b$11$bkOtqrjwEJV/qMF6v5yGFeWcmMMLWuPtXw8QydJx8FwxD/jFKFqdm'),
    ('viviana.pinto@vacanjef.com',  '$2b$11$TwlGAACpKwexKxaIZLxppe1nHuBe8S1RyIbbolvvGgRQOBjxAc7Eu')
) AS src(correo, hash)
WHERE NOT EXISTS (
    SELECT 1 FROM Usuarios u WHERE u.correo = src.correo
);
GO

-- ============================================================
-- VERIFICACIÓN FINAL
-- ============================================================
SELECT 'Cargos'    AS tabla, COUNT(*) AS registros FROM Cargos
UNION ALL
SELECT 'Clientes',   COUNT(*) FROM Clientes
UNION ALL
SELECT 'Mascotas',   COUNT(*) FROM Mascotas
UNION ALL
SELECT 'Empleados',  COUNT(*) FROM Empleados
UNION ALL
SELECT 'Reservas',   COUNT(*) FROM Reservas
UNION ALL
SELECT 'Facturas',   COUNT(*) FROM Facturas
UNION ALL
SELECT 'Usuarios',   COUNT(*) FROM Usuarios;

SELECT
    u.correo,
    u.rol,
    CASE u.rol
        WHEN 'Cliente'  THEN c.nombre + ' ' + c.apellido
        WHEN 'Empleado' THEN e.nombre + ' ' + e.apellido
        ELSE 'Admin'
    END AS nombre_real
FROM Usuarios u
LEFT JOIN Clientes  c ON c.id_cliente  = u.id_cliente
LEFT JOIN Empleados e ON e.id_empleado = u.id_empleado
ORDER BY u.rol, u.correo;

PRINT 'Script 03 completado. Todos los usuarios tienen cuenta de acceso.';
GO
