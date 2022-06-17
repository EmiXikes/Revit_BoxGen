using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EpicWallBoxGen.UI.View;
using EpicWallBoxGen.UI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace EpicWallBoxGen
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

            WallSnapSettingsStorage MySettingStorage = new WallSnapSettingsStorage();
            WallSnapSettingsData MySettings = MySettingStorage.ReadSettings(doc);

            if (MySettings == null)
            {
                // Default Values
                MySettings = new WallSnapSettingsData()
                {
                    DistanceFwd = 1500,
                    DistanceRev = 100,
                    ViewName = "EpicC"
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
            uiWin.Width = 250; uiWin.Height = 300;
            uiWin.ResizeMode = ResizeMode.NoResize;
            uiWin.Title = "LumiSnap Settings";

            WallSnapSettingsVM uiData = new WallSnapSettingsVM()
            {
                DistanceRev = MySettings.DistanceRev,
                DistanceFwd = MySettings.DistanceFwd,
                CollisionViewName = MySettings.ViewName,
                CollisionLinks = linkedDocTypes,

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
            if (uiData.CollisionLinks.Count > 0)
            {
                MySettings.LinkId = uiData.SelectedLink.Id;
            }
            else
            {
                MySettings.LinkId = ElementId.InvalidElementId;
            }


            MySettingStorage.WriteSettings(doc, MySettings);

            trans.Commit();
            return Result.Succeeded;
        }
    }
}
