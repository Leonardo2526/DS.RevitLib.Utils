using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitLib.Utils.Elements
{
    public enum CopyParameterOption
    {
        /// <summary>
        /// Copy all parameters
        /// </summary>
        All, 

        /// <summary>
        /// Copy only size parameters like diameter, width or height
        /// </summary>
        Sizes
    }
}
