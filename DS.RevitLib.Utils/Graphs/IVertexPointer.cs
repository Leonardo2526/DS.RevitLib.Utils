using DS.GraphUtils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    public interface IVertexPointer
    {
        IVertex Point(string pointMessage = null);
    }
}
