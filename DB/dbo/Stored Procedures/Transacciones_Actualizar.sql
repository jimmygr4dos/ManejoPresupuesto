﻿CREATE PROCEDURE Transacciones_Actualizar
@Id int,
@FechaTransaccion datetime,
@Monto decimal(18,2),
@MontoAnterior decimal(18,2),
@CuentaId int,
@CuentaAnteriorId int,
@CategoriaId int,
@Nota nvarchar(1000) = NULL
AS
BEGIN
	--Revertir transacción anterior
	UPDATE Cuentas
	SET Balance -= @MontoAnterior
	WHERE Id = @CuentaAnteriorId;

	--Realizar la nueva transacción
	UPDATE Cuentas
	SET Balance += @Monto
	WHERE Id = @CuentaId;

	UPDATE Transacciones
	SET Monto = ABS(@Monto), FechaTransaccion = @FechaTransaccion,
	CategoriaId = @CategoriaId, CuentaId = @CuentaId, Nota = @Nota
	WHERE Id = @Id;
END
