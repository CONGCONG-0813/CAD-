using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAD工具.Tool
{
    public static partial class RibbonTool
    {
        public static void AddRibbonTab(this RibbonControl ribbonControl,string Title,string ID,bool IsActive)
        {
            RibbonTab tab = new RibbonTab();
            tab.Title = Title;
            tab.Id = ID;
            ribbonControl.Tabs.Add(tab);
            tab.IsActive = IsActive;


        }
    }
}
