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
            FilteredElementCollector Collector = new FilteredElementCollector(doc).
                OfCategory(BuiltInCategory.OST_ElectricalFixtures);
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
            double verticalOffset = FixtureCenterOffset.Y;
            XYZ OffsetXYZ = new XYZ(0, 0, verticalOffset);

            itemPointData.LinkedFixtureLocation += OffsetXYZ;

            XYZ LevelOffset = new XYZ(0, 0, (itemPointData.TargetLevel as Level).Elevation);

            itemPointData.LinkedFixtureLocation -= LevelOffset;

            Debug.WriteLine(String.Format("Fixture offset: [{0}]", OffsetXYZ));

            itemPointData.scBoxFamSymbol.Activate();
            itemPointData.CreatedScBoxInstane = doc.Create.NewFamilyInstance(
                itemPointData.LinkedFixtureLocation,
                itemPointData.scBoxFamSymbol,
                (Level)itemPointData.TargetLevel,
                Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            RotateFamilyInstance(itemPointData.CreatedScBoxInstane, itemPointData.LinkedFixtureLocation, itemPointData.Rotation);

            itemPointData.CreatedScBoxInstane.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                Set(itemPointData.SystemMoniker + "_" + itemPointData.LinkedFixture.Symbol.FamilyName);
        }
        public static void CreateConduit(Document doc, PointData itemPointData)
        {
            var insCons = itemPointData.CreatedScBoxInstane.MEPModel.ConnectorManager.Connectors;
            List<Connector> scBoxInstanceConnectors = new List<Connector>();

            foreach (Connector instanceCon in insCons)
            {
                if (instanceCon.Description == "LeftCon" ||
                    instanceCon.Description == "RightCon" ||
                    instanceCon.Description == "TopCon" ||
                    instanceCon.Description == "BottomCon" ||
                    instanceCon.Description == "BottomRightCon" ||
                    instanceCon.Description == "BottomLeftCon"||
                    instanceCon.Description == "TopRightCon" ||
                    instanceCon.Description == "TopLeftCon")
                {
                    scBoxInstanceConnectors.Add(instanceCon);
                    //allscBoxConnectors.Add(instanceCon);
                }
            }

            Connector BotCon = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "BottomCon");
            Connector BotRightCon = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "BottomRightCon");
            
            if (itemPointData.ConnectionEnd == PointDataStructs.ConnectionEnd.BOX)
            {
                var floorBoxInstanceConnectors = itemPointData.CreatedConnectionBoxInstane.MEPModel.ConnectorManager.Connectors;

                foreach (Connector fBoxCon in floorBoxInstanceConnectors)
                {
                    Conduit conduitInstance = Conduit.Create(
                        doc,
                        itemPointData.conduitType.Id,
                        BotCon.Origin,
                        fBoxCon.Origin,
                        itemPointData.TargetLevel.Id
                        );
                    var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                    diameter.Set(20 / mmInFt);
                    conduitInstance.ConnectorManager.Lookup(0).ConnectTo(BotCon);
                    conduitInstance.ConnectorManager.Lookup(1).ConnectTo(fBoxCon);

                    if (itemPointData.LinkedFixture != null)
                    {
                        conduitInstance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                            Set(itemPointData.SystemMoniker + "_" + itemPointData.LinkedFixture.Symbol.FamilyName);
                    }
                    
                }
            }
        } 
        public static void CreateConduit2(Document doc, PointData itemPointData)
        {
            if (itemPointData.CreatedScBoxInstane == null) { return; }
            //if (itemPointData.CreatedConnectionBoxInstane == null) { return; }
            //itemPointData.ConduitDirection = PointDataStructs.ConduitDirection.UP;

            double ConduitTurnOffset = itemPointData.ConnectionOffset;
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






        public static void CreateConnectionBox(Document doc, PointData itemPointData)
        {
            if( itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.DOWN)
            {
                if (itemPointData.ConnectionEnd == PointDataStructs.ConnectionEnd.BOX)
                {
                    XYZ CornerBoxLocation = itemPointData.LinkedFixtureLocation;
                    var ConnectionOffset = itemPointData.ConnectionOffset;

                    if (itemPointData.ConnectionOffset < MinOffsetDistance && 
                        itemPointData.ConnectionOffset > -MinOffsetDistance)
                    {
                        ConnectionOffset = 0;
                    }


                    CornerBoxLocation = new XYZ(
                    CornerBoxLocation.X + (ConnectionOffset / mmInFt) * Math.Cos(itemPointData.Rotation),
                    CornerBoxLocation.Y + (ConnectionOffset / mmInFt) * Math.Sin(itemPointData.Rotation),
                    (itemPointData.TargetLevel as Level).Elevation
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
    }
}