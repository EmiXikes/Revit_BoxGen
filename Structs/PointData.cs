using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EpicWallBoxGen.PointDataStructs;

namespace EpicWallBoxGen
{
    public class PointData
    {
        public FamilyInstance LinkedFixture;
        public FamilyInstance CreatedScBoxInstane;
        public FamilyInstance CreatedConnectionBoxInstane;

        public Element TargetLevel;
        public XYZ LinkedFixtureLocation;
        //public XYZ DirectionVector;
        public double Rotation;
        public double InstallationHeight;
        public string Description;
        
        public FamilySymbol scBoxFamSymbol;
        public FamilySymbol scFloorCornerFamSymbol;
        public ConduitType conduitType;

        public ConduitDirection ConduitDirection;
        public FixtureEnd FixtureEnd;
        public ConnectionEnd ConnectionEnd;
        public SeperateConduitLine SeperateConduitLine;

    }
}
