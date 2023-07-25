using DS.ClassLib.VarUtils;

namespace DS.RevitLib.Utils.Visualisators
{
    public class Visualisator
    {
        private readonly IVisualisator _visualisator;

        public Visualisator(IVisualisator visualisator)
        {
            _visualisator = visualisator;
            _visualisator.Show();
        }
    }
}
