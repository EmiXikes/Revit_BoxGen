using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace EpicWallBoxGen
{
    internal static class HelperOps_DataCollectors
    {

        public static Document GetLinkedDocByName(Document doc, string SelectedSourceDocName)
        {
            List<Document> LinkedDocs = new List<Document>();
            foreach (Document LinkedDoc in doc.Application.Documents)
            {
                if (LinkedDoc.IsLinked)
                {
                    LinkedDocs.Add(LinkedDoc);
                }
            }
            Document SelectedSourceDoc = LinkedDocs.FirstOrDefault(x => x.PathName.Contains(SelectedSourceDocName));
            return SelectedSourceDoc;
        }

        public static List<FamilyInstance> GetSourcePointsRVT(Document doc, SourceData sData)
        {
            Document SelectedSourceDoc = GetLinkedDocByName(doc, sData.SourceRVTDocName);

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
                    var famname = x.Symbol.FamilyName;
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

        public static XYZ GetSurveyPoint(Document doc)
        {
            XYZ surveyPoint = new XYZ();
            IEnumerable<BasePoint> points = new FilteredElementCollector(doc)
                .OfClass(typeof(BasePoint))
                .Cast<BasePoint>();
            foreach (BasePoint bp in points)
            {
                if (bp.IsShared)
                {
                    BoundingBoxXYZ bb = bp.get_BoundingBox(null);
                    surveyPoint = bb.Min;
                }
            }

            return surveyPoint;
        }
    }
}