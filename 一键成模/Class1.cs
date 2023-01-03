using Autodesk.AutoCAD.Runtime;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace 一键成模
{
    public class Class1
    {
        [CommandMethod("Ribbon")]
        public static void Ribbon()
        {
            RibbonControl ribbonCtrl = ComponentManager.Ribbon;
            RibbonTab tab = new RibbonTab();
            tab.Title = "我的Ribbon界面";
            tab.Id = "ACAD.My_RibbonTab";
            ribbonCtrl.Tabs.Add(tab);
            tab.IsActive = true;
        }

    }
}
