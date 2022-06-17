using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using EpicWallBoxGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static EpicWallBoxGen.SourceDataStructs;

namespace EpicWallBoxGen
{
    [Transaction(TransactionMode.Manual)]
    internal class BoxGen : HelperOps, IExternalCommand
    {

        #region INPUT DATA
        // Ipnut data 
        SourceType SourceDataType = SourceType.RVT;

        // Input RVT
        BuiltInCategory SelectedSourceCategory = BuiltInCategory.OST_ElectricalFixtures;
        List<string> SelectedNamesFilter = new List<string>();// { "Switch", "Socket"};
        
        string SelectedSourceDocName = "Baltezers house 2022";
        string SelectedCollsiionDocName = "";
        string SelectedSourceLevelName = "1st Floor";
        double FloorLevelOffset = 0;
        double CeilingLevelOffset = 0;
        // Input data DWG


        #endregion


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

            List<FamilyInstance> linkedFixtures = new List<FamilyInstance>();

            switch (SourceDataType)
            {
                case SourceType.RVT:
                    linkedFixtures = GetSourcePointsRVT(doc, sData);
                    break;
                case SourceType.DWG:
                    break;
                default:
                    break;
            }

            #endregion

            // GetWallPoints(linkedfixtures)           

            Transaction trans = new Transaction(doc);
            trans.Start("Epic WallBox Generation");



            foreach (var item in linkedFixtures)
            {
                Debug.WriteLine(String.Format(
                    "Fixture: [{0}]  Type Name: {1}  Family Name: {2}  Level: {3}",
                    item.Id, item.Name, item.Symbol.FamilyName, item.LevelId)
                    );
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
                NearestWallPoint NearestPointFinder = FindNearestWallPoint(TestItemLocation, CollisionView, doc, CollisionCatsWall, revDist, fwdDist);//, doc, CollisionCatsWall, fwdDist, revDist);

                if (NearestPointFinder.IsNewPointFound)
                {
                    (TestItem.Location as LocationPoint).Point = NearestPointFinder.FoundPoint;
                    Double RotationAngle = XYZ.BasisX.AngleOnPlaneTo(NearestPointFinder.WallSurfaceVector, XYZ.BasisZ) + Math.PI / 2;

                    double existingRotation = (TestItem.Location as LocationPoint).Rotation;
                    RotationAngle = Math.Abs(RotationAngle - existingRotation);
                    RotateFamilyInstance(TestItem, NearestPointFinder.FoundPoint, RotationAngle);

                    CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.05);
                    CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.1);
                    CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.2);
                    CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.4);
                    CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.5);
                    CreateDebugMarker(doc, NearestPointFinder.FoundPoint + NearestPointFinder.WallSurfaceVector * -0.6);
                }
            }

            transResult = Result.Succeeded;

            trans.Commit();
            return transResult;

        }
    }

}
