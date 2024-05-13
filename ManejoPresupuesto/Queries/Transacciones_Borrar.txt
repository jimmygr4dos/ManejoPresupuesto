CREATE PROCEDURE Transacciones_Borrar
@Id int
AS
BEGIN
	DECLARE @Monto decimal(18,2);
	DECLARE @CuentaId int;
	DECLARE @TipoOperacionId int;

	SELECT @Monto = Monto, @CuentaId = CuentaId, @TipoOperacionId = cat.TipoOperacionId
	FROM Transacciones
	INNER JOIN Categorias cat
	ON cat.Id = Transacciones.CategoriaId
	WHERE Transacciones.Id = @Id;

	DECLARE @FactorMultiplicativo int = 1;

	IF(@TipoOperacionId = 2)
		SET @FactorMultiplicativo = -1;

	SET @Monto = @Monto * @FactorMultiplicativo;

	UPDATE Cuentas 
	SET Balance -= @Monto
	WHERE Id = @CuentaId;

	DELETE Transacciones
	WHERE Id = @Id;
END