using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Resolvers;
using DS.RevitCollisions.Models;
using Rhino.Geometry;
using Serilog;
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
    public class DummyResolveProcessorBuilder
    {
        public ResolveProcessor<IMEPCollision, string> Processor { get; private set; }


        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        public ResolveProcessor<IMEPCollision, string> GetProcessor()
        {
            var factories = new List<IResolveFactory<IMEPCollision, string>>();


            //add factories
            var factoryBuilder = new DummyFactoryBuilder()
            {
                Logger = Logger
            };
            IResolveFactory<IMEPCollision, string> resolveFactory1 = factoryBuilder.Create();
            factories.Add(resolveFactory1);

            var p = new ResolveProcessor<IMEPCollision, string>(factories)
            {
                Logger = Logger
            };
            return p;

        }
    }
}
