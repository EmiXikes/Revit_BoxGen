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
using static EpicWallBox.HelperOps_Creators;
using static EpicWallBox.HelperOps_NearestFinders;
using static EpicWallBox.InputData;
using Autodesk.Revit.UI.Selection;

namespace EpicWallBox
{
    #region BOT
    [Transaction(TransactionMode.Manual)]
    internal class ManualConduitBottomCenter : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            int ConduitSideDirection = 0;

            PointData pData = new PointData()
            {
                transferComments = true,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            pData.ConnectionSideOffset = pData.pSettings.ConduitSideOffset * ConduitSideDirection;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit Manual down");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = selectedFamInstance.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.CreatedScBoxInstane = selectedFamInstance;
                pData.LinkedFixtureLocation = (selectedFamInstance.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (selectedFamInstance.Location as LocationPoint).Rotation;

                CreateConnectionBox(doc, pData);

                CreateConduit2(doc, pData);
            }

            trans.Commit();
            return transResult;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class ManualConduitBottomLeft : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            int ConduitSideDirection = 1;

            PointData pData = new PointData()
            {
                transferComments = true,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            pData.ConnectionSideOffset = pData.pSettings.ConduitSideOffset * ConduitSideDirection;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit Manual down-left");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = selectedFamInstance.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.CreatedScBoxInstane = selectedFamInstance;
                pData.LinkedFixtureLocation = (selectedFamInstance.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (selectedFamInstance.Location as LocationPoint).Rotation;

                CreateConnectionBox(doc, pData);

                CreateConduit2(doc, pData);
            }

            trans.Commit();
            return transResult;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class ManualConduitBottomRight : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            int ConduitSideDirection = -1;

            PointData pData = new PointData()
            {
                transferComments = true,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            pData.ConnectionSideOffset = pData.pSettings.ConduitSideOffset * ConduitSideDirection;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit Manual down-right");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = selectedFamInstance.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.CreatedScBoxInstane = selectedFamInstance;
                pData.LinkedFixtureLocation = (selectedFamInstance.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (selectedFamInstance.Location as LocationPoint).Rotation;

                CreateConnectionBox(doc, pData);

                CreateConduit2(doc, pData);
            }

            trans.Commit();
            return transResult;
        }
    }
    #endregion

    #region TOP
    [Transaction(TransactionMode.Manual)]
    internal class ManualConduitTopCenter : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            int ConduitSideDirection = 0;

            PointData pData = new PointData()
            {
                transferComments = true,
                ConduitDirection = PointDataStructs.ConduitDirection.UP,
                ConnectionEnd = PointDataStructs.ConnectionEnd.CONDUIT
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            pData.ConnectionSideOffset = pData.pSettings.ConduitSideOffset * ConduitSideDirection;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit Manual up");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = selectedFamInstance.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.CreatedScBoxInstane = selectedFamInstance;
                pData.LinkedFixtureLocation = (selectedFamInstance.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (selectedFamInstance.Location as LocationPoint).Rotation;

                CreateConnectionBox(doc, pData);

                CreateConduit2(doc, pData);
            }

            trans.Commit();
            return transResult;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class ManualConduitTopLeft : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            int ConduitSideDirection = 1;

            PointData pData = new PointData()
            {
                transferComments = true,
                ConduitDirection = PointDataStructs.ConduitDirection.UP,
                ConnectionEnd = PointDataStructs.ConnectionEnd.CONDUIT
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            pData.ConnectionSideOffset = pData.pSettings.ConduitSideOffset * ConduitSideDirection;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit Manual up-left");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = selectedFamInstance.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.CreatedScBoxInstane = selectedFamInstance;
                pData.LinkedFixtureLocation = (selectedFamInstance.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (selectedFamInstance.Location as LocationPoint).Rotation;

                CreateConnectionBox(doc, pData);

                CreateConduit2(doc, pData);
            }

            trans.Commit();
            return transResult;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class ManualConduitTopRight : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            int ConduitSideDirection = -1;

            PointData pData = new PointData()
            {
                transferComments = true,
                ConduitDirection = PointDataStructs.ConduitDirection.UP,
                ConnectionEnd = PointDataStructs.ConnectionEnd.CONDUIT
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            pData.ConnectionSideOffset = pData.pSettings.ConduitSideOffset * ConduitSideDirection;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit Manual up-right");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = selectedFamInstance.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.CreatedScBoxInstane = selectedFamInstance;
                pData.LinkedFixtureLocation = (selectedFamInstance.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (selectedFamInstance.Location as LocationPoint).Rotation;

                CreateConnectionBox(doc, pData);

                CreateConduit2(doc, pData);
            }

            trans.Commit();
            return transResult;
        }
    }
    #endregion

    #region Additional Socket Boxes

    [Transaction(TransactionMode.Manual)]
    internal class ManualAddSocketBoxRight : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            // Which side the new socket box should be on.
            // 1 = left
            // -1 = right
            int CreationSide = -1;

            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionSideOffset = 0,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic SocketBox Add Right");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance SelectedFI = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = SelectedFI.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.LinkedFixtureLocation = (SelectedFI.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (SelectedFI.Location as LocationPoint).Rotation;

                // Location update, level offset and offset to side
                XYZ LevelOffset = new XYZ(0, 0, (pData.TargetLevel as Level).Elevation);
                double offsetDistance = CreationSide * pData.pSettings.AdjacentBoxOffset / mmInFt;
                XYZ OffsetLocation = pData.LinkedFixtureLocation;
                XYZ deltaOffset = new XYZ(
                    offsetDistance * Math.Cos(pData.Rotation),
                    offsetDistance * Math.Sin(pData.Rotation),
                    -(LevelOffset.Z));
                OffsetLocation = OffsetLocation + deltaOffset;

                // Creation
                pData.scBoxFamSymbol.Activate();
                var CreatedFI = doc.Create.NewFamilyInstance(
                    OffsetLocation,
                    pData.scBoxFamSymbol,
                    (Level)pData.TargetLevel,
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                RotateFamilyInstance(CreatedFI, OffsetLocation, pData.Rotation);

                // Copy comments
                CreatedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                    Set(SelectedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());

                // Connect to existing box
                var SelectedFICons = SelectedFI.MEPModel.ConnectorManager.Connectors;
                var CreatedFICons = CreatedFI.MEPModel.ConnectorManager.Connectors;

                if(pData.pSettings.AdjacentBoxOffset <= MinSocketWidth)
                {
                    foreach (Connector selectedFICon in SelectedFICons)
                    {
                        foreach (Connector createdFICon in CreatedFICons)
                        {
                            if (createdFICon.Origin.IsAlmostEqualTo(selectedFICon.Origin))
                            {
                                selectedFICon.ConnectTo(createdFICon);
                            }
                        }
                    }
                } else
                {
                    Connector sCon = null;
                    Connector cCon = null;
                    foreach (Connector selectedFICon in SelectedFICons)
                    {
                        if (selectedFICon.Description == "LeftCon") sCon = selectedFICon;
                    }

                    foreach (Connector createdFICon in CreatedFICons)
                    {
                        if (createdFICon.Description == "RightCon") cCon = createdFICon;
                    }

                    if (sCon != null && cCon != null)
                    {
                        Conduit conduitInstance = Conduit.Create(
                            doc,
                            pData.conduitType.Id,
                            sCon.Origin,
                            cCon.Origin,
                            pData.TargetLevel.Id
                            );

                        var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                        diameter.Set(25 / mmInFt);
                        conduitInstance.ConnectorManager.Lookup(0).ConnectTo(sCon);
                        conduitInstance.ConnectorManager.Lookup(1).ConnectTo(cCon);
                    }
                }

                uidoc.Selection.SetElementIds(new List<ElementId>() { CreatedFI.Id });

            }

            trans.Commit();
            return transResult;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class ManualAddSocketBoxLeft : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            // Which side the new socket box should be on.
            // 1 = left
            // -1 = right
            int CreationSide = 1;

            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionSideOffset = 0,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic SocketBox Add Left");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance SelectedFI = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = SelectedFI.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.LinkedFixtureLocation = (SelectedFI.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (SelectedFI.Location as LocationPoint).Rotation;

                // Location update, level offset and offset to side
                XYZ LevelOffset = new XYZ(0, 0, (pData.TargetLevel as Level).Elevation);
                double offsetDistance = CreationSide * pData.pSettings.AdjacentBoxOffset / mmInFt;
                XYZ OffsetLocation = pData.LinkedFixtureLocation;
                XYZ deltaOffset = new XYZ(
                    offsetDistance * Math.Cos(pData.Rotation),
                    offsetDistance * Math.Sin(pData.Rotation),
                    -(LevelOffset.Z));
                OffsetLocation += deltaOffset;

                // Creation
                pData.scBoxFamSymbol.Activate();
                var CreatedFI = doc.Create.NewFamilyInstance(
                    OffsetLocation,
                    pData.scBoxFamSymbol,
                    (Level)pData.TargetLevel,
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                RotateFamilyInstance(CreatedFI, OffsetLocation, pData.Rotation);

                // Copy comments
                CreatedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                    Set(SelectedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());

                // Connect to existing box
                var SelectedFICons = SelectedFI.MEPModel.ConnectorManager.Connectors;
                var CreatedFICons = CreatedFI.MEPModel.ConnectorManager.Connectors;

                if (pData.pSettings.AdjacentBoxOffset <= MinSocketWidth)
                {
                    foreach (Connector selectedFICon in SelectedFICons)
                    {
                        foreach (Connector createdFICon in CreatedFICons)
                        {
                            if (createdFICon.Origin.IsAlmostEqualTo(selectedFICon.Origin))
                            {
                                selectedFICon.ConnectTo(createdFICon);
                            }
                        }
                    }
                }
                else
                {
                    Connector sCon = null;
                    Connector cCon = null;
                    foreach (Connector selectedFICon in SelectedFICons)
                    {
                        if (selectedFICon.Description == "RightCon") sCon = selectedFICon;
                    }

                    foreach (Connector createdFICon in CreatedFICons)
                    {
                        if (createdFICon.Description == "LeftCon") cCon = createdFICon;
                    }

                    if (sCon != null && cCon != null)
                    {
                        Conduit conduitInstance = Conduit.Create(
                            doc,
                            pData.conduitType.Id,
                            sCon.Origin,
                            cCon.Origin,
                            pData.TargetLevel.Id
                            );

                        var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                        diameter.Set(25 / mmInFt);
                        conduitInstance.ConnectorManager.Lookup(0).ConnectTo(sCon);
                        conduitInstance.ConnectorManager.Lookup(1).ConnectTo(cCon);
                    }
                }

                uidoc.Selection.SetElementIds(new List<ElementId>() { CreatedFI.Id });

            }

            trans.Commit();
            return transResult;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class ManualAddSocketBoxTop : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            // Which side the new socket box should be on.
            // 1 = up
            // -1 = down
            int CreationSide = 1;

            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionSideOffset = 0,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic SocketBox Add Left");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance SelectedFI = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = SelectedFI.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.LinkedFixtureLocation = (SelectedFI.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (SelectedFI.Location as LocationPoint).Rotation;

                // Location update, level offset and offset to side
                XYZ LevelOffset = new XYZ(0, 0, (pData.TargetLevel as Level).Elevation);
                double offsetDistance = CreationSide * pData.pSettings.AdjacentBoxOffset / mmInFt;
                XYZ OffsetLocation = pData.LinkedFixtureLocation;
                XYZ deltaOffset = new XYZ(
                    0,
                    0,
                    -(LevelOffset.Z) + offsetDistance);
                OffsetLocation += deltaOffset;

                // Creation
                pData.scBoxFamSymbol.Activate();
                var CreatedFI = doc.Create.NewFamilyInstance(
                    OffsetLocation,
                    pData.scBoxFamSymbol,
                    (Level)pData.TargetLevel,
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                RotateFamilyInstance(CreatedFI, OffsetLocation, pData.Rotation);

                // Copy comments
                CreatedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                    Set(SelectedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());

                // Create a conduit between the two boxes and make the needed connections
                var SelectedFICons = SelectedFI.MEPModel.ConnectorManager.Connectors;
                var CreatedFICons = CreatedFI.MEPModel.ConnectorManager.Connectors;

                Connector sCon = null;
                Connector cCon = null;

                foreach (Connector selectedFICon in SelectedFICons)
                { 
                    if (selectedFICon.Description == "TopCon")
                    {
                        sCon = selectedFICon;
                    }
                }

                foreach (Connector createdFICon in CreatedFICons)
                {
                    if (createdFICon.Description == "BottomCon")
                    {
                        cCon = createdFICon;
                    }
                }

                if (sCon != null && cCon != null)
                {
                    Conduit conduitInstance = Conduit.Create(
                        doc,
                        pData.conduitType.Id,
                        sCon.Origin,
                        cCon.Origin,
                        pData.TargetLevel.Id
                        );

                    var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                    diameter.Set(20 / mmInFt);
                    conduitInstance.ConnectorManager.Lookup(0).ConnectTo(sCon);
                    conduitInstance.ConnectorManager.Lookup(1).ConnectTo(cCon);
                }

                uidoc.Selection.SetElementIds(new List<ElementId>() { CreatedFI.Id });
            }

            trans.Commit();
            return transResult;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class ManualAddSocketBoxBot : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            // Which side the new socket box should be on.
            // 1 = up
            // -1 = down
            int CreationSide = -1;

            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionSideOffset = 0,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic SocketBox Add Left");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance SelectedFI = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = SelectedFI.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.LinkedFixtureLocation = (SelectedFI.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (SelectedFI.Location as LocationPoint).Rotation;

                // Location update, level offset and offset to side
                XYZ LevelOffset = new XYZ(0, 0, (pData.TargetLevel as Level).Elevation);
                double offsetDistance = CreationSide * pData.pSettings.AdjacentBoxOffset / mmInFt;
                XYZ OffsetLocation = pData.LinkedFixtureLocation;
                XYZ deltaOffset = new XYZ(
                    0,
                    0,
                    -(LevelOffset.Z) + offsetDistance);
                OffsetLocation += deltaOffset;

                // Creation
                pData.scBoxFamSymbol.Activate();
                var CreatedFI = doc.Create.NewFamilyInstance(
                    OffsetLocation,
                    pData.scBoxFamSymbol,
                    (Level)pData.TargetLevel,
                    Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                RotateFamilyInstance(CreatedFI, OffsetLocation, pData.Rotation);

                // Copy comments
                CreatedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                    Set(SelectedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());

                // Create a conduit between the two boxes and make the needed connections
                var SelectedFICons = SelectedFI.MEPModel.ConnectorManager.Connectors;
                var CreatedFICons = CreatedFI.MEPModel.ConnectorManager.Connectors;

                Connector sCon = null;
                Connector cCon = null;

                foreach (Connector selectedFICon in SelectedFICons)
                {
                    if (selectedFICon.Description == "BottomCon")
                    {
                        sCon = selectedFICon;
                    }
                }

                foreach (Connector createdFICon in CreatedFICons)
                {
                    if (createdFICon.Description == "TopCon")
                    {
                        cCon = createdFICon;
                    }
                }

                if (sCon != null && cCon != null)
                {
                    Conduit conduitInstance = Conduit.Create(
                        doc,
                        pData.conduitType.Id,
                        sCon.Origin,
                        cCon.Origin,
                        pData.TargetLevel.Id
                        );

                    var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                    diameter.Set(20 / mmInFt);
                    conduitInstance.ConnectorManager.Lookup(0).ConnectTo(sCon);
                    conduitInstance.ConnectorManager.Lookup(1).ConnectTo(cCon);
                }

                uidoc.Selection.SetElementIds(new List<ElementId>() { CreatedFI.Id });
            }

            trans.Commit();
            return transResult;
        }
    }

