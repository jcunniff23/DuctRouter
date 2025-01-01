using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace DuctRouter
{
    public class TerminalSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category.Id.Value == (int)BuiltInCategory.OST_DuctTerminal;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
