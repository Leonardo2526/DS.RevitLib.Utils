using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Solids;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="geomOptions"></param>
        /// <param name="includeInserts"></param>
        /// <returns></returns>
        public static (List<Face> wallFaces, Dictionary<ElementId, List<Face>> insertsFaces) GetFaces(
           this Wall wall, Options geomOptions = null, bool includeInserts = false)
        {
            var insertsFaces = new Dictionary<ElementId, List<Face>>();

            List<Face> faceList = new List<Face>();
            List<Solid> solidList = SolidExtractor.GetSolids(wall, null, geomOptions);

            var insertsIds = includeInserts ? wall.FindInserts(true, false, true, true) : null;

            foreach (Solid solid in solidList)
            {
                foreach (Face face in solid.Faces)
                {
                    if (includeInserts)
                    { 
                       if(!tryAddInserts(wall, insertsFaces, insertsIds, face))
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
    }

}
