using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Specialized;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

using Autodesk.AutoCAD.Interop;  //引用需要添加 Autodesk.AutoCAD.Interop.dll
using System.IO;

[assembly: ExtensionApplication(typeof(GLTools.MenuTools))]  //启动时加载工具栏，注意typeof括号里的类库名

namespace GLTools
{
    //添加项目类引用
    public class MenuTools: Autodesk.AutoCAD.Runtime.IExtensionApplication
    {
        //重写初始化
        public void Initialize()
        {
            //加载后的初始化程序
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage("\n加载程序中...........\n");
            //加载菜单栏
            AddMenu();
        }

        //重写结束
        public void Terminate()
        {
            // do somehing to cleanup resource
        }

        //加载菜单栏
        public void AddMenu()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            doc.Editor.WriteMessage("添加菜单栏成功！>>>>>>>>>>>>>>");
            AcadApplication acadApp = Application.AcadApplication as AcadApplication;

            //创建建菜单栏的对象
            AcadPopupMenu myMenu = null;

            // 创建菜单
            if (myMenu == null)
            {
                // 菜单名称
                myMenu = acadApp.MenuGroups.Item(0).Menus.Add("管廊纵断面工具");

                myMenu.AddMenuItem(myMenu.Count, "测试程序", "CSCS ");
                myMenu.AddMenuItem(myMenu.Count, "标高及桩号初始化", "GLCSH ");
                myMenu.AddMenuItem(myMenu.Count, "两行数值相减", "SZXJ ");
                myMenu.AddMenuItem(myMenu.Count, "生成节点桩号", "JDZH ");
                myMenu.AddMenuItem(myMenu.Count, "拾取线生成标高", "QXBG ");
                myMenu.AddMenuItem(myMenu.Count, "选点生成桩号及标高", "QDZHBG ");

                myMenu.AddSeparator(myMenu.Count); //加入分割符号

                //开始加子菜单栏
                AcadPopupMenu subMenu = myMenu.AddSubMenu(myMenu.Count, "图号重排");  //子菜单对象
                subMenu.AddMenuItem(myMenu.Count, "按行重排", "THCP R ");
                subMenu.AddMenuItem(myMenu.Count, "按列重排", "THCP C ");
                
                //结束加子菜单栏

            }

            // 菜单是否显示  看看已经显示的菜单栏里面有没有这一栏
            bool isShowed = false;  //初始化没有显示
            foreach (AcadPopupMenu menu in acadApp.MenuBar)  //遍历现有所有菜单栏
            {
                if (menu == myMenu)
                {
                    isShowed = true;
                    break;
                }
            }

            // 显示菜单 加载自定义的菜单栏
            if (!isShowed)
            {
                myMenu.InsertInMenuBar(acadApp.MenuBar.Count);
            }
        }

    }
}
