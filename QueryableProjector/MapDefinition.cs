﻿using System;
using System.Collections.Generic;

namespace QueryableProjector {

    public class MapDefinition : IMapDefinition {
        private readonly Type _inType;
        private readonly IDictionary<string, string> _maps;
        private readonly bool _onlyExplicit;

        /// <summary>
        /// Holds custom mapping definition.
        /// </summary>
        /// <param name="inType">Source type.</param>
        /// <param name="maps">Property (or field) mapping. ! Key is the destination, value is the source !.</param>
        /// <param name="onlyExplicit">When true, only given properties will be mapped, others won't be mapped, even if the names are the same.</param>
        public MapDefinition(Type inType, IDictionary<string, string> maps, bool onlyExplicit = false) {
            _inType = InType;
            _maps = maps;
            _onlyExplicit = onlyExplicit;
        }

        public Type InType { get { return _inType; } }

        public IDictionary<string, string> Maps { get { return _maps; } }

        public bool OnlyExplicit { get { return _onlyExplicit; } }
    }

    public interface IMapDefinition {
        Type InType { get; }

        IDictionary<string, string> Maps { get; }
        bool OnlyExplicit { get; }
    }
}