using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus
{
    public interface IDynamicType
    {
        // TODO are the non-generic things useful?
        object Read(string name);
        object Read(string name, object[] index);
        TMember Read<TMember>(string name);
        TMember Read<TMember, TIndex>(string name, TIndex[] index);

        void Write(string name, object value);
        void Write(string name, object[] index, object value);
        void Write<TMember>(string name, TMember value);
        void Write<TMember, TIndex>(string name, TIndex[] index, TMember value);

        object Invoke(string name, object[] parameters);
        TResult Invoke<TResult>(string name, object[] parameters);
        TDelegate Method<TDelegate>(string name) where TDelegate : class;
    }

    /*
     * For instance method (e. g. getter, you might do: .Getter((dt, h) => h.Read<int>(dt, "_value"))
     * For static methods
     * 
     */

    // compile calls to IDynamicType calls to this class, or maybe just have one interface
    // and make the other methods static extensions... then they can be compiled away
    internal interface IInternalDynamicType
    {
        TMember Read<TMember, TIndex>(int memberId, TIndex[] index);
        void Write<TMember, TIndex>(int memberId, TIndex[] index, TMember value);
        TDelegate GetDelegate<TDelegate>(int memberId) where TDelegate : class;
    }

    public class Test : IInternalDynamicType
    {
        private int a;
        private bool b;

        private static readonly Func<Test, int, int> _add = Add;

        public int Add(int c)
        {
            return Add(this, c);
        }

        private static int Add(Test t, int c) 
        {
            t.Write<int, object>(0, null, t.Read<int, object>(0, null) + c);
            return 8;
        }

        public TMember Read<TMember, TIndex>(int memberId, TIndex[] index)
        {
            if (memberId == 0) { return (TMember)(object)this.a; }
            if (memberId == 1) { return (TMember)(object)this.b; }
            throw new Exception();
        }
        public void Write<TMember, TIndex>(int memberId, TIndex[] index, TMember value)
        {
        }
        public TDelegate GetDelegate<TDelegate>(int memberId)
            where TDelegate : class
        {
            if (memberId == 3) { return (TDelegate)(object)_add; }
            throw new Exception();
        }
    }
}
