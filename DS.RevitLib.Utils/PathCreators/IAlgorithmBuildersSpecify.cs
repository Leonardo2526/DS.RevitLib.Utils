using Autodesk.Revit.DB;
using DS.PathFinder.Algorithms.AStar;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Connections.PointModels;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Geometry;
using DS.RevitLib.Utils.PathCreators.AlgorithmBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.PathCreators
{

    #region Interfaces


    /// <summary>
    /// The interface to specify exclusions.
    /// </summary>
    public interface ISpecifyExclusions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectsToExclude"></param>
        /// <param name="exludedCathegories"></param>
        /// <returns></returns>
        ISpecifyToken SetExclusions(List<Element> objectsToExclude,
        List<BuiltInCategory> exludedCathegories);
    }

    /// <summary>
    /// The interface to specify point converter.
    /// </summary>
    public interface ISpecifyToken
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="externalCancellationToken"></param>
        /// <returns></returns>
        ISpecifyConnectionPointBoundaries SetExternalToken1(CancellationTokenSource externalCancellationToken);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="externalCancellationToken"></param>
        /// <returns></returns>
        ISpecifyVertexBoundaries SetExternalToken2(CancellationTokenSource externalCancellationToken);
    }    

    /// <summary>
    /// The interface to specify parameters.
    /// </summary>
    public interface ISpecifyParameter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ISpecifyParameter SetVisualisator();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        ISpecifyParameter SetNodeBuilder();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collisionDetector"></param>
        /// <param name="insulationAccount"></param>
        /// <returns></returns>
        ISpecifyParameter SetCollisionDetector(IElementCollisionDetector collisionDetector, bool insulationAccount,
            IElementsExtractor elementsExtractor);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="planeTypes"></param>
        /// <returns></returns>
        ISpecifyParameter SetDirectionIterator();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IBuildAlgorithm SetSearchLimit();
    }

    /// <summary>
    /// The interface to build algorithm.
    /// </summary>
    public interface IBuildAlgorithm
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        AStarAlgorithmCDF Build(bool minimizePathNodes = false);
    }

    #endregion
}
