using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Solids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DS.RevitLib.Utils.Solids;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.Lines;
using Rhino.Geometry;
using DS.ClassLib.VarUtils;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Wall"/>.
    /// </summary>
    public static class WallExtensions
    {
        /// <summary>
        /// Get <paramref name="wall"/> <see cref="Autodesk.Revit.DB.Face"/>s and inserts <see cref="Autodesk.Revit.DB.Face"/>s
        /// if <paramref name="includeInserts"/> is <see langword="true"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="activeDoc"></param>
        /// <param name="geomOptions"></param>
        /// <param name="includeInserts"></param>
        /// <returns></returns>
        public static (List<Face> wallFaces, Dictionary<ElementId, List<Face>> insertsFacesCollection) GetFaces(
           this Wall wall, Document activeDoc, Options geomOptions = null, bool includeInserts = false)
        {
            var insertsFaces = new Dictionary<ElementId, List<Face>>();
            var faceList = new List<Face>();

            List<Solid> solidList = wall.Document.IsLinked ?
                wall.GetTransformedSolids(wall.GetLink(activeDoc)) :
                SolidExtractor.GetSolids(wall, null, geomOptions);

            var insertsIds = includeInserts ? wall.FindInserts(true, false, true, true) : null;

            foreach (Solid solid in solidList)
            {
                foreach (Face face in solid.Faces)
                {
                    if (includeInserts)
                    {
                        if (!tryAddInserts(wall, insertsFaces, insertsIds, face))
                        { faceList.Add(face); }
                    }
                    else
                    { faceList.Add(face); }
                }
            }
            return (faceList, insertsFaces);

            static bool tryAddInserts(Wall wall,
                Dictionary<ElementId, List<Face>> insertsFaces, IList<ElementId> insertsIds,
                Face face)
            {
                var inserted = false;
                var genIds = wall.GetGeneratingElementIds(face);
                foreach (var gId in genIds)
                {
                    if (insertsIds.Contains(gId))
                    {
                        //add to dict
                        if (insertsFaces.TryGetValue(gId, out var valueFaces))
                        { valueFaces.Add(face); }
                        else
                        { insertsFaces.Add(gId, new List<Face>() { face }); }
                        inserted = true;
                    }
                }
                return inserted;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wall"></param>
        /// <returns></returns>
        public static PlanarFace GetMainPlanarFace(this Wall wall, Document activeDoc)
        {
            var geomOptions = new Options()
            {
                ComputeReferences = true, // expensive, avoid if not needed
                DetailLevel = ViewDetailLevel.Fine,
                IncludeNonVisibleObjects = false
            };
            (List<Face> wallFaces, Dictionary<ElementId, List<Face>> insertsFacesDist) = GetFaces(wall, activeDoc, geomOptions, false);

            return wallFaces.OfType<PlanarFace>().OrderByDescending(f => f.Area).First();
        }

        /// <summary>
        /// Get all walls joint to <paramref name="wall"/>.
        /// </summary>
        /// <param name="wall"></param>
        /// <returns>
        /// List of all walls joint to <paramref name="wall"/>.
        /// <para>
        /// Empty list if no jounts was found.
        /// </para>
        /// </returns>
        public static IEnumerable<ElementId> GetJoints(this Wall wall)
        {
            var result = new List<ElementId>();
            var locationCurve = wall.Location as LocationCurve;
            int i = 0;
            int emptyResults = 0;
            while (emptyResults < 3)
            {
                //break;
                IEnumerable<ElementId> joints;
                try
                { joints = GetAdjoiningElements(locationCurve, wall.Id, i); }
                catch (Exception)
                { break; }
                if (joints is not null && joints.Count() > 0)
                { result.AddRange(joints); }
                else { emptyResults ++; }
                i++;
            }
            return result;

            static List<ElementId> GetAdjoiningElements(
                LocationCurve locationCurve,
                ElementId wallId,
                int index)
            {
                var result = new List<ElementId>();
                ElementArray a = locationCurve.get_ElementsAtJoin(index);
                foreach (Element element in a)
                    if (element.Id != wallId)
                        result.Add(element.Id);
                return result;
            }
        }

        public static Autodesk.Revit.DB.Line GetHeightLine(this Wall wall, Document activeDoc)
        {
            PlanarFace planarFace = GetMainPlanarFace(wall, activeDoc);
            var curveLoops = planarFace.GetEdgesAsCurveLoops();

            var at = 3.DegToRad();
            foreach (var loop in curveLoops)
            {
                foreach (var item in loop)
                {
                    var line = item as Autodesk.Revit.DB.Line;
                    if (line != null)
                    {
                        var rLine = line.ToRhinoLine();
                        if (rLine.Direction.IsParallelTo(Vector3d.ZAxis, at) != 0)
                        { return line; }
                    }
                }
            }

            return null;
        }
    }

}
