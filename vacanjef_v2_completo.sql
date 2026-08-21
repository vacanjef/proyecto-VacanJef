-- ============================================================
-- VACANJEF · Base de datos completa v2.0 — DESDE CERO
-- Motor: SQL Server 2017+
-- Ejecutar en SSMS conectado a DESKTOP-RDBPFO1
--
-- Qué incluye:
--   · Elimina la BD anterior si existe
--   · 10 tablas sin errores de FK
--   · 5 cargos · 6 servicios · 7 métodos de pago
--   · 8 empleados · 15 clientes · 20 mascotas
--   · 25 reservas · 25 facturas
--   · 23 usuarios con contraseñas bcrypt
--     Admin      : admin@vacanjef.com        / admin123
--     Empleados  : [correo]@vacanjef.com      / emp2026
--     Clientes   : [correo]                   / cliente123
-- ============================================================

USE master;
GO

-- ── Eliminar BD anterior (desconecta sesiones activas) ───────
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'VacanJef')
BEGIN
    ALTER DATABASE VacanJef SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VacanJef;
END
GO

CREATE DATABASE VacanJef
    COLLATE Modern_Spanish_CI_AI;
GO

USE VacanJef;
GO

-- ============================================================
-- TABLAS (en orden de dependencia)
-- ============================================================

-- 1. Cargos
CREATE TABLE Cargos (
    id_cargo     INT IDENTITY(1,1) PRIMARY KEY,
    nombre_cargo NVARCHAR(100) NOT NULL UNIQUE
);
GO

-- 2. Clientes
CREATE TABLE Clientes (
    id_cliente     INT IDENTITY(1,1) PRIMARY KEY,
    nombre         NVARCHAR(100) NOT NULL,
    apellido       NVARCHAR(100) NOT NULL,
    telefono       NVARCHAR(20),
    direccion      NVARCHAR(255),
    correo         NVARCHAR(150) UNIQUE,
    fecha_registro DATETIME2     NOT NULL DEFAULT GETDATE(),
    activo         BIT           NOT NULL DEFAULT 1
);
GO

-- 3. Contactos_Emergencia
CREATE TABLE Contactos_Emergencia (
    id_contacto INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente  INT          NOT NULL,
    nombre      NVARCHAR(100) NOT NULL,
    apellido    NVARCHAR(100) NOT NULL DEFAULT '',
    telefono    NVARCHAR(20),
    correo      NVARCHAR(150),
    parentesco  NVARCHAR(50),
    CONSTRAINT FK_CE_Cliente FOREIGN KEY (id_cliente)
        REFERENCES Clientes(id_cliente) ON DELETE CASCADE
);
GO

-- 4. Mascotas
CREATE TABLE Mascotas (
    id_mascota       INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente       INT           NOT NULL,
    nombre           NVARCHAR(100) NOT NULL,
    especie          NVARCHAR(50)  NOT NULL,
    raza             NVARCHAR(100),
    sexo             CHAR(1)       CHECK (sexo IN ('M','H')),
    fecha_nacimiento DATE,
    color            NVARCHAR(50),
    peso_kg          DECIMAL(5,2),
    alergias         NVARCHAR(500),
    medicamentos     NVARCHAR(500),
    observaciones    NVARCHAR(1000),
    carnet_vacunas   NVARCHAR(255),
    activo           BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Mascota_Cliente FOREIGN KEY (id_cliente)
        REFERENCES Clientes(id_cliente) ON DELETE CASCADE
);
GO

-- 5. Empleados
CREATE TABLE Empleados (
    id_empleado           INT IDENTITY(1,1) PRIMARY KEY,
    nombre                NVARCHAR(100) NOT NULL,
    apellido              NVARCHAR(100) NOT NULL,
    telefono              NVARCHAR(20),
    direccion             NVARCHAR(255),
    correo                NVARCHAR(150) UNIQUE,
    id_cargo              INT,
    salario               DECIMAL(12,2) CHECK (salario >= 0),
    fecha_ingreso         DATE NOT NULL DEFAULT CAST(GETDATE() AS DATE),
    activo                BIT  NOT NULL DEFAULT 1,
    emergencia_nombre     NVARCHAR(200),
    emergencia_telefono   NVARCHAR(20),
    emergencia_parentesco NVARCHAR(50),
    CONSTRAINT FK_Empleado_Cargo FOREIGN KEY (id_cargo)
        REFERENCES Cargos(id_cargo) ON DELETE SET NULL
);
GO

-- 6. Tipos_Servicio
CREATE TABLE Tipos_Servicio (
    id_tipo_servicio INT IDENTITY(1,1) PRIMARY KEY,
    nombre_servicio  NVARCHAR(100) NOT NULL UNIQUE,
    descripcion      NVARCHAR(500),
    precio_base      DECIMAL(12,2) NOT NULL CHECK (precio_base >= 0)
);
GO

