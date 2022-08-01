using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using static EpicWallBox.HelperOps_Creators;

namespace EpicWallBox
{
    internal static class HelperOps_NearestFinders
    {
        public static NearestWallPoint FindNearestWallPoint(
            XYZ ReferencePoint,
            View3D CollisionView,
            Document CollisionDoc,
            List<BuiltInCategory> CollisionCatsWall,
            double revDist,
            double fwdDist,
            XYZ prefferedDirection = null)
        {
            //CreateDebugMarker(CollisionView.Document, ReferencePoint);

            List<NearestWallPoint> CollisionTestDirections = new List<NearestWallPoint>()
                    {
                        new NearestWallPoint() {SearchDirection = new XYZ(1,0,0) },
                        new NearestWallPoint() {SearchDirection = new XYZ(1,1,0) },
                        new NearestWallPoint() {SearchDirection = new XYZ(0,1,0) },
                        new NearestWallPoint() {SearchDirection = new XYZ(-1,1,0) },
                        new NearestWallPoint() {SearchDirection = new XYZ(-1,0,0) },
                        new NearestWallPoint() {SearchDirection = new XYZ(-1,-1,0) },
                        new NearestWallPoint() {SearchDirection = new XYZ(0,-1,0) },
                        new NearestWallPoint() {SearchDirection = new XYZ(1,-1,0) },
                    };

            if (prefferedDirection != null)
            {
                CollisionTestDirections = new List<NearestWallPoint>()
                {
                    new NearestWallPoint() {SearchDirection = prefferedDirection},
                    new NearestWallPoint() {SearchDirection = prefferedDirection * -1},
                };
            }

            foreach (var WallProximityTest in CollisionTestDirections)
            {
                //CreateDebugMarker(CollisionView.Document, ReferencePoint + WallProximityTest.SearchDirection);

                WallProximityTest.Distance = 100000;
                WallProximityTest.CollisionDoc = CollisionDoc;

                //List<BuiltInCategory> CollisionCatsWall = new List<BuiltInCategory>()
                //        {
                //            BuiltInCategory.OST_Walls
                //        };

                var WallFound = WallIntersectionRef(
                    ReferencePoint,
                    WallProximityTest.SearchDirection,
                    revDist,
                    fwdDist,
                    CollisionView,
                    CollisionCatsWall
                    );

                if (WallFound != null)
                {
                    //CreateDebugMarker(CollisionView.Document, WallFound.GlobalPoint);
                    Element CollisionElement = WallProximityTest.CollisionDoc.GetElement(WallFound.LinkedElementId);
                    Wall CollisionWall = (Wall)CollisionElement;

                    WallProximityTest.CollisionWall = CollisionWall;
                    WallProximityTest.CollisionView = CollisionView;

                    NearestWallPoint CT = GetClosestPointOnWall(ReferencePoint, WallProximityTest, CollisionCatsWall);

                    if (CT.IsNewPointFound)
                    {
                        WallProximityTest.FoundPoint = CT.FoundPoint;
                        WallProximityTest.Distance = ReferencePoint.DistanceTo(CT.FoundPoint);
                        WallProximityTest.WallSurfaceVector = CT.WallSurfaceVector;
                        WallProximityTest.IsNewPointFound = CT.IsNewPointFound;
                    }

                }
            }

            var ClosestCollision = CollisionTestDirections.
                First(c => c.Distance == CollisionTestDirections.Min(d => d.Distance));

            return ClosestCollision;
        }

        public static XYZ GetCeilingPoints(
            View3D view3D, 
            XYZ initialPosition, 
            double fwdDistance, 
            List<BuiltInCategory> builtInCats, 
            out List<ReferenceWithContext> snapRefs)
        {

            XYZ initialDeltaPosition = initialPosition;
            XYZ rayDirection = new XYZ(0, 0, 1);

            ElementMulticategoryFilter intersectFilter
              = new ElementMulticategoryFilter(builtInCats);


            ReferenceIntersector refIntersector;
            ReferenceWithContext referenceWithContext;

            refIntersector = new ReferenceIntersector(intersectFilter, FindReferenceTarget.Element, view3D);
            refIntersector.FindReferencesInRevitLinks = true;
            //refIntersector = new ReferenceIntersector(view3D) { FindReferencesInRevitLinks = true };
            //referenceWithContext = refIntersector.FindNearest(initialDeltaPosition, rayDirection);

            List<ReferenceWithContext> foundRefs = refIntersector.Find(initialDeltaPosition, rayDirection).ToList();

            //CreateDebugMarker(view3D.Document, initialDeltaPosition);

            if (foundRefs.Count > 0)
            {
                var rres = foundRefs.OrderBy(p => p.Proximity).First().GetReference().GlobalPoint;
                snapRefs = foundRefs;
                return rres;
            }

            //if (referenceWithContext != null)
            //{
            //    //var i = referenceWithContext.GetType();
            //    snapRef = referenceWithContext.GetReference();
            //    return referenceWithContext.GetReference().GlobalPoint;
            //}
            snapRefs = null;
            return initialPosition;
        }

