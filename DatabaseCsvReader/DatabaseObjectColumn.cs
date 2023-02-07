namespace DatabaseCsv
{
    using System;

    public class DatabaseObjectColumn : DatabaseObject
    { 
        public string Schema { get; }
        public string DataType { get; }
        public bool IsNullable { get; }

        public override string ToString()
        {
            return $"Column '{Name}' with {DataType} data type {(IsNullable ? "accepts nulls" : "with no nulls")}";
        }
            
        public DatabaseObjectColumn(string[] values) : base(values)
        {
            try
            {
                IsNullable = Convert.ToBoolean(Convert.ToInt32(values[6]));
            }
            catch
            {
                throw new FormatException("'isNullable' column value is incorrect");
            }

            Schema = values[(int)DatabaseObjectValueColumnIndex.Schema].Trim().Replace(" ", "");
            DataType = values[(int)DatabaseObjectValueColumnIndex.DataType];
        }
    }
}
