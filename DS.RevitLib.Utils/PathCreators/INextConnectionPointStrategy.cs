using Autodesk.Revit.DB;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{
    public interface INextConnectionPointStrategy
    {
        (Point3d point, Vector3d dir) GetPoint(Element element, XYZ point);
    }
}
