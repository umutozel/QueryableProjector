using System;
using System.Reflection;

namespace QueryableProjector {

    /// <summary>
    /// Used for holding property and field informations.
    /// </summary>
    internal class MapMember {
        private readonly string _name;
        private readonly Type _type;
        private readonly MemberInfo _memberInfo;
        private readonly bool _isPrimitive;

        internal MapMember(string name, Type type, MemberInfo memberInfo) {
            _name = name;
            _type = type;
            _memberInfo = memberInfo;
            
            _isPrimitive = Type.GetTypeCode(type) != TypeCode.Object // only primitive properties
                || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)); // including nullable ones
        }

        internal string Name { get { return _name; } }

        internal Type Type { get { return _type; } }

        internal MemberInfo MemberInfo { get { return _memberInfo; } }

        internal bool IsPrimitive { get { return _isPrimitive; } }
    }
}
