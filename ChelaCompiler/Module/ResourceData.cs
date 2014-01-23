using System;
using System.IO;

namespace Chela.Compiler.Module
{
    /// <summary>
    /// Embedded resource data.
    /// </summary>
    public class ResourceData
    {
        private string fileName;
        private string name;
        private FileStream fileStream;

        public ResourceData(string fileName, string name)
        {
            this.fileName = fileName;
            this.name = name;
            this.fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        }

        /// <summary>
        /// Gets the resource file.
        /// </summary>
        public FileStream File {
            get {
                return fileStream;
            }
        }

        /// <summary>
        /// Gets the resource length.
        /// </summary>
        public int Length {
            get {
                return (int)fileStream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the resource file name.
        /// </summary>
        public string FileName {
            get {
                return fileName;
            }
            set {
                fileName = value;
            }
        }

        /// <summary>
        /// Gets or sets the resource name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name {
            get {
                return name;
            }
            set {
                name = value;
            }
        }
    }
}

