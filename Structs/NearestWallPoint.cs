using Autodesk.Revit.DB;
using System;

namespace EpicWallBox
{
    class NearestWallPoint
    {
        public XYZ SearchDirection;
        public XYZ FoundPoint;
        public Double Distance;
        public Wall CollisionWall;
        public XYZ WallSurfaceVector;
        public View3D CollisionView;
        public Document CollisionDoc;
        public bool IsNewPointFound;
    }

}