    #region New SocketBox from picked

    [Transaction(TransactionMode.Manual)]
    internal class ManualAddSocketBoxFromPicked : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings

            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            PointData pData = new PointData()
            {
                transferComments = true,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };

            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            List<ElementId> CreatedElements = new List<ElementId>();

            while (1 == 1)
            {
                Transaction trans = new Transaction(doc);
                trans.Start("Epic SocketBox add from picked");
                try
                {
                    #region Picking the correct element

                    List<BuiltInCategory> AllowedCats = new List<BuiltInCategory>()
                        {
                            BuiltInCategory.OST_ElectricalFixtures,
                            BuiltInCategory.OST_GenericModel,
                        };

                    PickSelectionFilter PickFilter = new PickSelectionFilter(doc, AllowedCats);

                    var r = uidoc.Selection.PickObject(
                      ObjectType.PointOnElement, PickFilter,
                      "Pick something");

                    Element PickedElement;
                    var e = doc.GetElement(r.ElementId);
                    if (e is RevitLinkInstance)
                    {
                        Document LinkedDoc = ((RevitLinkInstance)e).GetLinkDocument();
                        PickedElement = LinkedDoc.GetElement(r.LinkedElementId);
                    }
                    else
                    {
                        PickedElement = e;
                    }

                    Debug.WriteLine(PickedElement.Name);

                    #endregion

                    pData.TargetLevel = GetClosestLevel(doc, r.GlobalPoint); ;
                    pData.LinkedFixture = (FamilyInstance)PickedElement;
                    pData.LinkedFixtureLocation = (PickedElement.Location as LocationPoint).Point;

                    #region Bounding Box
                    // BoundingBox variation

                    if (pData.pSettings.UseBoundingBox)
                    {
                        var bb = PickedElement.get_BoundingBox(null);
                        var bbc = (bb.Max + bb.Min) / 2;
                        pData.LinkedFixtureLocation = bbc;
                    }
                    // BoundingBox variation

                    #endregion

                    pData = WallCoordinateCorrection(doc, pData);

                    GetOrLoadSymbols(FamTypeNames, pData);

                    CreateSocketBox(doc, pData);
                    CreatedElements.Add(pData.CreatedScBoxInstane.Id);
                    
                    trans.Commit();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {   
                    //ESC pressed
                    uidoc.Selection.SetElementIds(CreatedElements);
                    trans.Commit();
                    return Result.Succeeded;
                }
                catch (Exception Ex)
                {
                    System.Windows.MessageBox.Show(Ex.Message);
                    return Result.Failed;
                }
            }
        }
        class PickSelectionFilter : ISelectionFilter
        {
            private Document _doc;
            private List<BuiltInCategory> _AllowedCats;

