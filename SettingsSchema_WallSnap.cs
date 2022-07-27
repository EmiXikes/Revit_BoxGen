using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicWallBox
{
    public static class SettingsSchema_WallSnap
    {
        #region User guide
        // Usage:
        // 1) Copy this class
        // 2) Change the class name
        // 3) Edit the entries in SettingsObj
        // 4) Change GUIDs in Schemas.GUIDs
        // 5) Use as follows:
        //    Get:
        //       SettingsData SettingsData = new SettingsData();
        //       SettingsObj MySettings = SettingsData.Get(doc);
        //    Set:
        //       SettingsData.Set(doc, MySettings);

        #endregion

        // Settings object for data to be saved to RVT file
        public class SettingsObj
        {
            // new entries must be added/changed in 4 places:
            // 1) here
            // 2) in SettingsData.Get()
            // 3) in SettingsData.Set()
            // 4) in Schemas.Settings.GetSchema()

            public string ViewName { get; set; }
            public double DistanceRev { get; set; }
            public double DistanceFwd { get; set; }
            public ElementId LinkId { get; set; }
        }
        // Get or set actual setting values
        public class SettingsData
        {
            public SettingsObj Get(Document doc)
            {
                var settingsEntity = GetSettingsEntity(doc);

                if (settingsEntity == null
                  || !settingsEntity.IsValid())
                {
                    return null;
                }

                SettingsObj settings = new SettingsObj();

                settings.ViewName = settingsEntity.Get<string>("ViewName");
                settings.DistanceRev = settingsEntity.Get<double>("DistanceRev", UnitTypeId.Millimeters);
                settings.DistanceFwd = settingsEntity.Get<double>("DistanceFwd", UnitTypeId.Millimeters);
                settings.LinkId = settingsEntity.Get<ElementId>("LinkId");

                return settings;
            }

            public void Set(
              Document doc,
              SettingsObj settings)
            {
                DataStorage settingDS = GetSettingsDataStorage(doc);

                if (settingDS == null)
                {
                    settingDS = DataStorage.Create(doc);
                }

                Entity settingsEntity = new Entity(Schemas.Settings.GetSchema());

                settingsEntity.Set("ViewName", settings.ViewName);
                settingsEntity.Set("DistanceRev", settings.DistanceRev, UnitTypeId.Millimeters);
                settingsEntity.Set("DistanceFwd", settings.DistanceFwd, UnitTypeId.Millimeters);
                settingsEntity.Set("LinkId", settings.LinkId);

                //Identify settings data storage

                Entity idEntity = new Entity(Schemas.ID.GetSchema());

                idEntity.Set("ID", Schemas.Settings.GetSchema().GUID);

                settingDS.SetEntity(idEntity);
                settingDS.SetEntity(settingsEntity);
            }

            #region Helpers

            private DataStorage GetSettingsDataStorage(Document doc)
            {   // Find the matching Data Storage
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                var dataStorages = collector.OfClass(typeof(DataStorage));
            
                // loop through all datastorage elements and
                // test if datastorage contains the identifcation schema
                foreach (DataStorage dataStorage in dataStorages)
                {
                    Entity settingIdEntity = dataStorage.GetEntity(Schemas.ID.GetSchema());
                    if (!settingIdEntity.IsValid()) continue;

                    return dataStorage;
                }
                return null;
            }

            private Entity GetSettingsEntity(Document doc)
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                var dataStorages = collector.OfClass(typeof(DataStorage));

                // loop through all datastorage elements and
                // search for settigns entity
                foreach (DataStorage dataStorage in dataStorages)
                {
                    Entity settingEntity = dataStorage.GetEntity(Schemas.Settings.GetSchema());
                    if (!settingEntity.IsValid()) continue;

                    return settingEntity;
                }

                return null;
            }

            #endregion


        }
        public static class Schemas
        {
            public static class GUIDs 
            { 
                public readonly static Guid Settings = new Guid(
                    "114546bc-d03f-462f-a2c8-929013cde0d4");
                public readonly static Guid ID = new Guid(
                    "b411ee7d-d1cc-4b1e-aa6b-d7a2e1a6667d");
            }

            // Get or create Schemas for settings.
            public static class Settings
            {
                readonly static Guid schemaGuid = Schemas.GUIDs.Settings;

                public static Schema GetSchema()
                {
                    Schema schema = Schema.Lookup(schemaGuid);

                    if (schema != null)
                    {   // Get existing schema
                        return schema;
                    }
                    // or create NEW
                    else
                    {
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
            }
            public static class ID
            {
                readonly static Guid schemaGuid = Schemas.GUIDs.ID;

                public static Schema GetSchema()
                {
                    Schema schema = Schema.Lookup(schemaGuid);

                    if (schema != null)
                    {   // Get existing schema
                        return schema;
                    }
                    // or create NEW
                    else
                    {
                        SchemaBuilder schemaBuilder = new SchemaBuilder(schemaGuid);

                        schemaBuilder.SetSchemaName("SettingsID");

                        schemaBuilder.AddSimpleField("ID", typeof(Guid));

                        return schemaBuilder.Finish();
                    }
                }
            }


        }

    }







}
