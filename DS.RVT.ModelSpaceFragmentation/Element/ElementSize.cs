using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

namespace DS.RVT.ModelSpaceFragmentation
{

    /// <summary>
    /// Get sizes of Elem1 and Elem2 and set class properties
    /// </summary>
    class ElementSize
    {
      
        public static double ElemDiameterF { get; set; }

        public void GetElementSizes(MEPCurve elMEPCurve)
        {
            string type = elMEPCurve.GetType().ToString();

            //Get element sizes
            if (type.Contains("Pipe"))
            {
                Pipe pipe = elMEPCurve as Pipe;

                ElemDiameterF = pipe.get_Parameter(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER).AsDouble();         

            }
        }
    }
}
