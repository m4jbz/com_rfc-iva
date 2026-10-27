using System;
using System.Runtime.InteropServices;

namespace ClienteComSat
{
    // Early Binding contra ServidorComSat.tlb.
    //
    // Un .tlb exportado desde un ensamblado CLR no puede reimportarse con tlbimp (MSB3290), y
    // si el cliente también es .NET, el CLR "desenvuelve" el objeto COM y devuelve directamente
    // el objeto administrado del servidor (InvalidCastException con tipos [ComImport]).
    // Para consumir el servidor como COM real, se crea el objeto con CoCreateInstance y se
    // invocan los métodos por la vtable de IValidadorSAT, cuyo diseño se conoce en compilación:
    //
    //   IUnknown   : 0 QueryInterface, 1 AddRef, 2 Release
    //   IDispatch  : 3 GetTypeInfoCount, 4 GetTypeInfo, 5 GetIDsOfNames, 6 Invoke
    //   IValidadorSAT: 7 ValidarRFC, 8 CalcularIva
    //
    // Los GUID deben coincidir con los del servidor.
    internal sealed class ValidadorSATEarly : IDisposable
    {
        private static readonly Guid Clsid = new Guid("796143f6-dba5-42cf-8230-f17df181b3e0");
        private static readonly Guid Iid = new Guid("377c5c35-b4ca-4db6-a094-472be28b7079");

        private const int IndiceRelease = 2;
        private const int IndiceValidarRfc = 7;
        private const int IndiceCalcularIva = 8;
        private const uint CLSCTX_INPROC_SERVER = 1;
        private const uint COINIT_MULTITHREADED = 0;

        [DllImport("ole32.dll")]
        private static extern int CoInitializeEx(IntPtr reservado, uint modelo);

        [DllImport("ole32.dll")]
        private static extern int CoCreateInstance(ref Guid rclsid, IntPtr pUnkOuter, uint contexto,
            ref Guid riid, out IntPtr ppv);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate uint ReleaseFn(IntPtr self);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ValidarRfcFn(IntPtr self,
            [MarshalAs(UnmanagedType.BStr)] string rfc, out short resultado);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int CalcularIvaFn(IntPtr self, double monto, out double resultado);

        private IntPtr _objeto;
        private readonly ReleaseFn _release;
        private readonly ValidarRfcFn _validarRfc;
        private readonly CalcularIvaFn _calcularIva;

        public ValidadorSATEarly()
        {
            // S_FALSE / RPC_E_CHANGED_MODE indican que el hilo ya tenía apartamento: es válido.
            CoInitializeEx(IntPtr.Zero, COINIT_MULTITHREADED);

            Guid clsid = Clsid, iid = Iid;
            int hr = CoCreateInstance(ref clsid, IntPtr.Zero, CLSCTX_INPROC_SERVER, ref iid, out _objeto);
            Marshal.ThrowExceptionForHR(hr);

            IntPtr vtable = Marshal.ReadIntPtr(_objeto);
            _release = Funcion<ReleaseFn>(vtable, IndiceRelease);
            _validarRfc = Funcion<ValidarRfcFn>(vtable, IndiceValidarRfc);
            _calcularIva = Funcion<CalcularIvaFn>(vtable, IndiceCalcularIva);
        }

        private static T Funcion<T>(IntPtr vtable, int indice)
        {
            IntPtr puntero = Marshal.ReadIntPtr(vtable, indice * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(puntero);
        }

        public bool ValidarRFC(string rfc)
        {
            short resultado;
            Marshal.ThrowExceptionForHR(_validarRfc(_objeto, rfc, out resultado));
            return resultado != 0; // VARIANT_BOOL: 0 = false, -1 = true
        }

        public double CalcularIva(double monto)
        {
            double resultado;
            Marshal.ThrowExceptionForHR(_calcularIva(_objeto, monto, out resultado));
            return resultado;
        }

        public void Dispose()
        {
            if (_objeto != IntPtr.Zero)
            {
                _release(_objeto);
                _objeto = IntPtr.Zero;
            }
        }
    }
}
