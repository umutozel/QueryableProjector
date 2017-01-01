using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryableProjector {

    public static class Helper {

        /// <summary>
        /// Returns private property value using reflection.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propName">Property name.</param>
        /// <returns>Value of the property.</returns>
        public static object GetPrivatePropertyValue(object obj, string propName) {
            var propertyInfo = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return propertyInfo.GetValue(obj, null);
        }

        /// <summary>
        /// Returns private field value using reflection.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="propName">Field name.</param>
        /// <returns>Value of the field.</returns>
        public static object GetPrivateFieldValue(object obj, string propName) {
            var fieldInfo = obj.GetType().GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return fieldInfo.GetValue(obj);
        }

        /// <summary>
        /// Get properties and fields for projection mapping.
        /// </summary>
        /// <param name="type">The type to search.</param>
        /// <param name="onlyWritable">When true, only writable properties will be included.</param>
        /// <param name="bindingFlags">Binding flags to filter properties and fields.</param>
        /// <returns>Properties and field for projection.</returns>
        internal static IEnumerable<MapMember> GetMapFields(Type type, bool onlyWritable = false,
                                                            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public) {
            IEnumerable<PropertyInfo> properties = type.GetProperties(bindingFlags);
            if (onlyWritable) {
                properties = properties.Where(p => p.CanWrite);
            }

            return type.GetFields(bindingFlags).Select(f => new MapMember(f.Name, f.FieldType, f))
                .Concat(properties.Select(p => new MapMember(p.Name, p.PropertyType, p)));
        }

        /// <summary>
        /// Returns Included navigations for query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Included navigations.</returns>
        public static IEnumerable<string> GetIncludes(IQueryable query) {
            var visitor = new IncludeVisitor();
            return visitor.GetIncludes(query);
        }

        /// <summary>
        /// Projects query to given generic argument type.
        /// For example: Converting Entity queries to DTO queries.
        /// </summary>
        /// <typeparam name="TOut">Target type.</typeparam>
        /// <param name="query">The query.</param>
        /// <param name="mapDefinitions">Map definitions (property mappings) for source and destination types.</param>
        /// <returns>Projected query.</returns>
        public static IQueryable<TOut> ProjectTo<TOut>(this IQueryable query, IMapDefinitionCollection mapDefinitions = null) {
            var projector = CreateProjector(query.ElementType, typeof(TOut), GetIncludes(query), mapDefinitions);
            return (IQueryable<TOut>)query.Provider.CreateQuery(
                Expression.Call(
                    typeof(Queryable), "Select",
                    new Type[] { query.ElementType, typeof(TOut) },
                    query.Expression, projector)
                );
        }

        /// <summary>
        /// Generates projector lambda for given generic argument types.
        /// </summary>
        /// <typeparam name="TIn">Source type.</typeparam>
        /// <typeparam name="TOut">Target type.</typeparam>
        /// <param name="includes">Navigation properties to include in projection.</param>
        /// <param name="mapDefinitions">Map definitions (property mappings) for source and destination types.</param>
        /// <returns>The reusable lambda expression.</returns>
        public static Expression<Func<TIn, TOut>> CreateProjector<TIn, TOut>(IEnumerable<string> includes = null, IMapDefinitionCollection mapDefinitions = null) {
            return (Expression<Func<TIn, TOut>>)CreateProjector(typeof(TIn), typeof(TOut), includes, mapDefinitions);
        }

        /// <summary>
        /// Generates projector lambda expression for given types.
        /// </summary>
        /// <param name="inType">Source type.</param>
        /// <param name="outType">Target type.</param>
        /// <param name="includes">Navigation properties to include in projection.</param>
        /// <returns>The reusable projector lambda expression.</returns>
        /// <param name="mapDefinitions">Map definitions (property mappings) for source and destination types.</param>
        public static LambdaExpression CreateProjector(Type inType, Type outType, IEnumerable<string> includes = null, IMapDefinitionCollection mapDefinitions = null) {
            var prmExp = Expression.Parameter(inType, "e");
            var memberInit = CreateAssigner(inType, outType, includes, mapDefinitions, prmExp);

            return Expression.Lambda(memberInit, prmExp);
        }

        /// <summary>
        /// Creates projector object initializer for given types.
        /// </summary>
        /// <param name="inType">Source type.</param>
        /// <param name="outType">Target type.</param>
        /// <param name="includes">Navigation properties to include in projection.</param>
        /// <param name="prmExp">ParameterExpression instance to point the source object.</param>
        /// <returns>The reusable projector object initializer.</returns>
        /// <param name="mapDefinitions">Map definitions (property mappings) for source and destination types.</param>
        private static MemberInitExpression CreateAssigner(Type inType, Type outType, IEnumerable<string> includes,
                                                           IMapDefinitionCollection mapDefinitions, Expression prmExp) {
            var inProperties = GetMapFields(inType);
            var outProperties = GetMapFields(outType, true);

            IMapDefinition mapDefinition;
            if (mapDefinitions != null) {
                mapDefinition = mapDefinitions.Resolve(inType, outType);
            }
            else mapDefinition = null;

            Dictionary<string, Tuple<MapMember, MapMember>> navigationMapping = new Dictionary<string, Tuple<MapMember, MapMember>>();
            var memberBindings = new List<MemberBinding>();
            foreach (var outProperty in outProperties) {
                string mappedInPropertyName;
                if (mapDefinition != null) {
                    if (!mapDefinition.Maps.TryGetValue(outProperty.Name, out mappedInPropertyName)) {
                        if (mapDefinition.OnlyExplicit) continue;

                        mappedInPropertyName = outProperty.Name;
                    }
                }
                else mappedInPropertyName = outProperty.Name;

                var inProperty = inProperties.FirstOrDefault(p => p.Name == mappedInPropertyName);
                if (inProperty != null) {
                    if (inProperty.IsPrimitive) {
                        var p1Exp = Expression.PropertyOrField(prmExp, inProperty.Name);
                        memberBindings.Add(Expression.Bind(outProperty.MemberInfo, p1Exp));
                    }
                    else {
                        navigationMapping[inProperty.Name] = new Tuple<MapMember, MapMember>(inProperty, outProperty);
                    }
                }
            }

            if (includes != null && includes.Any()) {
                var includeGroups = includes
                    .Select(i => {
                        var idx = i.IndexOf(".");
                        string path, other;
                        if (idx > 0) {
                            path = i.Substring(0, idx);
                            other = i.Substring(idx + 1);
                        }
                        else {
                            path = i;
                            other = null;
                        }

                        return new { path, other };
                    })
                    .GroupBy(i => i.path)
                    .Select(i => new { Property = i.Key, SubIncludes = i.Select(o => o.other).Where(o => o != null) })
                    .ToList();

                foreach (var includeGroup in includeGroups) {
                    Tuple<MapMember, MapMember> includeMapping;
                    if (!navigationMapping.TryGetValue(includeGroup.Property, out includeMapping)) continue;

                    var inIncludeProperty = includeMapping.Item1;
                    var outIncludeProperty = includeMapping.Item2;

                    var inIncludeType = inIncludeProperty.Type;
                    var outIncludeType = outIncludeProperty.Type;

                    var inIncludeEnumerableType = inIncludeType.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    var outIncludeEnumerableType = outIncludeType.GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
                    if (inIncludeEnumerableType != null) {
                        if (outIncludeEnumerableType == null)
                            throw new ArrayTypeMismatchException($"Navigation type mismatch for property {includeGroup.Property}");

                        inIncludeType = inIncludeEnumerableType.GetGenericArguments()[0];
                        outIncludeType = outIncludeEnumerableType.GetGenericArguments()[0];
                    }
                    else if (outIncludeEnumerableType != null)
                        throw new ArrayTypeMismatchException($"Navigation type mismatch for property {includeGroup.Property}");

                    Expression subProjector;
                    if (inIncludeEnumerableType != null) {
                        subProjector = CreateProjector(inIncludeType, outIncludeType, includeGroup.SubIncludes, mapDefinitions);

                        var subPropExp = Expression.PropertyOrField(prmExp, includeGroup.Property);
                        subProjector = Expression.Call(typeof(Enumerable), "Select", new Type[] { inIncludeType, outIncludeType }, subPropExp, subProjector);
                        subProjector = Expression.Call(typeof(Enumerable), "ToList", new Type[] { outIncludeType }, subProjector);
                    }
                    else {
                        var subPrmExp = Expression.MakeMemberAccess(prmExp, inIncludeProperty.MemberInfo);
                        subProjector = CreateAssigner(inIncludeType, outIncludeType, includeGroup.SubIncludes, mapDefinitions, subPrmExp);
                    }

                    memberBindings.Add(Expression.Bind(outIncludeProperty.MemberInfo, subProjector));
                }
            }

            return Expression.MemberInit(Expression.New(outType), memberBindings);
        }
    }
}
