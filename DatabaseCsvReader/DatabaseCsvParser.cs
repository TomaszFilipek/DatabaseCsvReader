namespace DatabaseCsv
{
    using System;
    using System.IO;
    using System.Linq;

    public class DatabaseCsvParser
    {
        public delegate void DatabaseParserInvalidLine(string line, string message);

        public event DatabaseParserInvalidLine DatabaseParserInvalidLineException;

        /// <summary>
        /// Method parsing csv file.
        /// </summary>
        /// <param name="fileToImport">Path to parsed file.</param>
        /// <param name="skipEmptyLines">A parameter specifying whether blank lines should be skipped.</param>
        /// <param name="hasHeaderRecord">If true first line will be skipped.</param>
        /// <param name="ignoreInvalidLines">If true, an exception will not be thrown in case of a line parsing error. Only the DataReaderInvalidLineException event will be raised.</param>
        /// <returns>The <see cref="DatabaseObjectCollection"/>.</returns>
        public DatabaseObjectCollection ParseDataFromFile(string fileToImport, bool skipEmptyLines = true, bool hasHeaderRecord = true, bool ignoreInvalidLines = false)
        {
            var countLineToSkip = hasHeaderRecord ? 1 : 0;
            var fileLines = File.ReadAllLines(fileToImport).Skip(countLineToSkip);

            DatabaseObjectCollection databaseObjects = new DatabaseObjectCollection();

            foreach (var fileLine in fileLines)
            {
                if (skipEmptyLines && string.IsNullOrEmpty(fileLine))
                    continue;

                try
                {
                    databaseObjects.Add(DatabaseObjectBuilder.BuildDatabaseObject(fileLine));
                }
                catch (Exception ex)
                {
                    if (ignoreInvalidLines)
                    {
                        if (DatabaseParserInvalidLineException != null)
                        {
                            DatabaseParserInvalidLineException(fileLine, ex.Message);
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return databaseObjects;
        }
    }
}
