using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FeliCaAccessPlugIn
{
    [Guid("0d295298-6887-4a00-8a9b-32602374426c")]
    [InterfaceTypeAttribute(ComInterfaceType.InterfaceIsIDispatch)]
    public interface FelicaEvent
    {
        [DispId(1)]
        void FelicaError(String message);

        [DispId(2)]
        void NotifyIDm(String message);
    }
}
