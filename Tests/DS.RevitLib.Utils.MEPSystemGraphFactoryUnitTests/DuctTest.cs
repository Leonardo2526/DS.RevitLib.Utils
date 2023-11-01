using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils.Graphs;
using NUnit.Framework;
using QuickGraph;
using RTF.Applications;
using RTF.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace DS.RevitLib.Utils.MEPSystemGraphFactoryUnitTests
{
    /// <summary>
    ///  An object that represents ducts tests for <see cref="MEPSystemGraphFactory"/>.
    /// </summary>
    [TestFixture]
    public partial class DuctTest
    {
        private UIDocument _uiDoc;
        private Document _doc;
        private GraphBuilder _graphBuilder;

        [SetUp]
        public void Setup()
        {
            _uiDoc = RevitTestExecutive.CommandData.Application.ActiveUIDocument;
            _doc = RevitTestExecutive.CommandData.Application.ActiveUIDocument.Document;
            _graphBuilder = new GraphBuilder(_uiDoc);
        }


        /// <summary>
        /// Duct test.
        /// </summary>
        [Test]
        [Category("Ducts")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Ducts\001DuctWithTees.rvt")]
        public void CreateGraph_1_001DuctWithTees_ShouldPass()
        {
            var elId = new ElementId(713409);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }


        /// <summary>
        /// Duct test.
        /// </summary>
        [Test]
        [Category("Ducts")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Ducts\002DuctWithSpudsAndRandomAngleElbows.rvt")]
        public void CreateGraph_2_002DuctWithSpudsAndRandomAngleElbows_ShouldPass()
        {
            var elId = new ElementId(713416);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }

    }
}
