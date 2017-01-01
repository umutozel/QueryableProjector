using System;
using System.Collections.Generic;

namespace QueryableProjector {

    public class MapDefinitionCollection: IMapDefinitionCollection {
        private readonly Dictionary<Tuple<Type, Type>, IMapDefinition> _maps = new Dictionary<Tuple<Type, Type>, IMapDefinition>();

        /// <summary>
        /// Registers the mapping.
        /// </summary>
        /// <param name="inType">Source type.</param>
        /// <param name="outType">Destination type.</param>
        /// <param name="mapDefinition">Property mappings.</param>
        public virtual void Register(Type inType, Type outType, IMapDefinition mapDefinition) {
            _maps[new Tuple<Type, Type>(inType, outType)] = mapDefinition;
        }

        /// <summary>
        /// Finds the defined property mappings for given types.
        /// </summary>
        /// <param name="inType">Source type.</param>
        /// <param name="outType">Destination type.</param>
        /// <returns>Property mappings.</returns>
        public virtual IMapDefinition Resolve(Type inType, Type outType) {
            IMapDefinition mapDefinition;
            _maps.TryGetValue(new Tuple<Type, Type>(inType, outType), out mapDefinition);
            return mapDefinition;
        }
    }

    public class MapDefinition : IMapDefinition {
        private readonly IDictionary<string, string> _maps;
        private readonly bool _onlyExplicit;

        /// <summary>
        /// Holds custom mapping definition.
        /// </summary>
        /// <param name="inType">Source type.</param>
        /// <param name="maps">Property (or field) mapping. ! Key is the destination, value is the source !.</param>
        /// <param name="onlyExplicit">When true, only given properties will be mapped, others won't be mapped, even if the names are the same.</param>
        public MapDefinition(IDictionary<string, string> maps, bool onlyExplicit = false) {
            _maps = maps;
            _onlyExplicit = onlyExplicit;
        }

        public IDictionary<string, string> Maps { get { return _maps; } }

        public bool OnlyExplicit { get { return _onlyExplicit; } }
    }

    public interface IMapDefinitionCollection {
        void Register(Type inType, Type outType, IMapDefinition mapDefinition);
        IMapDefinition Resolve(Type inType, Type outType);
    }

    public interface IMapDefinition {
        IDictionary<string, string> Maps { get; }
        bool OnlyExplicit { get; }
    }
}
