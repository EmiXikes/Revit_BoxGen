using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicWallBox
{
    // Settings object for data to be saved to RVT file
    public class SettingsSchema_ManualWallBox
    {
        public class SettingsObj
        {
            // new entries must be added/changed in 4 places:
            // 1) here
            // 2) in SettingsData.Get()
            // 3) in SettingsData.Set()
            // 4) in Schemas.Settings.GetSchema()

            public double FloorOffset { get; set; }
            public double ConduitOffset { get; set; }
            public string ViewName { get; set; }
            public ElementId LinkId { get; set; }
        }

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

                settings.ViewName =      settingsEntity.Get<string>("ViewName");
                settings.ConduitOffset = settingsEntity.Get<double>("ConduitOffset", UnitTypeId.Millimeters);
                settings.FloorOffset =   settingsEntity.Get<double>("FloorOffset", UnitTypeId.Millimeters);
                settings.LinkId =        settingsEntity.Get<ElementId>("LinkId");

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
                settingsEntity.Set("ConduitOffset", settings.ConduitOffset, UnitTypeId.Millimeters);
                settingsEntity.Set("FloorOffset", settings.FloorOffset, UnitTypeId.Millimeters);
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
                    "17154775-c265-4d79-ae1e-8c4516536498");
                public readonly static Guid ID = new Guid(
                    "48df5cf1-d0ef-4d2a-8865-d758dbe4fe05");
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

                        schemaBuilder.SetSchemaName("Manual WallBox Settings");

                        FieldBuilder myField;
                        schemaBuilder.AddSimpleField("ViewName", typeof(string));

                        myField = schemaBuilder.AddSimpleField("ConduitOffset", typeof(double));
                        myField.SetSpec(SpecTypeId.Length);

                        myField = schemaBuilder.AddSimpleField("FloorOffset", typeof(double));
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
