using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Visualisators
{
    public class Visualisator
    {
        private readonly IVisualisator _visualisator;

        public Visualisator(IVisualisator visualisator)
        {
            _visualisator = visualisator;
            _visualisator.Visualise();
        }
    }
}
