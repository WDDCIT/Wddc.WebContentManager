using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wddc.WebContentManager.Extensions
{
    /// <summary>
    /// Extends DefaultContractResolver to honour System.Text.Json [JsonIgnore]
    /// attributes and deduplicate properties that differ only by casing
    /// (e.g. "Id" from a base class vs "ID" on the entity).
    /// </summary>
    public sealed class SafeContractResolver : DefaultContractResolver
    {
        private static readonly Type StjIgnoreAttr =
            typeof(System.Text.Json.Serialization.JsonIgnoreAttribute);

        protected override IList<JsonProperty> CreateProperties(
            Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            // Step 1: Remove properties decorated with System.Text.Json [JsonIgnore]
            var filtered = new List<JsonProperty>(props.Count);
            foreach (var prop in props)
            {
                var attrs = prop.AttributeProvider?.GetAttributes(StjIgnoreAttr, true);
                if (attrs != null && attrs.Count > 0)
                    continue;

                filtered.Add(prop);
            }

            // Step 2: Deduplicate case-insensitive collisions, keeping the most-derived property
            // (lowest inheritance depth = declared closest to the concrete type)
            return filtered
                .GroupBy(p => p.PropertyName, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Count() == 1
                    ? g.First()
                    : g.OrderBy(p => GetInheritanceDepth(p.DeclaringType, type)).First())
                .ToList();
        }

        private static int GetInheritanceDepth(Type declaring, Type concrete)
        {
            int depth = 0;
            var t = concrete;
            while (t != null)
            {
                if (t == declaring) return depth;
                t = t.BaseType;
                depth++;
            }
            return int.MaxValue;
        }
    }
}
