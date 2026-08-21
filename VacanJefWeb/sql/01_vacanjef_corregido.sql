

CREATE DATABASE VacanJef;
GO

USE VacanJef;
GO

-- ============================================================
-- TABLA: Clientes
-- ============================================================
CREATE TABLE Clientes (
    id_cliente      INT IDENTITY(1,1) PRIMARY KEY,
    nombre          NVARCHAR(100) NOT NULL,
    apellido        NVARCHAR(100) NOT NULL,
    telefono        NVARCHAR(20),
    direccion       NVARCHAR(255),
    correo          NVARCHAR(150) UNIQUE,
    fecha_registro  DATETIME2 DEFAULT GETDATE(),
    activo          BIT DEFAULT 1
);
GO

-- ============================================================
-- TABLA: Contactos de Emergencia
-- ============================================================
CREATE TABLE Contactos_Emergencia (
    id_contacto     INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente      INT NOT NULL,
    nombre          NVARCHAR(100) NOT NULL,
    apellido        NVARCHAR(100) NOT NULL,
    telefono        NVARCHAR(20),
    correo          NVARCHAR(150),
    parentesco      NVARCHAR(50),
    CONSTRAINT FK_Emergencia_Cliente FOREIGN KEY (id_cliente)
        REFERENCES Clientes(id_cliente) ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLA: Mascotas
-- ============================================================
CREATE TABLE Mascotas (
    id_mascota       INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente       INT NOT NULL,
    nombre           NVARCHAR(100) NOT NULL,
    especie          NVARCHAR(50) NOT NULL,
    raza             NVARCHAR(100),
    sexo             CHAR(1) CHECK (sexo IN ('M','H')),
    fecha_nacimiento DATE,
    color            NVARCHAR(50),
    peso_kg          DECIMAL(5,2),
    alergias         NVARCHAR(500),
    medicamentos     NVARCHAR(500),
    observaciones    NVARCHAR(1000),
    carnet_vacunas   NVARCHAR(255),
    activo           BIT DEFAULT 1,
    CONSTRAINT FK_Mascota_Cliente FOREIGN KEY (id_cliente)
        REFERENCES Clientes(id_cliente) ON DELETE CASCADE
);
GO

-- ============================================================
-- TABLA: Cargos
-- ============================================================
CREATE TABLE Cargos (
    id_cargo     INT IDENTITY(1,1) PRIMARY KEY,
    nombre_cargo NVARCHAR(100) NOT NULL UNIQUE
);
GO

-- ============================================================
-- TABLA: Empleados
-- ============================================================
CREATE TABLE Empleados (
    id_empleado           INT IDENTITY(1,1) PRIMARY KEY,
    nombre                NVARCHAR(100) NOT NULL,
    apellido              NVARCHAR(100) NOT NULL,
    telefono              NVARCHAR(20),
    direccion             NVARCHAR(255),
    correo                NVARCHAR(150) UNIQUE,
    id_cargo              INT,
    salario               DECIMAL(12,2) CHECK (salario >= 0),
    fecha_ingreso         DATE DEFAULT CAST(GETDATE() AS DATE),
    activo                BIT DEFAULT 1,
    emergencia_nombre     NVARCHAR(200),
    emergencia_telefono   NVARCHAR(20),
    emergencia_parentesco NVARCHAR(50),
    CONSTRAINT FK_Empleado_Cargo FOREIGN KEY (id_cargo)
        REFERENCES Cargos(id_cargo) ON DELETE SET NULL
);
GO

-- ============================================================
-- TABLA: Tipos de Servicio
-- ============================================================
CREATE TABLE Tipos_Servicio (
    id_tipo_servicio INT IDENTITY(1,1) PRIMARY KEY,
    nombre_servicio  NVARCHAR(100) NOT NULL UNIQUE,
    descripcion      NVARCHAR(500),
    precio_base      DECIMAL(12,2) CHECK (precio_base >= 0)
);
GO

-- ============================================================
-- TABLA: Metodos de Pago
-- ============================================================
CREATE TABLE Metodos_Pago (
    id_metodo_pago INT IDENTITY(1,1) PRIMARY KEY,
    nombre_metodo  NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- ============================================================
-- TABLA: Reservas
-- ============================================================
CREATE TABLE Reservas (
    id_reserva       INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente       INT NOT NULL,
    id_mascota       INT NOT NULL,
    id_empleado      INT,
    id_tipo_servicio INT,
    fecha_inicio     DATETIME2 NOT NULL,
    fecha_fin        DATETIME2,
    estado_reserva   NVARCHAR(20) NOT NULL DEFAULT 'Pendiente'
                         CHECK (estado_reserva IN ('Pendiente','Confirmada','En curso','Finalizada','Cancelada')),
    observaciones    NVARCHAR(500),
    fecha_creacion   DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Reserva_Cliente  FOREIGN KEY (id_cliente)       REFERENCES Clientes(id_cliente),
    CONSTRAINT FK_Reserva_Mascota  FOREIGN KEY (id_mascota)       REFERENCES Mascotas(id_mascota),
    CONSTRAINT FK_Reserva_Empleado FOREIGN KEY (id_empleado)      REFERENCES Empleados(id_empleado) ON DELETE SET NULL,
    CONSTRAINT FK_Reserva_Servicio FOREIGN KEY (id_tipo_servicio) REFERENCES Tipos_Servicio(id_tipo_servicio)
);
GO

-- ============================================================
-- TABLA: Facturas
-- ============================================================
CREATE TABLE Facturas (
    id_factura     INT IDENTITY(1,1) PRIMARY KEY,
    id_cliente     INT NOT NULL,
    id_reserva     INT NOT NULL,
    id_metodo_pago INT,
    fecha_emision  DATETIME2 DEFAULT GETDATE(),
    subtotal       DECIMAL(12,2) NOT NULL DEFAULT 0 CHECK (subtotal >= 0),
    descuento      DECIMAL(12,2) DEFAULT 0 CHECK (descuento >= 0),
    impuesto_pct   DECIMAL(5,2)  DEFAULT 0 CHECK (impuesto_pct >= 0),
    total          AS (subtotal - descuento + (subtotal - descuento) * impuesto_pct / 100) PERSISTED,
    estado_pago    NVARCHAR(20) NOT NULL DEFAULT 'Pendiente'
                       CHECK (estado_pago IN ('Pagado','Pendiente','Sin cancelar','Anulado')),
    notas          NVARCHAR(500),
    CONSTRAINT FK_Factura_Cliente    FOREIGN KEY (id_cliente)     REFERENCES Clientes(id_cliente),
    CONSTRAINT FK_Factura_Reserva    FOREIGN KEY (id_reserva)     REFERENCES Reservas(id_reserva),
    CONSTRAINT FK_Factura_MetodoPago FOREIGN KEY (id_metodo_pago) REFERENCES Metodos_Pago(id_metodo_pago)
);
GO

-- ============================================================
-- ÍNDICES
-- ============================================================
CREATE INDEX IX_Mascotas_Cliente    ON Mascotas(id_cliente);
CREATE INDEX IX_Reservas_Cliente    ON Reservas(id_cliente);
CREATE INDEX IX_Reservas_Fecha      ON Reservas(fecha_inicio);
CREATE INDEX IX_Facturas_Cliente    ON Facturas(id_cliente);
CREATE INDEX IX_Facturas_EstadoPago ON Facturas(estado_pago);
GO

-- ============================================================
-- CATÁLOGOS BASE
-- ============================================================
INSERT INTO Cargos (nombre_cargo) VALUES
   ('Cuidador'), ('Recepcionista'), ('Administrador'), ('Auxiliar');

INSERT INTO Tipos_Servicio (nombre_servicio, descripcion, precio_base) VALUES
    ('Estadia','Tarifa diaria de estancia en las instalaciones', 50000),
    ('Paseo','Paseo corto con cuidador asignado (tarifa por visita)', 20000);
-- NOTA: el script original solo traía el servicio "Estadia" (id=1) con un
-- precio plano de 200000, pero las Facturas de ejemplo muestran que el cobro
-- real de una estadía escala a razón de 50000 por día (4 días=200000,
-- 5 días=250000, 7 días=350000). Además, las Reservas ya referencian
-- id_tipo_servicio=2 (Paseo) que no existía, lo que rompía la llave foránea
-- FK_Reserva_Servicio al insertar. precio_base ahora representa la tarifa
-- diaria de Estadia y el valor por visita de Paseo; ReservasController calcula
-- el costo multiplicando por la duración real de cada reserva.


INSERT INTO Metodos_Pago (nombre_metodo) VALUES
    ('Efectivo'),
    ('Tarjeta débito'),
    ('Tarjeta crédito'),
    ('Transferencia bancaria'),
    ('Nequi'),
    ('Daviplata'),
    ('PSE'),
    ('Cheque');
GO

-- ============================================================
-- CLIENTES (15 registros)
-- ============================================================
INSERT INTO Clientes (nombre, apellido, telefono, direccion, correo)
VALUES
    ('Carlos',    'Ramírez',  '3101234567', 'Calle 45 #12-30, Medellín',       'carlos.ramirez@gmail.com'),
    ('Lucía',     'Gómez',    '3152345678', 'Carrera 7 #80-15, Medellín',      'lucia.gomez@hotmail.com'),
    ('Andrés',    'Torres',   '3203456789', 'Av. El Poblado #23-10, Medellín', 'andres.torres@gmail.com'),
    ('Valentina', 'Herrera',  '3114567890', 'Calle 10 #43-40, Medellín',       'vale.herrera@outlook.com'),
    ('Sebastián', 'Morales',  '3165678901', 'Carrera 65 #33-20, Medellín',     'sebas.morales@gmail.com'),
    ('Daniela',   'Vargas',   '3006789012', 'Calle 30 #72-18, Medellín',       'dani.vargas@gmail.com'),
    ('Felipe',    'Castillo', '3187890123', 'Carrera 43A #15-60, Medellín',    'felipe.castillo@yahoo.com'),
    ('Mariana',   'Ríos',     '3138901234', 'Av. Las Vegas #22-35, Medellín',  'mariana.rios@gmail.com'),
    ('Camilo',    'Pedraza',  '3019012345', 'Calle 50 #80-12, Medellín',       'camilo.pedraza@gmail.com'),
    ('Sara',      'Mendoza',  '3170123456', 'Carrera 70 #10-90, Medellín',     'sara.mendoza@hotmail.com'),
    ('Miguel',    'Sánchez',  '3122345671', 'Calle 80 #34-20, Medellín',       'miguel.sanchez@gmail.com'),
    ('Natalia',   'Castro',   '3143456782', 'Carrera 32 #55-10, Medellín',     'natalia.castro@outlook.com'),
    ('Julián',    'Parra',    '3054567893', 'Av. Nutibara #12-40, Medellín',   'julian.parra@gmail.com'),
    ('Isabella',  'Ruiz',     '3185678904', 'Calle 33 #65-30, Medellín',       'isabella.ruiz@hotmail.com'),
    ('Esteban',   'Molina',   '3106789015', 'Carrera 48 #20-55, Medellín',     'esteban.molina@gmail.com');
GO

-- ============================================================
-- CONTACTOS DE EMERGENCIA (un contacto por cliente)
-- ============================================================
INSERT INTO Contactos_Emergencia (id_cliente, nombre, apellido, telefono, correo, parentesco) VALUES
    (1,  'María',    'Ramírez',  '3109876543', 'maria.ramirez@gmail.com',    'Familiar'),
    (2,  'Pedro',    'Gómez',    '3153456789', 'pedro.gomez@hotmail.com',    'Familiar'),
    (3,  'Sofía',    'Torres',   '3204567890', 'sofia.torres@gmail.com',     'Familiar'),
    (4,  'Juan',     'Herrera',  '3115678901', 'juan.herrera@outlook.com',   'Familiar'),
    (5,  'Laura',    'Morales',  '3166789012', 'laura.morales@gmail.com',    'Familiar'),
    (6,  'Camilo',   'Vargas',   '3007890123', 'camilo.vargas@gmail.com',    'Familiar'),
    (7,  'Ana',      'Castillo', '3188901234', 'ana.castillo@yahoo.com',     'Familiar'),
    (8,  'Luis',     'Ríos',     '3139012345', 'luis.rios@gmail.com',        'Familiar'),
    (9,  'Diana',    'Pedraza',  '3010123456', 'diana.pedraza@gmail.com',    'Familiar'),
    (10, 'Jorge',    'Mendoza',  '3171234567', 'jorge.mendoza@hotmail.com',  'Familiar'),
    (11, 'Rosa',     'Sánchez',  '3123456781', 'rosa.sanchez@gmail.com',     'Familiar'),
    (12, 'Ernesto',  'Castro',   '3144567892', 'ernesto.castro@outlook.com', 'Familiar'),
    (13, 'Carmen',   'Parra',    '3055678903', 'carmen.parra@gmail.com',     'Familiar'),
    (14, 'Tomás',    'Ruiz',     '3186789014', 'tomas.ruiz@hotmail.com',     'Familiar'),
    (15, 'Patricia', 'Molina',   '3107890125', 'patricia.molina@gmail.com',  'Familiar');
GO

-- ============================================================
-- MASCOTAS (20 registros)
-- fecha_nacimiento reemplaza "edad"
-- ============================================================
INSERT INTO Mascotas (id_cliente, nombre, especie, raza, sexo, fecha_nacimiento, color, peso_kg, alergias, medicamentos, observaciones, carnet_vacunas)
VALUES
    (1,  'Rocky',  'Perro',  'Labrador Retriever', 'M', '2023-03-10', 'Amarillo',      28.5, 'Ninguna',  'Ninguno',         'Le gusta jugar con agua',          'Al día'),
    (1,  'Luna',   'Gato',   'Siamés',             'H', '2024-01-15', 'Blanco/Gris',    4.2, 'Polen',    'Antihistamínico', 'Muy cariñosa',                     'Al día'),
    (2,  'Max',    'Perro',  'Bulldog Francés',    'M', '2021-06-20', 'Atigrado',      12.0, 'Ninguna',  'Ninguno',         'Ronca al dormir',                  'Al día'),
    (3,  'Bella',  'Perro',  'Golden Retriever',   'H', '2025-02-05', 'Dorado',        22.0, 'Ninguna',  'Ninguno',         'Cachorra muy activa',               'Al día'),
    (3,  'Simba',  'Gato',   'Persa',              'M', '2022-08-12', 'Naranja',        5.8, 'Ninguna',  'Ninguno',         'Pelo largo, requiere cepillado',   'Al día'),
    (4,  'Nala',   'Perro',  'Poodle',             'H', '2020-04-18', 'Blanco',         7.5, 'Ninguna',  'Ninguno',         'Muy obediente',                    'Al día'),
    (5,  'Thor',   'Perro',  'Rottweiler',         'M', '2024-05-01', 'Negro/Café',    38.0, 'Ninguna',  'Ninguno',         'Necesita correa resistente',       'Al día'),
    (5,  'Mia',    'Gato',   'Maine Coon',         'H', '2023-07-22', 'Gris',           6.5, 'Mariscos', 'Ninguno',         'No darle alimentos con mariscos',  'Al día'),
    (6,  'Toby',   'Perro',  'Beagle',             'M', '2022-11-30', 'Tricolor',      13.0, 'Ninguna',  'Ninguno',         'Muy curioso y olfatea todo',       'Al día'),
    (7,  'Coco',   'Ave',    'Loro Amazónico',     'M', '2019-03-14', 'Verde/Rojo',     0.5, 'Ninguna',  'Ninguno',         'Habla varias palabras',            'Al día'),
    (7,  'Daisy',  'Perro',  'Chihuahua',          'H', '2024-09-08', 'Café',           2.8, 'Ninguna',  'Ninguno',         'Se asusta con ruidos fuertes',     'Al día'),
    (8,  'Bruno',  'Perro',  'Pastor Alemán',      'M', '2021-01-25', 'Negro/Café',    32.0, 'Pollo',    'Dieta especial',  'Alérgico al pollo, dieta de res',  'Al día'),
    (8,  'Kitty',  'Gato',   'Ragdoll',            'H', '2025-03-10', 'Blanco',         4.0, 'Ninguna',  'Ninguno',         'Muy tranquila',                    'Al día'),
    (9,  'Duke',   'Perro',  'Doberman',           'M', '2023-06-17', 'Negro/Café',    30.0, 'Ninguna',  'Ninguno',         'Entrenado, obedece comandos',      'Al día'),
    (10, 'Pelusa', 'Conejo', 'Holland Lop',        'H', '2024-02-20', 'Blanco',         2.0, 'Ninguna',  'Ninguno',         'Jaula requerida por las noches',   'Al día'),
    (11, 'Loki',   'Perro',  'Husky Siberiano',    'M', '2022-04-05', 'Blanco/Gris',  25.0, 'Ninguna',  'Ninguno',         'Necesita mucho ejercicio',         'Al día'),
    (12, 'Canela', 'Perro',  'Cocker Spaniel',     'H', '2023-09-13', 'Canela',        12.5, 'Ninguna',  'Ninguno',         'Orejas largas, revisar higiene',   'Al día'),
    (13, 'Pisco',  'Ave',    'Cacatúa',            'M', '2021-12-01', 'Blanco/Amarillo',0.4, 'Ninguna',  'Ninguno',         'Le gusta la música',               'Al día'),
    (14, 'Sasha',  'Perro',  'Schnauzer',          'H', '2024-07-19', 'Gris',           8.0, 'Ninguna',  'Ninguno',         'Muy inteligente',                  'Al día'),
    (15, 'Gordo',  'Gato',   'British Shorthair',  'M', '2020-10-28', 'Azul/Gris',      7.2, 'Ninguna',  'Ninguno',         'Sedentario, controlar peso',       'Al día');
GO

-- ============================================================
-- EMPLEADOS (8 registros)
-- id_cargo referencia tabla Cargos:
-- 1=Veterinario, 2=Cuidador, 3=Recepcionista, 4=Administrador, 5=Auxiliar
-- ============================================================
INSERT INTO Empleados (nombre, apellido, telefono, direccion, correo, id_cargo, salario, emergencia_nombre, emergencia_telefono, emergencia_parentesco)
VALUES
    ('Jorge',    'Salinas',  '3121111111', 'Calle 50 #20-10','jorge.salinas@vacanjef.com',   1, 3500000, 'Rosa Salinas',   '3122222222', 'Familiar'),
    ('Patricia', 'Luna',     '3133333333', 'Carrera 9 #35-20','patricia.luna@vacanjef.com',   3, 1800000, 'Marco Luna',     '3134444444', 'Familiar'),
    ('Diego',    'Ospina',   '3145555555', 'Calle 80 #55-30',   'diego.ospina@vacanjef.com',    2, 2000000, 'Clara Ospina',   '3146666666', 'Familiar'),
    ('Natalia',  'Reyes',    '3157777777', 'Av. 30 #12-45',     'natalia.reyes@vacanjef.com',   2, 2000000, 'Tomás Reyes',    '3158888888', 'Familiar'),
    ('Andrés',   'Forero',   '3169999999', 'Carrera 22 #67-10', 'andres.forero@vacanjef.com',   5, 1700000, 'Helena Forero',  '3160000001', 'Familiar'),
    ('Claudia',  'Bernal',   '3170000002', 'Calle 13 #80-60,',   'claudia.bernal@vacanjef.com',  4, 3000000, 'Raúl Bernal',    '3170000003', 'Familiar'),
    ('Hernán',   'Quiroz',   '3181111112', 'Carrera 70 #40-15',  'hernan.quiroz@vacanjef.com',   2, 2000000, 'Marta Quiroz',   '3182222223', 'Familiar'),
    ('Viviana',  'Pinto',    '3193333334', 'Calle 25 #90-20',   'viviana.pinto@vacanjef.com',   5, 1700000, 'Carlos Pinto',   '3194444445', 'Familiar');
GO

-- ============================================================
-- RESERVAS (25 registros)
-- id_tipo_servicio: 1=Hospedaje, 2=Paseo, 3=Consulta básica
-- ============================================================
INSERT INTO Reservas (id_cliente, id_mascota, id_empleado, id_tipo_servicio, fecha_inicio, fecha_fin, estado_reserva, observaciones)
VALUES
    (1,  1,  3, 1, '2026-05-01 08:00', '2026-05-05 17:00', 'Finalizada', 'Traer su juguete favorito'),
    (2,  3,  4, 1, '2026-05-03 08:00', '2026-05-07 17:00', 'Finalizada', 'Ronca, es normal'),
    (3,  4,  3, 2, '2026-05-05 07:00', '2026-05-05 08:00', 'Finalizada', 'Paseo por parque Arví'),
    (4,  6,  5, 2, '2026-05-06 07:00', '2026-05-06 08:00', 'Finalizada', 'Ruta corta poodle mayor'),
    (5,  7,  3, 1, '2026-05-08 08:00', '2026-05-13 17:00', 'Finalizada', 'Necesita espacio amplio'),
    (6,  9,  7, 2, '2026-05-10 07:00', '2026-05-10 08:00', 'Finalizada', 'Ruta parque El Poblado'),
    (7,  10, 4, 1, '2026-05-12 08:00', '2026-05-19 17:00', 'Finalizada', 'Loro, mantener área tranquila'),
    (8,  12, 3, 1, '2026-05-14 08:00', '2026-05-21 17:00', 'Finalizada', 'Solo dieta de res, sin pollo'),
    (9,  14, 5, 2, '2026-05-15 07:00', '2026-05-15 08:00', 'Finalizada', 'Doberman entrenado, ruta larga'),
    (10, 15, 8, 1, '2026-05-16 08:00', '2026-05-20 17:00', 'Finalizada', 'Conejo, jaula incluida'),
    (11, 16, 3, 1, '2026-05-18 08:00', '2026-05-23 17:00', 'Finalizada', 'Husky necesita mucho ejercicio'),
    (12, 17, 7, 2, '2026-05-20 07:00', '2026-05-20 08:00', 'Finalizada', 'Revisar orejas al volver'),
    (13, 18, 4, 1, '2026-05-22 08:00', '2026-05-27 17:00', 'Finalizada', 'Cacatúa, ambiente silencioso'),
    (14, 19, 5, 2, '2026-05-24 07:00', '2026-05-24 08:00', 'Finalizada', 'Schnauzer, ruta media'),
    (15, 20, 3, 1, '2026-05-25 08:00', '2026-05-30 17:00', 'Finalizada', 'Gato sedentario, sin mucho movimiento'),
    (1,  2,  4, 1, '2026-06-01 08:00', '2026-06-06 17:00', 'Confirmada', 'Gato siamés, alérgico al polen'),
    (3,  5,  3, 2, '2026-06-02 07:00', '2026-06-02 08:00', 'Confirmada', 'Gato persa, paseo en cargador'),
    (5,  8,  7, 1, '2026-06-03 08:00', '2026-06-08 17:00', 'Confirmada', 'Maine Coon sin mariscos'),
    (6,  9,  5, 2, '2026-06-04 07:00', '2026-06-04 08:00', 'Confirmada', 'Beagle curioso, atención al olfato'),
    (8,  13, 8, 1, '2026-06-05 08:00', '2026-06-12 17:00', 'Confirmada', 'Ragdoll tranquila, sin ruido'),
    (9,  14, 5, 2, '2026-06-06 07:00', '2026-06-06 08:00', 'Confirmada', 'Duke, ruta larga por ciclovía'),
    (11, 16, 3, 1, '2026-06-07 08:00', '2026-06-14 17:00', 'Pendiente',  'Husky, área ventilada'),
    (13, 18, 4, 2, '2026-06-08 07:00', '2026-06-08 08:00', 'Pendiente',  'Cacatúa en cargador especial'),
    (14, 19, 7, 1, '2026-06-10 08:00', '2026-06-17 17:00', 'Pendiente',  'Schnauzer, cepillado diario'),
    (15, 20, 8, 2, '2026-06-12 07:00', '2026-06-12 08:00', 'Pendiente',  'Gato British, primer paseo');
GO

-- ============================================================
-- FACTURAS (25 registros)
-- id_metodo_pago: 1=Efectivo, 2=Tarjeta débito, 3=Tarjeta crédito
--                 4=Transferencia, 5=Nequi, 6=Daviplata, 7=PSE
-- subtotal = valor del servicio (total se calcula solo)
-- ============================================================
INSERT INTO Facturas (id_cliente, id_reserva, id_metodo_pago, fecha_emision, subtotal, descuento, impuesto_pct, estado_pago, notas)
VALUES
    (1,  1,  1, '2026-05-05', 200000, 0, 0, 'Pagado',      NULL),
    (2,  2,  4, '2026-05-07', 200000, 0, 0, 'Pagado',      NULL),
    (3,  3,  5, '2026-05-05',  20000, 0, 0, 'Pagado',      NULL),
    (4,  4,  1, '2026-05-06',  20000, 0, 0, 'Pagado',      NULL),
    (5,  5,  2, '2026-05-13', 250000, 0, 0, 'Pagado',      NULL),
    (6,  6,  6, '2026-05-10',  20000, 0, 0, 'Pagado',      NULL),
    (7,  7,  3, '2026-05-19', 350000, 0, 0, 'Pagado',      NULL),
    (8,  8,  4, '2026-05-21', 350000, 0, 0, 'Pagado',      NULL),
    (9,  9,  1, '2026-05-15',  20000, 0, 0, 'Pagado',      NULL),
    (10, 10, 2, '2026-05-20', 200000, 0, 0, 'Pagado',      NULL),
    (11, 11, 4, '2026-05-23', 250000, 0, 0, 'Pagado',      NULL),
    (12, 12, 1, '2026-05-20',  20000, 0, 0, 'Pagado',      NULL),
    (13, 13, 3, '2026-05-27', 250000, 0, 0, 'Pagado',      NULL),
    (14, 14, 6, '2026-05-24',  20000, 0, 0, 'Pagado',      NULL),
    (15, 15, 2, '2026-05-30', 250000, 0, 0, 'Pagado',      NULL),
    (1,  16, 3, '2026-06-06', 250000, 0, 0, 'Pendiente',   NULL),
    (3,  17, 1, '2026-06-02',  20000, 0, 0, 'Pendiente',   NULL),
    (5,  18, 4, '2026-06-08', 250000, 0, 0, 'Pendiente',   NULL),
    (6,  19, 6, '2026-06-04',  20000, 0, 0, 'Pagado',      NULL),
    (8,  20, 2, '2026-06-12', 350000, 0, 0, 'Pendiente',   NULL),
    (9,  21, 1, '2026-06-06',  20000, 0, 0, 'Pagado',      NULL),
    (11, 22, 3, '2026-06-14', 350000, 0, 0, 'Pendiente',   NULL),
    (13, 23, 1, '2026-06-08',  20000, 0, 0, 'Sin cancelar','Pendiente de cobro'),
    (14, 24, 4, '2026-06-17', 350000, 0, 0, 'Pendiente',   NULL),
    (15, 25, 6, '2026-06-12',  20000, 0, 0, 'Sin cancelar','Cliente no ha respondido');
GO

PRINT 'Base de datos VacanJef creada y cargada exitosamente.';
GO


select * from Clientes;
select * from Contactos_Emergencia;
select * from Mascotas;
select * from Empleados;
select * from Reservas;
select * from Facturas;
select * from Tipos_Servicio;
select * from Metodos_Pago;
select * from Cargos;
