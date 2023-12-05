using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Resolvers;
using DS.ClassLib.VarUtils.Selectors;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Selections.Validators;
using DS.RevitLib.Utils.Various.Selections;
using System;
using System.Collections.Generic;

namespace DS.RevitLib.Utils.Resolve.TaskCreators
{
    /// <inheritdoc/>
    public class ManualXYZElementTaskCreatorFactory :
        ValidatableTaskCreatorFactoryBase<(Element, XYZ)>
    {
        private readonly IElementCollisionDetector _elementCollisionDetector;
        private readonly XYZCollisionDetector _xYZCollisionDetector;

        /// <inheritdoc/>
        public ManualXYZElementTaskCreatorFactory(UIDocument uIDoc,
            IElementCollisionDetector elementCollisionDetector) : base(uIDoc)
        {
            _elementCollisionDetector = elementCollisionDetector;
            _xYZCollisionDetector = new XYZCollisionDetector(elementCollisionDetector);
        }

        /// <inheritdoc/>
        public override ITaskCreator<((Element, XYZ), (Element, XYZ))> Create()
        {
            var xyzSelectors = new XYZElementSelectors(_uIDoc)
            { AllowLink = false, Logger = Logger };
            var selectors = new List<Func<(Element, XYZ)>>()
            {
                xyzSelectors.SelectElement,
                xyzSelectors.SelectPointOnElement
            };

            var validators = GetValidators();
            var selector = new ValidatableSelector<(Element, XYZ)>(selectors)
            {
                Validators = validators,
                Messenger = Messenger,
                Logger = Logger
            };
            return new TupleValidatableTaskCreator<(Element, XYZ)>(selector);
        }

        /// <inheritdoc/>
        protected override List<IValidator<(Element, XYZ)>> GetValidators()
        {
            var validators = new List<IValidator<(Element, XYZ)>>();

            _xYZCollisionDetector.ElementClearance = TraceSettings.B;
            var collisionValidator = new XYZElementCollisionValidator(_doc, _elementCollisionDetector, _xYZCollisionDetector)
            { BaseMEPCurve = BaseMEPCurve };
            var limitsValidator = new XYZElementLimitsValidator(_doc)
            {
                BoundOutline = ExternalOutline,
                IsInsulationAccount = InsulationAccount,
                MinDistToFloor = TraceSettings.H,
                MinDistToCeiling = TraceSettings.B
            };
            var famInstCategoryValidator = new XYZElementCategoryValidator(_doc, AvailableCategories);

            validators.Add(collisionValidator);
            validators.Add(limitsValidator);
            validators.Add(famInstCategoryValidator);

            return validators;
        }
    }
}
