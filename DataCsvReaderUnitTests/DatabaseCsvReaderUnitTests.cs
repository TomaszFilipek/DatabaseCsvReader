using DatabaseCsv;
using DataCsvReaderUnitTests;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace DatabaseCsvUnitTests
{
    public class DatabaseCsvReaderUnitTests
    {
        [Fact]
        public void ParseDataFromFile_NotExistFile_FileNotFoundException()
        {
            var parser = new DatabaseCsvParser();

            Action act = () => parser.ParseDataFromFile("test");

            Assert.Throws<FileNotFoundException>(act);
        }

        [Fact]
        public void ParseDataFromFile_NotExistFile_ArgumentException()
        {
            var parser = new DatabaseCsvParser();

            Action act = () => parser.ParseDataFromFile("");

            Assert.Throws<ArgumentException>(act);
        }

        [Fact]
        public void ParseDataFromFile_ThreeRecordsWithoutHeader_CollectionWithTreeObjects()
        {
            StringBuilder fileContent = new StringBuilder();
            fileContent.AppendLine($"database;databasename;;;;;");
            fileContent.AppendLine($"database;databasename;;;;;");
            fileContent.AppendLine($"database;databasename;;;;;");

            using (TestFileGenerator testFileGenerator = new TestFileGenerator(fileContent))
            {
                var databaseCsvParser = new DatabaseCsvParser();
                var databaseObjectCollection = databaseCsvParser.ParseDataFromFile(fileToImport: testFileGenerator.FullPath, hasHeaderRecord: false);
                var databaseObjects = databaseObjectCollection.GetDatabaseObjects();

                Assert.Equal(3, databaseObjects.Count());
            }
        }

        [Fact]
        public void ParseDataFromFile_ThreeRecordsWithHeader_CollectionWithTreeObjects()
        {
            StringBuilder fileContent = new StringBuilder();
            fileContent.AppendLine($"Type;Name;Schema;ParentName;ParentType;DataType;IsNullable");
            fileContent.AppendLine($"database;databasename;;;;;");
            fileContent.AppendLine($"database;databasename;;;;;");
            fileContent.AppendLine($"database;databasename;;;;;");

            using (TestFileGenerator testFileGenerator = new TestFileGenerator(fileContent))
            {
                var databaseCsvParser = new DatabaseCsvParser();
                var databaseObjectCollection = databaseCsvParser.ParseDataFromFile(fileToImport: testFileGenerator.FullPath, hasHeaderRecord: true);
                var databaseObjects = databaseObjectCollection.GetDatabaseObjects();

                Assert.Equal(3, databaseObjects.Count());
            }
        }

        [Fact]
        public void ParseDataFromFile_OneDatabaseRecord_CollectionWithOneDatabaseObject()
        {
            string name = Guid.NewGuid().ToString();
            string fileContent = $"database;{name};;;;;";

            using (TestFileGenerator testFileGenerator = new TestFileGenerator(fileContent))
            {
                var databaseCsvParser = new DatabaseCsvParser();
                var databaseObjectCollection = databaseCsvParser.ParseDataFromFile(fileToImport: testFileGenerator.FullPath, hasHeaderRecord: false);
                var databaseObjects = databaseObjectCollection.GetDatabaseObjects();
                var databaseObject = databaseObjects.First();

                Assert.Single(databaseObjects);
                Assert.Equal(name, databaseObject.Name);
                Assert.Equal(DatabaseObjectType.database, databaseObject.Type);
                Assert.IsAssignableFrom<DatabaseObjectDatabase>(databaseObject);
            }
        }

        [Fact]
        public void ParseDataFromFile_OneTableRecord_CollectionWithOneTableObject()
        {
            string name = Guid.NewGuid().ToString();
            string schema = Guid.NewGuid().ToString();
            string parentName = Guid.NewGuid().ToString();
            string fileContent = $"Table;{name};{schema};{parentName};Database;NULL;";

            using (TestFileGenerator testFileGenerator = new TestFileGenerator(fileContent))
            {
                var databaseCsvParser = new DatabaseCsvParser();
                var databaseObjectCollection = databaseCsvParser.ParseDataFromFile(fileToImport: testFileGenerator.FullPath, hasHeaderRecord: false);
                var databaseObjects = databaseObjectCollection.GetDatabaseObjects();
                var databaseObject = databaseObjects.First();

                Assert.IsAssignableFrom<DatabaseObjectTable>(databaseObject);
                Assert.Single(databaseObjects);
                Assert.Equal(name, databaseObject.Name);
                Assert.Equal(DatabaseObjectType.table, databaseObject.Type);
                Assert.Equal(parentName, databaseObject.ParentName);
                Assert.Equal(DatabaseObjectType.database, databaseObject.ParentType);
                Assert.Equal(schema, ((DatabaseObjectTable)databaseObject).Schema);
            }
        }

        [Fact]
        public void ParseDataFromFile_AssingChildToParent_CollectionWithCorrectRelations()
        {
            string databaseName = Guid.NewGuid().ToString();
            string tableName = Guid.NewGuid().ToString();
            string columnName = Guid.NewGuid().ToString();
            string schema = Guid.NewGuid().ToString();
            string columnDataType = Guid.NewGuid().ToString();

            StringBuilder fileContent = new StringBuilder();
            fileContent.AppendLine($"database;{databaseName};;;;;");
            fileContent.AppendLine($"table;{tableName};{schema};{databaseName};database;;");
            fileContent.AppendLine($"column;{columnName};{schema};{tableName};table;{columnDataType};0");
            fileContent.AppendLine($"column;{columnName};{schema};{tableName};table;{columnDataType};0");

            using (TestFileGenerator testFileGenerator = new TestFileGenerator(fileContent))
            {
                var databaseCsvParser = new DatabaseCsvParser();
                var databaseObjectCollection = databaseCsvParser.ParseDataFromFile(fileToImport: testFileGenerator.FullPath, hasHeaderRecord: false);
                var databaseObjects = databaseObjectCollection.GetDatabaseObjects();
                var databaseObjectDatabase = databaseObjectCollection.GetMainDatabaseObjectList();
                var databaseObjectTable = databaseObjectCollection.GetDatabaseObjectChildren(databaseObjectDatabase.First());
                var databaseObjectColumn = databaseObjectCollection.GetDatabaseObjectChildren(databaseObjectTable.First());

                Assert.Equal(4, databaseObjects.Count());
                Assert.Single(databaseObjectDatabase);
                Assert.Single(databaseObjectTable);
                Assert.Equal(2, databaseObjectColumn.Count());

                Assert.IsAssignableFrom<DatabaseObjectDatabase>(databaseObjectDatabase.First());
                Assert.IsAssignableFrom<DatabaseObjectTable>(databaseObjectTable.First());
                Assert.IsAssignableFrom<DatabaseObjectColumn>(databaseObjectColumn.First());

                Assert.Equal(1, databaseObjectDatabase.First().NumberOfChildren);
                Assert.Equal(2, databaseObjectTable.First().NumberOfChildren);

                Assert.Equal(databaseName, databaseObjectDatabase.First().Name);
                Assert.Equal(DatabaseObjectType.database, databaseObjectDatabase.First().Type);
                Assert.Equal(DatabaseObjectType.none, databaseObjectDatabase.First().ParentType);

                Assert.Equal(tableName, databaseObjectTable.First().Name);
                Assert.Equal(databaseName, databaseObjectTable.First().ParentName);
                Assert.Equal(DatabaseObjectType.table, databaseObjectTable.First().Type);
                Assert.Equal(DatabaseObjectType.database, databaseObjectTable.First().ParentType);

                Assert.Equal(columnName, databaseObjectColumn.First().Name);
                Assert.Equal(columnDataType, ((DatabaseObjectColumn)databaseObjectColumn.First()).DataType);
                Assert.Equal(tableName, databaseObjectColumn.First().ParentName);
                Assert.Equal(DatabaseObjectType.column, databaseObjectColumn.First().Type);
                Assert.Equal(DatabaseObjectType.table, databaseObjectColumn.First().ParentType);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ParseDataFromFile_OneColumnRecord_CollectionWithOneColumnObject(bool isNullable)
        {
            string name = Guid.NewGuid().ToString();
            string schema = Guid.NewGuid().ToString();
            string parentName = Guid.NewGuid().ToString();
            string dataType = Guid.NewGuid().ToString();

            string fileContent = $"Column;{name};{schema};{parentName};Table;{dataType};{(isNullable ? "1" : "0")}";

            using (TestFileGenerator testFileGenerator = new TestFileGenerator(fileContent))
            {
                var databaseCsvParser = new DatabaseCsvParser();
                var databaseObjectCollection = databaseCsvParser.ParseDataFromFile(fileToImport: testFileGenerator.FullPath, hasHeaderRecord: false);
                var databaseObjects = databaseObjectCollection.GetDatabaseObjects();
                var databaseObject = databaseObjects.First();

                Assert.IsAssignableFrom<DatabaseObjectColumn>(databaseObject);
                Assert.Single(databaseObjects);
                Assert.Equal(name, databaseObject.Name);
                Assert.Equal(DatabaseObjectType.column, databaseObject.Type);
                Assert.Equal(parentName, databaseObject.ParentName);
                Assert.Equal(DatabaseObjectType.table, databaseObject.ParentType);
                Assert.Equal(schema, ((DatabaseObjectColumn)databaseObject).Schema);
                Assert.Equal(dataType, ((DatabaseObjectColumn)databaseObject).DataType);
                Assert.Equal(isNullable, ((DatabaseObjectColumn)databaseObject).IsNullable);
            }
        }


        [Theory]
        [InlineData("database", DatabaseObjectType.database, typeof(DatabaseObjectDatabase))]
        [InlineData("DATABASE", DatabaseObjectType.database, typeof(DatabaseObjectDatabase))]
        [InlineData("table", DatabaseObjectType.table, typeof(DatabaseObjectTable))]
        [InlineData("TABLE", DatabaseObjectType.table, typeof(DatabaseObjectTable))]
        [InlineData("column", DatabaseObjectType.column, typeof(DatabaseObjectColumn))]
        [InlineData("COLUMN", DatabaseObjectType.column, typeof(DatabaseObjectColumn))]
        public void ParseDataFromFile_TypeNameIsNotCaseSensitive_CollectionWithOneObjectCorrectType(string typeName, DatabaseObjectType databaseObjectType, Type objectType)
        {
            string name = Guid.NewGuid().ToString();

            string fileContent = $"{typeName};{name};;;;;0";

            using (TestFileGenerator testFileGenerator = new TestFileGenerator(fileContent))
            {
                var databaseCsvParser = new DatabaseCsvParser();
                var databaseObjectCollection = databaseCsvParser.ParseDataFromFile(fileToImport: testFileGenerator.FullPath, hasHeaderRecord: false);
                var databaseObjects = databaseObjectCollection.GetDatabaseObjects();
                var databaseObject = databaseObjects.First();

                Assert.IsType(objectType, databaseObject);
                Assert.Single(databaseObjects);
                Assert.Equal(name, databaseObject.Name);
                Assert.Equal(databaseObjectType, databaseObject.Type);
            }
        }

        [Fact]
        public void ParseDataFromFile_WrongTypeName_Exception()
        {
            string name = Guid.NewGuid().ToString();
            string typeName = Guid.NewGuid().ToString();
            string fileContent = $"{typeName};{name};;;;;";

            using (TestFileGenerator testFileGenerator = new TestFileGenerator(fileContent))
            {
                var databaseCsvParser = new DatabaseCsvParser();
                Action act = () => databaseCsvParser.ParseDataFromFile(fileToImport: testFileGenerator.FullPath, hasHeaderRecord: false);

                Assert.Throws<Exception>(act);
            }
        }
    }
}