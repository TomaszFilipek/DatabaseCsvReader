using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCsvReaderUnitTests
{
    class TestFileGenerator:IDisposable
    {
        public string FullPath { get; private set; }
        private string directory;

        public TestFileGenerator(string fileContent)
        {
            CreateTestFile(fileContent);
        }
        
        public TestFileGenerator(StringBuilder fileContent)
        {
            CreateTestFile(fileContent.ToString());
        }

        ~TestFileGenerator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {

            if (disposing)
            {
                GC.SuppressFinalize(this);
            }

            if (!string.IsNullOrEmpty(directory))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch { }
                this.directory = null;
                this.FullPath = null;
            }
        }

        private void CreateTestFile(string fileContent)
        {
            directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
            Directory.CreateDirectory(directory);

            FullPath = Path.Combine(directory, Path.GetRandomFileName());
            File.WriteAllText(FullPath, fileContent);
        }
    }
}
