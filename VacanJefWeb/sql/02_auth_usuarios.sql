-- ============================================================
-- VACANJEF · Script de autenticación
-- Ejecutar DESPUÉS de vacanjef_corregido.sql, sobre la misma BD.
-- Agrega una tabla nueva (Usuarios) sin modificar Clientes ni
-- Empleados; cumple RNF03 (contraseñas cifradas con bcrypt).
-- ============================================================

USE VacanJef;
GO

CREATE TABLE Usuarios (
    id_usuario      INT IDENTITY(1,1) PRIMARY KEY,
    correo          NVARCHAR(150) NOT NULL UNIQUE,
    password_hash   NVARCHAR(255) NOT NULL,
    rol             NVARCHAR(20)  NOT NULL
                        CHECK (rol IN ('Administrador','Empleado','Cliente')),
    id_cliente      INT NULL,
    id_empleado     INT NULL,
    activo          BIT DEFAULT 1,
    fecha_creacion  DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Usuario_Cliente  FOREIGN KEY (id_cliente)  REFERENCES Clientes(id_cliente)   ON DELETE SET NULL,
    CONSTRAINT FK_Usuario_Empleado FOREIGN KEY (id_empleado) REFERENCES Empleados(id_empleado) ON DELETE SET NULL
);
GO

-- ============================================================
-- Usuarios de demostración (mismas credenciales que la maqueta
-- de login original, ahora respaldadas por hash bcrypt real).
-- admin@vacanjef.com          / admin123    -> Administradora Claudia Bernal
-- jorge.salinas@vacanjef.com  / emp2026     -> Cuidador
-- patricia.luna@vacanjef.com  / emp2026     -> Recepcionista
-- carlos.ramirez@gmail.com    / cliente123  -> Cliente
-- lucia.gomez@hotmail.com     / cliente123  -> Cliente
-- ============================================================
INSERT INTO Usuarios (correo, password_hash, rol, id_cliente, id_empleado) VALUES
    ('admin@vacanjef.com',         '$2b$11$zxcYjLyefvCPoVIzxeN2EO3AG8rOIIw7C0yrmDbMiX0BOgNPGcqZa', 'Administrador', NULL, (SELECT id_empleado FROM Empleados WHERE correo = 'claudia.bernal@vacanjef.com')),
    ('jorge.salinas@vacanjef.com', '$2b$11$r0AU.2dUQOfSaFmGxwBzPeoznjZF2T8p1xlBjGwmNjkkEB.cfZAUC',  'Empleado',       NULL, (SELECT id_empleado FROM Empleados WHERE correo = 'jorge.salinas@vacanjef.com')),
    ('patricia.luna@vacanjef.com','$2b$11$7vrCJcDdIMbRASKM.YsBk.vZL1l.0QPkkrM4XToWtdBuU9j3Kz5Fq',   'Empleado',       NULL, (SELECT id_empleado FROM Empleados WHERE correo = 'patricia.luna@vacanjef.com')),
    ('carlos.ramirez@gmail.com',  '$2b$11$LY7lNEEGor9.Kky3EoWurOeOFrvMmOb.C1Fm6oUdviJoLyw23nbRC',   'Cliente',        (SELECT id_cliente FROM Clientes WHERE correo = 'carlos.ramirez@gmail.com'), NULL),
    ('lucia.gomez@hotmail.com',   '$2b$11$/NIr5VFNx.dqSE.ahxztHOwIZGKKv4unuTi6qTsSEhGLHQifii2QS',   'Cliente',        (SELECT id_cliente FROM Clientes WHERE correo = 'lucia.gomez@hotmail.com'),   NULL);
GO

PRINT 'Tabla Usuarios creada y usuarios de demostración cargados.';
GO
