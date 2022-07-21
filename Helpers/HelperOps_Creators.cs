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
            #region Creating box fixture

            double verticalOffset = FixtureCenterOffset.Y;
            XYZ OffsetXYZ = new XYZ(0, 0, verticalOffset);

            itemPointData.LinkedFixtureLocation = itemPointData.LinkedFixtureLocation + OffsetXYZ;
            //
            //lnkFixtrLocation = lnkFixtrLocation + OffsetXYZ;
            Debug.WriteLine(String.Format("Fixture offset: [{0}]", OffsetXYZ));

            itemPointData.scBoxFamSymbol.Activate();
            itemPointData.CreatedScBoxInstane = doc.Create.NewFamilyInstance(
                itemPointData.LinkedFixtureLocation,
                itemPointData.scBoxFamSymbol,
                itemPointData.TargetLevel,
                Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            //itemPointData.CreatedScBoxInstane = scBoxInstance;

            RotateFamilyInstance(itemPointData.CreatedScBoxInstane, itemPointData.LinkedFixtureLocation, itemPointData.Rotation);

            itemPointData.CreatedScBoxInstane.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(itemPointData.LinkedFixture.Symbol.FamilyName);

            #endregion

            #region Creating Floor Corner Box

            if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.DOWN)
            {
                CreateConnectionBox(doc, itemPointData);

                CreateConduit(doc, itemPointData);

            }
            #endregion

            #region Creating Above Ceiling Box



            if (itemPointData.ConduitDirection == PointDataStructs.ConduitDirection.UP)
            {
                #region Location Correction
                XYZ CeilingBoxLocation = GetCeilingBoxLocation(doc, MySettings, itemPointData);

                #endregion
                //scCeilingCornerFamSymbol.Activate();
                //FamilyInstance ceilingBoxInstance = doc.Create.NewFamilyInstance(
                //    CornerBoxLocation,
                //    scCeilingCornerFamSymbol,
                //    TargetLevel,
                //    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                //RotateFamilyInstance(ceilingBoxInstance, CornerBoxLocation, RotationAngle);

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

                Connector TopCon = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "TopCon");
                XYZ CeilConduitPoint = new XYZ(TopCon.Origin.X, TopCon.Origin.Y, CeilingBoxLocation.Z);

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
            #endregion
        }

        private static void CreateConduit(Document doc, PointData itemPointData)
        {
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

            Connector BotCon = scBoxInstanceConnectors.FirstOrDefault(c => c.Description == "BottomCon");
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

        private static void CreateConnectionBox(Document doc, PointData itemPointData)
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
}