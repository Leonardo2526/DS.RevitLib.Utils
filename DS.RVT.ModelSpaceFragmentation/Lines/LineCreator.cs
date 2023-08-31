using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace DS.RVT.ModelSpaceFragmentation.Lines
{
    class LineCreator
    {
        public Line Create(ILine line)
        {
            return line.Create();
        }

        public void CreateCurves(ICurves curves)
        {
            curves.Create();
        }
    }
}
