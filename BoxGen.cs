using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI;
using EpicWallBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static EpicWallBox.SourceDataStructs;
using static EpicWallBox.InputData;
using static EpicWallBox.HelperOps_Creators;
using static EpicWallBox.SettingsSchema_WallSnap;

namespace EpicWallBox
{
    [Transaction(TransactionMode.Manual)]
    internal class BoxGen : HelperOps, IExternalCommand
    {
        Result transResult = Result.Cancelled;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            #region GETTING SOURCE POINTS

            var sData = new SourceData()
            {
                SourceRVTDocName = SelectedSourceDocName,
                SourceRVTLevelName = SelectedSourceLevelName,
                SourceRVTCat = SelectedSourceCategory,
                SourceElementNameFilter = SelectedNamesFilter,
                SourceDataType = SourceDataType
            };

            sData.SourceRVTDocName = "BALT59_HVAC";
            sData.SourceRVTLevelName = "1st Floor";

            List<FamilyInstance> linkedFixtures = new List<FamilyInstance>();

            switch (SourceDataType)
            {
                case SourceType.RVT:
                    linkedFixtures = HelperOps_DataCollectors.GetSourcePointsRVT(doc, sData);
                    break;
                case SourceType.DWG:
                    break;
                default:
                    break;
            }

            #endregion
   
            Transaction trans = new Transaction(doc);
            trans.Start("Epic WallBox Generation");


            // TESTING
            #region Definitions of Revit elements, families, types, ect
            Document SelectedSourceDoc = HelperOps_DataCollectors.GetLinkedDocByName(doc, sData.SourceRVTDocName);

            var TargetLevel = new FilteredElementCollector(doc).
                OfClass(typeof(Level)).
                OfCategory(BuiltInCategory.OST_Levels).ToList().
                FirstOrDefault(L => L.Name == SelectedTargetLevelName);

            var scBoxFamSymbol = (FamilySymbol)new FilteredElementCollector(doc).
                OfCategory(BuiltInCategory.OST_MechanicalEquipment).
                FirstOrDefault(x => x.Name == SocketBoxFamilyTypeName);
            var scFloorCornerFamSymbol = (FamilySymbol)new FilteredElementCollector(doc).
                OfCategory(BuiltInCategory.OST_MechanicalEquipment).
                FirstOrDefault(x => x.Name == ConnectionBoxBottomSingleTypeName);
            var scCeilingCornerFamSymbol = (FamilySymbol)new FilteredElementCollector(doc).
                OfCategory(BuiltInCategory.OST_MechanicalEquipment).
                FirstOrDefault(x => x.Name == ConnectionBoxTopSingleTypeName);

            var conduitTypes =
                new FilteredElementCollector(doc)
                .OfClass(typeof(ConduitType))
                .OfType<ConduitType>()
                .ToList();
            ConduitType conduitType = conduitTypes.FirstOrDefault(n => n.Name == ConduitTypeName);

            #endregion

            // Survey point
            XYZ surveyPoint = HelperOps_DataCollectors.GetSurveyPoint(doc);

            SettingsObj MySettings = GetUserSettings_WallboxGen(doc);

            foreach (var LinkedFixture in linkedFixtures)
            {
                Debug.WriteLine(String.Format(
                    "Fixture: [{0}]  Type Name: {1}  Family Name: {2}  Level: {3}",
                    LinkedFixture.Id, LinkedFixture.Name, LinkedFixture.Symbol.FamilyName, LinkedFixture.LevelId)
                    );

                // New data item
                PointData itemPointData = new PointData()
                {
                    LinkedFixture = LinkedFixture,
                    LinkedFixtureLocation = (LinkedFixture.Location as LocationPoint).Point,
                    Rotation = XYZ.BasisX.AngleOnPlaneTo(LinkedFixture.FacingOrientation, XYZ.BasisZ) + Math.PI / 2,
                    TargetLevel = TargetLevel,
                    scBoxFamSymbol = scBoxFamSymbol,
                    conBoxBotFamSymbol = scFloorCornerFamSymbol,
                    conduitType = conduitType,
                    InstallationHeight = 0,
                    Description = "",
                    ConnectionOffset = 0,
                    SnapSettings = MySettings,
                    ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                    ConnectionEnd = PointDataStructs.ConnectionEnd.BOX,
                    SeperateConduitLine = PointDataStructs.SeperateConduitLine.NO,
                    FixtureEnd = PointDataStructs.FixtureEnd.SOCKET,
                    SystemMoniker = "AVK",
                };

                // Custom data additions
                #region Custom data

                if (itemPointData.LinkedFixture.Symbol.FamilyName.Contains("Switch"))
                {
                    itemPointData.ConduitDirection = PointDataStructs.ConduitDirection.UP;
                    itemPointData.ConnectionEnd = PointDataStructs.ConnectionEnd.CONDUIT;
                }

                #endregion

                // Coordinate corrections
                #region Coordinate Corrections
                               
                itemPointData = WallCoordinateCorrection(doc, itemPointData, MySettings);

                #endregion
                                
                // Create elements
                CreatePointElements(doc, MySettings, itemPointData);

                
            }

            //System.Windows.MessageBox.Show("Testing...");

            transResult = Result.Succeeded;

            trans.Commit();
            return transResult;

        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class BoxGenTest : HelperOps, IExternalCommand
    {
        Result transResult = Result.Cancelled;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Document doc = uiapp.ActiveUIDocument.Document;

            var TestItems = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .ToList();

            View3D CollisionView = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .First<View3D>(x => x.Name == "CollisionTestView");


            Transaction trans = new Transaction(doc);
            trans.Start("Epic WallBox Generation");

            List<BuiltInCategory> CollisionCatsWall = new List<BuiltInCategory>()
                        {
                            BuiltInCategory.OST_Walls
                        };
            double fwdDist = 5000;
            double revDist = 0;

            foreach (var TestItem in TestItems)
            {
                XYZ TestItemLocation = (TestItem.Location as LocationPoint).Point;
                NearestWallPoint NearestPointFinder = HelperOps_NearestFinders.FindNearestWallPoint(TestItemLocation, CollisionView, doc, CollisionCatsWall, revDist, fwdDist);//, doc, CollisionCatsWall, fwdDist, revDist);

                if (NearestPointFinder.IsNewPointFound)
                {
                    (TestItem.Location as LocationPoint).Point = NearestPointFinder.FoundPoint;
                    Double RotationAngle = XYZ.BasisX.AngleOnPlaneTo(NearestPointFinder.WallSurfaceVector, XYZ.BasisZ) + Math.PI / 2;

                    double existingRotation = (TestItem.Location as LocationPoint).Rotation;
                    RotationAngle = Math.Abs(RotationAngle - existingRotation);
                    RotateFamilyInstance(TestItem, NearestPointFinder.FoundPoint, RotationAngle);
                    HelperOps_Creators.
                                        CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.05);
                    HelperOps_Creators.CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.1);
                    HelperOps_Creators.CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.2);
                    HelperOps_Creators.CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.4);
                    HelperOps_Creators.CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.5);
                    HelperOps_Creators.CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.6);
                }
            }

            transResult = Result.Succeeded;

            trans.Commit();
            return transResult;

        }
    }

}
