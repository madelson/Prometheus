using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus
{
    public sealed class AssemblyCreator : ICreatorWithName<AssemblyCreator>, InvalidCreatorException.ICreator
    {
        public AssemblyName AssemblyName { get; set; }
        public string Name
        {
            get
            {
                return this.AssemblyName.NullSafe(an => an.Name);
            }
            set
            {
                (this.AssemblyName ?? (this.AssemblyName = new AssemblyName())).Name = value;
            }
        }

        private readonly List<ModuleCreator> modules = new List<ModuleCreator>();
        public ICollection<ModuleCreator> Modules { get { return this.modules; } }

        public ModuleCreator Module()
        {
            const string DefaultNamePrefix = "Module";
            var defaultName = Enumerable.Range(0, int.MaxValue).Select(i => DefaultNamePrefix + i)
                .First(n => !this.Modules.Any(m => m.Name == n));
            var module = new ModuleCreator { Name = defaultName };
            this.Modules.Add(module);
            return module;
        }

        string InvalidCreatorException.ICreator.DisplayName
        {
            get { return "Assembly" + this.Name.NullSafe(n => " '" + n + "'"); }
        }

        public AssemblyBuilder ToAssembly(AssemblyBuilderAccess access)
        {
            this.RequireName();

            var result = this.WrapWithException(() =>
            {
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(this.AssemblyName, access);
                var moduleBuilders = this.Modules.Select(m => Tuple.Create(m, m.CreateBuilder(assemblyBuilder)))
                    .ToList();
                moduleBuilders.ForEach(t => t.Item1.CompleteBuilder(t.Item2));

                return assemblyBuilder;
            });
            return result;
        }
    }

    public sealed class ModuleCreator : ICreatorWithName<ModuleCreator>, InvalidCreatorException.ICreator
    {
        internal ModuleCreator() { }

        public string Name { get; set; }

        private readonly List<TypeCreator> types = new List<TypeCreator>();
        public ICollection<TypeCreator> Types { get { return this.types; } }

        public TypeCreator Type(string name = null)
        {
            const string DefaultNamePrefix = "Type";
            var typeName = name ?? Enumerable.Range(0, int.MaxValue).Select(i => DefaultNamePrefix + i).First(n => !this.Types.Any(t => t.Name == n));
            var type = new TypeCreator { Name = typeName };
            this.Types.Add(type);
            return type;
        }

        string InvalidCreatorException.ICreator.DisplayName
        {
            get { return "Module" + this.Name.NullSafe(n => " '" + n + "'"); }
        }

        internal ModuleBuilder CreateBuilder(AssemblyBuilder assemblyBuilder)
        {
            this.RequireName();

            var result = this.WrapWithException(() =>
            {
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(this.Name);
                return moduleBuilder;
            });          
            return result;
        }

        internal void CompleteBuilder(ModuleBuilder moduleBuilder)
        {
        }
    }
}