-- 7. Metodos_Pago
CREATE TABLE Metodos_Pago (
    id_metodo_pago INT IDENTITY(1,1) PRIMARY KEY,
    nombre_metodo  NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- 8. Reservas
CREATE TABLE Reservas (
    id_reserva       INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente       INT           NOT NULL,
    id_mascota       INT           NOT NULL,
    id_empleado      INT,
    id_tipo_servicio INT,
    fecha_inicio     DATETIME2     NOT NULL,
    fecha_fin        DATETIME2,
    estado_reserva   NVARCHAR(20)  NOT NULL DEFAULT 'Pendiente'
                         CHECK (estado_reserva IN
                                ('Pendiente','Confirmada','En curso','Finalizada','Cancelada')),
    observaciones    NVARCHAR(500),
    fecha_creacion   DATETIME2     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Res_Cliente  FOREIGN KEY (id_cliente)       REFERENCES Clientes(id_cliente),
    CONSTRAINT FK_Res_Mascota  FOREIGN KEY (id_mascota)       REFERENCES Mascotas(id_mascota),
    CONSTRAINT FK_Res_Empleado FOREIGN KEY (id_empleado)      REFERENCES Empleados(id_empleado) ON DELETE SET NULL,
    CONSTRAINT FK_Res_Servicio FOREIGN KEY (id_tipo_servicio) REFERENCES Tipos_Servicio(id_tipo_servicio)
);
GO

-- 9. Facturas  (total es columna calculada persistida)
CREATE TABLE Facturas (
    id_factura     INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente     INT           NOT NULL,
    id_reserva     INT           NOT NULL UNIQUE,
    id_metodo_pago INT,
    fecha_emision  DATETIME2     NOT NULL DEFAULT GETDATE(),
    subtotal       DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (subtotal >= 0),
    descuento      DECIMAL(12,2)          DEFAULT 0 CHECK (descuento >= 0),
    impuesto_pct   DECIMAL(5,2)           DEFAULT 0 CHECK (impuesto_pct >= 0),
    total AS (subtotal - descuento + (subtotal - descuento) * impuesto_pct / 100) PERSISTED,
    estado_pago    NVARCHAR(20)  NOT NULL DEFAULT 'Pendiente'
                       CHECK (estado_pago IN ('Pagado','Pendiente','Sin cancelar','Anulado')),
    notas          NVARCHAR(500),
    CONSTRAINT FK_Fac_Cliente    FOREIGN KEY (id_cliente)     REFERENCES Clientes(id_cliente),
    CONSTRAINT FK_Fac_Reserva    FOREIGN KEY (id_reserva)     REFERENCES Reservas(id_reserva),
    CONSTRAINT FK_Fac_MetodoPago FOREIGN KEY (id_metodo_pago) REFERENCES Metodos_Pago(id_metodo_pago)
);
GO

-- 10. Usuarios  (autenticación con bcrypt)
CREATE TABLE Usuarios (
    id_usuario    INT IDENTITY(1,1) PRIMARY KEY,
    correo        NVARCHAR(150) NOT NULL UNIQUE,
    password_hash NVARCHAR(255) NOT NULL,
    rol           NVARCHAR(20)  NOT NULL
                      CHECK (rol IN ('Administrador','Empleado','Cliente')),
    id_cliente    INT,
    id_empleado   INT,
    activo        BIT       NOT NULL DEFAULT 1,
    fecha_creacion DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Usr_Cliente  FOREIGN KEY (id_cliente)  REFERENCES Clientes(id_cliente)  ON DELETE SET NULL,
    CONSTRAINT FK_Usr_Empleado FOREIGN KEY (id_empleado) REFERENCES Empleados(id_empleado) ON DELETE SET NULL
);
GO

-- ============================================================
-- ÍNDICES
-- ============================================================
CREATE INDEX IX_Mascotas_Cliente    ON Mascotas(id_cliente);
CREATE INDEX IX_Reservas_Cliente    ON Reservas(id_cliente);
CREATE INDEX IX_Reservas_Fecha      ON Reservas(fecha_inicio);
CREATE INDEX IX_Reservas_Estado     ON Reservas(estado_reserva);
CREATE INDEX IX_Facturas_Cliente    ON Facturas(id_cliente);
CREATE INDEX IX_Facturas_Estado     ON Facturas(estado_pago);
CREATE INDEX IX_Usuarios_Correo     ON Usuarios(correo);
GO

-- ============================================================
-- CATÁLOGOS BASE
-- ============================================================

-- 5 cargos (coincide exactamente con los id usados en Empleados)
INSERT INTO Cargos (nombre_cargo) VALUES
    ('Administrador'),      -- id=1
    ('Cuidador'),           -- id=2
    ('Recepcionista'),      -- id=3
    ('Auxiliar de servicios'), -- id=4
    ('Veterinario');        -- id=5
GO

-- 6 tipos de servicio
INSERT INTO Tipos_Servicio (nombre_servicio, descripcion, precio_base) VALUES
    ('Estadia',               'Tarifa diaria de hospedaje en instalaciones',     50000),  -- id=1
    ('Paseo',                 'Paseo con cuidador (tarifa por visita)',           20000),  -- id=2
    ('Bano y peluqueria',     'Baño completo, secado y corte básico',            45000),  -- id=3
    ('Consulta veterinaria',  'Revisión básica de salud con veterinario',        60000),  -- id=4
    ('Entrenamiento',         'Sesión de entrenamiento de obediencia básica',    70000),  -- id=5
    ('Guarderia express',     'Cuidado de medio día (hasta 5 horas)',            30000);  -- id=6
GO

-- 7 métodos de pago
INSERT INTO Metodos_Pago (nombre_metodo) VALUES
    ('Efectivo'),               -- id=1
    ('Tarjeta debito'),         -- id=2
    ('Tarjeta credito'),        -- id=3
    ('Transferencia bancaria'), -- id=4
    ('Nequi'),                  -- id=5
    ('Daviplata'),              -- id=6
    ('PSE');                    -- id=7
GO

-- ============================================================
-- CLIENTES (15)
-- ============================================================
INSERT INTO Clientes (nombre, apellido, telefono, direccion, correo, fecha_registro) VALUES
    ('Carlos',    'Ramirez',  '3101234567', 'Calle 45 #12-30, Medellin',       'carlos.ramirez@gmail.com',    '2025-11-03'),
    ('Lucia',     'Gomez',    '3152345678', 'Carrera 7 #80-15, Medellin',      'lucia.gomez@hotmail.com',     '2025-11-10'),
    ('Andres',    'Torres',   '3203456789', 'Av. El Poblado #23-10, Medellin', 'andres.torres@gmail.com',     '2025-11-18'),
    ('Valentina', 'Herrera',  '3114567890', 'Calle 10 #43-40, Medellin',       'vale.herrera@outlook.com',    '2025-12-01'),
    ('Sebastian', 'Morales',  '3165678901', 'Carrera 65 #33-20, Medellin',     'sebas.morales@gmail.com',     '2025-12-15'),
    ('Daniela',   'Vargas',   '3006789012', 'Calle 30 #72-18, Medellin',       'dani.vargas@gmail.com',       '2026-01-05'),
    ('Felipe',    'Castillo', '3187890123', 'Carrera 43A #15-60, Medellin',    'felipe.castillo@yahoo.com',   '2026-01-20'),
    ('Mariana',   'Rios',     '3138901234', 'Av. Las Vegas #22-35, Medellin',  'mariana.rios@gmail.com',      '2026-02-08'),
    ('Camilo',    'Pedraza',  '3019012345', 'Calle 50 #80-12, Medellin',       'camilo.pedraza@gmail.com',    '2026-02-22'),
    ('Sara',      'Mendoza',  '3170123456', 'Carrera 70 #10-90, Medellin',     'sara.mendoza@hotmail.com',    '2026-03-10'),
    ('Miguel',    'Sanchez',  '3122345671', 'Calle 80 #34-20, Medellin',       'miguel.sanchez@gmail.com',    '2026-03-25'),
    ('Natalia',   'Castro',   '3143456782', 'Carrera 32 #55-10, Medellin',     'natalia.castro@outlook.com',  '2026-04-07'),
    ('Julian',    'Parra',    '3054567893', 'Av. Nutibara #12-40, Medellin',   'julian.parra@gmail.com',      '2026-04-18'),
    ('Isabella',  'Ruiz',     '3185678904', 'Calle 33 #65-30, Medellin',       'isabella.ruiz@hotmail.com',   '2026-05-02'),
    ('Esteban',   'Molina',   '3106789015', 'Carrera 48 #20-55, Medellin',     'esteban.molina@gmail.com',    '2026-05-20');
GO

-- ============================================================
-- CONTACTOS DE EMERGENCIA (1 por cliente)
-- ============================================================
INSERT INTO Contactos_Emergencia (id_cliente, nombre, apellido, telefono, correo, parentesco) VALUES
    (1,  'Maria',    'Ramirez',  '3109876543', 'maria.ramirez@gmail.com',    'Mama'),
    (2,  'Pedro',    'Gomez',    '3153456789', 'pedro.gomez@hotmail.com',    'Esposo'),
    (3,  'Sofia',    'Torres',   '3204567890', 'sofia.torres@gmail.com',     'Hermana'),
    (4,  'Juan',     'Herrera',  '3115678901', 'juan.herrera@outlook.com',   'Papa'),
    (5,  'Laura',    'Morales',  '3166789012', 'laura.morales@gmail.com',    'Mama'),
    (6,  'Camilo',   'Vargas',   '3007890123', 'camilo.vargas@gmail.com',    'Hermano'),
    (7,  'Ana',      'Castillo', '3188901234', 'ana.castillo@yahoo.com',     'Mama'),
    (8,  'Luis',     'Rios',     '3139012345', 'luis.rios@gmail.com',        'Esposo'),
    (9,  'Diana',    'Pedraza',  '3010123456', 'diana.pedraza@gmail.com',    'Mama'),
    (10, 'Jorge',    'Mendoza',  '3171234567', 'jorge.mendoza@hotmail.com',  'Papa'),
    (11, 'Rosa',     'Sanchez',  '3123456781', 'rosa.sanchez@gmail.com',     'Mama'),
    (12, 'Ernesto',  'Castro',   '3144567892', 'ernesto.castro@outlook.com', 'Papa'),
    (13, 'Carmen',   'Parra',    '3055678903', 'carmen.parra@gmail.com',     'Mama'),
    (14, 'Tomas',    'Ruiz',     '3186789014', 'tomas.ruiz@hotmail.com',     'Papa'),
    (15, 'Patricia', 'Molina',   '3107890125', 'patricia.molina@gmail.com',  'Mama');
GO

-- ============================================================
-- MASCOTAS (20)
-- ============================================================
INSERT INTO Mascotas
    (id_cliente, nombre, especie, raza, sexo, fecha_nacimiento, color, peso_kg,
     alergias, medicamentos, observaciones, carnet_vacunas)
VALUES
    (1,  'Rocky',  'Perro', 'Labrador Retriever', 'M', '2023-03-10', 'Amarillo',       28.5, 'Ninguna',   'Ninguno',          'Le gusta jugar con agua',              'Al dia'),
    (1,  'Luna',   'Gato',  'Siames',             'H', '2024-01-15', 'Blanco/Gris',     4.2, 'Polen',     'Antihistaminico',  'Muy carinhosa',                        'Al dia'),
    (2,  'Max',    'Perro', 'Bulldog Frances',    'M', '2021-06-20', 'Atigrado',        12.0, 'Ninguna',   'Ninguno',          'Ronca al dormir',                      'Al dia'),
    (3,  'Bella',  'Perro', 'Golden Retriever',   'H', '2025-02-05', 'Dorado',          22.0, 'Ninguna',   'Ninguno',          'Cachorra muy activa',                  'Al dia'),
    (3,  'Simba',  'Gato',  'Persa',              'M', '2022-08-12', 'Naranja',          5.8, 'Ninguna',   'Ninguno',          'Pelo largo, requiere cepillado diario','Al dia'),
    (4,  'Nala',   'Perro', 'Poodle',             'H', '2020-04-18', 'Blanco',           7.5, 'Ninguna',   'Ninguno',          'Muy obediente',                        'Al dia'),
    (5,  'Thor',   'Perro', 'Rottweiler',         'M', '2024-05-01', 'Negro/Cafe',      38.0, 'Ninguna',   'Ninguno',          'Necesita correa resistente',           'Al dia'),
    (5,  'Mia',    'Gato',  'Maine Coon',         'H', '2023-07-22', 'Gris',             6.5, 'Mariscos',  'Ninguno',          'No darle alimentos con mariscos',      'Al dia'),
    (6,  'Toby',   'Perro', 'Beagle',             'M', '2022-11-30', 'Tricolor',        13.0, 'Ninguna',   'Ninguno',          'Muy curioso, olfatea todo',            'Al dia'),
    (7,  'Coco',   'Ave',   'Loro Amazonico',     'M', '2019-03-14', 'Verde/Rojo',       0.5, 'Ninguna',   'Ninguno',          'Habla varias palabras',                'Al dia'),
    (7,  'Daisy',  'Perro', 'Chihuahua',          'H', '2024-09-08', 'Cafe',             2.8, 'Ninguna',   'Ninguno',          'Se asusta con ruidos fuertes',         'Al dia'),
    (8,  'Bruno',  'Perro', 'Pastor Aleman',      'M', '2021-01-25', 'Negro/Cafe',      32.0, 'Pollo',     'Dieta especial',   'Alergico al pollo, dieta de res',      'Al dia'),
    (8,  'Kitty',  'Gato',  'Ragdoll',            'H', '2025-03-10', 'Blanco',           4.0, 'Ninguna',   'Ninguno',          'Muy tranquila',                        'Al dia'),
    (9,  'Duke',   'Perro', 'Doberman',           'M', '2023-06-17', 'Negro/Cafe',      30.0, 'Ninguna',   'Ninguno',          'Entrenado, obedece comandos',          'Al dia'),
    (10, 'Pelusa', 'Conejo','Holland Lop',        'H', '2024-02-20', 'Blanco',           2.0, 'Ninguna',   'Ninguno',          'Requiere jaula por las noches',        'Al dia'),
    (11, 'Loki',   'Perro', 'Husky Siberiano',    'M', '2022-04-05', 'Blanco/Gris',     25.0, 'Ninguna',   'Ninguno',          'Necesita mucho ejercicio diario',      'Al dia'),
    (12, 'Canela', 'Perro', 'Cocker Spaniel',     'H', '2023-09-13', 'Canela',          12.5, 'Ninguna',   'Ninguno',          'Orejas largas, revisar higiene',       'Al dia'),
    (13, 'Pisco',  'Ave',   'Cacatua',            'M', '2021-12-01', 'Blanco/Amarillo',  0.4, 'Ninguna',   'Ninguno',          'Le gusta la musica',                   'Al dia'),
    (14, 'Sasha',  'Perro', 'Schnauzer',          'H', '2024-07-19', 'Gris',             8.0, 'Ninguna',   'Ninguno',          'Muy inteligente',                      'Al dia'),
    (15, 'Gordo',  'Gato',  'British Shorthair',  'M', '2020-10-28', 'Azul/Gris',        7.2, 'Ninguna',   'Ninguno',          'Sedentario, controlar peso',           'Al dia');
GO

-- ============================================================
-- EMPLEADOS (8)
-- id_cargo: 1=Admin 2=Cuidador 3=Recepcionista 4=Auxiliar 5=Veterinario
-- ============================================================
INSERT INTO Empleados
    (nombre, apellido, telefono, direccion, correo,
     id_cargo, salario, fecha_ingreso,
     emergencia_nombre, emergencia_telefono, emergencia_parentesco)
VALUES
    ('Claudia', 'Bernal',  '3170000002', 'Calle 13 #80-60, Medellin',  'claudia.bernal@vacanjef.com',  1, 4500000, '2020-01-15', 'Raul Bernal',    '3170000003', 'Esposo'),
    ('Jorge',   'Salinas', '3121111111', 'Calle 50 #20-10, Medellin',  'jorge.salinas@vacanjef.com',   2, 2200000, '2021-03-01', 'Rosa Salinas',   '3122222222', 'Mama'),
    ('Patricia','Luna',    '3133333333', 'Carrera 9 #35-20, Medellin', 'patricia.luna@vacanjef.com',   3, 2000000, '2021-06-15', 'Marco Luna',     '3134444444', 'Esposo'),
    ('Diego',   'Ospina',  '3145555555', 'Calle 80 #55-30, Medellin',  'diego.ospina@vacanjef.com',    2, 2200000, '2022-01-10', 'Clara Ospina',   '3146666666', 'Mama'),
    ('Natalia', 'Reyes',   '3157777777', 'Av. 30 #12-45, Medellin',   'natalia.reyes@vacanjef.com',   2, 2200000, '2022-04-20', 'Tomas Reyes',    '3158888888', 'Papa'),
    ('Andres',  'Forero',  '3169999999', 'Carrera 22 #67-10, Medellin','andres.forero@vacanjef.com',   4, 1800000, '2023-02-01', 'Helena Forero',  '3160000001', 'Mama'),
    ('Hernan',  'Quiroz',  '3181111112', 'Carrera 70 #40-15, Medellin','hernan.quiroz@vacanjef.com',   2, 2200000, '2023-07-15', 'Marta Quiroz',   '3182222223', 'Esposa'),
    ('Viviana', 'Pinto',   '3193333334', 'Calle 25 #90-20, Medellin',  'viviana.pinto@vacanjef.com',   4, 1800000, '2024-01-08', 'Carlos Pinto',   '3194444445', 'Papa');
GO

-- ============================================================
-- RESERVAS (25)
-- id_tipo_servicio: 1=Estadia 2=Paseo 3=Bano 4=Consulta 5=Entrenamiento 6=Express
-- ============================================================
INSERT INTO Reservas
    (id_cliente, id_mascota, id_empleado, id_tipo_servicio,
     fecha_inicio, fecha_fin, estado_reserva, observaciones)
VALUES
-- Historial mayo 2026 (Finalizadas)
    (1,  1,  4, 1, '2026-05-01 08:00', '2026-05-05 17:00', 'Finalizada', 'Traer su juguete favorito'),
    (2,  3,  5, 1, '2026-05-03 08:00', '2026-05-07 17:00', 'Finalizada', 'Ronca, es normal'),
    (3,  4,  4, 2, '2026-05-05 07:00', '2026-05-05 08:00', 'Finalizada', 'Paseo por parque Arvi'),
    (4,  6,  6, 2, '2026-05-06 07:00', '2026-05-06 08:00', 'Finalizada', 'Ruta corta poodle mayor'),
    (5,  7,  4, 1, '2026-05-08 08:00', '2026-05-13 17:00', 'Finalizada', 'Necesita espacio amplio'),
    (6,  9,  7, 2, '2026-05-10 07:00', '2026-05-10 08:00', 'Finalizada', 'Ruta parque El Poblado'),
    (7,  10, 5, 1, '2026-05-12 08:00', '2026-05-19 17:00', 'Finalizada', 'Loro, area tranquila'),
    (8,  12, 4, 1, '2026-05-14 08:00', '2026-05-21 17:00', 'Finalizada', 'Solo dieta de res, sin pollo'),
    (9,  14, 6, 2, '2026-05-15 07:00', '2026-05-15 08:00', 'Finalizada', 'Doberman, ruta larga'),
    (10, 15, 8, 1, '2026-05-16 08:00', '2026-05-20 17:00', 'Finalizada', 'Conejo, jaula incluida'),
    (11, 16, 4, 1, '2026-05-18 08:00', '2026-05-23 17:00', 'Finalizada', 'Husky, mucho ejercicio'),
    (12, 17, 7, 2, '2026-05-20 07:00', '2026-05-20 08:00', 'Finalizada', 'Revisar orejas al volver'),
    (13, 18, 5, 1, '2026-05-22 08:00', '2026-05-27 17:00', 'Finalizada', 'Cacatua, ambiente silencioso'),
    (14, 19, 6, 2, '2026-05-24 07:00', '2026-05-24 08:00', 'Finalizada', 'Schnauzer, ruta media'),
    (15, 20, 4, 1, '2026-05-25 08:00', '2026-05-30 17:00', 'Finalizada', 'Gato sedentario'),
-- Reservas activas junio 2026 (Confirmadas)
    (1,  2,  5, 1, '2026-06-01 08:00', '2026-06-06 17:00', 'Confirmada', 'Gato siames, alergico al polen'),
    (3,  5,  4, 6, '2026-06-02 08:00', '2026-06-02 13:00', 'Confirmada', 'Gato persa, guarderia express'),
    (5,  8,  7, 1, '2026-06-03 08:00', '2026-06-08 17:00', 'Confirmada', 'Maine Coon sin mariscos'),
    (6,  9,  6, 2, '2026-06-04 07:00', '2026-06-04 08:00', 'Confirmada', 'Beagle curioso, atencion al olfato'),
    (8,  13, 8, 1, '2026-06-05 08:00', '2026-06-12 17:00', 'Confirmada', 'Ragdoll tranquila, sin ruido'),
    (9,  14, 6, 3, '2026-06-06 09:00', '2026-06-06 11:00', 'Confirmada', 'Bano y peluqueria Doberman'),
-- Reservas pendientes
    (11, 16, 4, 1, '2026-06-07 08:00', '2026-06-14 17:00', 'Pendiente',  'Husky, area ventilada'),
    (13, 18, 5, 2, '2026-06-08 07:00', '2026-06-08 08:00', 'Pendiente',  'Cacatua en cargador especial'),
    (14, 19, 7, 1, '2026-06-10 08:00', '2026-06-17 17:00', 'Pendiente',  'Schnauzer, cepillado diario'),
    (15, 20, 8, 2, '2026-06-12 07:00', '2026-06-12 08:00', 'Pendiente',  'Gato British, primer paseo');
GO

-- ============================================================
-- FACTURAS (25)
-- ============================================================
INSERT INTO Facturas
    (id_cliente, id_reserva, id_metodo_pago, fecha_emision,
     subtotal, descuento, impuesto_pct, estado_pago, notas)
VALUES
    (1,  1,  1, '2026-05-05', 200000, 0, 0, 'Pagado',       NULL),
    (2,  2,  4, '2026-05-07', 200000, 0, 0, 'Pagado',       NULL),
    (3,  3,  5, '2026-05-05',  20000, 0, 0, 'Pagado',       NULL),
    (4,  4,  1, '2026-05-06',  20000, 0, 0, 'Pagado',       NULL),
    (5,  5,  2, '2026-05-13', 250000, 0, 0, 'Pagado',       NULL),
    (6,  6,  6, '2026-05-10',  20000, 0, 0, 'Pagado',       NULL),
    (7,  7,  3, '2026-05-19', 350000, 0, 0, 'Pagado',       NULL),
    (8,  8,  4, '2026-05-21', 350000, 0, 0, 'Pagado',       NULL),
    (9,  9,  1, '2026-05-15',  20000, 0, 0, 'Pagado',       NULL),
    (10, 10, 2, '2026-05-20', 200000, 0, 0, 'Pagado',       NULL),
    (11, 11, 4, '2026-05-23', 250000, 0, 0, 'Pagado',       NULL),
    (12, 12, 1, '2026-05-20',  20000, 0, 0, 'Pagado',       NULL),
    (13, 13, 3, '2026-05-27', 250000, 0, 0, 'Pagado',       NULL),
    (14, 14, 6, '2026-05-24',  20000, 0, 0, 'Pagado',       NULL),
    (15, 15, 2, '2026-05-30', 250000, 0, 0, 'Pagado',       NULL),
    (1,  16, 3, '2026-06-06', 250000, 0, 0, 'Pendiente',    NULL),
    (3,  17, 1, '2026-06-02',  30000, 0, 0, 'Pendiente',    NULL),
    (5,  18, 4, '2026-06-08', 250000, 0, 0, 'Pendiente',    NULL),
    (6,  19, 6, '2026-06-04',  20000, 0, 0, 'Pagado',       NULL),
    (8,  20, 2, '2026-06-12', 350000, 0, 0, 'Pendiente',    NULL),
    (9,  21, 1, '2026-06-06',  45000, 0, 0, 'Pagado',       NULL),
    (11, 22, 3, '2026-06-14', 350000, 0, 0, 'Pendiente',    NULL),
    (13, 23, 1, '2026-06-08',  20000, 0, 0, 'Sin cancelar', 'Pendiente de cobro'),
    (14, 24, 4, '2026-06-17', 350000, 0, 0, 'Pendiente',    NULL),
    (15, 25, 6, '2026-06-12',  20000, 0, 0, 'Sin cancelar', 'Cliente no ha respondido');
GO

-- ============================================================
-- USUARIOS (23 en total: 1 admin + 7 empleados + 15 clientes)
-- Todos los hashes generados con bcrypt ronda 11
-- ============================================================
INSERT INTO Usuarios (correo, password_hash, rol, id_empleado, id_cliente) VALUES
-- Administrador
('admin@vacanjef.com',
 '$2b$11$V7F68cKKcRrnP4/hwBg.R.To5PmuWNjTkBOjj9/5NnWAmZk2kFSZm',
 'Administrador',
 (SELECT id_empleado FROM Empleados WHERE correo='claudia.bernal@vacanjef.com'), NULL),
-- Empleados
('jorge.salinas@vacanjef.com',
 '$2b$11$G3zSJJCV5A6mvS1gxgXZfOlZsBxDMJcjeOXpT7t1c67RGw5opsocu',
 'Empleado',
 (SELECT id_empleado FROM Empleados WHERE correo='jorge.salinas@vacanjef.com'), NULL),
('patricia.luna@vacanjef.com',
 '$2b$11$uun78utI71Syp9ALjqRBzu.20D6brX5JphVoBdZbEpLBv7H8O.MLy',
 'Empleado',
 (SELECT id_empleado FROM Empleados WHERE correo='patricia.luna@vacanjef.com'), NULL),
('diego.ospina@vacanjef.com',
 '$2b$11$A3gCyjUDYUiYXj1/NW2dz.BzjKDVwCpE04nVMfmsbzVBWsUa1XHx6',
 'Empleado',
 (SELECT id_empleado FROM Empleados WHERE correo='diego.ospina@vacanjef.com'), NULL),
('natalia.reyes@vacanjef.com',
 '$2b$11$3Sk9T7RfRW2qeEBTjtaGoOgACxbR2FZVSBkVgTVANq2vrqewwfyyq',
 'Empleado',
 (SELECT id_empleado FROM Empleados WHERE correo='natalia.reyes@vacanjef.com'), NULL),
('andres.forero@vacanjef.com',
 '$2b$11$.k0BAWeO7zEjWilz4ZRWre9C00dOt30pIh99hy9PpNiuDghXBY5CC',
 'Empleado',
 (SELECT id_empleado FROM Empleados WHERE correo='andres.forero@vacanjef.com'), NULL),
('hernan.quiroz@vacanjef.com',
 '$2b$11$RaHcT2eK658zHagRmANJE.547KMuxnRVjecWQew/B7GJi7imsj/AW',
 'Empleado',
 (SELECT id_empleado FROM Empleados WHERE correo='hernan.quiroz@vacanjef.com'), NULL),
('viviana.pinto@vacanjef.com',
 '$2b$11$OenZzED0s0iT7fBh1uT5d.yx7wCpxRieorHClQKfejmQ5wo1uJwjS',
 'Empleado',
 (SELECT id_empleado FROM Empleados WHERE correo='viviana.pinto@vacanjef.com'), NULL),
-- Clientes (15)
('carlos.ramirez@gmail.com',
 '$2b$11$pg.rND.Pjz9FtHggngSxQunkHlDLlVl3CGYiTPviIi8WgskcuHa1S',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='carlos.ramirez@gmail.com')),
