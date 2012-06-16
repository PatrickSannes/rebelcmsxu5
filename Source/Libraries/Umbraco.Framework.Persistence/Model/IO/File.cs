using System;
using System.IO;

namespace Umbraco.Framework.Persistence.Model.IO
{
    public class File : TypedEntity
    {
        public File()
        {
            this.SetupFromSchema<FileSchema>();
            IsContainer = false;
        }

        public File(TypedEntity fromEntity)
        {
            this.SetupFromEntity(fromEntity);

        }

        public File(HiveId id)
            : this()
        {
            Id = id;
        }

        public string Name
        {
            get { return (string)Attributes["name"].DynamicValue; }
            set
            {                
                if (!RootedPath.IsNullOrWhiteSpace() && Path.GetFileName(RootedPath) != value)
                {
                    //if the locatin is set, then we need to update it too
                    var rootLocation = RootedPath.Substring(0, RootedPath.LastIndexOf(Name));
                    Attributes["rootedPath"].DynamicValue = Path.Combine(rootLocation, value);
                }
                Attributes["name"].DynamicValue = value;
            }
        }

        public bool IsContainer
        {
            get { return (bool)Attributes["isContainer"].DynamicValue; }
            set { Attributes["isContainer"].DynamicValue = value; }
        }

        public string RootedPath
        {
            get { return (string)Attributes["rootedPath"].DynamicValue; }
            set
            {
                Attributes["rootedPath"].DynamicValue = value;
                
                //as we are changing the location the name will need to be updated as well
                if (Name != Path.GetFileName(value))
                {
                    Attributes["name"].DynamicValue = Path.GetFileName(value);    
                }
            }
        }

        public string RootRelativePath
        {
            get { return (string)Attributes["rootRelativePath"].DynamicValue; }
            set { Attributes["rootRelativePath"].DynamicValue = value; }
        }

        public string PublicUrl
        {
            get { return (string)Attributes["publicUrl"].DynamicValue; }
            set { Attributes["publicUrl"].DynamicValue = value; }
        }

        public byte[] ContentBytes
        {
            get
            {
                var content = (byte[])Attributes["contentBytes"].DynamicValue;
                if (content == null)
                {
                    if (ContentStreamFactory == null)
                        return new byte[0];
                        // throw new InvalidOperationException("The ContentStream property has not been set, therefore the Content bytes cannot be accessed");
                    using (var streamValue = ContentStreamFactory.Invoke(this))
                    {
                        ContentBytes = content = streamValue.ReadAllBytes();
                    }
                }
                return content;
            }
            set
            {
                if (IsContainer)
                    throw new InvalidOperationException(
                        string.Format(
                            "Entity '{0}' is a container and hence cannot have content assigned to it. To set content ensure that the IsContainer property is false.",
                            Id));

                Attributes["contentBytes"].DynamicValue = value;
            }
        }

        protected Func<File, Stream> ContentStreamFactory { get; private set; }

        public void SetContentStreamFactory(Func<File, Stream> streamFactory)
        {
            ContentStreamFactory = streamFactory;
        }
    }
}