            public PickSelectionFilter(Document doc, List<BuiltInCategory> AllowedCats)
            {
                _doc = doc;
                _AllowedCats = AllowedCats;
            }
            public Document LinkedDocument { get; private set; } = null;
            public bool AllowElement(Element elem)
            {
                return true;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                Element e = _doc.GetElement(reference);

                if (e is RevitLinkInstance)
                {
                    RevitLinkInstance li = e as RevitLinkInstance;
                    LinkedDocument = li.GetLinkDocument();
                    e = LinkedDocument.GetElement(reference.LinkedElementId);
                }

                if (e != null)
                {
                    BuiltInCategory cat = 0;
                    cat = _AllowedCats.FirstOrDefault(x => ((int)x) == e.Category.Id.IntegerValue);

                    if (cat != 0 && cat != (BuiltInCategory)(-1)) return true;

                    //return e.Category.Id.IntegerValue == ((int)BuiltInCategory.OST_ElectricalFixtures);
                }
                return false;
            }
        }
    }


    #endregion


    #endregion

    [Transaction(TransactionMode.Manual)]
    internal class EmptyPlaceholderButton : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;

            #endregion

            Transaction trans = new Transaction(doc);
            trans.Start("Nothing");

            System.Windows.MessageBox.Show("Nothing here, but dragons.", "Nothing");

            trans.Commit();
            return transResult;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class DeleteConnected : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings

            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionSideOffset = 0,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            //GetSymbols(FamTypeNames, pData);

            //GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Delete Connected Stuff");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance SelectedFI = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                if (SelectedFI == null) continue;

                var epicID = SelectedFI.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                //GetOrLoadSymbols(FamTypeNames, pData);

                int SearchDepth = 10;

                List<ElementId> ConnectedElements = GetConnectedElementTree(doc, SelectedElement, SearchDepth);

                foreach (ElementId ElementId in ConnectedElements)
                {
                    if (SelectedElement.Id != ElementId)
                    {
                        doc.Delete(ElementId);
                    }
                }
            }

            trans.Commit();
            return transResult;
        }

        private static List<ElementId> GetConnectedElementTree(Document doc, Element SelectedElement, int SearchDepth)
        {
            List<ElementId> ConnectedElements = GetConnectedElements(SelectedElement, new List<ElementId>());

            for (int i = 0; i < SearchDepth - 1; i++)
            {
                foreach (ElementId ElementId in ConnectedElements)
                {
                    Element elm = doc.GetElement(ElementId);
                    var tempList = GetConnectedElements(elm, ConnectedElements);
                    ConnectedElements = tempList;
                }
            }

            return ConnectedElements;
        }

        private static List<ElementId> GetConnectedElements(Element SelectedElement, List<ElementId> ConnectedElements)
        {
            List<ElementId> elementIds = new List<ElementId>();
            elementIds.AddRange(ConnectedElements);

            //var refElement = SelectedElement;
            ConnectorSet ConSet = new ConnectorSet();
            if (SelectedElement.GetType() == typeof(Conduit))
            {
                ConSet = ((Conduit)SelectedElement).ConnectorManager.Connectors;
            }
            else
            {
                ConSet = ((FamilyInstance)SelectedElement).MEPModel.ConnectorManager.Connectors;
            }
            //if (SelectedElement.GetType() == typeof(FamilyInstance))
            //{
            //    ConSet = ((FamilyInstance)SelectedElement).MEPModel.ConnectorManager.Connectors;
            //}

            foreach (Connector connector in ConSet)
            {
                foreach (Connector C in connector.AllRefs)
                {
                    if (!elementIds.Contains(C.Owner.Id))
                    {
                        elementIds.Add(C.Owner.Id);
                    }
                }


            }

            return elementIds;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class SnapToSelected : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings

            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionSideOffset = 0,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            //GetSymbols(FamTypeNames, pData);

            //GetSettings(pData);
            List<ElementId> CreatedElements = new List<ElementId>();

            Transaction trans = new Transaction(doc);
            trans.Start("Snap to Selected");

            while (1 == 1)
            {
                try
                {
                    var selection = uidoc.Selection.GetElementIds();

                    if (selection.Count != 1)
                    {
                        trans.Commit();
                        return Result.Cancelled;
                    }

                    Element SelectedElement = doc.GetElement(selection.First());
                    FamilyInstance SelectedFI = (FamilyInstance)SelectedElement;

                    if (!CreatedElements.Contains(SelectedFI.Id)) CreatedElements.Add(SelectedFI.Id);

                    // Validity check
                    #region Validity check

                    var epicID = SelectedFI.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                    if (epicID == null)
                    {
                        trans.Commit();
                        return Result.Cancelled;
                    }
                    else
                    {
                        if (epicID.AsString() != EpicID_SocketBox)
                        {
                            trans.Commit();
                            return Result.Cancelled;
                        };
                    }
                    #endregion

                    WallBoxPickFilter WallBoxPickFilter = new WallBoxPickFilter(doc);

                    var r = uidoc.Selection.PickObject(
                        ObjectType.Element, WallBoxPickFilter,
                        "Pick WallBox to snap to selection");
                    Element PickedElement = doc.GetElement(r.ElementId);
                    FamilyInstance PickedFI = (FamilyInstance)PickedElement;

                    var SelectedFICons = SelectedFI.MEPModel.ConnectorManager.Connectors;
                    var PickedFICons = PickedFI.MEPModel.ConnectorManager.Connectors;

                    XYZ deltaVector = XYZ.Zero;
                    Connector sConClosest = null;
                    Connector pConClosest = null;
                    double d = 1000000;

                    foreach (Connector sCon in SelectedFICons)
                    {
                        foreach (Connector pCon in PickedFICons)
                        {
                            double dt = sCon.Origin.DistanceTo(pCon.Origin);
                            if (dt < d)
                            {
                                d = dt;
                                deltaVector = sCon.Origin - pCon.Origin;
                                sConClosest = sCon;
                                pConClosest = pCon;
                            }
                        }
                    }

                    //CreateDebugMarker(doc, sConClosest.Origin);
                    //CreateDebugMarker(doc, pConClosest.Origin);

                    XYZ newPickedLocation = (PickedFI.Location as LocationPoint).Point + deltaVector;
                    (PickedFI.Location as LocationPoint).Point = newPickedLocation;

                    sConClosest.ConnectTo(pConClosest);
                    if (!CreatedElements.Contains(PickedFI.Id)) CreatedElements.Add(PickedFI.Id);

                    uidoc.Selection.SetElementIds(new List<ElementId>() { PickedFI.Id });

                    //trans.Commit();
                    //return transResult;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    //ESC pressed
                    uidoc.Selection.SetElementIds(CreatedElements);
                    trans.Commit();
                    return Result.Succeeded;
                }
                catch (Exception Ex)
                {
                    System.Windows.MessageBox.Show(Ex.Message);
                    return Result.Failed;
                }
            }


        }

        class WallBoxPickFilter : ISelectionFilter
        {
            private Document _doc;

            public WallBoxPickFilter(Document doc)
            {
                _doc = doc;
            }   

            public bool AllowElement(Element elem)
            {
                return true;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                Element e = _doc.GetElement(reference);

                if (e != null)
                {
                    if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment) return true;
                }
                return false;
            }
        }

    }

    [Transaction(TransactionMode.Manual)]
    internal class ConduitToSelected : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings

            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionSideOffset = 0,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            

            //GetSettings(pData);
            List<ElementId> CreatedElements = new List<ElementId>();

            Transaction trans = new Transaction(doc);
            trans.Start("Snap to Selected");

            GetOrLoadSymbols(FamTypeNames, pData);

            while (1 == 1)
            {
                try
                {
                    var selection = uidoc.Selection.GetElementIds();

                    if (selection.Count != 1)
                    {
                        trans.Commit();
                        return Result.Cancelled;
                    }

                    Element SelectedElement = doc.GetElement(selection.First());
                    FamilyInstance SelectedFI = (FamilyInstance)SelectedElement;

                    // Validity check
                    #region Validity check

                    var epicID = SelectedFI.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                    if (epicID == null)
                    {
                        trans.Commit();
                        return Result.Cancelled;
                    }
                    else
                    {
                        if (epicID.AsString() != EpicID_SocketBox)
                        {
                            trans.Commit();
                            return Result.Cancelled;
                        };
                    }
                    #endregion

                    pData.TargetLevel = GetElementLevel(doc, SelectedElement); ;
                    if (!CreatedElements.Contains(SelectedFI.Id)) CreatedElements.Add(SelectedFI.Id);

                    WallBoxPickFilter WallBoxPickFilter = new WallBoxPickFilter(doc);

                    var r = uidoc.Selection.PickObject(
                        ObjectType.Element, WallBoxPickFilter,
                        "Pick a WallBox to create a conduit to..");
                    Element PickedElement = doc.GetElement(r.ElementId);
                    FamilyInstance PickedFI = (FamilyInstance)PickedElement;

                    var SelectedFICons = SelectedFI.MEPModel.ConnectorManager.Connectors;
                    var PickedFICons = PickedFI.MEPModel.ConnectorManager.Connectors;

                    XYZ deltaVector = XYZ.Zero;
                    Connector sConClosest = null;
                    Connector pConClosest = null;
                    double d = 1000000;

                    foreach (Connector sCon in SelectedFICons)
                    {
                        foreach (Connector pCon in PickedFICons)
                        {
                            double dt = sCon.Origin.DistanceTo(pCon.Origin);
                            if (dt < d)
                            {
                                d = dt;
                                deltaVector = sCon.Origin - pCon.Origin;
                                sConClosest = sCon;
                                pConClosest = pCon;
                            }
                        }
                    }

                    //CreateDebugMarker(doc, sConClosest.Origin);
                    //CreateDebugMarker(doc, pConClosest.Origin);


                    // Create conduit if there is no vertical offset.
                    // TODO needs to change
                    if (new XYZ(sConClosest.Origin.X, sConClosest.Origin.Y, 0)
                        .IsAlmostEqualTo(new XYZ(pConClosest.Origin.X, pConClosest.Origin.Y, 0)))
                    {
                        Conduit conduitInstance = Conduit.Create(
                            doc,
                            pData.conduitType.Id,
                            sConClosest.Origin,
                            pConClosest.Origin,
                            pData.TargetLevel.Id
                            );
                        var diameter = conduitInstance.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                        diameter.Set(20 / mmInFt);
                        conduitInstance.ConnectorManager.Lookup(0).ConnectTo(sConClosest);
                        conduitInstance.ConnectorManager.Lookup(1).ConnectTo(pConClosest);

                        var sComments = SelectedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                        if (sComments != null)
                        {
                            conduitInstance.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                                Set(sComments.AsString());
                        }
                    }





                    //XYZ newPickedLocation = (PickedFI.Location as LocationPoint).Point + deltaVector;
                    //(PickedFI.Location as LocationPoint).Point = newPickedLocation;

                    //sConClosest.ConnectTo(pConClosest);
                    //if (!CreatedElements.Contains(PickedFI.Id)) CreatedElements.Add(PickedFI.Id);

                    uidoc.Selection.SetElementIds(new List<ElementId>() { PickedFI.Id });

                    //trans.Commit();
                    //return transResult;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    //ESC pressed
                    //uidoc.Selection.SetElementIds(CreatedElements);
                    trans.Commit();
                    return Result.Succeeded;
                }
                catch (Exception Ex)
                {
                    System.Windows.MessageBox.Show(Ex.Message);
                    return Result.Failed;
                }
            }


        }

        class WallBoxPickFilter : ISelectionFilter
        {
            private Document _doc;

            public WallBoxPickFilter(Document doc)
            {
                _doc = doc;
            }

            public bool AllowElement(Element elem)
            {
                return true;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                Element e = _doc.GetElement(reference);

                if (e != null)
                {
                    if (e.Category.Id.IntegerValue == (int)BuiltInCategory.OST_MechanicalEquipment) return true;
                }
                return false;
            }
        }

    }

    [Transaction(TransactionMode.Manual)]
    internal class ConduitTest03 : HelperOps, IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region input settings
            ManualWallBoxFamilyTypeNames FamTypeNames = new ManualWallBoxFamilyTypeNames()
            {
                conduitTypeName = ConduitTypeName,
                scBoxFamTypeName = SocketBoxFamilyTypeName,
                conBoxBotFamTypeName = ConnectionBoxBottomSingleTypeName,
                conBoxTopFamTypeName = ConnectionBoxTopSingleTypeName
            };

            int ConduitSideDirection = 0;

            PointData pData = new PointData()
            {
                transferComments = true,
                ConduitDirection = PointDataStructs.ConduitDirection.DOWN,
                ConnectionEnd = PointDataStructs.ConnectionEnd.BOX
            };
            #endregion

            #region transaction stuff setup
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            //Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;
            Document doc = uidoc.Document;
            Result transResult = Result.Succeeded;
            pData.doc = doc;
            #endregion

            GetSettings(pData);

            pData.ConnectionSideOffset = pData.pSettings.ConduitSideOffset * ConduitSideDirection;

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit Manual down");

            var selection = uidoc.Selection.GetElementIds();

            foreach (ElementId selectedElementId in selection)
            {

                Element SelectedElement = doc.GetElement(selectedElementId);
                FamilyInstance selectedFamInstance = (FamilyInstance)SelectedElement;

                // Validity check
                #region Validity check
                var epicID = selectedFamInstance.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

                if (epicID == null)
                {
                    continue;
                }
                else
                {
                    //var s1 = epicID.AsString();
                    //var s2 = EpicID_SocketBox;
                    if (epicID.AsString() != EpicID_SocketBox) continue;
                }
                #endregion

                GetOrLoadSymbols(FamTypeNames, pData);

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.CreatedScBoxInstane = selectedFamInstance;
                pData.LinkedFixtureLocation = (selectedFamInstance.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (selectedFamInstance.Location as LocationPoint).Rotation;

                //CreateConnectionBox(doc, pData);

                CreateConduit3(doc, pData);
            }

            trans.Commit();
            return transResult;
        }



    }
}
