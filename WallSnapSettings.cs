using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EpicWallBox.UI.View;
using EpicWallBox.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static EpicWallBox.SettingsSchema_WallSnap;

namespace EpicWallBox
{
    [Transaction(TransactionMode.Manual)]
    internal class WallSnapSettings : HelperOps, IExternalCommand
    {
        UIApplication uiapp;
        UIDocument uidoc;
        Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic WallSnap Settings");

            SettingsData MySettingStorage = new SettingsData();
            SettingsObj MySettings = MySettingStorage.Get(doc);

            if (MySettings == null)
            {
                // Default Values
                MySettings = new SettingsObj()
                {
                    DistanceFwd = 1500,
                    DistanceRev = 100,
                    ViewName = "EpicC",
                    UseBoxOffset = false,
                    ScBoxOffsetX = 0,
                    ScBoxOffsetY = 0,
                    UseBoundingBox = false
                };
            }


            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<ElementId> linkedDocIdSet =
              collector
              .OfCategory(BuiltInCategory.OST_RvtLinks)
              .OfClass(typeof(RevitLinkType))
              .ToElementIds();


            List<Document> linkedDocs = new List<Document>();
            List<RevitLinkType> linkedDocTypes = new List<RevitLinkType>();

            foreach (ElementId linkedFileId in linkedDocIdSet)
            {
                RevitLinkType link = doc.GetElement(linkedFileId) as RevitLinkType;
                linkedDocTypes.Add(link);

            }

            foreach (Document LinkedDoc in uiapp.Application.Documents)
            {
                if (LinkedDoc.IsLinked)
                {
                    linkedDocs.Add(LinkedDoc);
                }
            }


            // UI
            Window uiWin = new Window();
            uiWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            uiWin.Width = 420; uiWin.Height = 300;
            uiWin.ResizeMode = ResizeMode.NoResize;
            uiWin.Title = "Epic BoxGen Settings";

            WallSnapSettingsVM uiData = new WallSnapSettingsVM()
            {
                DistanceRev = MySettings.DistanceRev,
                DistanceFwd = MySettings.DistanceFwd,
                CollisionViewName = MySettings.ViewName,
                CollisionLinks = linkedDocTypes,
                UseBoxOffset = MySettings.UseBoxOffset,
                ScBoxOffsetX = MySettings.ScBoxOffsetX,
                ScBoxOffsetY = MySettings.ScBoxOffsetY,
                UseBoundingBox = MySettings.UseBoundingBox,
            };

            int ind = 0;
            foreach (var link in linkedDocTypes)
            {
                if (MySettings.LinkId == link.Id)
                {
                    ind = linkedDocTypes.IndexOf(link);
                    break;
                }
            }
            uiData.SelectedIndex = ind;

            uiData.OnRequestClose += (s, e) => uiWin.Close();

            uiWin.Content = new WallSnapSettingsUI();
            uiWin.DataContext = uiData;
            uiWin.ShowDialog();

            if (uiData.RevitTransactionResult == Result.Cancelled)
            {
                trans.Dispose();
                return Result.Cancelled;
            }

            MySettings.DistanceRev = uiData.DistanceRev;
            MySettings.DistanceFwd = uiData.DistanceFwd;
            MySettings.ViewName = uiData.CollisionViewName;
            MySettings.UseBoundingBox = uiData.UseBoundingBox;
            MySettings.UseBoxOffset = uiData.UseBoxOffset;
            MySettings.ScBoxOffsetX = uiData.ScBoxOffsetX;
            MySettings.ScBoxOffsetY = uiData.ScBoxOffsetY;


            if (uiData.CollisionLinks.Count > 0)
            {
                MySettings.LinkId = uiData.SelectedLink.Id;
            }
            else
            {
                MySettings.LinkId = ElementId.InvalidElementId;
            }


            MySettingStorage.Set(doc, MySettings);

            trans.Commit();
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class AdvancedSettings : HelperOps, IExternalCommand
    {
        UIApplication uiapp;
        UIDocument uidoc;
        Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic WallSnap Settings");

            SettingsData MySettingStorage = new SettingsData();
            SettingsObj MySettings = MySettingStorage.Get(doc);

            if (MySettings == null)
            {
                // Default Values
                MySettings = new SettingsObj()
                {
                    DistanceFwd = 1500,
                    DistanceRev = 100,
                    ViewName = "EpicC",
                    UseBoundingBox = false,
                    ScBoxOffsetX = 0,
                    ScBoxOffsetY = 0,
                };
            }


            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<ElementId> linkedDocIdSet =
              collector
              .OfCategory(BuiltInCategory.OST_RvtLinks)
              .OfClass(typeof(RevitLinkType))
              .ToElementIds();


            List<Document> linkedDocs = new List<Document>();
            List<RevitLinkType> linkedDocTypes = new List<RevitLinkType>();

            foreach (ElementId linkedFileId in linkedDocIdSet)
            {
                RevitLinkType link = doc.GetElement(linkedFileId) as RevitLinkType;
                linkedDocTypes.Add(link);

            }

            foreach (Document LinkedDoc in uiapp.Application.Documents)
            {
                if (LinkedDoc.IsLinked)
                {
                    linkedDocs.Add(LinkedDoc);
                }
            }




            // UI
            Window uiWin = new Window();
            uiWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            uiWin.Width = 800; uiWin.Height = 450;
            uiWin.MinWidth = 800; uiWin.MinHeight = 450;
            uiWin.MaxWidth = 800; 
            uiWin.ResizeMode = ResizeMode.CanResizeWithGrip;
            uiWin.Title = "BoxGen Settings";

            WallSnapSettingsVM uiData = new WallSnapSettingsVM()
            {
                DistanceRev = MySettings.DistanceRev,
                DistanceFwd = MySettings.DistanceFwd,
                CollisionViewName = MySettings.ViewName,

                UseBoundingBox = MySettings.UseBoundingBox,
                ScBoxOffsetX = MySettings.ScBoxOffsetX,
                ScBoxOffsetY = MySettings.ScBoxOffsetY,

                CollisionLinks = linkedDocTypes,

            };

            //int ind = 0;
            //foreach (var link in linkedDocTypes)
            //{
            //    if (MySettings.LinkId == link.Id)
            //    {
            //        ind = linkedDocTypes.IndexOf(link);
            //        break;
            //    }
            //}
            //uiData.SelectedIndex = ind;

            uiData.OnRequestClose += (s, e) => uiWin.Close();

            uiWin.Content = new SettingsMainPanel();
            uiWin.DataContext = uiData;
            uiWin.ShowDialog();

            if (uiData.RevitTransactionResult == Result.Cancelled)
            {
                trans.Dispose();
                return Result.Cancelled;
            }

            //MySettings.DistanceRev = uiData.DistanceRev;
            //MySettings.DistanceFwd = uiData.DistanceFwd;
            //MySettings.ViewName = uiData.CollisionViewName;
            //if (uiData.CollisionLinks.Count > 0)
            //{
            //    MySettings.LinkId = uiData.SelectedLink.Id;
            //}
            //else
            //{
            //    MySettings.LinkId = ElementId.InvalidElementId;
            //}


            //MySettingStorage.Set(doc, MySettings);

            trans.Commit();
            return Result.Succeeded;
        }
    }
}
