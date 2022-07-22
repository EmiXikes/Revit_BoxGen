using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using EpicWallBoxGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EpicWallBoxGen.InputData;

namespace EpicWallBoxGen
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

        public static XYZ GetCeilingBoxLocation(Document doc, WallSnapSettingsData MySettings, PointData itemPointData)
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
                        SetVisibleLink(doc, MySettings.LinkId, CollisionViewFloor);
            #endregion

            XYZ LevelOffset = new XYZ(0, 0, (itemPointData.TargetLevel as Level).Elevation);
            XYZ CornerBoxLocation = itemPointData.LinkedFixtureLocation + LevelOffset;

            List<ReferenceWithContext> foundRefs = null;
            CornerBoxLocation = HelperOps_NearestFinders.GetCeilingPoints(
                CollisionViewFloor,
                CornerBoxLocation + itemPointData.LinkedFixture.FacingOrientation * 0.2,
                100,
                CollisionCatsFloor,
                out foundRefs);

            if (foundRefs.Count > 1)
            {
                newLocation = foundRefs[1].GetReference().GlobalPoint;
            }

            CornerBoxLocation = new XYZ(
                itemPointData.LinkedFixtureLocation.X,
                itemPointData.LinkedFixtureLocation.Y,
                foundRefs[1].GetReference().GlobalPoint.Z);
            return CornerBoxLocation;
        }

        public static PointData WallCoordinateCorrection(Document doc, PointData itemPointData, WallSnapSettingsData MySettings)
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
                    (itemPointData.TargetLevel as Level).Elevation + deltaZ);
            }

            // For HVAC only
            // For HVAC only
            // For HVAC only
            #endregion


            #region creating snap check View
            RevitLinkType link = doc.GetElement(MySettings.LinkId) as RevitLinkType;

            //Document CollisionDoc = linkedDocs.FirstOrDefault(d => d.PathName.Contains(link.Name));

            Document CollisionDoc = HelperOps_DataCollectors.GetLinkedDocByName(doc, link.Name);


            View3D CollisionView = HelperOps_ViewOps.GetOrCreate3DView(doc, MySettings.ViewName);

            List<BuiltInCategory> CollisionCatsWall = new List<BuiltInCategory>();
            CollisionCatsWall.Add(BuiltInCategory.OST_Walls);
            HelperOps_ViewOps.
                        SetVisibleCats(doc, CollisionCatsWall, CollisionView);
            HelperOps_ViewOps.
                        SetVisibleLink(doc, MySettings.LinkId, CollisionView);
            #endregion


            NearestWallPoint NearestPointFinder = HelperOps_NearestFinders.FindNearestWallPoint(
                itemPointData.LinkedFixtureLocation,
                CollisionView,
                CollisionDoc,
                CollisionCatsWall,
                MySettings.DistanceRev,
                MySettings.DistanceFwd,
                PrefferedDirection);

            //double RotationAngle = XYZ.BasisX.AngleOnPlaneTo(LinkedFixture.FacingOrientation, XYZ.BasisZ) + Math.PI / 2;

            if (NearestPointFinder.IsNewPointFound)
            {
                itemPointData.LinkedFixtureLocation = NearestPointFinder.FoundPoint;
                itemPointData.Rotation = XYZ.BasisX.AngleOnPlaneTo(NearestPointFinder.WallSurfaceVector, XYZ.BasisZ) + Math.PI / 2;
            }

            return itemPointData;
        }

        public static WallSnapSettingsData GetUserSettings_WallboxGen(Document doc)
        {
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
            return MySettings;
        }


    }
}
