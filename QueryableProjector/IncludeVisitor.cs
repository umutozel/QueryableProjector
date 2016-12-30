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
                        // converting human readable string paths.
                        _includes.Add(string.Join(".", navs));
                    }
                }
            }

            return base.VisitMethodCall(m);
        }
    }
}
