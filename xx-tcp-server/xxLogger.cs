using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xx_tcp
{
    public interface xxLogger
    {
        xxLogManager CreateLogManager(Type type);
    }
}
