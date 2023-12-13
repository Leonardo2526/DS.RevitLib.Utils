﻿using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisions;
using DS.ClassLib.VarUtils.Resolvers;
using DS.RevitCollisions.Models;
using DS.RevitCollisions.Resolve.ResolveFactories;
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

namespace DS.RevitCollisions.Resolve.ResolveProcessors
{
    public class DummyResolveProcessorBuilder
    {
        public ResolveProcessor<string> Processor { get; private set; }


        /// <summary>
        /// The core Serilog, used for writing log events.
        /// </summary>
        public ILogger Logger { get; set; }

        public ResolveProcessor< string> GetProcessor()
        {
            var factories = new List<IResolveFactory<string>>();


            //add factories
            var factoryBuilder = new DummyFactoryBuilder()
            {
                Logger = Logger
            };
            IResolveFactory<string> resolveFactory1 = factoryBuilder.Create();
            factories.Add(resolveFactory1);

            var p = new ResolveProcessor<string>(factories)
            {
                Logger = Logger
            };
            return p;

        }
    }
}