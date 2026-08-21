-- ============================================================
-- VACANJEF · Script 05 — Turnos laborales de empleados
-- Ejecutar en SSMS sobre la BD VacanJef
-- ============================================================
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
