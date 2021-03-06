﻿namespace DisPlay.Umbraco.EmbeddedContent.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class EmbeddedContentConfig
    {
        [DataMember(Name = "documentTypes")]
        public IEnumerable<EmbeddedContentConfigDocumentType> DocumentTypes { get; set; }

        [DataMember(Name = "enableCollapsing")]
        public string EnableCollapsing { get; set; }

        [DataMember(Name = "enableFiltering")]
        public string EnableFiltering { get; set; }

        [DataMember(Name = "groups")]
        public string[] Groups { get; set; }

        [DataMember(Name = "maxItems")]
        public int? MaxItems { get; set; }

        [DataMember(Name = "minItems")]
        public int? MinItems { get; set; }
    }
}
