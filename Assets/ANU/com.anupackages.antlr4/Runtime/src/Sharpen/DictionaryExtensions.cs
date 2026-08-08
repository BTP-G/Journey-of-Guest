/* Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using System.Collections.Generic;

namespace Antlr4.Runtime.Sharpen {
    internal static class DictionaryExtensions {
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TValue : class {
            if (!dictionary.TryGetValue(key, out var value)) {
                return null;
            }

            return value;
        }

        public static TValue Put<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
            where TValue : class {
            if (!dictionary.TryGetValue(key, out var previous)) {
                previous = null;
            }

            dictionary[key] = value;
            return previous;
        }
    }
}
