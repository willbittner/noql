using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashx.CEP.Adapters
{
    interface IOutputAdapter : IAdapter
    {
        void OnData(object data);
    }
}
