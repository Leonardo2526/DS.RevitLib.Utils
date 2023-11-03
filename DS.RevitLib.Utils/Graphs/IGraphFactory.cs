using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Graphs
{
    public interface IGraphFactory<TGraph, TDataSource>
    {
        public TGraph Graph { get; }

        TGraph Create(TDataSource dataSource);
    }

    public abstract class MEPSystemGraphFactoryBase<TGraph> : IGraphFactory<TGraph, Element>
    {
        protected readonly Document _doc;
        protected TGraph _graph;

        public MEPSystemGraphFactoryBase(Document doc)
        {
            _doc = doc;
        }
        public TGraph Graph => _graph;
       

        public abstract TGraph Create(Element element);
    }
}
