﻿using Autodesk.Revit.DB;
using System;

namespace DS.RevitLib.Utils
{
    public static class ElementsMover
    {

        public static Element MoveElement(Element element, XYZ vector)
        {
            Document Doc = element.Document;

            using (Transaction transNew = new Transaction(Doc, "MoveElement"))
            {
                try
                {
                    transNew.Start();
                    ElementTransformUtils.MoveElement(Doc, element.Id, vector);
                }

                catch (Exception e)
                { return null; }

                if (transNew.HasStarted())
                {
                    transNew.Commit();
                }
            }

            return element;
        }
    }
}
