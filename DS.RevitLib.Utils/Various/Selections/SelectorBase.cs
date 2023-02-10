using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Various
{
    /// <summary>
    /// An object that retrieve the currently selected <typeparamref name="T"/> in Autodesk Revit.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SelectorBase<T>
    {
        protected readonly UIDocument _uiDoc;
        protected readonly Document _doc;

        /// <summary>
        /// Instantiate an object to retrieve the currently selected <typeparamref name="T"/> in Autodesk Revit.
        /// </summary>
        /// <param name="uiDoc"></param>
        protected SelectorBase(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
        }


        /// <summary>
        /// Specify if it's allowed to select <see cref="RevitLinkInstance"/>.
        /// </summary>
        public bool AllowLink { get; set; } = true;

        /// <summary>
        /// Filter for picking elements.
        /// </summary>
        protected abstract ISelectionFilter Filter { get; }

        /// <summary>
        /// Filter for picking elements in link.
        /// </summary>
        protected abstract ISelectionFilter FilterInLink { get; }

        /// <summary>
        /// Inititate object selection by picking in Autodesk Revit model.
        /// </summary>
        /// <param name="statusPrompt"></param>
        /// <param name="promptSuffix"></param>
        /// <returns>Returns an <typeparamref name="T"/> that represents the active selection.</returns>
        public abstract T Pick(string statusPrompt = null, string promptSuffix = null);

        /// <summary>
        /// Set selection by <paramref name="elements"/>. 
        /// </summary>
        /// <param name="elements"></param>
        public abstract void Set(List<T> elements);


        /// <summary>
        /// Build string for status prompt.
        /// </summary>
        /// <param name="statusPrompt"></param>
        /// <param name="promptSuffix"></param>
        /// <returns>Returns <see cref="string"/> statusPrompt to PickObject.</returns>
        protected string GetStatusPrompt(string statusPrompt = null, string promptSuffix = null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(statusPrompt ?? $"Select {typeof(T).Name}");
            stringBuilder.AppendLine(promptSuffix is null ? null : promptSuffix);

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Inititate element selection in <paramref name="link"/> by picking it in Autodesk Revit model.
        /// </summary>
        /// <param name="link"></param>
        /// <returns>Returns element in <paramref name="link"/>.</returns>
        protected Element PickInLink(RevitLinkInstance link)
        {
            Reference refElemLinked = 
                _uiDoc.Selection.PickObject(ObjectType.LinkedElement, FilterInLink, $"Please pick an element in link: {link.Name}");
            return link.GetLinkDocument().GetElement(refElemLinked.LinkedElementId);
        }
    }
}
