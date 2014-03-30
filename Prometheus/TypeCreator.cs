using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus
{
    public sealed class TypeCreator
    {
        // TODO we need to know the lambda signature BEFORE we create the primary type, so that we can create calls to the nested helper type
        // -> can execute the func twice. Once with the type builder to learn the signature, then again with the actual type!
        private readonly List<Tuple<string, MethodAttributes, Func<TypeFacade, LambdaExpression>>> methodFactories = 
            new List<Tuple<string, MethodAttributes, Func<TypeFacade, LambdaExpression>>>();
        private readonly List<FieldCreator> fields = new List<FieldCreator>();

        public TypeCreator()
        {
            this.Attributes = TypeAttributes.Public;
        }

        public string Name { get; set; }
        public TypeAttributes Attributes { get; set; }

        public ICollection<FieldCreator> Fields { get { return this.fields; } }

        public FieldCreator Field(string name, Type type)
        {
            var fieldCreator = new FieldCreator { Name = name, Type = type };
            this.fields.Add(fieldCreator);
            return fieldCreator;
        }

        public void Method(string name, Func<TypeFacade, LambdaExpression> lambdaFactory, MethodAttributes attributes = MethodAttributes.Public)
        {
            Throw.IfNull(lambdaFactory, "lambdaFactory");

            this.methodFactories.Add(Tuple.Create(name, attributes, lambdaFactory));
        }

        internal TypeBuilder CreateBuilder(ModuleBuilder module)
        {
            return null;
        }

        internal Type CreateType(TypeBuilder typeBuilder)
        {
            return null;
        }

        internal void CompleteType(Type type)
        {
        }

        internal Type ToType(ModuleBuilder module)
        {
            // add attributes
            // add fields
            // add properties -> defines additional methods
            // add method signatures (& bodies for raw IL methods)
            // add method bodies

            Throw<InvalidOperationException>.If(string.IsNullOrWhiteSpace(this.Name), "Name is required");
            var typeBuilder = module.DefineType(this.Name, this.Attributes);
            var pb = typeBuilder.DefineProperty("", PropertyAttributes.HasDefault, null, null);
            
            // TODO add attributes

            // add fields
            var fields = new List<FieldInfo>();
            foreach (var fieldCreator in this.Fields)
            {
                fields.Add(fieldCreator.ToField(typeBuilder));
            }

            // TODO add properties

            // add methods
            // create the nested helper type
            var helperTypeBuilder = typeBuilder.DefineNestedType("Helper_" + Guid.NewGuid().ToString("N"), TypeAttributes.NestedPrivate);

            // define all methods
            var helperMethodBuilders = new List<MethodBuilder>();
            var partialTypeFacade = new TypeFacade(typeBuilder, fields, Empty<PropertyInfo>.Array, Empty<MethodInfo>.Array);
            foreach (var t in this.methodFactories)
            {
                var lambda = t.Item3(partialTypeFacade);
                Type[] parameterTypes;
                if (!t.Item2.HasFlag(MethodAttributes.Static)
                    && lambda.Parameters.Count > 0
                    && lambda.Parameters[0].Type.IsAssignableFrom(typeBuilder))
                {
                    parameterTypes = lambda.Parameters.Skip(1).Select(p => p.Type).ToArray();
                }
                else
                {
                    parameterTypes = lambda.Parameters.Select(p => p.Type).ToArray();
                }

                var methodBuilder = typeBuilder.DefineMethod(t.Item1, t.Item2, lambda.ReturnType, parameterTypes);
                var helperParameterTypes = methodBuilder.IsStatic ? parameterTypes : new[] { typeBuilder }.Concat(parameterTypes).ToArray();
                var helperMethodBuilder = helperTypeBuilder.DefineMethod(t.Item1 + "Helper", MethodAttributes.Public | MethodAttributes.Static, methodBuilder.ReturnType, helperParameterTypes);
                var ilGen = methodBuilder.GetILGenerator();
                for (var i = 0; i < helperParameterTypes.Length; ++i)
                {
                    ilGen.EmitLoadArg(i);
                }
                ilGen.Emit(OpCodes.Call, helperMethodBuilder);
                ilGen.Emit(OpCodes.Ret);

                helperMethodBuilders.Add(helperMethodBuilder);
            }

            var type = typeBuilder.CreateType();
            var completedTypeFacade = new TypeFacade(type, type.GetFields(ReflectionHelpers.AllBindingFlags), type.GetProperties(ReflectionHelpers.AllBindingFlags), type.GetMethods(ReflectionHelpers.AllBindingFlags));

            // implement all methods
            for (var i = 0; i < helperMethodBuilders.Count; ++i)
            {
                var lambda = this.methodFactories[i].Item3(completedTypeFacade);
                lambda.CompileToMethod(helperMethodBuilders[i]);
            }
            helperTypeBuilder.CreateType();

            return type;
        }
    }
}
