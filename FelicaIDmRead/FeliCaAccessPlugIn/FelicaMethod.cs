using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FeliCaAccessPlugIn
{
    [Guid("fda006e1-0fc2-4234-b0d9-ad7fcb667827")]
    public interface FelicaMethod
    {
        [DispId(1)]
        void Init();
    }
}
