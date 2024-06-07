CREATE PROCEDURE Transacciones_Insertar
@UsuarioId int,
@FechaTransaccion date,
@Monto decimal(18,2),
@CategoriaId int,
@CuentaId int,
@Nota nvarchar(1000) = NULL
AS
BEGIN
	INSERT INTO Transacciones
	(UsuarioId, FechaTransaccion, Monto, CategoriaId, CuentaId, Nota)
	VALUES
	(@UsuarioId, @FechaTransaccion, ABS(@Monto), @CategoriaId, @CuentaId, @Nota)

	UPDATE Cuentas
	SET Balance += @Monto
	WHERE Id = @CuentaId

	SELECT SCOPE_IDENTITY();
END