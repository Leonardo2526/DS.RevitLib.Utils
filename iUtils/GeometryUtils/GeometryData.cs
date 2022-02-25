using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtils
{
    public class GeometryData
    {
        public const string HorizontalPlanarFaces = "HorizontalPlanarFaces";
        public const string OtherPlanarFaces = "OtherPlanarFaces";
        public const string VerticalPlanarFaces = "VerticalPlanarFaces";
        public const string OtherCylindricalFaces = "OtherCylindricalFaces";
        public const string VerticalCylindricalFaces = "VerticalCylindricalFaces";
        public const string OtherFaces = "OtherFaces";
        public const string Allfaces = "AllFaces";
        private bool _getFaces;
        public Transform Transform { get; }
        public Element Element { get; private set; }
        public Dictionary<string, List<Face>> faces { get; private set; }
        public List<Solid> Solids { get; set; }
        public BoundingBoxXYZ BoundingBox { get; set; }
        //public XYZ CenterByBoundingBox { get; set; }
        public GeometryData(Element element, Transform transform = null, bool getFaces = true)
        {
            _getFaces = getFaces;
            BoundingBox = element.get_BoundingBox(null);
            this.Transform = transform;
            if(transform!=null & BoundingBox != null)
            {
                BoundingBox.Transform = transform;
            }
            //if (transform != null)
            //{
            //    this.transform = transform;
            //    CenterByBoundingBox = transform.OfPoint((element.get_BoundingBox(null).Max + element.get_BoundingBox(null).Min) / 2);
            //}
            //else
            //{
            //    CenterByBoundingBox = (element.get_BoundingBox(null).Max + element.get_BoundingBox(null).Min) / 2;
            //}
            Element = element;
            Solids = new List<Solid>();
            faces = GetFaces(element);
        }


        
        private Dictionary<string, List<Face>> GetFaces(Element e)
        {
            Options options = new Options() { DetailLevel = ViewDetailLevel.Fine, IncludeNonVisibleObjects = true, ComputeReferences = true };

            var geometryElement = e.get_Geometry(options).GetTransformed(Autodesk.Revit.DB.Transform.Identity);

            List<Face> HorizontalPlanarFaces = new List<Face>();
            List<Face> VerticalPlanarFaces = new List<Face>();
            List<Face> OtherPlanarFaces = new List<Face>();
            List<Face> OtherCylindricalFaces = new List<Face>();
            List<Face> VerticalCylindricalFaces = new List<Face>();
            List<Face> OtherFaces = new List<Face>();
            List<Face> AllFaces = new List<Face>();


            if (geometryElement != null)
            {
                foreach (GeometryObject item in geometryElement)
                {
                    if (item is Solid solid)
                    {
                        if(solid.Volume>1e-9)
                        {
                            if (null != Transform)
                            {
                                solid = SolidUtils.CreateTransformed(solid, Transform);
                            }
                            Solids.Add(solid);

                            if (_getFaces)
                            {
                                foreach (Face face in solid.Faces)
                                {
                                    if (face is CylindricalFace cface)
                                    {
                                        if (_JUtil.IsVertical(cface))
                                            VerticalCylindricalFaces.Add(cface);
                                        else
                                            OtherCylindricalFaces.Add(cface);
                                    }
                                    else if (face is PlanarFace pface)
                                    {
                                        if (_JUtil.IsHorizontal(pface))
                                            HorizontalPlanarFaces.Add(pface);
                                        else if (_JUtil.IsVertical(pface))
                                            VerticalPlanarFaces.Add(pface);
                                        else
                                            OtherPlanarFaces.Add(pface);
                                    }
                                    else
                                        OtherFaces.Add(face);

                                    AllFaces.Add(face);
                                }
                            }
                        } 
                    }
                }
            }
            if (_getFaces)
            {
                var result = new Dictionary<string, List<Face>>
            {
                { GeometryData.HorizontalPlanarFaces, HorizontalPlanarFaces },
                { GeometryData.VerticalPlanarFaces, VerticalPlanarFaces },
                { GeometryData.OtherPlanarFaces, OtherPlanarFaces },
                { GeometryData.OtherCylindricalFaces, OtherCylindricalFaces },
                { GeometryData.VerticalCylindricalFaces, VerticalCylindricalFaces },
                { GeometryData.OtherFaces, OtherFaces },
                { GeometryData.Allfaces, AllFaces }
            };
                return result;
            }
            return null;
        }



         
    }
}
