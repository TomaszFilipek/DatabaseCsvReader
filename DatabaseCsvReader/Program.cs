namespace DatabaseCsvReader
{
    using System;

    internal class Program
    {
        internal static void Main(string[] args)
        {
            try
            {
                var reader = new DatabaseCsvParser();
                reader.DatabaseParserInvalidLineException += DataParserInvalidLineExceptionHandler;
                var databaseObjects = reader.ParseDataFromFile(fileToImport: "data.csv", ignoreInvalidLines: true);
             
                PrintData(databaseObjects);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("\nPress any key to exit");
            Console.ReadKey();
        }

        private static void DataParserInvalidLineExceptionHandler(string line, string message)
        {
            Console.WriteLine($"Line processing error occurred!\nLine: {line}\nError message: {message}\n");
        }

        private static void PrintData(DatabaseObjectCollection databaseObjects)
        {
            var mainDatabaseObjectList = databaseObjects.GetMainDatabaseObjectList();

            foreach (var mainObject in mainDatabaseObjectList)
            {
                Console.WriteLine(mainObject);
                PrintChildrens(databaseObjects, mainObject);
            }
        }

        private static void PrintChildrens(DatabaseObjectCollection databaseObjects, DatabaseObject mainObject, int level = 1)
        {
            var mainObjectChildren = databaseObjects.GetDatabaseObjectChildren(mainObject);

            foreach (DatabaseObject children in mainObjectChildren)
            {
                Console.WriteLine($"{new string('\t', level)}{children}");

                if (children.NumberOfChildren > 0)
                {
                    PrintChildrens(databaseObjects, children, level + 1);
                }
            }
        }
    }
}
