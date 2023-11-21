using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Collisions;
using DS.RevitCollisions.CollisionBuilers;
using DS.RevitCollisions.Impl;
using DS.RevitCollisions.Models;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Various;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCollisions.ManualTest.TestCases
{

    internal class ElementCollisionFactoryTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trfIn;
        private readonly ContextTransactionFactory _trfOut;
        private readonly ContextTransactionFactory _trfAuto;
        private readonly UIApplication _uiApp;
        private ILogger _logger;
        private ElementCollisionFactory _factory;
        private readonly ICollisionVisualizator<Collision> _collisionVisualizator;

        public ElementCollisionFactoryTest(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _uiDoc = uiApp.ActiveUIDocument;
            _doc = _uiDoc.Document;
            _trfIn = new ContextTransactionFactory(_doc, RevitContextOption.Inside);
            _trfOut = new ContextTransactionFactory(_doc, RevitContextOption.Outside);
            _trfAuto = new ContextTransactionFactory(_doc);


            _logger = new LoggerConfiguration()
                 .MinimumLevel.Information()
                 .WriteTo.Debug()
                 .CreateLogger();

            _collisionVisualizator = new CollisionVisualizator(uiApp);
        }

        public Collision Collision { get; private set; }

        public ElementCollisionFactoryTest BuildFactory()
        {
            _factory = new ElementCollisionFactory()
            {
                Visualizator = _collisionVisualizator,
                ExcludeTraversableArchitecture = true,
                MinIntersectionVolume = 0,
                Logger = _logger
            };

            return this;
        }


        public ElementCollisionFactoryTest CreateCollision()
        {
            var e1 = new ElementSelector(_uiDoc).Pick();
            var e2 = new ElementSelector(_uiDoc).Pick();

            Collision = _factory.CreateCollision(e1, e2);
            Collision.Show();

            return this;
        }

    }
}
