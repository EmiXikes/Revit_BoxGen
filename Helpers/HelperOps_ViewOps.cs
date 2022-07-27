using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace EpicWallBox
{
    internal static class HelperOps_ViewOps
    {
        public static View3D GetOrCreate3DView(Document doc, string viewName)
        {
            var existingView = (new FilteredElementCollector(doc)).OfClass(typeof(View3D)).
                Cast<View3D>().FirstOrDefault(x => x.Name == viewName);
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
    }
}