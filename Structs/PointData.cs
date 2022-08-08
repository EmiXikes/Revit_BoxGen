using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EpicWallBox.PointDataStructs;
using static EpicWallBox.SettingsSchema_WallSnap;

namespace EpicWallBox
{
    public class PointData
    {
        public Document doc;

        public FamilyInstance LinkedFixture;
        public FamilyInstance CreatedScBoxInstane;
        public FamilyInstance CreatedConnectionBoxInstane;
        // yes
        public Element TargetLevel;
        public XYZ LinkedFixtureLocation;
        //public XYZ DirectionVector;
        public double Rotation;
        public double InstallationHeight;
        public string Description;
        public string SystemMoniker;

        public bool transferComments = false;

        public SettingsObj pSettings;
        public double ConnectionSideOffset;
        //public double AdjacentBoxOffset = 71;
        //public double ConduitSideOffset = 100;
        
        public FamilySymbol scBoxFamSymbol;
        public FamilySymbol conBoxBotFamSymbol;
        public FamilySymbol conBoxTopFamSymbol;
        public ConduitType conduitType;

        public ConduitDirection ConduitDirection;
        public FixtureEnd FixtureEnd;
        public ConnectionEnd ConnectionEnd;
        public SeperateConduitLine SeperateConduitLine;

    }

    public class ManualWallBoxFamilyTypeNames
    {
        public string scBoxFamTypeName;
        public string conBoxBotFamTypeName;
        public string conBoxTopFamTypeName;
        public string conduitTypeName;
    }
}
