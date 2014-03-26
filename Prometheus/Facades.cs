using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus
{
    internal static class FacadeHelpers
    {
        public static TMember FindByName<TMember>(IEnumerable<TMember> members, string name, bool ignoreCase)
            where TMember : MemberInfo
        {
            var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            TMember found = null;
            foreach (var member in members)
            {
                if (comparer.Equals(member.Name, name))
                {
                    if (found != null)
                    {
                        throw new AmbiguousMatchException(string.Format("Multiple {0}s found with the name '{1}'", typeof(TMember).Name, name));
                    }
                    found = member;
                }
            }

            if (found == null)
            {
                throw new InvalidOperationException(string.Format("No {0} could be found with the name '{1}'", typeof(TMember).Name, name));
            }
            return found;
        }
    }

    public sealed class TypeFacade
    {
        private readonly IReadOnlyCollection<PropertyInfo> properties;
        private readonly IReadOnlyCollection<MethodInfo> methods;

        internal TypeFacade(Type type, IReadOnlyList<FieldInfo> fields, IReadOnlyList<PropertyInfo> properties, IReadOnlyList<MethodInfo> methods)
        {
            Throw.IfNull(type, "type");
            Throw.IfNull(fields, "fields");
            Throw.IfNull(properties, "properties");
            Throw.IfNull(methods, "methods");

            this.type = type;
            this.fields = fields;
            this.properties = properties;
            this.methods = methods;
        }

        private readonly Type type;
        public Type Type { get { return this.type; } }

        private readonly IReadOnlyList<FieldInfo> fields;
        public IReadOnlyList<FieldInfo> Fields { get { return this.fields; } }
        public FieldInfo Field(string name, bool ignoreCase = false)
        {
            return FacadeHelpers.FindByName(this.Fields, name, ignoreCase); 
        }
    }
}
