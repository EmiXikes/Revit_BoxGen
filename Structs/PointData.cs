using Autodesk.Revit.DB;
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
        public XYZ Location;
        public XYZ DirectionVector;
        public double Rotation;
        public double InstallationHeight;
        public string Description;

        ConduitDirection ConduitDirection;
        FixtureEnd FixtureEnd;
        ConnectionEnd ConnectionEnd;
        SeperateConduitLine SeperateConduitLine;

    }
}
