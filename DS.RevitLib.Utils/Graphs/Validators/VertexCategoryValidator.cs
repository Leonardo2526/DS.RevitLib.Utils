﻿using Autodesk.Revit.DB;
using DS.ClassLib.VarUtils;
using DS.GraphUtils.Entities;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.Selections.Validators;
using QuickGraph;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DS.RevitLib.Utils.Graphs
{
    /// <summary>
    /// Validator to check <see cref="IVertex"/> categories.
    /// </summary>
    public class VertexCategoryValidator : XYZElementCategoryValidator, IValidator<IVertex>
    {
        private readonly IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> _graph;

        /// <summary>
        /// Instansiate validator to check <see cref="IVertex"/> categories.
        /// </summary>
        public VertexCategoryValidator(Document doc, 
            Dictionary<BuiltInCategory, List<PartType>> availableCategories,
            IVertexAndEdgeListGraph<IVertex, Edge<IVertex>> graph) : 
            base(doc, availableCategories)
        {
            _graph = graph;
        }

        /// <inheritdoc/>
        public bool IsValid(IVertex value)
        {
            var pointElement = value.ToXYZElement(_doc);
            return Validate(new ValidationContext(pointElement)).Count() == 0;
        }
    }
}
