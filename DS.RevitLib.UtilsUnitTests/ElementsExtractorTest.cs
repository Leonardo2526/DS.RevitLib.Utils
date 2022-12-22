using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using DS.RevitLib.Utils.Elements;
using NUnit.Framework;
using RTF.Applications;
using RTF.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTestProject.UnitTests
{
    [TestFixture]
    public class ElementsExtractorTest
    {
        private Document _doc;
        private List<BuiltInCategory> _exludedCathegories;

        [SetUp]
        public void Setup()
        {
            _doc = RevitTestExecutive.CommandData.Application.ActiveUIDocument.Document;
            _exludedCathegories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_DuctFittingInsulation,
                BuiltInCategory.OST_DuctInsulations,
                BuiltInCategory.OST_DuctCurvesInsulation,
                BuiltInCategory.OST_PipeFittingInsulation,
                BuiltInCategory.OST_PipeInsulations,
                BuiltInCategory.OST_PipeCurvesInsulation,
                BuiltInCategory.OST_TelephoneDevices,
                BuiltInCategory.OST_Materials
            };
        }


        //[Test]
        //[TestModel(@"C:\Models\test_models_2019\*.rvt")]
        public void MethodName_Scenario_ExpectedBehaviour()
        {
            XYZ xYZ = new XYZ(0, 0, 0);
            XYZ xYZ1 = new XYZ(0, 0, 1);
            XYZ result = xYZ + xYZ1;

            Assert.AreEqual(result.Z, 1);
        }

        [Test]
        [TestModel(@"E:\YandexDisk\TestModels\" + "001_HeatPipesNodeWithInsulation.rvt")]
        public void ElementsExtractor_DocElementsCount_CountAreEqual()
        {
            var (docElements, linkElementsDict) = new ElementsExtractor(_doc, _exludedCathegories).GetAll();

            Assert.AreEqual(72, docElements.Count);
        }

        [Test]
        [TestModel(@"E:\YandexDisk\TestModels\" + "001_HeatPipesNodeWithInsulation.rvt")]
        public void ElementsExtractor_DocElementsCount_CountAreNotEqual()
        {
            var (docElements, linkElementsDict) = new ElementsExtractor(_doc, _exludedCathegories).GetAll();

            Assert.AreNotEqual(0, docElements.Count);
        }
    }
}
