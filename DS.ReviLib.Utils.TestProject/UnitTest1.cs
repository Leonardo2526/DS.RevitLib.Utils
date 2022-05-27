using Autodesk.Revit.DB;
using DS.RevitLib.Utils;

namespace DS.ReviLib.Utils.TestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.IsTrue(Class1.EqualsV());
        }

        [TestMethod]
        public void TestMethod2()
        {           
            bool eq = true;
            Assert.IsTrue(eq);
        }

        [TestMethod]
        public void TestMethod3()
        {
            XYZ vector1 = new XYZ(1, 0, 0);
            XYZ vector2 = new XYZ(1, 0, 0);
            bool eq = vector1.IsAlmostEqualTo(vector2);
            Assert.IsTrue(eq);
        }
    }
}