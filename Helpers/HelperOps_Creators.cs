using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static EpicWallBoxGen.InputData;

namespace EpicWallBoxGen
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

        public static void CreatePointElements(Document doc, WallSnapSettingsData MySettings, PointData itemPointData)
        {
            CreateSocketBox(doc, itemPointData);

            CreateConnectionBox(doc, itemPointData, MySettings);

            CreateConduit(doc, itemPointData);

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

            itemPointData.CreatedScBoxInstane.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(itemPointData.LinkedFixture.Symbol.FamilyName);
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
                }
            }
        } 
        public static void CreateConduit2(Document doc, PointData itemPointData)
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

            double PointRotation = itemPointData.Rotation;

            double CndAngle = 33 * (Math.PI/180);
            double CndAngleHOffset = Math.Sin(CndAngle);
            double CndAngleVOffset = -1 * Math.Cos(CndAngle);

            var dt = new XYZ(BotRightCon.Origin.X, BotRightCon.Origin.Y, 0).DistanceTo(
                            new XYZ(BotCon.Origin.X, BotCon.Origin.Y, 0));

            CndAngleHOffset = (100 / mmInFt) - dt;
            CndAngleVOffset = -1 * CndAngleHOffset / Math.Tan(CndAngle);


            XYZ Point1 = BotRightCon.Origin;
            XYZ Point2 = BotRightCon.Origin + 
                new XYZ(
                    CndAngleHOffset * Math.Cos(PointRotation),
                    CndAngleHOffset * Math.Sin(PointRotation),
                    CndAngleVOffset
                    );
            XYZ Point3 = new XYZ(Point2.X, Point2.Y, 0);

            Conduit conduitInstance = Conduit.Create(
                doc,
                itemPointData.conduitType.Id,
                Point1,
                Point2,
                itemPointData.TargetLevel.Id
                );
            var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
            diameter.Set(20 / mmInFt);
            conduitInstance.ConnectorManager.Lookup(0).ConnectTo(BotRightCon);

            //var takeoff = doc.Create.NewTakeoffFitting(conduitInstance.ConnectorManager.Lookup(1), conduitInstance);


            //next segment
            Conduit conduitInstance2 = Conduit.Create(
                doc,
                itemPointData.conduitType.Id,
                Point2,
                Point3,
                itemPointData.TargetLevel.Id
                );



            var p = itemPointData.conduitType.RoutingPreferenceManager;
            diameter = conduitInstance2.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
            diameter.Set(20 / mmInFt);

            var fittingInstance = doc.Create.NewElbowFitting(conduitInstance.ConnectorManager.Lookup(1), conduitInstance2.ConnectorManager.Lookup(0));


        }
        public static void CreateConnectionBox(Document doc, PointData itemPointData, WallSnapSettingsData MySettings)
        {
            if( itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.DOWN)
            {
                if (itemPointData.ConnectionEnd == PointDataStructs.ConnectionEnd.BOX)
                {
                    XYZ CornerBoxLocation = itemPointData.LinkedFixtureLocation;

                    CornerBoxLocation = new XYZ(
                        CornerBoxLocation.X,
                        CornerBoxLocation.Y,
                        (itemPointData.TargetLevel as Level).Elevation
                        );

                    itemPointData.scFloorCornerFamSymbol.Activate();
                    itemPointData.CreatedConnectionBoxInstane = doc.Create.NewFamilyInstance(
                        CornerBoxLocation,
                        itemPointData.scFloorCornerFamSymbol,
                        itemPointData.TargetLevel,
                        Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                    RotateFamilyInstance(itemPointData.CreatedConnectionBoxInstane, CornerBoxLocation, itemPointData.Rotation);
                }
            }

            if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.UP)
            {
                if (itemPointData.ConnectionEnd == PointDataStructs.ConnectionEnd.CONDUIT)
                {
                    
                    // get scBox instance connectors
                    var insCons = itemPointData.CreatedScBoxInstane.MEPModel.ConnectorManager.Connectors;
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

                    XYZ CeilingBoxLocation = GetCeilingBoxLocation(doc, MySettings, itemPointData);

                    Connector TopCon = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "TopCon");
                    XYZ CeilConduitPoint = new XYZ(TopCon.Origin.X, TopCon.Origin.Y, CeilingBoxLocation.Z);

                    //CreateDebugMarker(doc, CeilConduitPoint);

                    Conduit conduitInstance = Conduit.Create(
                        doc,
                        itemPointData.conduitType.Id,
                        TopCon.Origin,
                        CeilConduitPoint,
                        itemPointData.TargetLevel.Id
                        );
                    var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                    diameter.Set(20 / mmInFt);
                    conduitInstance.ConnectorManager.Lookup(0).ConnectTo(TopCon);
                }

                
            }


            
        }
    }
}