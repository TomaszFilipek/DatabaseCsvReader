namespace DatabaseCsv
{
    public class DatabaseObjectTable : DatabaseObject
    {
        public string Schema { get; }

        public override string ToString()
        {
            return $"Table '{Schema}.{Name}' ({NumberOfChildren} columns)";
        }
        public DatabaseObjectTable(string[] values) : base(values)
        {
            Schema = values[(int)DatabaseObjectValueColumnIndex.Schema].Trim().Replace(" ", "");
        }
    }
}
