namespace DatabaseCsv
{
    public class DatabaseObjectDatabase : DatabaseObject
    {
        public override string ToString()
        {
            return $"Database {Name} ({NumberOfChildren} tables)";
        }

        public DatabaseObjectDatabase(string[] values) : base(values)
        {
        }
    }
}
