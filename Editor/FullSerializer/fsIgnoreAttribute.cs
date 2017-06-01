using System;

namespace SubjectNerd.PsdImporter.FullSerializer {
    /// <summary>
    /// The given property or field annotated with [JsonIgnore] will not be
    /// serialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class fsIgnoreAttribute : Attribute {
    }
}