('lucia.gomez@hotmail.com',
 '$2b$11$KPrpOb.fnmEzUnbqw9fVNu8DL/ctC0EtttkinBfwhdLokew6zZAPC',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='lucia.gomez@hotmail.com')),
('andres.torres@gmail.com',
 '$2b$11$vSjkKVxJsWB4y/hPEUCx0eOn.8.iSHxtWcvnbw6vIjAHMOld.aCpO',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='andres.torres@gmail.com')),
('vale.herrera@outlook.com',
 '$2b$11$K2WZTln9FElA0SAMqRixueRlj0ahAwCQNbGytW70gUUfkeVz0l9pG',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='vale.herrera@outlook.com')),
('sebas.morales@gmail.com',
 '$2b$11$pATLmsw3rJ65MC.tUaK.3easCm1sJSeayG0nxzs2Pbirq.p/p/S4y',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='sebas.morales@gmail.com')),
('dani.vargas@gmail.com',
 '$2b$11$pMDGMFh6wveS9fAdYZaTK./OnCm6uDpgW3Xq6GhzaJn9rt0Ng3nd6',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='dani.vargas@gmail.com')),
('felipe.castillo@yahoo.com',
 '$2b$11$SVnz3.C6aVtvUVilIpVIyuHmKcLoWUkL8pNrVRn4VqIdj6NhMPcUK',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='felipe.castillo@yahoo.com')),
