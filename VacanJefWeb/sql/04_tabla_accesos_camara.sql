-- ============================================================
-- VACANJEF · Script 04 — Tabla AccesosCamara
-- Ejecutar en SSMS sobre la BD VacanJef
-- ============================================================
USE VacanJef;
GO

CREATE TABLE AccesosCamara (
    id_acceso        INT IDENTITY(1,1) PRIMARY KEY,
    id_reserva       INT          NOT NULL UNIQUE,
    token            NVARCHAR(64) NOT NULL UNIQUE,
    fecha_creacion   DATETIME2    NOT NULL DEFAULT GETDATE(),
    fecha_expiracion DATETIME2    NOT NULL,
    activo           BIT          NOT NULL DEFAULT 1,
    CONSTRAINT FK_Acceso_Reserva
        FOREIGN KEY (id_reserva) REFERENCES Reservas(id_reserva) ON DELETE CASCADE
);
GO

CREATE INDEX IX_AccesosCamara_Token ON AccesosCamara(token);
GO

PRINT 'Tabla AccesosCamara creada correctamente.';
GO
