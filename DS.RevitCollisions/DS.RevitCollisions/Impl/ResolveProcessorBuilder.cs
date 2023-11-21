using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Resolvers;
using DS.RevitCollisions.Models;
using Rhino.Geometry;
using Serilog.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.Impl
{
    public class ResolveProcessorBuilder
    {
        //public ResolveProcessor<IMEPCollision, object> Processor { get; private set; }
        

        //public ResolveProcessor<IMEPCollision, object> GetProcessor()
        //{
        //    var factories = new List<IResolveFactory<IMEPCollision, object>>();

        //    //add factories
        //    var fb1 = new PathFindFactoryBuilder();
        //    IResolveFactory<IMEPCollision, List<Point3d>> f1 = fb1.Create();

        //    //var f2 = new ResolveFactory<IMEPCollision, int, Line>(null, null);
        //    factories.Add((IResolveFactory<IMEPCollision, object>)f1);
        //    //factories.Add((IResolveFactory<IMEPCollision, object>)f2);

        //    var p = new ResolveProcessor<IMEPCollision, object>(factories);
        //    //var p = new ResolveProcessor<IMEPCollision, List<Point3d>>(factories);
        //    return p;

        //}
    }
}