('mariana.rios@gmail.com',
 '$2b$11$tdOTkmZz1Pxb9FG9WRvvH.fDNB2NUZzdr5C4nNUK1ji.YbjYFD8dC',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='mariana.rios@gmail.com')),
('camilo.pedraza@gmail.com',
 '$2b$11$DzFH75vjJ7wNjtTEQ6ek..3Ca23a4HLepeN2huUjsSGxGxiHhzVAi',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='camilo.pedraza@gmail.com')),
('sara.mendoza@hotmail.com',
 '$2b$11$nszKQYvkB320T7nXGqHzBe1HkfTkQIos/TZda7qPEQdvdD7Rs06hS',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='sara.mendoza@hotmail.com')),
('miguel.sanchez@gmail.com',
 '$2b$11$jQGUhOR38NwI81I4sLcCFeH41BCOD32GGz8smwmxfcM3mtLlI1Mj6',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='miguel.sanchez@gmail.com')),
('natalia.castro@outlook.com',
 '$2b$11$abBvlAmze31Z3nJkvb.op.BGHJuobAyMJ1N4G26YGFw.u13icZxAq',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='natalia.castro@outlook.com')),
('julian.parra@gmail.com',
 '$2b$11$COAkS4FbgR00yfrDgYuTi.NLFhBIccg2tphwNIo9AHqDeIFoyOQoq',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='julian.parra@gmail.com')),
