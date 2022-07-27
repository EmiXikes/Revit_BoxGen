using Autodesk.Revit.DB;
using System.Collections.Generic;
using static EpicWallBox.SourceDataStructs;

namespace EpicWallBox
{
    internal class SourceData
    {
        public SourceData()
        {
        }
        // Common
        public SourceType SourceDataType;
        public List<string> SourceElementNameFilter;

        // RVT
        public string SourceRVTDocName;
        public string SourceRVTLevelName;
        public BuiltInCategory SourceRVTCat;
        
        //public Document SourceRVTDoc { get; set; }
        //public Element SourceRVTLevel;

        // DWG

        public string SourceDWGPath;
        public string SourceDWGAttribute;

    }
}