using Autodesk.Revit.DB;

namespace DS.RevitLib.Utils.Extensions
{
   
    public static class CategoryExtension

    { 
        /// <summary>
      ///Get BuiltInCategory of current category
      /// </summary>
        public static BuiltInCategory GetBuiltInCategory(this Category category)
        {
            if (System.Enum.IsDefined(typeof(BuiltInCategory),
                                          category.Id.IntegerValue))
            {
                var builtInCategory = (BuiltInCategory)category.Id.IntegerValue;
                return builtInCategory;
            }

            return BuiltInCategory.INVALID;
        }
    }
}
