using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace Prometheus
{
    internal static class EmitHelpers
    {
        public static void EmitLoadArg(this ILGenerator @this, int index)
        {
            Throw<ArgumentOutOfRangeException>.If(index < 0, "index");
           
            // see http://stackoverflow.com/questions/8485988/il-calling-a-method-with-2-array-arguments-using-reflection-emit
            switch (index)
            {
                case 0: @this.Emit(OpCodes.Ldarg_0); break;
                case 1: @this.Emit(OpCodes.Ldarg_1); break;
                case 2: @this.Emit(OpCodes.Ldarg_2); break;
                case 3: @this.Emit(OpCodes.Ldarg_3); break;
                default:
                    if (index <= byte.MaxValue)
                    {
                        @this.Emit(OpCodes.Ldarg_S, (byte)index);
                    }
                    else
                    {
                        @this.Emit(OpCodes.Ldarg, index);
                    }
                    break;
            }
        }
    }
}
