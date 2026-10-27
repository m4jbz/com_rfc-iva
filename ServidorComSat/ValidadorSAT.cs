    using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace ServidorComSat
{
    // Interfaz COM del Módulo de Servicios Fiscales.
    // Dual: permite Early Binding (vtable) y Late Binding (IDispatch).
    [ComVisible(true)]
    [Guid("377c5c35-b4ca-4db6-a094-472be28b7079")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IValidadorSAT
    {
        [DispId(1)]
        bool ValidarRFC(string rfc);

        [DispId(2)]
        double CalcularIva(double monto);
    }

    // Clase COM (CoClass) registrada con el ProgID ServidorComSat.ValidadorSAT.
    [ComVisible(true)]
    [Guid("796143f6-dba5-42cf-8230-f17df181b3e0")]
    [ProgId("ServidorComSat.ValidadorSAT")]
    [ClassInterface(ClassInterfaceType.None)]
    public class ValidadorSAT : IValidadorSAT
    {
        private const int LongitudPersonaMoral = 12;
        private const int LongitudPersonaFisica = 13;
        private const decimal TasaIva = 0.16m;

        public bool ValidarRFC(string rfc)
        {
            if (string.IsNullOrEmpty(rfc))
                return false;

            // Elimina todo espacio en blanco y convierte a mayúsculas.
            string limpio = new string(rfc.Where(c => !char.IsWhiteSpace(c)).ToArray())
                .ToUpperInvariant();

            return limpio.Length == LongitudPersonaMoral
                || limpio.Length == LongitudPersonaFisica;
        }

        public double CalcularIva(double monto)
        {
            // decimal evita errores de representación binaria al redondear.
            decimal iva = (decimal)monto * TasaIva;
            return (double)Math.Round(iva, 2, MidpointRounding.AwayFromZero);
        }
    }
}