('isabella.ruiz@hotmail.com',
 '$2b$11$MECY3iqmK2OM6l7jGTUAgOABayvIZq1MwLqOlJ7yE7bc9Vkp9CyxC',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='isabella.ruiz@hotmail.com')),
('esteban.molina@gmail.com',
 '$2b$11$PMyTkoDWN/2z71tWUznZBe.jzyMmWK2IAq6g.hu/I8bmXk.ozr0dy',
 'Cliente', NULL,
 (SELECT id_cliente FROM Clientes WHERE correo='esteban.molina@gmail.com'));
GO

-- ============================================================
-- VERIFICACIÓN FINAL
-- ============================================================
SELECT tabla, registros FROM (
    SELECT 'Cargos'          AS tabla, COUNT(*) AS registros FROM Cargos
    UNION ALL SELECT 'Tipos_Servicio',  COUNT(*) FROM Tipos_Servicio
    UNION ALL SELECT 'Metodos_Pago',    COUNT(*) FROM Metodos_Pago
    UNION ALL SELECT 'Clientes',        COUNT(*) FROM Clientes
    UNION ALL SELECT 'Contactos_Emer',  COUNT(*) FROM Contactos_Emergencia
    UNION ALL SELECT 'Mascotas',        COUNT(*) FROM Mascotas
    UNION ALL SELECT 'Empleados',       COUNT(*) FROM Empleados
    UNION ALL SELECT 'Reservas',        COUNT(*) FROM Reservas
    UNION ALL SELECT 'Facturas',        COUNT(*) FROM Facturas
    UNION ALL SELECT 'Usuarios',        COUNT(*) FROM Usuarios
) t ORDER BY tabla;

