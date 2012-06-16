using System;
using System.IO;
using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model;

namespace Umbraco.Framework.IO.Model
{
    public class File : TypedEntity, IFileInfo
    {
        public File()
        {
            this.SetupEntityFromSchema<FileSchema>();
            IsContainer = false;
        }

        public File(HiveId id)
            : this()
        {
            Id = id;
        }

        public string Name
        {
            get { return (string) Attributes["name"].DynamicValue; }
            set { Attributes["name"].DynamicValue = value; }
        }

        public string Location
        {
            get { return (string)Attributes["location"].DynamicValue; }
            set { Attributes["location"].DynamicValue = value; }
        }

        [Obsolete]
        public string Key
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool IsContainer
        {
            get { return (bool)Attributes["isContainer"].DynamicValue; }
            set { Attributes["isContainer"].DynamicValue = value; }
        }

        public string AbsolutePath
        {
            get { return (string)Attributes["absolutePath"].DynamicValue; }
            set { Attributes["absolutePath"].DynamicValue = value; }
        }

        private byte[] _content;

        /// <summary>
        /// The lazy delegate to get at the file stream
        /// </summary>
        public Lazy<Stream> ContentStream { get; internal set; }
        
        /// <summary>
        /// The byte content of the file, this will be lazy loaded based on the ContentStream
        /// </summary>
        public byte[] Content
        {
            get
            {
                if (_content == null)
                {
                    if (ContentStream == null)
                        throw new InvalidOperationException("The ContentStream property has not been set, therefore the Content bytes cannot be accessed");
                    using (var streamValue = ContentStream.Value)
                    {
                        _content = streamValue.ReadAllBytes();
                    }
                }
                return _content;
            }
            set
            {
                if (IsContainer)
                    throw new InvalidOperationException(
                        string.Format(
                            "Entity '{0}' is a container and hence cannot have content assigned to it. To set content ensure that the IsContainer property is false.",
                            Id));

                _content = value;
            }
        }

        
    }
}
