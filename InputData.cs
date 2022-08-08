using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static EpicWallBox.SourceDataStructs;

namespace EpicWallBox
{
    
    internal class InputData
    {
        public static float mmInFt = 304.8f;
        public static double MinOffsetDistance = 50;
        public static double MinSocketWidth = 71;

        #region INPUT DATA
        // Ipnut data 
        public static SourceType SourceDataType = SourceType.RVT;
        //public static string FixtureBoxTypeName1 = "DOWN";
        //public static string FixtureBoxTypeName2 = "UP";
        //public static string SelectedTargetLevelName = "Roof 1";
        public static string SelectedTargetLevelName = "1st Floor";
        //public static Vector2 FixtureCenterOffset = new Vector2(0, 42 / mmInFt); <-- moved to settings

        // Input RVT
        public static BuiltInCategory SelectedSourceCategory = BuiltInCategory.OST_ElectricalFixtures;
        public static List<string> SelectedNamesFilter = new List<string>();// { "Switch", "Socket"};

        public static string SelectedSourceDocName = "Baltezers house (BP)";
        //public static string SelectedSourceDocName = "Baltezers house 2022";
        //public static string SelectedSourceDocName = "BALT59_HVAC";

        //public static string SelectedCollsiionDocName = "";

        //public static string SelectedSourceLevelName = "Roof 1";
        public static string SelectedSourceLevelName = "1. stāvs";
        //public static string SelectedSourceLevelName = "1st Floor";

        //public static double FloorLevelOffset = 0;
        //public static double CeilingLevelOffset = 0;

        #region Family Resources

        public static string FamilyResourcesPath = @"C:\Epic\RevitAddInsSetup\Resources\Families\BoxGen";
        // Family names (file names)
        public static string ScBoxFamilyName = "ConcreteBox";
        public static string ConBoxBotFamilyName = "ConcreteVoid_FloorCorner_Single";
        public static string ConduitBendFamilyName = "ConcreteConduitBend";

        // Type names
        public static string SocketBoxFamilyTypeName = "Socket Box Concrete (d=50)";
        public static string ConnectionBoxBottomSingleTypeName = "ConcreteVoid_FloorCorner_Single";
        public static string ConnectionBoxTopSingleTypeName = "ConcreteVoid_CeilingCorner_Single";
        public static string ConduitTypeName = "Concrete Conduit";

        #endregion

        public static string EpicID_SocketBox = "WallBox_SocketBox";

        // Input data DWG

        #endregion
    }
}
