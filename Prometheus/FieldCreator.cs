using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus
{
    public sealed class FieldCreator : ICreatorWithAttributes<FieldCreator, FieldAttributes>, ICreatorWithName<FieldCreator>
    {
        internal FieldCreator()
        {
            this.Attributes = FieldAttributes.Private;
        }

        public string Name { get; set; }
        public Type Type { get; set; }
        public FieldAttributes Attributes { get; set; }

        internal FieldBuilder ToField(TypeBuilder typeBuilder)
        {
            Throw<InvalidOperationException>.If(string.IsNullOrWhiteSpace(this.Name), "Name is required");
            Throw<InvalidOperationException>.If(this.Type == null, "Type is required");

            return typeBuilder.DefineField(this.Name, this.Type, this.Attributes);
        }
    }
}
