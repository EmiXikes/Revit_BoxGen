using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using EpicWallBox;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EpicWallBox.InputData;
using static EpicWallBox.SettingsSchema_WallSnap;

namespace EpicWallBox
{
    internal class HelperOps
    {
        public static void RotateFamilyInstance(FamilyInstance ElementInstance, XYZ CenterPoint, double RotationAngle)
        {

            XYZ axisPoint1 = CenterPoint;
            XYZ axisPoint2 = new XYZ(
                CenterPoint.X,
                CenterPoint.Y,
                CenterPoint.Z + 1
                );
            Line axis = Line.CreateBound(axisPoint1, axisPoint2);

            ElementTransformUtils.RotateElement(
                ElementInstance.Document,
                ElementInstance.Id,
                axis,
                RotationAngle);
        }

        public static XYZ GetCeilingPoint(Document doc, PointData itemPointData)
        {
            XYZ newLocation = XYZ.Zero;

            #region creating snap check View
            List<BuiltInCategory> CollisionCatsFloor = new List<BuiltInCategory>();
            CollisionCatsFloor.Add(BuiltInCategory.OST_Ceilings);
            CollisionCatsFloor.Add(BuiltInCategory.OST_Floors);
            CollisionCatsFloor.Add(BuiltInCategory.OST_StructuralFraming);


            View3D CollisionViewFloor = HelperOps_ViewOps.GetOrCreate3DView(doc, "CeilCheckBK");
            HelperOps_ViewOps.
                        SetVisibleCats(doc, CollisionCatsFloor, CollisionViewFloor);
            HelperOps_ViewOps.
                        SetVisibleLink(doc, itemPointData.SnapSettings.LinkId, CollisionViewFloor);
            #endregion

            XYZ LevelOffset = new XYZ(0, 0, (itemPointData.TargetLevel as Level).Elevation);
            //XYZ CornerBoxLocation = itemPointData.LinkedFixtureLocation;
            XYZ CornerBoxLocation = (itemPointData.CreatedScBoxInstane.Location as LocationPoint).Point;
            XYZ CeilTestOffsetPoint = itemPointData.CreatedScBoxInstane.FacingOrientation * 0.2;

            List<ReferenceWithContext> foundRefs = null;

            var r = CornerBoxLocation = HelperOps_NearestFinders.GetCeilingPoints(
                CollisionViewFloor,
                CornerBoxLocation + CeilTestOffsetPoint,
                100,
                CollisionCatsFloor,
                out foundRefs);

            if (foundRefs != null && foundRefs.Count > 1)
            {
                newLocation = foundRefs[1].GetReference().GlobalPoint;
            }

            CornerBoxLocation = new XYZ(
                itemPointData.LinkedFixtureLocation.X,
                itemPointData.LinkedFixtureLocation.Y,
                newLocation.Z);
            return CornerBoxLocation;
        }

