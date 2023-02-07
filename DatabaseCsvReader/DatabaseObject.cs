namespace DatabaseCsv
{
    using System;

    public abstract class DatabaseObject
    {
        public string Name { get; }
        public DatabaseObjectType Type { get; }
        public string ParentName { get; }
        public DatabaseObjectType ParentType { get; }
        public int NumberOfChildren { get; set; }
        public bool HasParent
        {
            get
            {
                return this.ParentType != DatabaseObjectType.none;
            }
        }

        protected DatabaseObject(string[] values)
        {
            this.Name = values[(int)DatabaseObjectValueColumnIndex.Name].Trim().Replace(" ", "");
            this.ParentName = values[(int)DatabaseObjectValueColumnIndex.ParentName].Trim().Replace(" ", "");

            DatabaseObjectType type;
            if (!Enum.TryParse(values[(int)DatabaseObjectValueColumnIndex.Type].Trim().Replace(" ", ""), true, out type))
            {
                throw new Exception($"Unsupported data type: {values[(int)DatabaseObjectValueColumnIndex.Type]}");
            }
            this.Type = type;

            string parentTypeName = values[(int)DatabaseObjectValueColumnIndex.ParentType].Trim().Replace(" ", "");
            DatabaseObjectType parentType;
            if (string.IsNullOrEmpty(parentTypeName))
            {
                this.ParentType = DatabaseObjectType.none;
            }
            else
            {
                if (!Enum.TryParse(parentTypeName, true, out parentType))
                {
                    throw new Exception($"Unsupported data type: {parentTypeName}");
                }
                this.ParentType = parentType;
            }
        }
    }
}
