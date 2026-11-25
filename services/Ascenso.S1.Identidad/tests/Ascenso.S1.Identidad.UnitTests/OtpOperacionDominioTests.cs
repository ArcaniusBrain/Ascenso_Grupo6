using Ascenso.S1.Identidad.Domain.Agregados;
using Ascenso.SharedKernel.Seguridad;
using Xunit;

namespace Ascenso.S1.Identidad.UnitTests;

public sealed class OtpOperacionDominioTests
{
    [Fact]
    public void DesafioOtpOperacion_Valida_Vinculo_Estricto_Consumo_Unico_Idempotencia_Y_R42()
    {
        var ahora = new DateTimeOffset(2026, 11, 25, 15, 0, 0, TimeSpan.Zero);
        var usuarioId = Guid.NewGuid();
        var recursoId = Guid.NewGuid().ToString("D");
        var opOriginal = Guid.NewGuid();

        var desafio = DesafioOtpOperacion.Crear(
            usuarioId,
            TipoOperacionSensible.CONFIRMAR_PAGO,
            recursoId,
            CanalCodigo.CORREO,
            "482913",
            ahora);

        Assert.DoesNotContain("482913", desafio.CodigoHash);
        Assert.Equal(ahora.AddMinutes(15), desafio.ExpiraEnUtc);

        // Vínculo distinto (otro usuario, otra operación o otro recurso) -> rechazado sin consumir
        Assert.Equal(
            ResultadoConsumoOtp.VINCULO_NO_COINCIDE,
            desafio.ValidarYConsumir(Guid.NewGuid(), TipoOperacionSensible.CONFIRMAR_PAGO, recursoId, "482913", opOriginal, ahora.AddMinutes(1)));
        Assert.Equal(
            ResultadoConsumoOtp.VINCULO_NO_COINCIDE,
            desafio.ValidarYConsumir(usuarioId, TipoOperacionSensible.REVOCAR_BECA, recursoId, "482913", opOriginal, ahora.AddMinutes(1)));
        Assert.Equal(
            ResultadoConsumoOtp.VINCULO_NO_COINCIDE,
            desafio.ValidarYConsumir(usuarioId, TipoOperacionSensible.CONFIRMAR_PAGO, Guid.NewGuid().ToString("D"), "482913", opOriginal, ahora.AddMinutes(1)));

        // Consumo válido con el vínculo exacto
        Assert.Equal(
            ResultadoConsumoOtp.CONSUMIDO,
            desafio.ValidarYConsumir(usuarioId, TipoOperacionSensible.CONFIRMAR_PAGO, recursoId, "482913", opOriginal, ahora.AddMinutes(2)));
        Assert.Equal(1, desafio.Version);

        // Reintento por corte de red con el MISMO operationId -> idempotente
        Assert.Equal(
            ResultadoConsumoOtp.CONSUMIDO_IDEMPOTENTE,
            desafio.ValidarYConsumir(usuarioId, TipoOperacionSensible.CONFIRMAR_PAGO, recursoId, "482913", opOriginal, ahora.AddMinutes(2)));

        // R42 / HU-20.1 E4: nuevo intento con DISTINTO operationId -> rechazado
        Assert.Equal(
            ResultadoConsumoOtp.RECHAZADO_REUSO_O_INVALIDADO,
            desafio.ValidarYConsumir(usuarioId, TipoOperacionSensible.CONFIRMAR_PAGO, recursoId, "482913", Guid.NewGuid(), ahora.AddMinutes(3)));
    }

    [Fact]
    public void DesafioOtpOperacion_Expirado_A_Los_15_Minutos_Y_Bloqueo_Tras_3_Intentos()
    {
        var ahora = new DateTimeOffset(2026, 11, 25, 15, 0, 0, TimeSpan.Zero);
        var usuarioId = Guid.NewGuid();
        var recursoId = Guid.NewGuid().ToString("D");

        // HU-20.1 Escenario 12: 16 minutos después -> EXPIRADO
        var desafioExp = DesafioOtpOperacion.Crear(usuarioId, TipoOperacionSensible.CONFIRMAR_PAGO, recursoId, CanalCodigo.SMS, "111222", ahora);
        Assert.Equal(
            ResultadoConsumoOtp.EXPIRADO,
            desafioExp.ValidarYConsumir(usuarioId, TipoOperacionSensible.CONFIRMAR_PAGO, recursoId, "111222", Guid.NewGuid(), ahora.AddMinutes(16)));

        // HU-20.1 Escenario 13: 3 intentos fallidos -> BLOQUEO_POR_INTENTOS + BloqueoOtpOperacion 1 hora
        var desafioFallos = DesafioOtpOperacion.Crear(usuarioId, TipoOperacionSensible.MODIFICAR_DATOS_PAGO, recursoId, CanalCodigo.CORREO, "333444", ahora);
        Assert.Equal(ResultadoConsumoOtp.CODIGO_INCORRECTO, desafioFallos.ValidarYConsumir(usuarioId, TipoOperacionSensible.MODIFICAR_DATOS_PAGO, recursoId, "000001", Guid.NewGuid(), ahora.AddMinutes(1)));
        Assert.Equal(ResultadoConsumoOtp.CODIGO_INCORRECTO, desafioFallos.ValidarYConsumir(usuarioId, TipoOperacionSensible.MODIFICAR_DATOS_PAGO, recursoId, "000002", Guid.NewGuid(), ahora.AddMinutes(2)));
        Assert.Equal(ResultadoConsumoOtp.BLOQUEO_POR_INTENTOS, desafioFallos.ValidarYConsumir(usuarioId, TipoOperacionSensible.MODIFICAR_DATOS_PAGO, recursoId, "000003", Guid.NewGuid(), ahora.AddMinutes(3)));

        var bloqueo = BloqueoOtpOperacion.Crear(usuarioId, TipoOperacionSensible.MODIFICAR_DATOS_PAGO, recursoId, ahora.AddMinutes(3).AddHours(1));
        Assert.True(bloqueo.EstaVigente(ahora.AddMinutes(62)));
        Assert.False(bloqueo.EstaVigente(ahora.AddMinutes(63).AddSeconds(1)));
    }
}
