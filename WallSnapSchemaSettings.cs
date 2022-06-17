using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicWallBoxGen
{
    public static class SchemaGUIDs
    {
        public readonly static Guid settings_GUID = new Guid(
            "114546bc-d03f-462f-a2c8-929013cde0d4");
        public readonly static Guid settingsID_GUID = new Guid(
            "b411ee7d-d1cc-4b1e-aa6b-d7a2e1a6667d");
    }


    // Settings object for data to be saved to RVT file
    public class WallSnapSettingsData
    {
        public string ViewName { get; set; }
        public double DistanceRev { get; set; }
        public double DistanceFwd { get; set; }
        public ElementId LinkId { get; set; }
    }


    // Get or create Schema for settings.
    public static class WallSnapSettingsSchema
    {
        //readonly static Guid schemaGuid = new Guid(
        //  "41EB8254-C9F3-418D-AD4B-1FE08FD0A1A2");
        readonly static Guid schemaGuid = SchemaGUIDs.settings_GUID;

        public static Schema GetSchema()
        {
            Schema schema = Schema.Lookup(schemaGuid);

            if (schema != null) return schema;

            SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);

            schemaBuilder.SetSchemaName("LumiSnapSettings");

            FieldBuilder myField;
            schemaBuilder.AddSimpleField("ViewName", typeof(string));

            myField = schemaBuilder.AddSimpleField("DistanceRev", typeof(double));
            myField.SetSpec(SpecTypeId.Length);

            myField = schemaBuilder.AddSimpleField("DistanceFwd", typeof(double));
            myField.SetSpec(SpecTypeId.Length);

            myField = schemaBuilder.AddSimpleField("LinkId", typeof(ElementId));

            return schemaBuilder.Finish();
        }
    }
    public static class WallSnapSettingsIdSchema
    {
        //readonly static Guid schemaGuid = new Guid(
        //  "d5c3f7f2-3c1d-4007-ba62-57fdf7e26201");
        readonly static Guid schemaGuid = SchemaGUIDs.settingsID_GUID;

        public static Schema GetSchema()
        {
            Schema schema = Schema.Lookup(schemaGuid);

            if (schema != null) return schema;

            SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);

            schemaBuilder.SetSchemaName("SettingsID");

            schemaBuilder.AddSimpleField("ID", typeof(Guid));

            return schemaBuilder.Finish();
        }
    }
    public class WallSnapSettingsStorage
    {
        //readonly Guid settingDsId = new Guid(
        //  "d5c3f7f2-3c1d-4007-ba62-57fdf7e26201");
  //      readonly Guid settingDsId = new Guid(
  //"d5c3f7f2-3c1d-4007-ba62-57fdf7e26201");

        public WallSnapSettingsData ReadSettings(Document doc)
        {
            var settingsEntity = GetSettingsEntity(doc);

            if (settingsEntity == null
              || !settingsEntity.IsValid())
            {
                return null;
            }

            WallSnapSettingsData settings = new WallSnapSettingsData();

            settings.ViewName = settingsEntity.Get<string>("ViewName");
            settings.DistanceRev = settingsEntity.Get<double>("DistanceRev", UnitTypeId.Millimeters);
            settings.DistanceFwd = settingsEntity.Get<double>("DistanceFwd", UnitTypeId.Millimeters);
            settings.LinkId = settingsEntity.Get<ElementId>("LinkId");

            return settings;
        }

        public void WriteSettings(
          Document doc,
          WallSnapSettingsData settings)
        {
            DataStorage settingDs = GetSettingsDataStorage(doc);

            if (settingDs == null)
            {
                settingDs = DataStorage.Create(doc);
            }

            Entity settingsEntity = new Entity(WallSnapSettingsSchema.GetSchema());

            settingsEntity.Set("ViewName", settings.ViewName);
            settingsEntity.Set("DistanceRev", settings.DistanceRev, UnitTypeId.Millimeters);
            settingsEntity.Set("DistanceFwd", settings.DistanceFwd, UnitTypeId.Millimeters);
            settingsEntity.Set("LinkId", settings.LinkId);

            //Identify settings data storage

            Entity idEntity = new Entity(WallSnapSettingsIdSchema.GetSchema());

            idEntity.Set("ID", WallSnapSettingsSchema.GetSchema().GUID);

            settingDs.SetEntity(idEntity);
            settingDs.SetEntity(settingsEntity);
        }

        private DataStorage GetSettingsDataStorage(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var dataStorages = collector.OfClass(typeof(DataStorage));


            foreach (DataStorage dataStorage in dataStorages)
            {
                Entity settingIdEntity = dataStorage.GetEntity(WallSnapSettingsIdSchema.GetSchema());

                // If a DataStorage contains 
                // setting entity, we found it

                if (!settingIdEntity.IsValid()) continue;

                return dataStorage;
            }

            return null;

        }

        private Entity GetSettingsEntity(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            var dataStorages = collector.OfClass(typeof(DataStorage));

            // Find setting data storage

            foreach (DataStorage dataStorage in dataStorages)
            {
                Entity settingEntity = dataStorage.GetEntity(WallSnapSettingsSchema.GetSchema());

                // If a DataStorage contains 
                // setting entity, we found it

                if (!settingEntity.IsValid()) continue;

                return settingEntity;
            }

            return null;
        }
    }




    internal class WallSnapSchemaSettings
    {
    }
}
