namespace DisPlay.Umbraco.EmbeddedContent.Models
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class EmbeddedContentConfigDocumentType
    {
        [DataMember(Name = "allowEditingName")]
        public string AllowEditingName { get; set; }

        [DataMember(Name = "documentTypeAlias")]
        public string DocumentTypeAlias { get; set; }

        [DataMember(Name = "group")]
        public string Group { get; set; }

        [DataMember(Name = "maxInstances")]
        public int? MaxInstances { get; set; }

        [DataMember(Name = "nameTemplate")]
        public string NameTemplate { get; set; }

        [DataMember(Name = "settingsTabKey")]
        public Guid? SettingsTabKey { get; set; }
    }
}
