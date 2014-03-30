using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus
{
    public struct TypeOrCreator 
    {
        private readonly Type type;
        private readonly TypeCreator creator;
        private readonly bool isType;

        private TypeOrCreator(Type type, TypeCreator creator, bool isType) 
        {
            this.type = type;
            this.creator = creator;
            this.isType = isType;
        }

        public bool IsType { get { return this.isType; } }
        public bool HasValue { get { return this.creator != null || this.type != null; } }

        public override string ToString()
        {
            return (this.IsType ? this.type.As<object>() : this.creator).ToString();
        }

        public static implicit operator Type(TypeOrCreator value) 
        {
            Throw<InvalidCastException>.If(!value.IsType, "value: must be a Type");
            return value.type;
        }

        public static implicit operator TypeOrCreator(Type value) 
        {
            return new TypeOrCreator(value, null, isType: true);
        }

        public static implicit operator TypeCreator(TypeOrCreator value) 
        {
            Throw<InvalidCastException>.If(value.IsType, "value: must be a TypeCreator");
            return value.creator;
        }

        public static implicit operator TypeOrCreator(TypeCreator value) 
        {
            return new TypeOrCreator(null, value, isType: false);
        }
    }

    public abstract class MethodCreatorBase<TCreator> : ICreatorWithAttributes<TCreator, MethodAttributes>, ICreatorWithName<TCreator>
        where TCreator : MethodCreatorBase<TCreator>, ICreatorWithAttributes<TCreator, MethodAttributes>, ICreatorWithName<TCreator>
    {
        internal MethodCreatorBase()
        {
        }

        public MethodAttributes Attributes { get; set; }
        public string Name { get; set; }
    }

    public sealed class MethodCreator : MethodCreatorBase<MethodCreator>
    {
    }

    public sealed class MethodCreator<TBase> : MethodCreatorBase<MethodCreator<TBase>>
    {
    }
}
