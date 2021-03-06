using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static EpicWallBoxGen.SourceDataStructs;

namespace EpicWallBoxGen
{
    
    internal class InputData
    {
        public static float mmInFt = 304.8f;

        #region INPUT DATA
        // Ipnut data 
        public static SourceType SourceDataType = SourceType.RVT;
        public static string FixtureBoxTypeName1 = "DOWN";
        public static string FixtureBoxTypeName2 = "UP";
        public static string SelectedTargetLevelName = "1st Floor";
        public static Vector2 FixtureCenterOffset = new Vector2(0, 42 / mmInFt);

        // Input RVT
        public static BuiltInCategory SelectedSourceCategory = BuiltInCategory.OST_ElectricalFixtures;
        public static List<string> SelectedNamesFilter = new List<string>();// { "Switch", "Socket"};

        public static string SelectedSourceDocName = "Baltezers house 2022";
        //public static string SelectedSourceDocName = "BALT59_HVAC";

        public static string SelectedCollsiionDocName = "";

        public static string SelectedSourceLevelName = "1. stāvs";
        //public static string SelectedSourceLevelName = "1st Floor";

        public static double FloorLevelOffset = 0;
        public static double CeilingLevelOffset = 0;

        public static string BoxFamilyTypeName = "Socket Box Concrete (d=50)";
        public static string FloorCornerSingleTypeName = "ConcreteVoid_FloorCorner_Single";
        public static string CeilingCornerSingleTypeName = "ConcreteVoid_CeilingCorner_Single";

        // Input data DWG

        #endregion
    }
}
