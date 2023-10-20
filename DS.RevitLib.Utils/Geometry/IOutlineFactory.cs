﻿using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Connections.PointModels;

namespace DS.RevitLib.Utils.Geometry
{
    /// <summary>
    /// The interface is used to create <see cref="Autodesk.Revit.DB.Outline"/>. 
    /// </summary>
    public interface IOutlineFactory
    {
        /// <summary>
        /// Create <see cref="Autodesk.Revit.DB.Outline"/> by <paramref name="startPoint"/> and <paramref name="endPoint"/>.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <returns>
        /// <see cref="Autodesk.Revit.DB.Outline"/> if <paramref name="startPoint"/> and <paramref name="endPoint"/> are valid.
        /// <para>
        /// Otherwise returns <see langword="null"/>.
        /// </para>
        /// </returns>      
        Outline Create(ConnectionPoint startPoint, ConnectionPoint endPoint);
    }
}