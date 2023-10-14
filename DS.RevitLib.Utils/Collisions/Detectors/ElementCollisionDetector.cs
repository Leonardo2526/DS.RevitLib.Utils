using Autodesk.Revit.DB;
using DS.RevitLib.Utils.Collisions.Detectors.AbstractDetectors;
using DS.RevitLib.Utils.Collisions.Models;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
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
        private IElementsExtractor _elementsExtractor;
        private List<Element> _activeDocElements;
        private Dictionary<RevitLinkInstance, List<Element>> _linkElements;
        private bool _isInsulationAccount;

        /// <summary>
        /// Instantiate a new object to find collisions (intersections) between <see cref="object"/>'s and <see cref="Autodesk.Revit.DB.Element"/>'s.
        /// </summary>
        /// <param name="activeDocument"></param>
        /// <param name="intersectionFactory">Factory to find intersecrtions between elements.</param>
        /// <param name="elementsExtractor">Object to get documents elements.</param>
        public ElementCollisionDetector(Document activeDocument, IElementIntersectionFactory intersectionFactory,
            IElementsExtractor elementsExtractor = null)
        {
            _activeDocument = activeDocument;
            _intersectionFactory = intersectionFactory;
            _elementsExtractor = elementsExtractor;
        }

        /// <summary>
        /// Elements in active document.
        /// </summary>
        public List<Element> ActiveDocElements
        {
            get => _activeDocElements ??= ElementsExtractor.ModelElements ?? ElementsExtractor.GetFromDoc();
            set => _activeDocElements = value;
        }

        /// <summary>
        /// Elements in all loaded links.
        /// </summary>
        public Dictionary<RevitLinkInstance, List<Element>> LinkElements
        {
            get => _linkElements ??= ElementsExtractor.LinkElements ?? ElementsExtractor.GetFromLinks();
            set => _linkElements = value;
        }

        /// <inheritdoc/>
        public List<(object, Element)> Collisions { get; } = new List<(object, Element)>();

        /// <inheritdoc/>
        public double MinVolume { get; set; }

        /// <inheritdoc/>
        public List<Element> ExludedElements { get; set; }


        #region PrivateMethods

        private IElementsExtractor ElementsExtractor =>
            _elementsExtractor ??= new GeometryElementsExtractor(_activeDocument);

        /// <inheritdoc/>
        public bool IsInsulationAccount 
        { 
            get => _isInsulationAccount;
            set 
            { 
                _isInsulationAccount = value;
                if(_intersectionFactory is ElementIntersectionFactory factory)
                {
                    if (_isInsulationAccount)
                    {
                        factory.ExculdedTypes = factory.ExculdedTypes.
                            Where(t=> t.Name != typeof(InsulationLiningBase).Name).ToList();
                    }
                    else 
                    {
                        factory.ExculdedTypes.Add(typeof(InsulationLiningBase));
                    }
                }
            } 
        }

        /// <inheritdoc/>
        public List<(Element, Element)> GetCollisions(Element checkObject) => GetObjectCollisions(checkObject);

        /// <inheritdoc/>
        public List<(Solid, Element)> GetCollisions(Solid checkObject) => GetObjectCollisions(checkObject);

        private List<(T, Element)> GetObjectCollisions<T>(T checkObject)
        {
            Collisions.Clear();

            var collisions = new List<(T, Element)>();
            var activeModel = (_activeDocument, ActiveDocElements);
            var activeDocCollisions = GetDocCollisions(activeModel, checkObject);

            Collisions.AddRange(activeDocCollisions);

            foreach (var linkElem in LinkElements)
            {
                var linkModel = (linkElem.Key.GetLinkDocument(), linkElem.Value);
                var linkCollisions = GetDocCollisions(linkModel, checkObject);
                Collisions.AddRange(linkCollisions);
            }

            Collisions.ForEach(c => collisions.Add(((T)c.Item1, c.Item2)));
            return collisions;
        }

        private List<(object, Element)> GetDocCollisions((Document, List<Element>) checkModel2, object checkObject)
        {
            if (checkModel2.Item2.Count == 0) { return new List<(object, Element)>(); }

            _intersectionFactory.Build(checkModel2);
            _intersectionFactory.ExcludedElements = ExludedElements;
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

        /// <inheritdoc/>
        public void Rebuild(List<Element> activeDocElements)
        {
            _activeDocElements = activeDocElements;
        }

        #endregion

    }
}
