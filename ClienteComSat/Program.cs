using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ClienteComSat
{
    internal static class Program
    {
        private const string ProgId = "ServidorComSat.ValidadorSAT";

        private static readonly string[] RfcsPrueba =
        {
            "ABC010203XYZ",     // 12 caracteres: Persona Moral
            "ABCD010203XY1",    // 13 caracteres: Persona Física
            "  abcd 010203 xy1 ", // con espacios y minúsculas (13 al limpiar)
            "ABC123",           // demasiado corto
            "ABCDE010203XYZ12", // demasiado largo
            ""                  // vacío
        };

        private static readonly double[] MontosPrueba = { 100.00, 1234.56, 0.99, 99999.99, 0 };

        private static void Main()
        {
            Console.WriteLine("=== ClienteComSat: consumo de ServidorComSat.ValidadorSAT ===");

            try
            {
                EnlaceTemprano();
                EnlaceTardio();
            }
            catch (COMException ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error COM: " + ex.Message);
                Console.WriteLine("¿Se registró el componente con registrar.bat (como Administrador)?");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Presione una tecla para salir...");
            Console.ReadKey();
        }

        // ---------------------------------------------------------------
        // Módulo 1: Early Binding (contrato del .tlb conocido en compilación,
        // ver ServidorComSatInterop.cs; llamadas por vtable, sin IDispatch)
        // ---------------------------------------------------------------
        private static void EnlaceTemprano()
        {
            Console.WriteLine();
            Console.WriteLine("--- MÓDULO 1: ENLACE TEMPRANO (Early Binding) ---");

            // CoCreateInstance(CLSID, IID_IValidadorSAT) + llamadas por vtable.
            using (var validador = new ValidadorSATEarly())
            {
                foreach (string rfc in RfcsPrueba)
                    Console.WriteLine("ValidarRFC(\"{0}\") = {1}", rfc, validador.ValidarRFC(rfc));

                foreach (double monto in MontosPrueba)
                    Console.WriteLine("CalcularIva({0:F2}) = {1:F2}", monto, validador.CalcularIva(monto));
            }
        }

        // ---------------------------------------------------------------
        // Módulo 2: Late Binding (solo ProgID, sin referencias en compilación)
        // ---------------------------------------------------------------
        private static void EnlaceTardio()
        {
            Console.WriteLine();
            Console.WriteLine("--- MÓDULO 2: ENLACE TARDÍO (Late Binding) ---");

            Type tipo = Type.GetTypeFromProgID(ProgId, true);
            object validador = Activator.CreateInstance(tipo);
            try
            {
                foreach (string rfc in RfcsPrueba)
                {
                    object r = tipo.InvokeMember("ValidarRFC", BindingFlags.InvokeMethod,
                        null, validador, new object[] { rfc });
                    Console.WriteLine("ValidarRFC(\"{0}\") = {1}", rfc, r);
                }

                foreach (double monto in MontosPrueba)
                {
                    object r = tipo.InvokeMember("CalcularIva", BindingFlags.InvokeMethod,
                        null, validador, new object[] { monto });
                    Console.WriteLine("CalcularIva({0:F2}) = {1:F2}", monto, (double)r);
                }
            }
            finally
            {
                // Si el servidor es .NET y está en el mismo proceso, el CLR devuelve el objeto
                // administrado sin RCW; solo se libera cuando es un objeto COM.
                if (Marshal.IsComObject(validador))
                    Marshal.ReleaseComObject(validador);
            }
        }
    }
}
