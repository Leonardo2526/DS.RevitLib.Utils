using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Utils;
using System.Collections.Generic;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// An object to zoom in model.
    /// </summary>
    public class Zoomer
    {
        private readonly Document _doc;
        private readonly UIApplication _app;
        private readonly UIView _uiview;

        /// <summary>
        /// Instantiate an object to zoom in model.
        /// </summary>
        public Zoomer(Document doc, UIApplication app)
        {
            _doc = doc;
            _app = app;
            _uiview = GetUIView();
        }

        /// <summary>
        /// Zoom by solid
        /// </summary>
        /// <param name="solid"></param>
        /// <param name="offset">Offset from <paramref name="solid"/> center point.</param>
        /// <returns>Returns <see cref="BoundingBoxXYZ"/> used to zoom.</returns>
        public BoundingBoxXYZ Zoom(Solid solid, double offset = 1)
        {
            var center = solid.ComputeCentroid();
            var delta = new XYZ(offset, offset, offset);
            var min = center - delta;
            var max = center + delta;
            var boxXYZ = new BoundingBoxXYZ() { Min = min, Max = max };

            _uiview.ZoomAndCenterRectangle(boxXYZ.Min, boxXYZ.Max);

            return boxXYZ;
        }

        /// <summary>
        /// Zoom by <paramref name="p1"/> and <paramref name="p2"/>.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="offset">Offset from each point.</param>
        /// <returns>Returns <see cref="BoundingBoxXYZ"/> used to zoom.</returns>
        public BoundingBoxXYZ Zoom(XYZ p1, XYZ p2, double offset = 1)
        {
            var boxXYZ = GetBoxXYZ(p1, p2, offset);
            _uiview.ZoomAndCenterRectangle(boxXYZ.Min, boxXYZ.Max);

            return boxXYZ;
        }

        private BoundingBoxXYZ GetBoxXYZ(XYZ p1, XYZ p2, double offset)
        {
            var points = new List<XYZ>() { p1, p2 };
            (XYZ minPoint, XYZ maxPoint) = XYZUtils.CreateMinMaxPoints(points);
            var delta = new XYZ(offset, offset, offset);
            minPoint -= delta;
            maxPoint += delta;

            return new BoundingBoxXYZ() { Min = minPoint, Max = maxPoint };
            //return ElementUtils.GetBoundingBox(points, offset);
        }

        private UIView GetUIView()
        {
            var uidoc = new UIDocument(_doc);
            var view = uidoc.ActiveGraphicalView;
            UIView uiview = null;
            var uiviews = _app.ActiveUIDocument.GetOpenUIViews();
            foreach (UIView uv in uiviews)
            {
                if (uv.ViewId.Equals(view.Id))
                {
                    uiview = uv;
                    break;
                }
            }
            return uiview;
        }
    }
}
