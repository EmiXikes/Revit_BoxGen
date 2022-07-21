using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EpicWallBoxGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicWallBoxGen
{
    [Transaction(TransactionMode.Manual)]
    internal class WallSnap : HelperOps, IExternalCommand
    {
        Result transResult = Result.Cancelled;
        double mmInFt = 304.8;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;

            List<Document> linkedDocs = new List<Document>();
            foreach (Document LinkedDoc in app.Documents)
            {
                if (LinkedDoc.IsLinked)
                {
                    linkedDocs.Add(LinkedDoc);
                }
            }

            List<BuiltInCategory> CollisionCatsWall = new List<BuiltInCategory>();
            CollisionCatsWall.Add(BuiltInCategory.OST_Walls);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Snap to nearest wall");

            #region Getting saved settings
            // getting saved settings
            WallSnapSettingsStorage MySettingStorage = new WallSnapSettingsStorage();
            WallSnapSettingsData MySettings = MySettingStorage.ReadSettings(doc);
            if (MySettings == null)
            {
                // Default Values
                MySettings = new WallSnapSettingsData()
                {
                    DistanceFwd = 1000,
                    DistanceRev = 0,
                    ViewName = "EpicWallC"
                };
            }

            #endregion

            #region creating snap check View
            View3D CollisionView = HelperOps_ViewOps.GetOrCreate3DView(doc, MySettings.ViewName);
            HelperOps_ViewOps.

                        SetVisibleCats(doc, CollisionCatsWall, CollisionView);
            HelperOps_ViewOps.
                        SetVisibleLink(doc, MySettings.LinkId, CollisionView);


            #endregion

            RevitLinkType link = doc.GetElement(MySettings.LinkId) as RevitLinkType;

            Document CollisionDoc = linkedDocs.FirstOrDefault(d=>d.PathName.Contains(link.Name));

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {
                Element selectedElement1 = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)selectedElement1;
     
                XYZ initialPoint = (selectedFamInstance.Location as LocationPoint).Point;

                NearestWallPoint NearestPointFinder = HelperOps_NearestFinders.FindNearestWallPoint(
                    initialPoint,
                    CollisionView,
                    CollisionDoc,
                    CollisionCatsWall,
                    MySettings.DistanceRev,
                    MySettings.DistanceFwd);

                if (NearestPointFinder.IsNewPointFound)
                {
                    (selectedFamInstance.Location as LocationPoint).Point = NearestPointFinder.FoundPoint;
                    Double RotationAngle = XYZ.BasisX.AngleOnPlaneTo(NearestPointFinder.WallSurfaceVector, XYZ.BasisZ) + Math.PI / 2;
                    double existingRotation = (selectedFamInstance.Location as LocationPoint).Rotation;
                    RotationAngle = Math.Abs(RotationAngle - existingRotation);
                    RotateFamilyInstance(selectedFamInstance, NearestPointFinder.FoundPoint, RotationAngle);
                }
            }


            transResult = Result.Succeeded;

            trans.Commit();
            return transResult;

            //throw new NotImplementedException();
        }
    }
}
