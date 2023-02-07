namespace DatabaseCsvReader
{
    using System.Collections.Generic;
    using System.Linq;

    public class DatabaseObjectCollection
    {
        private List<DatabaseObject> databaseObjects;
        
        public DatabaseObjectCollection()
        {
            databaseObjects = new List<DatabaseObject>();
        }

        public void Add(DatabaseObject databaseObject)
        {
            databaseObject.NumberOfChildren = databaseObjects.Count(o => o.ParentType == databaseObject.Type && o.ParentName == databaseObject.Name);

            databaseObjects.Add(databaseObject);

            IncreaseChildrenCounterDatabaseObjectParent(databaseObject);
        }

        private void IncreaseChildrenCounterDatabaseObjectParent(DatabaseObject databaseObject)
        {
            if (databaseObject.ParentType != DatabaseObjectType.none && !string.IsNullOrEmpty(databaseObject.ParentName))
            {
                var databaseObjectParent = databaseObjects.FirstOrDefault(o => o.Type == databaseObject.ParentType && o.Name == databaseObject.ParentName);
                if (databaseObjectParent is object)
                {
                    databaseObjectParent.NumberOfChildren++;
                }
            }
        }

        public IEnumerable<DatabaseObject> GetMainDatabaseObjectList()
        {
            return databaseObjects.Where(o => !o.HasParent);
        }

        public IEnumerable<DatabaseObject> GetDatabaseObjectChildren(DatabaseObject parentDatabaseObject)
        {
            return databaseObjects.Where(o => o.ParentType == parentDatabaseObject.Type && o.ParentName == parentDatabaseObject.Name);
        }
    }
}