        public static NearestWallPoint GetClosestPointOnWall(
            XYZ TestPoint, 
            NearestWallPoint WallDataItem, 
            List<BuiltInCategory> CollisionCatsWall)
        {
            NearestWallPoint Result = new NearestWallPoint()
            {
                CollisionWall = WallDataItem.CollisionWall,
                SearchDirection = WallDataItem.SearchDirection
            };

            double ReferenceDistance = 1000000;

            GeometryElement WallGeometry = WallDataItem.CollisionWall.get_Geometry(
                new Options() { ComputeReferences = true });

            foreach (GeometryObject WallGeometryItem in WallGeometry)
            {
                Solid SolidObejct = WallGeometryItem as Solid;
                if (SolidObejct != null)
                {
                    // Loop through all wall faces
                    foreach (Face WallFace in SolidObejct.Faces)
                    {
                        // Get intersection points along face surface normals
                        XYZ SurfaceNormal = ((PlanarFace)WallFace).FaceNormal;

                        // Nullify UP/DOWN because of false positives from solid top/bottom faces
                        SurfaceNormal = new XYZ(SurfaceNormal.X, SurfaceNormal.Y, SurfaceNormal.Z * 0);

                        //ReferenceIntersector WallFinder = new ReferenceIntersector(
                        //    WallDataItem.CollisionWall.Id,
                        //    FindReferenceTarget.Face,
                        //    WallDataItem.CollisionView)
                        //{ FindReferencesInRevitLinks = true };

                        //List<BuiltInCategory> CollisionCatsWall = new List<BuiltInCategory>();
                        //CollisionCatsWall.Add(BuiltInCategory.OST_Walls);

                        ElementMulticategoryFilter intersectFilter = new ElementMulticategoryFilter(CollisionCatsWall);

                        ReferenceIntersector WallFinder = new ReferenceIntersector(
                            intersectFilter,
                            FindReferenceTarget.Face,
                            WallDataItem.CollisionView)
                        { FindReferencesInRevitLinks = true };


                        ////////
                        var WallFoundRef = WallFinder.FindNearest(TestPoint, SurfaceNormal);

                        if (WallFoundRef != null &&
                            WallFoundRef.GetReference().LinkedElementId != WallDataItem.CollisionWall.Id)
                        {
                            WallFoundRef = null;
                        }


                        if (WallFoundRef != null)
                        {
                            XYZ ProximityTestPoint = WallFoundRef.GetReference().GlobalPoint;

                            // Compare distances
                            var ProximityTestDistance = WallFoundRef.Proximity;
                            if (ProximityTestDistance < ReferenceDistance)
                            {
                                ReferenceDistance = ProximityTestDistance;
                                Result.FoundPoint = ProximityTestPoint;
                                Result.IsNewPointFound = true;

                                // Check if point is inside the wall and adjust vector
                                var WallFoundRefs = WallFinder.Find(TestPoint, SurfaceNormal);

                                if (WallFoundRefs.Count == 0 ||
                                    WallFoundRefs.Count % 2 == 0)
                                {
                                    Result.WallSurfaceVector = SurfaceNormal;
                                }
                                else
                                {
                                    Result.WallSurfaceVector = SurfaceNormal * -1;
                                }
                            }
                        }
                    }
                }
            }

            return Result;
        }

        public static Reference WallIntersectionRef(
            XYZ originPoint,
            XYZ rayDirection,
            double searchDistanceReverse,
            double searchDistanceForward,
            View3D collisionView3D,
            List<BuiltInCategory> filterCategories)
        {
            double mmInFf = 304.8;

            var n = collisionView3D.Name;

            XYZ deltaVector = rayDirection * searchDistanceReverse;
            XYZ initialDeltaPosition = new XYZ();


            ElementMulticategoryFilter intersectFilter = new ElementMulticategoryFilter(filterCategories);

            ReferenceIntersector refIntersector;
            ReferenceWithContext referenceWithContext;


            //initialDeltaPosition = originPoint + deltaVector;

            //CreateDebugMarker(collisionView3D.Document, initialDeltaPosition);

            refIntersector = new ReferenceIntersector(intersectFilter, FindReferenceTarget.Face, collisionView3D);
            refIntersector.FindReferencesInRevitLinks = true;

            referenceWithContext = refIntersector.FindNearest(originPoint, -rayDirection);

            if (referenceWithContext != null)
            {
                //var prr = referenceWithContext.Proximity * mmInFf;
                //var dist = searchDistanceForward * mmInFf;

                var collisionDistance = originPoint.DistanceTo(referenceWithContext.GetReference().GlobalPoint);

                if (collisionDistance < searchDistanceForward)
                {
                    return referenceWithContext.GetReference();
                }
            }

            return null;
        }

        public static Level GetClosestLevel(Document doc, XYZ AbsoluteRefPoint)
        {
            // Get level that corresponds to actual location of the element
            FilteredElementCollector levelCollector = new FilteredElementCollector(doc);
            List<Level> rvtLevels = levelCollector.OfClass(typeof(Level)).OfType<Level>().OrderBy(lev => lev.Elevation).ToList();

            Level SelectedLevel = (Level)rvtLevels[0];
            foreach (Level lvl in rvtLevels)
            {
                if (AbsoluteRefPoint.Z < lvl.Elevation)
                {
                    break;
                }
                SelectedLevel = lvl;
            }

            return SelectedLevel;
        }
    }
}