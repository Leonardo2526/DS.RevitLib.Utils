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
    ///  An object that represents pipes tests for <see cref="MEPSystemGraphFactory"/>.
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
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\001SinglePipe.rvt")]
        public void CreateGraph_1_001SinglePipe_ShouldPass()
        {
            var elId = new ElementId(702695);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }

        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\001SinglePipe.rvt")]
        public void CreateGraph_2_001SinglePipe_ShouldFail()
        {
            var elId = new ElementId(702695);
            var graph = _graphBuilder.CreateGraph(elId);
            GraphTester.IsFailEmptyVertices(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\002PipeWithElbow90.rvt")]
        public void CreateGraph_3_002PipeWithElbow90_ShouldPass()
        {
            var elId = new ElementId(705395);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }

        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\003PipeWithElbow30.rvt")]
        public void CreateGraph_4_003PipeWithElbow30_ShouldPass()
        {
            var elId = new ElementId(763314);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\004PipeChainRotated.rvt")]
        public void CreateGraph_5_004PipeChainRotated_ShouldPass()
        {
            var elId = new ElementId(709254);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\005PipeFamiliesInstChain.rvt")]
        public void CreateGraph_6_005PipeFamiliesInstChain_ShouldPass()
        {
            var elId = new ElementId(766180);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\006PipeWithElbow30AndSpuds.rvt")]
        public void CreateGraph_7_006PipeWithElbow30AndSpuds_ShouldPass()
        {
            var elId = new ElementId(763558);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\007PipeWithSpuds.rvt")]
        public void CreateGraph_8_007PipeWithSpuds_ShouldPass()
        {
            var elId = new ElementId(702533);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\008PipeWithManySpuds.rvt")]
        public void CreateGraph_9_008PipeWithManySpuds_ShouldPass()
        {
            var elId = new ElementId(703409);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\009PipeWithTees.rvt")]
        public void CreateGraph_10_009PipeWithTees_ShouldPass()
        {
            var elId = new ElementId(702537);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\010PipeWithManyTees.rvt")]
        public void CreateGraph_11_010PipeWithManyTees_ShouldPass()
        {
            var elId = new ElementId(698669);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsValidSimpleChainOrTreeItemsCount(graph);
            GraphTester.IsSimple(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\011PipeWithSpudCycle.rvt")]
        public void CreateGraph_12_011PipeWithSpudCycle_ShouldPass()
        {
            var elId = new ElementId(767148);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsSimple(graph);
            GraphTester.IsSpecificVerticesCount(graph, 8);
            GraphTester.IsSpecificEdgesCount(graph, 8);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\012PipeWithElbowCycle.rvt")]
        public void CreateGraph_13_012PipeWithElbowCycle_ShouldPass()
        {
            var elId = new ElementId(767434);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsSimple(graph);
            GraphTester.IsSimpleCycle(graph);
        }

        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\013PipeWithTeeCycle.rvt")]
        public void CreateGraph_14_013PipeWithTeeCycle_ShouldPass()
        {
            var elId = new ElementId(768018);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsSimple(graph);
            GraphTester.IsSpecificVerticesCount(graph, 9);
            GraphTester.IsSpecificEdgesCount(graph, 9);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\014PipeWithEquipmentCycle.rvt")]
        public void CreateGraph_15_014PipeWithEquipmentCycle_ShouldPass()
        {
            var elId = new ElementId(722048);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsSimple(graph);
            GraphTester.IsSimpleCycle(graph);
        }


        /// <summary>
        /// Pipe test.
        /// </summary>
        [Test]
        [Category("Pipes")]
        [TestModel(@"D:\YandexDisk\TestModels\MEPSystemGraphFactoryTest\Pipes\015PipeWithEquipments.rvt")]
        public void CreateGraph_16_015PipeWithEquipments_ShouldPass()
        {
            var elId = new ElementId(723419);
            var graph = _graphBuilder.CreateGraph(elId);

            GraphTester.IsSimple(graph);
            GraphTester.IsSpecificVerticesCount(graph, 40);
            GraphTester.IsSpecificEdgesCount(graph, 42);
        }
    }
}