-- Resumen de usuarios creados
SELECT u.correo, u.rol,
    ISNULL(c.nombre + ' ' + c.apellido,
    ISNULL(e.nombre + ' ' + e.apellido, '—')) AS nombre_real
FROM Usuarios u
LEFT JOIN Clientes  c ON c.id_cliente  = u.id_cliente
LEFT JOIN Empleados e ON e.id_empleado = u.id_empleado
ORDER BY
    CASE u.rol WHEN 'Administrador' THEN 1 WHEN 'Empleado' THEN 2 ELSE 3 END,
    u.correo;

PRINT '============================================================';
PRINT 'VacanJef v2.0 creada exitosamente.';
PRINT '  10 tablas · 5 cargos · 6 servicios · 7 metodos de pago';
PRINT '  15 clientes · 8 empleados · 20 mascotas';
PRINT '  25 reservas · 25 facturas · 23 usuarios';
PRINT '============================================================';
GO


USE VacanJef;
GO

ALTER TABLE Empleados ADD turno_inicio    TIME NULL;
ALTER TABLE Empleados ADD turno_fin       TIME NULL;
ALTER TABLE Empleados ADD dias_laborales  NVARCHAR(40) NULL;
GO

-- Turnos de ejemplo: mañana (7am-3pm), tarde (1pm-9pm), fines de semana, etc.
UPDATE Empleados SET turno_inicio='07:00', turno_fin='15:00', dias_laborales='Lun,Mar,Mie,Jue,Vie' WHERE correo='claudia.bernal@vacanjef.com';
UPDATE Empleados SET turno_inicio='07:00', turno_fin='15:00', dias_laborales='Lun,Mar,Mie,Jue,Vie' WHERE correo='jorge.salinas@vacanjef.com';
UPDATE Empleados SET turno_inicio='09:00', turno_fin='17:00', dias_laborales='Lun,Mar,Mie,Jue,Vie' WHERE correo='patricia.luna@vacanjef.com';
UPDATE Empleados SET turno_inicio='13:00', turno_fin='21:00', dias_laborales='Lun,Mar,Mie,Jue,Vie' WHERE correo='diego.ospina@vacanjef.com';
UPDATE Empleados SET turno_inicio='07:00', turno_fin='15:00', dias_laborales='Mar,Mie,Jue,Vie,Sab' WHERE correo='natalia.reyes@vacanjef.com';
UPDATE Empleados SET turno_inicio='13:00', turno_fin='21:00', dias_laborales='Lun,Mar,Mie,Jue,Vie' WHERE correo='andres.forero@vacanjef.com';
UPDATE Empleados SET turno_inicio='09:00', turno_fin='17:00', dias_laborales='Mie,Jue,Vie,Sab,Dom' WHERE correo='hernan.quiroz@vacanjef.com';
UPDATE Empleados SET turno_inicio='07:00', turno_fin='15:00', dias_laborales='Sab,Dom,Lun,Mar,Mie' WHERE correo='viviana.pinto@vacanjef.com';
GO

PRINT 'Turnos laborales asignados a los 8 empleados.';
GO




select * from Cargos
select * from Clientes
select * from Contactos_Emergencia
select * from Empleados 
select * from Facturas
select * from Mascotas
select * from Metodos_Pago
select * from Reservas
select * from Tipos_Servicio
select * from Usuarios