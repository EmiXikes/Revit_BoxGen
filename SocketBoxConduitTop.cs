using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using EpicWallBoxGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static EpicWallBoxGen.SourceDataStructs;

namespace EpicWallBoxGen
{
    [Transaction(TransactionMode.Manual)]
    internal class SocketBoxConduitTop : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            double mmInFt = 304.8;
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;

            List<Document> linkedDocs = new List<Document>();
            foreach (Document LinkedDoc in app.Documents)
            {
                if (LinkedDoc.IsLinked)
                {
                    linkedDocs.Add(LinkedDoc);
                }
            }

            List<BuiltInCategory> CollisionCatsFloor = new List<BuiltInCategory>();
            CollisionCatsFloor.Add(BuiltInCategory.OST_Ceilings);
            CollisionCatsFloor.Add(BuiltInCategory.OST_Floors);
            CollisionCatsFloor.Add(BuiltInCategory.OST_StructuralFraming);

            var conduitTypes =
                new FilteredElementCollector(doc)
                .OfClass(typeof(ConduitType))
                .OfType<ConduitType>()
                .ToList();
            var conduitType = conduitTypes.FirstOrDefault(n => n.Name == "Conduit PVC");

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
            View3D CollisionView = HelperOps_ViewOps.GetOrCreate3DView(doc, "FloorCollisionView");
            HelperOps_ViewOps.
                        SetVisibleCats(doc, CollisionCatsFloor, CollisionView);
            HelperOps_ViewOps.
                        SetVisibleLink(doc, MySettings.LinkId, CollisionView);


            #endregion

            RevitLinkType link = doc.GetElement(MySettings.LinkId) as RevitLinkType;

            Document CollisionDoc = linkedDocs.FirstOrDefault(d => d.PathName.Contains(link.Name));

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {
                Element selectedElement1 = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)selectedElement1;

                if (selectedFamInstance.Symbol.FamilyName != "ConcreteBox") { continue; }

                XYZ itemLocation = (selectedFamInstance.Location as LocationPoint).Point;

                // get instance connectors
                var insCons = selectedFamInstance.MEPModel.ConnectorManager.Connectors;
                List<Connector> scBoxInstanceConnectors = new List<Connector>();

                foreach (Connector instanceCon in insCons)
                {
                    if (instanceCon.Description == "LeftCon" ||
                        instanceCon.Description == "RightCon" ||
                        instanceCon.Description == "TopCon" ||
                        instanceCon.Description == "BottomCon")
                    {
                        scBoxInstanceConnectors.Add(instanceCon);
                        //allscBoxConnectors.Add(instanceCon);
                    }
                }

                XYZ CornerBoxLocation = itemLocation;
                List<ReferenceWithContext> foundRefs = null;
                CornerBoxLocation = HelperOps_NearestFinders.GetCeilingPoints(
                    CollisionView,
                    CornerBoxLocation + selectedFamInstance.FacingOrientation * 0.2,
                    100,
                    CollisionCatsFloor,
                    out foundRefs);

                if (foundRefs != null && foundRefs.Count > 1)
                {
                    CornerBoxLocation = new XYZ(
                        itemLocation.X,
                        itemLocation.Y,
                        foundRefs[1].GetReference().GlobalPoint.Z);
                } else { continue; }

                Connector TopCon = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "TopCon");
                XYZ CeilConduitPoint = new XYZ(TopCon.Origin.X, TopCon.Origin.Y, CornerBoxLocation.Z);

                Conduit conduitInstance = Conduit.Create(
                    doc,
                    conduitType.Id,
                    TopCon.Origin,
                    CeilConduitPoint,
                    selectedElement1.LevelId
                    );
                var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                diameter.Set(20 / mmInFt);
                conduitInstance.ConnectorManager.Lookup(0).ConnectTo(TopCon);

            }



            trans.Commit();
            return transResult;
        }
    }
}
