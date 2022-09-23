using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.MEP;
using System.Linq;
using System.Xml.Linq;

namespace ClassLibrary1
{
    public class Class1
    {
        Document _doc;
        private readonly Element _element;

        public Class1(Document doc, Element element)
        {
            _doc = doc;
            _element = element;
        }

        public void Run()
        {
            var builder = new TransactionBuilder<Element>(_doc);
            builder.Build(() => DeleteElem(_element));
        }

        private Element DeleteElem(Element element)
        {
            var elem = _doc.Delete(element.Id).First();
            return _doc.GetElement(elem);
        }
    }
}
