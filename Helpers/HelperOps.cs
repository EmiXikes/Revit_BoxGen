using Autodesk.Revit.DB;
using EpicWallBoxGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicWallBoxGen
{
    internal class HelperOps
    {
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


            initialDeltaPosition = originPoint + deltaVector;

            refIntersector = new ReferenceIntersector(intersectFilter, FindReferenceTarget.Face, collisionView3D);
            refIntersector.FindReferencesInRevitLinks = true;

            referenceWithContext = refIntersector.FindNearest(initialDeltaPosition, -rayDirection);

            if (referenceWithContext != null)
            {
                var prr = referenceWithContext.Proximity * mmInFf;
                var dist = searchDistanceForward * mmInFf;

                var collisionDistance = originPoint.DistanceTo(referenceWithContext.GetReference().GlobalPoint);

                if (collisionDistance < searchDistanceForward)
                {
                     return referenceWithContext.GetReference();
                }
            }

            return null;
        }

        public void CreateDebugMarker(Document doc, XYZ MarkerPoint)
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
        public NearestWallPoint FindNearestWallPoint(XYZ ReferencePoint, 
            View3D CollisionView, 
            Document CollisionDoc, 
            List<BuiltInCategory> CollisionCatsWall,
            double revDist,
            double fwdDist
            )
        {
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

            foreach (var WallProximityTest in CollisionTestDirections)
            {
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





        public NearestWallPoint GetClosestPointOnWall(XYZ TestPoint, NearestWallPoint WallDataItem, List<BuiltInCategory> CollisionCatsWall)
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












        public static void RotateFamilyInstance(FamilyInstance ElementInstance, XYZ CenterPoint, double RotationAngle)
        {

            XYZ axisPoint1 = CenterPoint;
            XYZ axisPoint2 = new XYZ(
                CenterPoint.X,
                CenterPoint.Y,
                CenterPoint.Z + 1
                );
            Line axis = Line.CreateBound(axisPoint1, axisPoint2);

            ElementTransformUtils.RotateElement(
                ElementInstance.Document,
                ElementInstance.Id,
                axis,
                RotationAngle);
        }
        public View3D GetOrCreate3DView(Document doc, string viewName)
        {
            var existingView = (new FilteredElementCollector(doc)).OfClass(typeof(View3D)).
                Cast<View3D>().FirstOrDefault(x=>x.Name == viewName);
            if (existingView != null)
            {
                return existingView;
            }

            var collector = new FilteredElementCollector(doc);
            var viewFamilyType = collector.OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>()
              .FirstOrDefault(x => x.ViewFamily == ViewFamily.ThreeDimensional);

            View3D view3D = View3D.CreateIsometric(doc, viewFamilyType.Id);
            view3D.Name = viewName;
            view3D.Discipline = ViewDiscipline.Coordination;

            return view3D;
        }
        public static void SetVisibleCats(Document doc, List<BuiltInCategory> snapCats, View3D view3D)
        {
            List<int> snapCatsIds = new List<int>();
            foreach (var sC in snapCats)
            {
                snapCatsIds.Add(new ElementId(sC).IntegerValue);
            }

            Categories categories = doc.Settings.Categories;

            foreach (Category category in categories)
            {
                if (snapCatsIds.Contains(category.Id.IntegerValue))
                {
                    category.set_Visible(view3D, true);
                }
                else
                {
                    if (category.get_AllowsVisibilityControl(view3D))
                    {
                        category.set_Visible(view3D, false);
                    }
                }
            }
        }
        public static void SetVisibleLink(Document doc, ElementId LinkedFileID, View3D ceilCheckView)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ICollection<ElementId> linkedDocIdSet =
              collector
              .OfCategory(BuiltInCategory.OST_RvtLinks)
              .OfClass(typeof(RevitLinkType))
              .ToElementIds();

            foreach (ElementId linkedFileId in linkedDocIdSet)
            {
                Element link = doc.GetElement(linkedFileId);
                if (link.CanBeHidden(ceilCheckView))
                {
                    if (LinkedFileID == link.Id)
                    {
                        ceilCheckView.UnhideElements(new List<ElementId>() { link.Id });
                    }
                    else
                    {
                        ceilCheckView.HideElements(new List<ElementId>() { link.Id });
                    }
                }


            }
        }

        public static Document GetLinkDoc(Document doc, string SelectedSourceDocName)
        {
            List<Document> LinkedDocs = new List<Document>();
            foreach (Document LinkedDoc in doc.Application.Documents)
            {
                if (LinkedDoc.IsLinked)
                {
                    LinkedDocs.Add(LinkedDoc);
                }
            }
            Document SelectedSourceDoc = LinkedDocs.FirstOrDefault(x => System.IO.Path.GetFileNameWithoutExtension(x.PathName) == SelectedSourceDocName);
            return SelectedSourceDoc;
        }

        public static List<FamilyInstance> GetSourcePointsRVT(Document doc, SourceData sData)
        {
            Document SelectedSourceDoc = GetLinkDoc(doc, sData.SourceRVTDocName);

            var SourceDocLevels = new FilteredElementCollector(SelectedSourceDoc).
                OfClass(typeof(Level)).
                OfCategory(BuiltInCategory.OST_Levels).ToList();

            var SelectedSourceLevel = SourceDocLevels.FirstOrDefault(LN => LN.Name == sData.SourceRVTLevelName);

            var linkedFixtures = new FilteredElementCollector(SelectedSourceDoc)
                .OfCategory(BuiltInCategory.OST_ElectricalFixtures)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(x =>
                {
                    Level linkedFixtureLvl = (Level)SourceDocLevels.FirstOrDefault(y => y.Id == x.LevelId);
                    if (linkedFixtureLvl.Name.Contains(sData.SourceRVTLevelName))
                    {
                        if (sData.SourceElementNameFilter.Count == 0)
                        {
                            return true;
                        }
                        foreach (var n in sData.SourceElementNameFilter)
                        {
                            var famN = x.Symbol.FamilyName.ToLower();
                            if (famN.Contains(n.ToLower()))
                            {
                                return true;
                            }
                        }
                    }

                    return false;

                }).ToList();

            //foreach (var item in linkedFixtures)
            //{
            //    Debug.WriteLine(String.Format(
            //        "Fixture: [{0}]  Type Name: {1}  Family Name: {2}  Level: {3}",
            //        item.Id, item.Name, item.Symbol.FamilyName, item.LevelId)
            //        );
            //}

            return linkedFixtures;
        }


    }
}
