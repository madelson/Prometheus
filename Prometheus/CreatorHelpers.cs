using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Prometheus
{
    public static class CreatorHelpers
    {
        public static TCreator Name<TCreator>(this ICreatorWithName<TCreator> @this, string name)
            where TCreator : ICreatorWithName<TCreator>
        {
            @this.Name = name;
            return (TCreator)@this;
        }

        internal static void RequireName<TCreator>(this TCreator @this)
            where TCreator : ICreatorWithName<TCreator>, InvalidCreatorException.ICreator
        {
            @this.ThrowIf(string.IsNullOrWhiteSpace(@this.Name), "name is required");
        }

        public static TCreator Static<TCreator, TAttributes>(this ICreatorWithAttributes<TCreator, TAttributes> @this, bool isStatic = true)
            where TCreator : ICreatorWithAttributes<TCreator, TAttributes>
            where TAttributes : struct
        {
            @this.Attributes = AttributeHelper<TAttributes>.SetFlags(@this.Attributes, AttributeHelper<TAttributes>.Static.Value, remove: !isStatic);
            return (TCreator)@this;
        }

        public static TCreator Type<TCreator>(this ICreatorWithType<TCreator> @this, Type type)
            where TCreator : ICreatorWithType<TCreator>
        {
            @this.Type = type;
            return (TCreator)@this;
        }

        public static TCreator Type<TCreator>(this ICreatorWithType<TCreator> @this, TypeCreator typeCreator)
            where TCreator : ICreatorWithType<TCreator>
        {
            @this.Type = typeCreator;
            return (TCreator)@this;
        }

        #region ---- Attribute Helper ----
        private static class AttributeHelper<TAttributes>
            where TAttributes : struct
        {
            public static readonly Lazy<TAttributes> Static = GetValue("Static");
            public static readonly Lazy<Func<TAttributes, TAttributes, TAttributes>> BitwiseOr = new Lazy<Func<TAttributes, TAttributes, TAttributes>>(
                    () =>
                    {
                        var param1 = Expression.Parameter(typeof(TAttributes));
                        var param2 = Expression.Parameter(typeof(TAttributes));
                        var baseType = Enum.GetUnderlyingType(typeof(TAttributes));
                        var lambda = Expression.Lambda<Func<TAttributes, TAttributes, TAttributes>>(
                            Expression.Convert(Expression.Or(Expression.Convert(param1, baseType), Expression.Convert(param1, baseType)), typeof(TAttributes)),
                            param1,
                            param2
                        );
                        return lambda.Compile();
                    }
                ),
                BitwiseAnd = new Lazy<Func<TAttributes, TAttributes, TAttributes>>(
                    () =>
                    {
                        var param1 = Expression.Parameter(typeof(TAttributes));
                        var param2 = Expression.Parameter(typeof(TAttributes));
                        var baseType = Enum.GetUnderlyingType(typeof(TAttributes));
                        var lambda = Expression.Lambda<Func<TAttributes, TAttributes, TAttributes>>(
                            Expression.Convert(Expression.And(Expression.Convert(param1, baseType), Expression.Convert(param1, baseType)), typeof(TAttributes)),
                            param1,
                            param2
                        );
                        return lambda.Compile();
                    }
                );
            public static readonly Lazy<Func<TAttributes, TAttributes>> BitwiseNegate = new Lazy<Func<TAttributes, TAttributes>>(
                    () =>
                    {
                        var param = Expression.Parameter(typeof(TAttributes));
                        var baseType = Enum.GetUnderlyingType(typeof(TAttributes));
                        var lambda = Expression.Lambda<Func<TAttributes, TAttributes>>(
                            Expression.Convert(Expression.Not(Expression.Convert(param, baseType)), typeof(TAttributes)),
                            param
                        );
                        return lambda.Compile();
                    }
                );

            public static TAttributes SetFlags(TAttributes value, TAttributes flags, bool remove)
            {
                if (remove)
                {
                    return BitwiseAnd.Value(value, BitwiseNegate.Value(flags));
                }
                return BitwiseOr.Value(value, flags);
            }

            private static Lazy<TAttributes> GetValue(string name)
            {
                return new Lazy<TAttributes>(() => (TAttributes)Enum.Parse(typeof(TAttributes), name), System.Threading.LazyThreadSafetyMode.PublicationOnly);
            }
        }
#endregion
    }

    #region ---- Interfaces ----
    public interface ICreatorWithAttributes<TCreator, TAttributes>
        where TCreator : ICreatorWithAttributes<TCreator, TAttributes>
        where TAttributes : struct
    {
        TAttributes Attributes { get; set; }
    }

    public interface ICreatorWithName<TCreator>
        where TCreator : ICreatorWithName<TCreator>
    {
        string Name { get; set; }
    }

    public interface ICreatorWithType<TCreator>
        where TCreator : ICreatorWithType<TCreator>
    {
        TypeOrCreator Type { get; set; }
    }
    #endregion
}
