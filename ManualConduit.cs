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
using static EpicWallBox.InputData;

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

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionOffset = 0,
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

            GetSymbols(FamTypeNames, pData);

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit X");

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

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionOffset = 100,
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

            GetSymbols(FamTypeNames, pData);

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit");

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

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionOffset = -100,
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

            GetSymbols(FamTypeNames, pData);

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit");

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

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionOffset = 0,
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

            GetSymbols(FamTypeNames, pData);

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit");

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

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionOffset = 100,
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

            GetSymbols(FamTypeNames, pData);

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit");

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

            PointData pData = new PointData()
            {
                transferComments = true,
                ConnectionOffset = -100,
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

            GetSymbols(FamTypeNames, pData);

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Epic Conduit");

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
                ConnectionOffset = 0,
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

            GetSymbols(FamTypeNames, pData);

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Add SocketBox Right");

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

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.LinkedFixtureLocation = (SelectedFI.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (SelectedFI.Location as LocationPoint).Rotation;
                pData.SocketWidthOffset = SocketWidthOffset / mmInFt;
                // Try to get socket box width from actual element
                var SocketBoxWidth = SelectedFI.get_Parameter(new System.Guid("7e384aa5-3ee2-4c95-8162-ddb4d9fa7c74"));
                if (SocketBoxWidth != null)
                {
                    pData.SocketWidthOffset = SocketBoxWidth.AsDouble();
                }

                // Location update, level offset and offset to side
                XYZ LevelOffset = new XYZ(0, 0, (pData.TargetLevel as Level).Elevation);
                double offsetDistance = CreationSide * pData.SocketWidthOffset;
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

                RotateFamilyInstance(CreatedFI, pData.LinkedFixtureLocation, pData.Rotation);

                // Copy comments
                CreatedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                    Set(SelectedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());

                // Connect to existing box
                var SelectedFICons = SelectedFI.MEPModel.ConnectorManager.Connectors;
                var CreatedFICons = CreatedFI.MEPModel.ConnectorManager.Connectors;

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
                ConnectionOffset = 0,
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

            GetSymbols(FamTypeNames, pData);

            GetSettings(pData);

            Transaction trans = new Transaction(doc);
            trans.Start("Add SocketBox Left");

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

                Element TargetLevel = GetElementLevel(doc, SelectedElement);

                pData.LinkedFixtureLocation = (SelectedFI.Location as LocationPoint).Point;
                pData.TargetLevel = TargetLevel;
                pData.Rotation = (SelectedFI.Location as LocationPoint).Rotation;
                pData.SocketWidthOffset = SocketWidthOffset / mmInFt;
                // Try to get socket box width from actual element
                var SocketBoxWidth = SelectedFI.get_Parameter(new System.Guid("7e384aa5-3ee2-4c95-8162-ddb4d9fa7c74"));
                if (SocketBoxWidth != null)
                {
                    pData.SocketWidthOffset = SocketBoxWidth.AsDouble();
                }

                // Location update, level offset and offset to side
                XYZ LevelOffset = new XYZ(0, 0, (pData.TargetLevel as Level).Elevation);
                double offsetDistance = CreationSide * pData.SocketWidthOffset;
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

                RotateFamilyInstance(CreatedFI, pData.LinkedFixtureLocation, pData.Rotation);

                // Copy comments
                CreatedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
                    Set(SelectedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());

                // Connect to existing box
                var SelectedFICons = SelectedFI.MEPModel.ConnectorManager.Connectors;
                var CreatedFICons = CreatedFI.MEPModel.ConnectorManager.Connectors;

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

                uidoc.Selection.SetElementIds(new List<ElementId>() { CreatedFI.Id });

            }

            trans.Commit();
            return transResult;
        }
    }

    [Transaction(TransactionMode.Manual)]
    internal class ManualCenterPlaceholder : HelperOps, IExternalCommand
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
                ConnectionOffset = 0,
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
            trans.Start("Nothing");

            System.Windows.MessageBox.Show("What should this button do?","Question");

            //var selection = uidoc.Selection.GetElementIds();

            //foreach (ElementId selectedElementId in selection)
            //{

            //    Element SelectedElement = doc.GetElement(selectedElementId);
            //    FamilyInstance SelectedFI = (FamilyInstance)SelectedElement;

            //    // Validity check
            //    #region Validity check
            //    var epicID = SelectedFI.Symbol.get_Parameter(new System.Guid("e66469d5-4b01-4d47-be17-c3ce2224aac7"));

            //    if (epicID == null)
            //    {
            //        continue;
            //    }
            //    else
            //    {
            //        //var s1 = epicID.AsString();
            //        //var s2 = EpicID_SocketBox;
            //        if (epicID.AsString() != EpicID_SocketBox) continue;
            //    }
            //    #endregion

            //    Element TargetLevel = GetElementLevel(doc, SelectedElement);

            //    pData.LinkedFixtureLocation = (SelectedFI.Location as LocationPoint).Point;
            //    pData.TargetLevel = TargetLevel;
            //    pData.Rotation = (SelectedFI.Location as LocationPoint).Rotation;
            //    pData.SocketWidthOffset = SocketWidthOffset / mmInFt;
            //    // Try to get socket box width from actual element
            //    var SocketBoxWidth = SelectedFI.get_Parameter(new System.Guid("7e384aa5-3ee2-4c95-8162-ddb4d9fa7c74"));
            //    if (SocketBoxWidth != null)
            //    {
            //        pData.SocketWidthOffset = SocketBoxWidth.AsDouble();
            //    }

            //    // Location update, level offset and offset to side
            //    XYZ LevelOffset = new XYZ(0, 0, (pData.TargetLevel as Level).Elevation);
            //    double offsetDistance = CreationSide * pData.SocketWidthOffset;
            //    XYZ OffsetLocation = pData.LinkedFixtureLocation;
            //    XYZ deltaOffset = new XYZ(
            //        offsetDistance * Math.Cos(pData.Rotation),
            //        offsetDistance * Math.Sin(pData.Rotation),
            //        -(LevelOffset.Z));
            //    OffsetLocation += deltaOffset;

            //    // Creation
            //    pData.scBoxFamSymbol.Activate();
            //    var CreatedFI = doc.Create.NewFamilyInstance(
            //        OffsetLocation,
            //        pData.scBoxFamSymbol,
            //        (Level)pData.TargetLevel,
            //        Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

            //    RotateFamilyInstance(CreatedFI, pData.LinkedFixtureLocation, pData.Rotation);

            //    // Copy comments
            //    CreatedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).
            //        Set(SelectedFI.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString());

            //    // Connect to existing box
            //    var SelectedFICons = SelectedFI.MEPModel.ConnectorManager.Connectors;
            //    var CreatedFICons = CreatedFI.MEPModel.ConnectorManager.Connectors;

            //    foreach (Connector selectedFICon in SelectedFICons)
            //    {
            //        foreach (Connector createdFICon in CreatedFICons)
            //        {
            //            if (createdFICon.Origin.IsAlmostEqualTo(selectedFICon.Origin))
            //            {
            //                selectedFICon.ConnectTo(createdFICon);
            //            }
            //        }
            //    }

            //    uidoc.Selection.SetElementIds(new List<ElementId>() { CreatedFI.Id });

            //}

            trans.Commit();
            return transResult;
        }
    }


    #endregion

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
                ConnectionOffset = 0,
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

}
