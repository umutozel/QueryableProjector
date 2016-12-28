using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace QueryableProjector {

    internal class IncludeVisitor : ExpressionVisitor {
        private readonly List<string> _includes = new List<string>();

        /// <summary>
        /// Visits the query and returns the included navigation members.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Included navigations members.</returns>
        internal ReadOnlyCollection<string> GetIncludes(IQueryable query) {
            if (query == null) throw new ArgumentNullException(nameof(query));

            _includes.Clear();
            Visit(query.Expression);

            return _includes.AsReadOnly();
        }

        protected override Expression VisitMethodCall(MethodCallExpression m) {
            if (m.Method.Name == "IncludeSpan") {
                foreach (var arg in m.Arguments) {
                    var spanList = (IEnumerable<object>)Helper.GetPrivatePropertyValue(Helper.GetPrivatePropertyValue(arg, "Value"), "SpanList");

                    foreach (var span in spanList) {
                        var navs = (IEnumerable<string>)Helper.GetPrivateFieldValue(span, "Navigations");
                        _includes.Add(string.Join(".", navs));
                    }
                }
            }
            else if (m.Method.Name == "Include") {
                var include = (string)Helper.GetPrivatePropertyValue(m.Arguments.First(), "Value");

                if (include != null) {
                    _includes.Add(include);
                }
            }

            return base.VisitMethodCall(m);
        }
    }
}
