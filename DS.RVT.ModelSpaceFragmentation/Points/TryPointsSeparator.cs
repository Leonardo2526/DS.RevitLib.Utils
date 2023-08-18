using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RVT.ModelSpaceFragmentation.Lines;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DS.RevitLib.Utils.Extensions;

namespace DS.RVT.ModelSpaceFragmentation
{
    class TryPointsSeparator
    {
        private readonly List<XYZ> _spacePoints;
        private readonly List<Solid> _solids;
        private readonly List<Outline> _outLines;

        public Dictionary<Outline, List<Solid>> OutlinesWithSolids { get; }

        public TryPointsSeparator(List<XYZ> spacePoints, Dictionary<Outline, List<Solid>> outlinesSolids)
        {
            _spacePoints = spacePoints;
            _solids = outlinesSolids.SelectMany(obj => obj.Value).ToList();
            _solids = _solids.Distinct().ToList();
            _outLines = outlinesSolids.Keys.ToList();
            OutlinesWithSolids = outlinesSolids;
        }

        public List<XYZ> PassablePoints { get; set; } = new List<XYZ>();
        public List<XYZ> UnpassablePoints { get; set; } = new List<XYZ>();


        public void Separate()
        {
            Queue<XYZ> queue = new Queue<XYZ>();
            _spacePoints.ForEach(p => queue.Enqueue(p));

            List<XYZ> list = new List<XYZ>();
            while (queue.Count > 0)
            {
                XYZ p = queue.Dequeue();
                if (_outLines.Any(s => s.Contains(p, 0)))
                { list.Add(p); }
            }

            queue = new Queue<XYZ>();
            list.ForEach(p => queue.Enqueue(p));
            while (queue.Count > 0) 
            {
                XYZ p = queue.Dequeue();    
                if (_solids.TrueForAll(s => !s.Contains(p)))
                { UnpassablePoints.Add(p); }
                else { PassablePoints.Add(p); }
            }
        }
    }
}
