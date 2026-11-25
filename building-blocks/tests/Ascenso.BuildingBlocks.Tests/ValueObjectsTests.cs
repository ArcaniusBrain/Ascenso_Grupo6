using Ascenso.SharedKernel.Tiempo;
using Ascenso.SharedKernel.TiposBase;
using Ascenso.SharedKernel.ValueObjects;
using Xunit;

namespace Ascenso.BuildingBlocks.Tests;

public sealed class ValueObjectsTests
{
    private sealed class RelojFijoProvider(DateTimeOffset utcFijo) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcFijo;
    }

    [Fact]
    public void Dinero_SumaRestaYMultiplica_MismaMoneda()
    {
        var cuota = new Dinero(2000m, "usd");
        var total = cuota.Multiplicar(8);
        var extra = new Dinero(500m, "USD");

        Assert.Equal("USD", cuota.Moneda);
        Assert.Equal(16000m, total.Cantidad);
        Assert.Equal(16500m, total.Sumar(extra).Cantidad);
        Assert.Equal(15500m, total.Restar(extra).Cantidad);
        Assert.True(total.EsMayorQue(cuota));
    }

    [Fact]
    public void Dinero_RechazaNegativosMonedaInvalidaYMonedasDistintas()
    {
        Assert.Throws<ExcepcionDeDominio>(() => new Dinero(-1m, "USD"));
        Assert.Throws<ExcepcionDeDominio>(() => new Dinero(10m, "US"));
        Assert.Throws<ExcepcionDeDominio>(() => new Dinero(10m, "USD").Sumar(new Dinero(5m, "EUR")));
        Assert.Throws<ExcepcionDeDominio>(() => new Dinero(10m, "USD").Restar(new Dinero(11m, "USD")));
    }

    [Fact]
    public void Cedula_NormalizaYRechazaInvalidas()
    {
        var cedula = new Cedula(" v-12.345.678 ");
        Assert.Equal("V-12345678", cedula.Numero);

        Assert.Throws<ExcepcionDeDominio>(() => new Cedula(""));
        Assert.Throws<ExcepcionDeDominio>(() => new Cedula("ABC-123"));
    }

    [Fact]
    public void Contacto_ValidaCorreoYTelefono()
    {
        var contacto = new Contacto(" Maria.Rodriguez@Correo.COM ", "+58 412-5550134");
        Assert.Equal("maria.rodriguez@correo.com", contacto.Correo);
        Assert.Equal("+58 412-5550134", contacto.Telefono);

        Assert.Throws<ExcepcionDeDominio>(() => new Contacto("correo-invalido", "04125550134"));
        Assert.Throws<ExcepcionDeDominio>(() => new Contacto("ok@correo.com", "123"));
    }

    [Fact]
    public void VentanaFechasYFechaLimite_AplicaReglaR20_UtcMenos4Hasta2359()
    {
        var ventana = new VentanaFechas(new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
        var limite = new FechaLimite(new DateOnly(2026, 11, 30));

        // 23:59:59 en UTC-4 equivale a 03:59:59 UTC del día siguiente (1 dic): sigue vigente (R20).
        var ultimoSegundoUtc = new DateTimeOffset(2026, 12, 1, 3, 59, 59, TimeSpan.Zero);
        Assert.True(ventana.Contiene(ultimoSegundoUtc));
        Assert.False(limite.Vencida(ultimoSegundoUtc));

        // 00:00:00 en UTC-4 del 1 dic equivale a 04:00:00 UTC del 1 dic: ya venció (R20).
        var primerSegundoVencidoUtc = new DateTimeOffset(2026, 12, 1, 4, 0, 0, TimeSpan.Zero);
        Assert.False(ventana.Contiene(primerSegundoVencidoUtc));
        Assert.True(ventana.EstaVencida(primerSegundoVencidoUtc));
        Assert.True(limite.Vencida(primerSegundoVencidoUtc));
    }

    [Fact]
    public void RelojSistemaCaracas_ConvierteUtcFijoACaracas()
    {
        // 02:30 UTC del 1 dic = 22:30 UTC-4 del 30 nov.
        var reloj = new RelojSistemaCaracas(new RelojFijoProvider(new DateTimeOffset(2026, 12, 1, 2, 30, 0, TimeSpan.Zero)));
        Assert.Equal(new DateOnly(2026, 11, 30), reloj.HoyCaracas);
        Assert.Equal(TimeSpan.FromHours(-4), reloj.AhoraCaracas.Offset);
    }

    [Fact]
    public void PromedioJustificacionReferenciaArchivoYCodigoPeriodo_ValidanInvariantes()
    {
        var p1 = new Promedio(17.456m);
        var p2 = new Promedio(16m);
        Assert.Equal(17.46m, p1.Valor);
        Assert.True(p1.EsMayorOIgual(p2));
        Assert.Throws<ExcepcionDeDominio>(() => new Promedio(-0.1m));

        var justificacion = new Justificacion("  Cumple con todos los criterios  ");
        Assert.Equal("Cumple con todos los criterios", justificacion.Texto);
        Assert.Throws<ExcepcionDeDominio>(() => new Justificacion("ab"));

        var archivo = new ReferenciaArchivo("postulaciones/2026-2/notas.pdf");
        Assert.Equal("postulaciones/2026-2/notas.pdf", archivo.RutaFirebase);
        Assert.Throws<ExcepcionDeDominio>(() => new ReferenciaArchivo("https://storage.googleapis.com/publico.pdf"));

        var periodo = new CodigoPeriodo(" 2026-2 ");
        Assert.Equal("2026-2", periodo.Valor);
        Assert.Throws<ExcepcionDeDominio>(() => new CodigoPeriodo("   "));
    }
}
