using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
        protected readonly Type _type;
        protected string _defaultStatusPrompt;

        /// <summary>
        /// Instantiate an object to retrieve the currently selected <typeparamref name="T"/> in Autodesk Revit.
        /// </summary>
        /// <param name="uiDoc"></param>
        protected SelectorBase(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
            _defaultStatusPrompt = $"Select {typeof(T).Name}";
        }

        /// <summary>
        /// Inititate object selection in Autodesk Revit model.
        /// </summary>
        /// <param name="statusPrompt"></param>
        /// <param name="promptSuffix"></param>
        /// <returns>Returns an <typeparamref name="T"/> that represents the active selection.</returns>
        public abstract T Select(string statusPrompt = null, string promptSuffix = null);

        /// <summary>
        /// Build string for status prompt.
        /// </summary>
        /// <param name="statusPrompt"></param>
        /// <param name="promptSuffix"></param>
        /// <returns>Returns <see cref="string"/> statusPrompt to PickObject.</returns>
        protected string GetStatusPrompt(string statusPrompt = null, string promptSuffix = null)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(statusPrompt ?? _defaultStatusPrompt);
            stringBuilder.AppendLine(promptSuffix is null ? null : promptSuffix);

            return stringBuilder.ToString();
        }
    }
}
