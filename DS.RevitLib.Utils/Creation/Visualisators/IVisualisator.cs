using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Visualisators
{
    public interface ISVisualisator
    {
        public void Show();
    }

    public interface IObjectVisualisator
    {
        public void Show(object objectToShow);
    }

    public interface IPointVisualisator<T>
    {
        public void Show(T point);
    }
}
