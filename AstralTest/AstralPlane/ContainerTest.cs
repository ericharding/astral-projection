using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Astral.Plane.Container;
using System.IO;

namespace AstralTest.AstralPlane
{
    [TestClass]
    public class ContainerTest
    {
        static Action<string> TryDelete = AstralTest.Utility.TestUtility.TryDelete;
        string text = "hello world";

        [TestMethod]
        public void TestFSContainer()
        {
            string path = Path.GetTempPath() + "FileSystemContainer\\";
            TryDelete(path);
            using (var container = new FilesystemContainer(path))
            {
                TestWriteFile(container, "hello.txt");
                Assert.IsTrue(File.Exists(path + "hello.txt"));
                TestReadFile(container, "hello.txt");
            }

            TryDelete(path);
            Assert.IsFalse(Directory.Exists(path));
        }

        [TestMethod]
        public void TestZipContainer1()
        {
            string path = Path.GetTempPath() + "test.zip";
            string filename = "hello.txt";

            TestZipContainer(path, filename);
        }

        [TestMethod]
        public void TestZipContainerDirectories()
        {
            string path = Path.GetTempPath() + "test.zip";
            string filename = "images\\hello.txt";

            TestZipContainer(path, filename);
        }

        private void TestZipContainer(string path, string filename)
        {
            using (var container = new ZipFileContainer(path))
            {
                TestWriteFile(container, filename);
                TestReadFile(container, filename);
            }

            TryDelete(path);
            Assert.IsFalse(File.Exists(path));
        }
        

        private void TestReadFile(IContainer container, string filename)
        {
            Stream readStream = container.GetFileStream(filename, false);
            StreamReader reader = new StreamReader(readStream);
            string read = reader.ReadLine();
            reader.Close();

            Assert.IsTrue(read == this.text);
        }

        private void TestWriteFile(IContainer container, string filename)
        {
            // Add something to the container
            
            Stream writeStream = container.GetFileStream(filename, true);
            StreamWriter writer = new StreamWriter(writeStream);
            writer.WriteLine(this.text);
            writer.Close();

            Assert.IsTrue(container.ContainsFile(filename));
        }
    }
}
