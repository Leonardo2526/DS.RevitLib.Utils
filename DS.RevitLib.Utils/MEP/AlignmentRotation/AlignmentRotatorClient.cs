using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Elements.Creators;
using DS.RevitLib.Utils.MEP.AlignmentRotation.Strategies;
using DS.RevitLib.Utils.TransactionCommitter;

namespace DS.RevitLib.Utils.MEP.AlignmentRotation
{
    public class AlignmentRotatorClient : AbstractCreator
    {
        private readonly Element _operationElement;
        private readonly Element _targetElement;
        private readonly ElementCreator _creator;

        public AlignmentRotatorClient(Element operationElement, Element targetElement)
        {
            _operationElement = operationElement;
            _targetElement = targetElement;
            _creator = new ElementCreator(_committer, _transactionPrefix);
        }

        public Element RotateAroundCenterLine()
        {
            var centerLineRotator = new CenterLineRotator(_operationElement, _targetElement, _creator);
            return centerLineRotator.Rotate();
        }

        public Element RotateAroundNormal()
        {
            var normalLineRotator = new NormalLineRotator(_operationElement, _targetElement, _creator);
            return normalLineRotator.Rotate();
        }

        public Element FullRotation()
        {
            RotateAroundCenterLine();
            Element element = RotateAroundNormal();
            return element;
        }
    }
}
