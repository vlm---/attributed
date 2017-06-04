// Copyright 2015 Destructurama Contributors, Serilog Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using Destructurama.Util;
using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;

namespace Destructurama.Attributed
{
    class AttributedDestructuringPolicyConcurrent : IDestructuringPolicy
    {
        readonly ConcurrentDictionary<Type, object> _ignored = new ConcurrentDictionary<Type, object>();
        readonly ConcurrentDictionary<Type, Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue>> _cache = new ConcurrentDictionary<Type, Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue>>();

        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
        {
            var t = value.GetType();
            object ignored;
            if (_ignored.TryGetValue(t, out ignored))
            {
                result = null;
                return false;
            }

            Func<object, ILogEventPropertyValueFactory, LogEventPropertyValue> cached;
            if (_cache.TryGetValue(t, out cached))
            {
                result = cached(value, propertyValueFactory);
                return true;
            }

            var ti = t.GetTypeInfo();

            var logAsScalar = ti.GetCustomAttribute<LogAsScalarAttribute>();
            if (logAsScalar != null)
            {
                _cache.TryAdd(t, (o, f) => AttributedDestructuringPolicy.MakeScalar(o, logAsScalar.IsMutable));
            }
            else
            {
                var properties = t.GetPropertiesRecursive()
                    .ToList();
                if (properties.Any(pi =>
                    pi.GetCustomAttribute<LogAsScalarAttribute>() != null ||
                    pi.GetCustomAttribute<NotLoggedAttribute>() != null))
                {
                    var loggedProperties = properties
                        .Where(pi => pi.GetCustomAttribute<NotLoggedAttribute>() == null)
                        .ToList();

                    var scalars = loggedProperties
                        .Where(pi => pi.GetCustomAttribute<LogAsScalarAttribute>() != null)
                        .ToDictionary(pi => pi, pi => pi.GetCustomAttribute<LogAsScalarAttribute>().IsMutable);

                    _cache.TryAdd(t, (o, f) => AttributedDestructuringPolicy.MakeStructure(o, loggedProperties, scalars, f, t));
                }
                else
                {
                    _ignored.TryAdd(t, null);
                }
            }

            return TryDestructure(value, propertyValueFactory, out result);
        }
    }
}
