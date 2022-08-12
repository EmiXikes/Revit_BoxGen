using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static EpicWallBox.InputData;
using static EpicWallBox.SettingsSchema_WallSnap;

namespace EpicWallBox
{
    internal class HelperOps_Creators : HelperOps
    {
        public static void CreateDebugMarker(Document doc, XYZ MarkerPoint)
        {
            FilteredElementCollector hostlevelsCollector = new FilteredElementCollector(doc);
            List<Element> hostLevels = hostlevelsCollector.OfClass(typeof(Level)).OfCategory(BuiltInCategory.OST_Levels).ToList();
            FilteredElementCollector Collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_ElectricalFixtures);
            FamilySymbol PointMarker = (FamilySymbol)Collector.FirstOrDefault(x => x.Name == "TestPointMarker");
            PointMarker.Activate();
            FamilyInstance scBoxInstance = doc.Create.NewFamilyInstance(
                    MarkerPoint,
                    PointMarker,
                    hostLevels.First(),
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        public static void CreatePointElements(Document doc, SettingsObj MySettings, PointData itemPointData)
        {
            CreateSocketBox(doc, itemPointData);

            CreateConnectionBox(doc, itemPointData);

            CreateConduit2(doc, itemPointData);
        }
        public static void CreateSocketBox(Document doc, PointData itemPointData)
        {
            if (itemPointData.pSettings.UseBoxOffset)
            {
                double verticalOffset = itemPointData.pSettings.ScBoxOffsetY / mmInFt;
                //double horizontalOffset = itemPointData.SnapSettings.ScBoxOffsetX / mmInFt;

                XYZ OffsetXYZ = new XYZ(0, 0, verticalOffset);
                itemPointData.LinkedFixtureLocation += OffsetXYZ;
            }

            XYZ LevelOffset = new XYZ(0, 0, (itemPointData.TargetLevel as Level).Elevation);

            itemPointData.LinkedFixtureLocation -= LevelOffset;

            itemPointData.scBoxFamSymbol.Activate();
            itemPointData.CreatedScBoxInstane = doc.Create.NewFamilyInstance(
                itemPointData.LinkedFixtureLocation,
                itemPointData.scBoxFamSymbol,
                (Level)itemPointData.TargetLevel,
                Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            RotateFamilyInstance(itemPointData.CreatedScBoxInstane, itemPointData.LinkedFixtureLocation, itemPointData.Rotation);

            string CommentString;
            if(itemPointData.SystemMoniker != null && itemPointData.SystemMoniker.Trim() != "")
            {
                CommentString = itemPointData.SystemMoniker + "_" + itemPointData.LinkedFixture.Symbol.FamilyName;
            } else
            {
                CommentString = itemPointData.LinkedFixture.Symbol.FamilyName;
            }

            itemPointData.CreatedScBoxInstane.
                get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                Set(CommentString);
        }
        public static void CreateConnectionBox(Document doc, PointData itemPointData)
        {
            if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.DOWN)
            {
                if (itemPointData.ConnectionEnd == PointDataStructs.ConnectionEnd.BOX)
                {
                    XYZ CornerBoxLocation = itemPointData.LinkedFixtureLocation;
                    var ConnectionOffset = itemPointData.ConnectionSideOffset;

                    if (itemPointData.ConnectionSideOffset < MinOffsetDistance &&
                        itemPointData.ConnectionSideOffset > -MinOffsetDistance)
                    {
                        ConnectionOffset = 0;
                    }


                    CornerBoxLocation = new XYZ(
                    CornerBoxLocation.X + (ConnectionOffset / mmInFt) * Math.Cos(itemPointData.Rotation),
                    CornerBoxLocation.Y + (ConnectionOffset / mmInFt) * Math.Sin(itemPointData.Rotation),
                    (itemPointData.TargetLevel as Level).Elevation + (itemPointData.pSettings.BottomFloorOffset / mmInFt)
                    );

                    itemPointData.conBoxBotFamSymbol.Activate();
                    itemPointData.CreatedConnectionBoxInstane = doc.Create.NewFamilyInstance(
                        CornerBoxLocation,
                        itemPointData.conBoxBotFamSymbol,
                        itemPointData.TargetLevel,
                        Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    RotateFamilyInstance(itemPointData.CreatedConnectionBoxInstane, CornerBoxLocation, itemPointData.Rotation);

                    if (itemPointData.LinkedFixture != null)
                    {
                        itemPointData.CreatedConnectionBoxInstane.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                            Set(itemPointData.SystemMoniker + "_" + itemPointData.LinkedFixture.Symbol.FamilyName);
                    }
                    // transfer comments
                    if (itemPointData.transferComments)
                    {
                        itemPointData.CreatedConnectionBoxInstane.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                            Set(itemPointData.CreatedScBoxInstane.get_Parameter(
                                BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
                    }
                }
            }




        }
        public static void CreateConduit2(Document doc, PointData itemPointData)
        {
            if (itemPointData.CreatedScBoxInstane == null) { return; }
            //if (itemPointData.CreatedConnectionBoxInstane == null) { return; }
            //itemPointData.ConduitDirection = PointDataStructs.ConduitDirection.UP;

            double ConduitTurnOffset = itemPointData.ConnectionSideOffset;
            double PointRotation = itemPointData.Rotation;

            #region Getting SocketBox Connectors

            var insCons = itemPointData.CreatedScBoxInstane.MEPModel.ConnectorManager.Connectors;
            List<Connector> scBoxInstanceConnectors = new List<Connector>();

            foreach (Connector instanceCon in insCons)
            {
                if (instanceCon.Description == "LeftCon" ||
                    instanceCon.Description == "RightCon" ||
                    instanceCon.Description == "TopCon" ||
                    instanceCon.Description == "BottomCon" ||
                    instanceCon.Description == "BottomRightCon" ||
                    instanceCon.Description == "BottomLeftCon" ||
                    instanceCon.Description == "TopRightCon" ||
                    instanceCon.Description == "TopLeftCon")
                {
                    scBoxInstanceConnectors.Add(instanceCon);
                    //allscBoxConnectors.Add(instanceCon);
                }
            }

            Connector UsedScBoxConnector = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "BottomCon");
            Connector ScBoxCenterConnector = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "BottomCon");
            //Connector BotRightCon = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "BottomRightCon");

            #endregion

            #region Chosing connectors and calculating coordinates for line turning points
            var OffsetDirectionSign = Math.Abs(ConduitTurnOffset) / ConduitTurnOffset;
            double ConnectorAngle = 33;

            // right direction 
            if (ConduitTurnOffset >= MinOffsetDistance)
            {
                if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.DOWN)
                {
                    // DOWN
                    UsedScBoxConnector = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "BottomRightCon");
                }
                if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.UP)
                {
                    // UP
                    UsedScBoxConnector = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "TopRightCon");
                    ConnectorAngle = 147;
                }
            }
            // straight 
            if (ConduitTurnOffset < MinOffsetDistance && 
                ConduitTurnOffset > -MinOffsetDistance)
            {
                ConduitTurnOffset = 0;
                OffsetDirectionSign = 1;
                if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.DOWN)
                {
                    // DOWN
                    UsedScBoxConnector = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "BottomCon");
                }
                if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.UP)
                {
                    // UP
                    UsedScBoxConnector = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "TopCon");
                }
            }
            // left direction
            if (ConduitTurnOffset <= -MinOffsetDistance)
            {
                if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.DOWN)
                {
                    // DOWN
                    UsedScBoxConnector = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "BottomLeftCon");
                }
                if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.UP)
                {
                    // UP
                    UsedScBoxConnector = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "TopLeftCon");
                    ConnectorAngle = 147;
                }
            }

            double CndAngle = ConnectorAngle * (Math.PI/180) * OffsetDirectionSign;
            double CndAngleHOffset = Math.Sin(CndAngle);
            double CndAngleVOffset = -1 * Math.Cos(CndAngle);

            var dt = new XYZ(ScBoxCenterConnector.Origin.X, ScBoxCenterConnector.Origin.Y, 0).DistanceTo(
                            new XYZ(UsedScBoxConnector.Origin.X, UsedScBoxConnector.Origin.Y, 0));

            CndAngleHOffset = (ConduitTurnOffset / mmInFt) - dt * OffsetDirectionSign;
            CndAngleVOffset = -1 * CndAngleHOffset / Math.Tan(CndAngle);

            #endregion

            XYZ Point1 = UsedScBoxConnector.Origin;
            XYZ Point2 = UsedScBoxConnector.Origin + 
                new XYZ(
                    CndAngleHOffset * Math.Cos(PointRotation),
                    CndAngleHOffset * Math.Sin(PointRotation),
                    CndAngleVOffset
                    );

            XYZ Point3_ConnectionEnd = new XYZ();

            #region Calculating the final conduit point (connection end)

            // Final conduit point for CONDUIT end
            if (itemPointData.ConnectionEnd == PointDataStructs.ConnectionEnd.CONDUIT)
            {   
                if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.DOWN)
                {
                    Point3_ConnectionEnd = new XYZ(Point2.X, Point2.Y, (itemPointData.TargetLevel as Level).Elevation);
                }
                if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.UP)
                {
                    XYZ scBoxLocation = (itemPointData.CreatedScBoxInstane.Location as LocationPoint).Point;
                    XYZ debugPoint = scBoxLocation + itemPointData.CreatedScBoxInstane.FacingOrientation;
                    //CreateDebugMarker(doc, debugPoint);

                    XYZ CeilingBoxLocation = GetCeilingPoint(doc, itemPointData);
                    Point3_ConnectionEnd = new XYZ(Point2.X, Point2.Y, CeilingBoxLocation.Z);
                }
            }

            // Final conduit point for BOX end
            Connector ConBoxConnector = null;
            if (itemPointData.ConnectionEnd == PointDataStructs.ConnectionEnd.BOX)
            {
                var ConBoxCons = itemPointData.CreatedConnectionBoxInstane.MEPModel.ConnectorManager.Connectors;

                List<Connector> Cons = new List<Connector>();
                foreach (Connector C in ConBoxCons) { Cons.Add(C); }

                ConBoxConnector = Cons[0];
                Point3_ConnectionEnd = Cons[0].Origin;
            }
            #endregion


            if (ConduitTurnOffset == 0) { Point2 = Point3_ConnectionEnd; }

            Conduit conduitInstance = Conduit.Create(
                doc,
                itemPointData.conduitType.Id,
                Point1,
                Point2,
                itemPointData.TargetLevel.Id
                );
            var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
            diameter.Set(20 / mmInFt);
            conduitInstance.ConnectorManager.Lookup(0).ConnectTo(UsedScBoxConnector);
            // write comments
            if (itemPointData.LinkedFixture != null)
            {
                conduitInstance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                    Set(itemPointData.SystemMoniker + "_" + itemPointData.LinkedFixture.Symbol.FamilyName);
            }
            // transfer comments
            if (itemPointData.transferComments)
            {
                conduitInstance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                    Set(itemPointData.CreatedScBoxInstane.get_Parameter(
                        BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
            }

            if (ConduitTurnOffset == 0)
            {
                if (itemPointData.ConnectionEnd == PointDataStructs.ConnectionEnd.BOX)
                {
                    conduitInstance.ConnectorManager.Lookup(1).ConnectTo(ConBoxConnector);
                }
            }


            //next segment (only if line was not straight)
            if (ConduitTurnOffset != 0)
            {
                Conduit conduitInstance2 = Conduit.Create(doc,
                    itemPointData.conduitType.Id,
                    Point2,
                    Point3_ConnectionEnd,
                    itemPointData.TargetLevel.Id);

                var p = itemPointData.conduitType.RoutingPreferenceManager;
                diameter = conduitInstance2.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                diameter.Set(20 / mmInFt);

                if (itemPointData.LinkedFixture != null)
                {
                    conduitInstance2.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                        Set(itemPointData.SystemMoniker + "_" + itemPointData.LinkedFixture.Symbol.FamilyName);
                }
                if (itemPointData.transferComments)
                {
                    conduitInstance2.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                        Set(itemPointData.CreatedScBoxInstane.get_Parameter(
                            BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
                }

                if (itemPointData.ConnectionEnd == PointDataStructs.ConnectionEnd.BOX)
                {
                    conduitInstance2.ConnectorManager.Lookup(1).ConnectTo(ConBoxConnector);
                }

                var fittingInstance = doc.Create.NewElbowFitting(conduitInstance.ConnectorManager.Lookup(1), conduitInstance2.ConnectorManager.Lookup(0));
                if (itemPointData.LinkedFixture != null)
                {
                    fittingInstance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                        Set(itemPointData.SystemMoniker + "_" + itemPointData.LinkedFixture.Symbol.FamilyName);
                }
                if (itemPointData.transferComments)
                {
                    fittingInstance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                        Set(itemPointData.CreatedScBoxInstane.get_Parameter(
                            BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());
                }
            }
        }

        public static void CreateConduit3(Document doc, PointData itemPointData)
        {
            if (itemPointData.CreatedScBoxInstane == null) { return; }

            double ConduitTurnOffset = itemPointData.ConnectionSideOffset;
            double PointRotation = itemPointData.Rotation;

            #region Getting SocketBox Connectors

            var insCons = itemPointData.CreatedScBoxInstane.MEPModel.ConnectorManager.Connectors;
            List<Connector> scBoxInstanceConnectors = new List<Connector>();

            foreach (Connector instanceCon in insCons)
            {
                if (instanceCon.Description == "LeftCon" ||
                    instanceCon.Description == "RightCon" ||
                    instanceCon.Description == "TopCon" ||
                    instanceCon.Description == "BottomCon" ||
                    instanceCon.Description == "BottomRightCon" ||
                    instanceCon.Description == "BottomLeftCon" ||
                    instanceCon.Description == "TopRightCon" ||
                    instanceCon.Description == "TopLeftCon")
                {
                    scBoxInstanceConnectors.Add(instanceCon);
                    //allscBoxConnectors.Add(instanceCon);
                }
            }
            Connector UsedScBoxConnector = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "TopLeftCon");

            #endregion

            //var ConnectorDistanceFromCenter = UsedScBoxConnector.Origin - itemPointData.LinkedFixtureLocation;
            XYZ ConnectorNormalVector = UsedScBoxConnector.CoordinateSystem.BasisZ;
            double ConduitDiameter = 25 / mmInFt;
            double TravelPlaneDepth = -60 / mmInFt;
            double TravelRouteOffset = 50 / mmInFt;
            double dist1 = 15 / mmInFt;

            Plane WallPlane = Plane.CreateByNormalAndOrigin(itemPointData.CreatedScBoxInstane.FacingOrientation, itemPointData.LinkedFixtureLocation);
            
            XYZ TravelPlaneOrigin = itemPointData.LinkedFixtureLocation +
                itemPointData.CreatedScBoxInstane.FacingOrientation * TravelPlaneDepth;
            Plane TravelPlane = Plane.CreateByNormalAndOrigin(itemPointData.CreatedScBoxInstane.FacingOrientation, TravelPlaneOrigin);

            XYZ sVector = new XYZ(
                -itemPointData.CreatedScBoxInstane.FacingOrientation.Y,
                itemPointData.CreatedScBoxInstane.FacingOrientation.X, 0).Normalize();

            // Change this to change LEFT/RIGHT direction
            // TODO must automate this
            sVector *= 1;
            sVector = sVector * TravelRouteOffset;

            Plane TravelPlaneA = Plane.CreateByNormalAndOrigin(sVector.Normalize(), itemPointData.LinkedFixtureLocation + sVector);

            Line Line_ConOrigToWallPlane = Line.CreateBound(UsedScBoxConnector.Origin, UsedScBoxConnector.Origin + TravelPlane.Normal);
            XYZ Point_OrigProjectionOnWallPlane = LinePlaneIntersection(Line_ConOrigToWallPlane, WallPlane, true, out double ddd1);

            XYZ Point1 = UsedScBoxConnector.Origin;
            XYZ Point2 = UsedScBoxConnector.Origin + (ConnectorNormalVector * dist1);

            XYZ Point_ConDirectionProjection = Point2 + (ConnectorNormalVector * 10);
            Line Line_ConDirProjPointToWallPlane = Line.CreateBound(Point_ConDirectionProjection, Point_ConDirectionProjection + TravelPlane.Normal);
            XYZ Point_ConDirProjectionOnWallPlane = LinePlaneIntersection(Line_ConDirProjPointToWallPlane, WallPlane, true, out double ddd );

            Line Line_ConToTravelRouteOnWallPlane = 
                Line.CreateBound(Point_OrigProjectionOnWallPlane, Point_ConDirProjectionOnWallPlane + (ConnectorNormalVector * 10));
            XYZ Point_TravelPlaneIntersection = LinePlaneIntersection(Line_ConToTravelRouteOnWallPlane, TravelPlaneA, true, out double ddd33);

            XYZ Point3 = Point_TravelPlaneIntersection + TravelPlaneDepth * TravelPlane.Normal;

            double DirectionUpDown = 1;
            var deltaVector = UsedScBoxConnector.Origin - itemPointData.LinkedFixtureLocation;
            if (deltaVector.Z < 0) DirectionUpDown = -1;

            XYZ Point4 = Point3 + new XYZ(0, 0, DirectionUpDown);



            //CreateDebugMarker(doc, Point1);
            //CreateDebugMarker(doc, Point2);
            //CreateDebugMarker(doc, Point3);
            //CreateDebugMarker(doc, Point4);


            Conduit conduitInstance1 = Conduit.Create(doc, itemPointData.conduitType.Id,
                Point1,
                Point2,
                itemPointData.TargetLevel.Id);
            conduitInstance1.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(ConduitDiameter);

            Conduit conduitInstance2 = Conduit.Create(doc, itemPointData.conduitType.Id,
                Point2,
                Point3,
                itemPointData.TargetLevel.Id);
            conduitInstance2.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(ConduitDiameter);

            doc.Create.NewElbowFitting(conduitInstance1.ConnectorManager.Lookup(1), conduitInstance2.ConnectorManager.Lookup(0));

            Conduit conduitInstance3 = Conduit.Create(doc, itemPointData.conduitType.Id,
                Point3,
                Point4,
                itemPointData.TargetLevel.Id);
            conduitInstance3.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(ConduitDiameter);

            doc.Create.NewElbowFitting(conduitInstance2.ConnectorManager.Lookup(1), conduitInstance3.ConnectorManager.Lookup(0));

        }

        public static XYZ LinePlaneIntersection(
            Line line,
            Plane plane,
            bool enforceResultOnLine,
            out double lineParameter)
        {
            var planePoint = plane.Origin;
            var planeNormal = plane.Normal;
            var linePoint = line.GetEndPoint(0);

            var lineDirection = (line.GetEndPoint(1) - linePoint).Normalize();

            // Is the line parallel to the plane, i.e.,
            // perpendicular to the plane normal?

            if (IsZero(planeNormal.DotProduct(lineDirection)))
            {
                lineParameter = double.NaN;
                return null;
            }

            lineParameter = (planeNormal.DotProduct(planePoint)
                             - planeNormal.DotProduct(linePoint))
                            / planeNormal.DotProduct(lineDirection);

            // Test whether the line parameter is inside 
            // the line using the "isInside" method.

            if (enforceResultOnLine
                && !line.IsInside(lineParameter))
            {
                lineParameter = double.NaN;
                return null;
            }

            return linePoint + lineParameter * lineDirection;
        }


        public const double _eps = 1.0e-9;
        //public static double Eps => _eps;

        public static bool IsZero(
            double a,
            double tolerance = _eps)
        {
            return tolerance > Math.Abs(a);
        }
    }
}