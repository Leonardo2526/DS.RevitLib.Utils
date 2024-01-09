using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Filters;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Collisions;
using DS.RevitLib.Utils.Creation.Transactions;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.Lines;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.Openings;
using DS.RevitLib.Utils.Various;
using Rhino.Geometry;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DS.RevitLib.Test.TestedClasses
{
    internal class WallsCollisionFilterTest
    {
        private readonly UIDocument _uiDoc;
        private readonly Document _doc;
        private readonly ContextTransactionFactory _trf;
        private bool _checkTraverseDirection = true;
        private double _maxEdgeLength = 1000.MMToFeet();


        public WallsCollisionFilterTest(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _trf = new ContextTransactionFactory(_doc, Utils.RevitContextOption.Inside);
        }

        public void RunCase1()
        {
            var mEPCurve = new ElementSelector(_uiDoc).Pick() as MEPCurve;
            var wallElem = new ElementSelector(_uiDoc).Pick();
            var wall = wallElem as Wall;

            var collisions = new List<(Solid, Element)>();
            var collision = (mEPCurve.Solid(), wall);
            collisions.Add(collision);

            if (_checkTraverseDirection)
            {
                var dir = mEPCurve.Direction().ToVector3d();
                var rools = new List<Func<(Solid, Element), bool>>
                {SolidElementRulesFilterSet.WallTraversableDirectionRule(dir)};
                Func<(Solid, Element), bool> ruleCollisionFilter = new RulesFilterFactory<Solid, Element>(rools).GetFilter();
                collisions = collisions.Where(ruleCollisionFilter).ToList();
            }

            var filter = GetFilter(_doc);
            collisions = collisions.Where(filter).ToList();

            Debug.WriteLine("Collisions count is: " + collisions.Count);
        }

        private Func<(Solid, Element), bool> GetFilter(Document doc)
        {
            var rools = new List<Func<(Solid, Element), bool>>
            {
                SolidElementRulesFilterSet.WallDistanceToEdgeRule(doc, _maxEdgeLength),
                //SolidElementRulesFilterSet.WallConstructionRule(doc)
            };
            return new RulesFilterFactory<Solid, Element>(rools).GetFilter();
        }

        private Func<(Solid, Element), bool> GetResultBySolid(Document doc,
        Solid solid,
        Element wall,
        ILogger logger,
        ITransactionFactory trf)
        {
            var profileCreator = new RectangleSolidProfileCreator(doc)
            {
                Offset = 100.MMToFeet(),
                TransactionFactory = trf,
                Logger = logger
            };
            var wBuilder = new WallOpeningProfileValidator<Solid>(doc, profileCreator)
            {
                InsertsOffset = 500.MMToFeet(),
                WallOffset = 1000.MMToFeet(),
                JointsOffset = 0,
                Logger = logger,
                TransactionFactory = trf
            };

            (Solid, Element) mArg = (solid, wall);

            var b = Func(mArg);
            if (!wBuilder.ValidationResults.Any()) return b;
            var sb = new StringBuilder();
            wBuilder.ValidationResults.ToList().ForEach(r => sb.Append(r.ErrorMessage));
            logger?.Information(sb.ToString());

            return b;

            bool Func((Solid, Element) f) => f.Item2 is Wall wall1 && wBuilder.IsValid((wall1, f.Item1));
        }

    }
}
