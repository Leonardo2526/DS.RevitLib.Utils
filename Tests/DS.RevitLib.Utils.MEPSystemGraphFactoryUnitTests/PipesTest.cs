using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using NUnit.Framework;
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
    ///  An object that represents resolve collision tests for <see cref="CollisionItemViewModel"/>.
    /// </summary>
    [TestFixture]
    public partial class PipesTest
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
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\001Pipe.rvt")]
        public void CreateGraph_1_001Pipe_ShouldPass()
        {
            var elId = new ElementId(702695);
            var graph = _graphBuilder.CreateGraph(elId);
            GraphTester.IsValidSimpleChainOrTreeVertexCount(graph);
        }

        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\001Pipe.rvt")]
        public void CreateGraph_2_001Pipe_ShouldFail()
        {
            var elId = new ElementId(702695);
            var graph = _graphBuilder.CreateGraph(elId);
            GraphTester.IsFailEmptyVertices(graph);
        }
    }
}