        public static PointData WallCoordinateCorrection(Document doc, PointData itemPointData)
        {
            XYZ PrefferedDirection = null;
            double WallWidthFactor = 1;

            var itemHost = itemPointData.LinkedFixture.Host;
            if (itemHost != null)
            {
                BuiltInCategory catID = (BuiltInCategory)itemPointData.LinkedFixture.Host.Category.Id.IntegerValue;

                if (catID == BuiltInCategory.OST_Walls)
                {
                    WallWidthFactor = (itemPointData.LinkedFixture.Host as Wall).Width / 2;
                    itemPointData.LinkedFixtureLocation += itemPointData.LinkedFixture.FacingOrientation * WallWidthFactor;
                    PrefferedDirection = itemPointData.LinkedFixture.FacingOrientation;
                }
            }

            #region HVAC
            // For HVAC only
            // For HVAC only
            // For HVAC only
            var deltaZparam = itemPointData.LinkedFixture.GetParameters("H");
            if (deltaZparam.Count > 0)
            {
                var deltaZ = deltaZparam.First().AsDouble();
                itemPointData.LinkedFixtureLocation = new XYZ(
                itemPointData.LinkedFixtureLocation.X,
                itemPointData.LinkedFixtureLocation.Y,
                itemPointData.LinkedFixtureLocation.Z + deltaZ );

                //if ( itemPointData.TargetLevel != null)
                //{
                //    itemPointData.LinkedFixtureLocation = new XYZ(
                //        itemPointData.LinkedFixtureLocation.X,
                //        itemPointData.LinkedFixtureLocation.Y,
                //        (itemPointData.TargetLevel as Level).Elevation + deltaZ);
                //}

            }

            // For HVAC only
            // For HVAC only
            // For HVAC only
            #endregion


            #region creating snap check View
            RevitLinkType link = doc.GetElement(itemPointData.SnapSettings.LinkId) as RevitLinkType;

            //Document CollisionDoc = linkedDocs.FirstOrDefault(d => d.PathName.Contains(link.Name));

            Document CollisionDoc = HelperOps_DataCollectors.GetLinkedDocByName(doc, link.Name);


            View3D CollisionView = HelperOps_ViewOps.GetOrCreate3DView(doc, itemPointData.SnapSettings.ViewName);

            List<BuiltInCategory> CollisionCatsWall = new List<BuiltInCategory>();
            CollisionCatsWall.Add(BuiltInCategory.OST_Walls);
            HelperOps_ViewOps.
                        SetVisibleCats(doc, CollisionCatsWall, CollisionView);
            HelperOps_ViewOps.
                        SetVisibleLink(doc, itemPointData.SnapSettings.LinkId, CollisionView);
            #endregion


            NearestWallPoint NearestPointFinder = HelperOps_NearestFinders.FindNearestWallPoint(
                itemPointData.LinkedFixtureLocation,
                CollisionView,
                CollisionDoc,
                CollisionCatsWall,
                itemPointData.SnapSettings.DistanceRev / mmInFt,
                itemPointData.SnapSettings.DistanceFwd / mmInFt,
                PrefferedDirection);

            //double RotationAngle = XYZ.BasisX.AngleOnPlaneTo(LinkedFixture.FacingOrientation, XYZ.BasisZ) + Math.PI / 2;

            if (NearestPointFinder.IsNewPointFound)
            {
                itemPointData.LinkedFixtureLocation = NearestPointFinder.FoundPoint;
                itemPointData.Rotation = XYZ.BasisX.AngleOnPlaneTo(NearestPointFinder.WallSurfaceVector, XYZ.BasisZ) + Math.PI / 2;
            }

            return itemPointData;
        }

        public static SettingsObj GetUserSettings_WallboxGen(Document doc)
        {
            #region Getting saved settings

            // getting saved settings
            SettingsData MySettingStorage = new SettingsData();
            SettingsObj MySettings = MySettingStorage.Get(doc);
            if (MySettings == null)
            {
                // Default Values
                MySettings = new SettingsObj()
                {
                    DistanceFwd = 1000,
                    DistanceRev = 0,
                    ViewName = "EpicWallC"
                };
            }
            #endregion
            return MySettings;
        }

        public static void GetSymbols(ManualWallBoxFamilyTypeNames FamTypeNames, PointData pData)
        {
            var conduitTypes =
                new FilteredElementCollector(pData.doc)
                .OfClass(typeof(ConduitType))
                .OfType<ConduitType>()
                .ToList();
            pData.conduitType = conduitTypes.FirstOrDefault(n => n.Name == FamTypeNames.conduitTypeName);

            pData.conBoxBotFamSymbol = (FamilySymbol)new FilteredElementCollector(pData.doc).
                OfCategory(BuiltInCategory.OST_MechanicalEquipment).
                FirstOrDefault(x => x.Name == FamTypeNames.conBoxBotFamTypeName);

            pData.scBoxFamSymbol = (FamilySymbol)new FilteredElementCollector(pData.doc).
                OfCategory(BuiltInCategory.OST_MechanicalEquipment).
                FirstOrDefault(x => x.Name == FamTypeNames.scBoxFamTypeName);
        }

        public static void GetSettings(PointData pData)
        {
            #region Getting saved settings
            // getting saved settings
            SettingsData MySettingStorage = new SettingsData();
            pData.SnapSettings = MySettingStorage.Get(pData.doc);
            if (pData.SnapSettings == null)
            {
                // Default Values
                pData.SnapSettings = new SettingsObj()
                {
                    DistanceFwd = 1000,
                    DistanceRev = 0,
                    ViewName = "EpicWallC"
                };
            }

            #endregion
        }

        public static Element GetElementLevel(Document doc, Element SelectedElement)
        {
            var selectedElementLevel = SelectedElement.GetParameters("Host").First().AsString();
            selectedElementLevel = selectedElementLevel.Replace(":", "");
            selectedElementLevel = selectedElementLevel.Replace("Level", "").Trim();

            var TargetLevel = new FilteredElementCollector(doc).
                OfClass(typeof(Level)).
                OfCategory(BuiltInCategory.OST_Levels).ToList().
                FirstOrDefault(L => L.Name == selectedElementLevel);
            return TargetLevel;
        }


    }
}
