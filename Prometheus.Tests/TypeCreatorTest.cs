using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace Prometheus.Tests
{
    [TestClass]
    public class TypeCreatorTest
    {
        [TestMethod]
        public void TestSimpleInstanceMethod()
        {
            var typeCreator = new TypeCreator
            {
                Name = "A",
                Attributes = TypeAttributes.Public,
            };

            typeCreator.Method("X", t => Expression.Lambda(Expression.Constant("abc")));

            var type = typeCreator.ToType(this.GetModuleBuilder());
            var instance = Activator.CreateInstance(type);
            var result = type.GetMethod("X").Invoke(instance, new object[0]);
            Assert.AreEqual("abc", (string)result);
        }

        [TestMethod]
        public void TestIncrementerMethod()
        {
            var typeCreator = new TypeCreator
            {
                Name = "A",
                Attributes = TypeAttributes.Public,
            };

            typeCreator.Field("counter", typeof(int));
            typeCreator.Method("Next", t => {
                var @this = Expression.Parameter(t.Type);
                return Expression.Lambda(Expression.Increment(Expression.Field(@this, t.Field("counter"))), @this);
            });

            var type = typeCreator.ToType(this.GetModuleBuilder());
            var instance = Activator.CreateInstance(type);
            var result = type.GetMethod("Next").Invoke(instance, new object[0]);
            Assert.AreEqual(1, result);
        }

        private ModuleBuilder GetModuleBuilder()
        {
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("Test_" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.RunAndCollect);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            return moduleBuilder;
        }

        [TestMethod]
        public void SimpleTest()
        {
            //var factory = new TypeCreator();
            ////factory.Method<Func<string>>("ToString", () => "a");
            //var parameter = Expression.Parameter(typeof(object));
            ////var lambda = Expression.Lambda<Func<object, bool>>(Expression.TypeIs(parameter, factory._typeBuilder), parameter);
            ////var cmp = lambda.Compile();

            ////var tb = ((ModuleBuilder)factory.typeBuilder.Module).DefineType("newType");
            //var tb = factory.typeBuilder.DefineNestedType("newTypeNested");
            //var tbm = tb.DefineMethod("a", System.Reflection.MethodAttributes.Public | MethodAttributes.Static, typeof(int), Type.EmptyTypes);
            ////var tbt = tb.CreateType();
            ////Console.WriteLine(tbt.GetMethod("a").Invoke(null, new object[0]));

            //var m = factory.typeBuilder.DefineMethod("x", MethodAttributes.Public | MethodAttributes.Static, typeof(int), Type.EmptyTypes);
            //var mil = m.GetILGenerator();
            //mil.Emit(OpCodes.Call, tbm);
            //mil.Emit(OpCodes.Ret);

            //var type = factory.ToType();

            //var il = tbm.GetILGenerator();
            //il.Emit(OpCodes.Ldc_I4, 100);
            //il.Emit(OpCodes.Ret);
            //tb.CreateType();

            //Console.WriteLine("X IS " + type.GetMethod("x").Invoke(null, new object[0]));
            //Console.WriteLine(string.Join(", ", type.GetMethods().AsEnumerable()));
            //Console.WriteLine(type.GetType());
            //var instance = Activator.CreateInstance(type);
            ////Console.WriteLine(cmp(instance));
            //Console.WriteLine(instance.ToString());
        }
    }
}
