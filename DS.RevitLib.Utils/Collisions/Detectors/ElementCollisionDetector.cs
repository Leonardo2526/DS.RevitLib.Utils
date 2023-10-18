using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DS.RevitLib.Utils.Collisions.Detectors
{
    /// <summary>
    /// An object to find collisions (intersections) between <see cref="object"/>'s and <see cref="Autodesk.Revit.DB.Element"/>'s.
    /// </summary>
    public class ElementCollisionDetector : IElementCollisionDetector
    {
        private readonly Document _activeDocument;
        private readonly IElementIntersectionFactory _intersectionFactory;
        private readonly List<(RevitLinkInstance, Transform, List<Element>)> _loadedLinksDict = new();

        /// <summary>
        /// Instantiate a new object to find collisions (intersections) between <see cref="object"/>'s and <see cref="Autodesk.Revit.DB.Element"/>'s.
        /// </summary>
        /// <param name="activeDocument"></param>
        /// <param name="intersectionFactory">Factory to find intersecrtions between elements.</param>
        public ElementCollisionDetector(Document activeDocument, IElementIntersectionFactory intersectionFactory)
        {
            _activeDocument = activeDocument;
            _intersectionFactory = intersectionFactory;
            var loadedLinks = activeDocument.GetLoadedLinks() ?? new List<RevitLinkInstance>();

            foreach (var link in loadedLinks)
            {
                (RevitLinkInstance, Transform, List<Element>) model = (link, link.GetLinkTransform(), null);
                _loadedLinksDict.Add(model);
            }
        }


        #region Properties

        /// <inheritdoc/>
        public List<(object, Element)> Collisions { get; } = new List<(object, Element)>();

        /// <inheritdoc/>
        public double MinVolume { get; set; }

        /// <inheritdoc/>
        public List<BuiltInCategory> ExculdedCategories
        {
            get => _intersectionFactory.ExculdedCategories;
            set => _intersectionFactory.ExculdedCategories = value;
        }

        /// <inheritdoc/>
        public List<Type> ExculdedTypes
        {
            get => _intersectionFactory.ExculdedTypes;
            set => _intersectionFactory.ExculdedTypes = value;
        }

        /// <inheritdoc/>
        public List<Element> ExcludedElements
        {
            get => _intersectionFactory.ExcludedElements;
            set => _intersectionFactory.ExcludedElements = value;
        }

        /// <inheritdoc/>
        public List<Element> ActiveDocElements { get; set; }

        /// <inheritdoc/>
        public List<(RevitLinkInstance, Transform, List<Element>)> LinkElements { get; set; }

        /// <inheritdoc/>
        public bool IsInsulationAccount
        {
            get => _intersectionFactory.IsInsulationAccount;
            set
            {
                if (value)
                {
                    _intersectionFactory.ExculdedTypes = _intersectionFactory.ExculdedTypes.
                            Where(t => t.Name != typeof(InsulationLiningBase).Name).ToList();
                }
                else if (_intersectionFactory.IsInsulationAccount)
                {
                    _intersectionFactory.ExculdedTypes.Add(typeof(InsulationLiningBase));
                }

            }
        }

        #endregion


        /// <inheritdoc/>
        public List<(Element, Element)> GetCollisions(Element checkObject) => GetObjectCollisions(checkObject);

        /// <inheritdoc/>
        public List<(Solid, Element)> GetCollisions(Solid checkObject) => GetObjectCollisions(checkObject);


        #region PrivateMethods

        private List<(T, Element)> GetObjectCollisions<T>(T checkObject)
        {
            Collisions.Clear();

            var collisions = new List<(T, Element)>();
            var activeModel = (_activeDocument, ActiveDocElements);
            var activeDocCollisions = GetDocCollisions(activeModel, checkObject);

            Collisions.AddRange(activeDocCollisions);

            var links = LinkElements ?? _loadedLinksDict;

            foreach (var link in links)
            {
                var linkCollisions = GetLinkCollisions(link, checkObject);
                Collisions.AddRange(linkCollisions);
            }

            Collisions.ForEach(c => collisions.Add(((T)c.Item1, c.Item2)));
            return collisions;
        }

        private List<(object, Element)> GetDocCollisions((Document, List<Element>) checkModel2, object checkObject)
        {
            _intersectionFactory.Build(checkModel2);
            return GetModelCollisions(checkObject);
        }

        private List<(object, Element)> GetLinkCollisions((RevitLinkInstance, Transform, List<Element>) checkModel2, object checkObject)
        {
            _intersectionFactory.Build(checkModel2);
            return GetModelCollisions(checkObject);
        }

        private List<(object, Element)> GetModelCollisions(object checkObject)
        {
            List<Element> intersectionElements = GetIntersections(_intersectionFactory, checkObject);

            var collisions = new List<(object, Element)>();
            bool isChecked = true;
            foreach (var e in intersectionElements)
            {
                if (MinVolume != 0)
                {
                    if (checkObject is Element element)
                    {
                        var collision = (element, e);
                        if (collision.GetIntersectionSolid(MinVolume) == null)
                        { isChecked = false; }
                    }
                    else if (checkObject is Solid solid)
                    {
                        var collision = (solid, e);
                        if (collision.GetIntersectionSolid(_activeDocument, MinVolume) == null)
                        { isChecked = false; }
                    }
                }
                if (isChecked) { collisions.Add((checkObject, e)); }
            }

            return collisions;

            List<Element> GetIntersections(IElementIntersectionFactory factory, object checkObject)
            {
                List<Element> intersectionElements = null;
                if (checkObject is Element element)
                { intersectionElements = factory.GetIntersections(element); }
                else if (checkObject is Solid solid)
                { intersectionElements = factory.GetIntersections(solid); }

                return intersectionElements;
            }

        }

        #endregion

    }
}
