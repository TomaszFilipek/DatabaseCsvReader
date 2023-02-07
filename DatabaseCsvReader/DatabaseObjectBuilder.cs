namespace DatabaseCsvReader
{
    using System;

   
    internal class DatabaseObjectBuilder
    {  
        public static DatabaseObject BuildDatabaseObject(string csvLine, char csvSeparator = ';')
        {
            var csvLineValues = csvLine.Split(csvSeparator);
            DatabaseObjectType databaseObjectType;
            string typeName = csvLineValues[(int)DatabaseObjectValueColumnIndex.Type].Trim().Replace(" ", "");

            if (!Enum.TryParse(typeName, true, out databaseObjectType))
            {
                throw new Exception($"Unsupported data type: {typeName}");
            }

            switch (databaseObjectType)
            {
                case DatabaseObjectType.database: return new DatabaseObjectDatabase(csvLineValues);
                case DatabaseObjectType.table: return new DatabaseObjectTable(csvLineValues);
                case DatabaseObjectType.column: return new DatabaseObjectColumn(csvLineValues);
                default: throw new Exception($"Unsupported data type: {typeName}");
            }
        }
    }
}
