using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IN8
{
    public interface Hardware
    {
        void PortWritten(byte port, byte value);
    }
}
