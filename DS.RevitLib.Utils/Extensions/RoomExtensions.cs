﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using DS.ClassLib.VarUtils.Collisons;
using DS.RevitLib.Utils.Collisions.Detectors;
using DS.RevitLib.Utils.Collisions.Models;
using Rhino.UI;

namespace DS.RevitLib.Utils.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Room"/>.
    /// </summary>
    public static class RoomExtensions
    {
        /// <summary>
        /// Get the elements that the <paramref name="room"/> contains.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="activeDoc"></param>
        /// <param name="elementIntersectionFactory"></param>
        /// <returns>
        /// The list of <see cref="Autodesk.Revit.DB.Element"/> inside <paramref name="room"/>.
        /// <para>
        /// Empty list if no elements exist inside <paramref name="room"/>.
        /// </para>
        /// </returns>
        public static IEnumerable<Element> GetInsideElements(this Room room, 
            Document activeDoc, 
            ITIntersectionFactory<Element, Solid> elementIntersectionFactory)
        {           
            var roomSolid = room.GetSolidInLink(activeDoc);
            return elementIntersectionFactory.GetIntersections(roomSolid);
        }

        /// <summary>
        /// Check if <paramref name="room"/> contains <paramref name="checkSolid"/>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="room"/> is in <see cref="RevitLinkInstance"/>, it's solid will be transformed automatically.
        /// </remarks>
        /// <param name="room"></param>
        /// <param name="checkSolid"></param>
        /// <param name="activeDoc"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="room"/> solid intersect with <paramref name="checkSolid"/>.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool Contains(this Room room, Solid checkSolid, Document activeDoc)
        {
            var roomSolid = room.GetSolidInLink(activeDoc);
            var solid = DS.RevitLib.Utils.Solids.SolidUtils.GetIntersection(roomSolid, checkSolid);

            return solid != null && solid.Volume > 0.001;
        }

        /// <summary>
        /// Check if <paramref name="room"/> contains <paramref name="point"/>.
        /// </summary>
        /// <remarks>
        /// If <paramref name="room"/> is in <see cref="RevitLinkInstance"/> <paramref name="point"/> will be transformed automatically.
        /// </remarks>
        /// <param name="room"></param>
        /// <param name="activeDoc"></param>
        /// <param name="point"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="room"/> contains <paramref name="point"/> or its transformed instance.
        /// <para>
        /// Otherwise <see langword="false"/>.
        /// </para>
        /// </returns>
        public static bool IsPointInLinkRoom(this Room room, XYZ point, Document activeDoc)
        {
            var revitLink = room.GetLink(activeDoc);
            var linkTransform = revitLink?.GetTotalTransform();
            var checkPoint = linkTransform is null || linkTransform.AlmostEqual(Transform.Identity) ? 
                point : 
                linkTransform.Inverse.OfPoint(point);

            return room.IsPointInRoom(checkPoint);
        }
    }
}
