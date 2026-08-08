/* Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */

using Antlr4.Runtime.Atn;
using Antlr4.Runtime.Sharpen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Antlr4.Runtime.Misc {
    /// <author>Sam Harwell</author>
    public class RuleDependencyChecker {
        private static readonly HashSet<string> checkedAssemblies = new HashSet<string>();

        public static void CheckDependencies(Assembly assembly) {
            if (IsChecked(assembly)) {
                return;
            }

            var typesToCheck = GetTypesToCheck(assembly);
            var dependencies = new List<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>();
            foreach (var clazz in typesToCheck) {
                dependencies.AddRange(GetDependencies(clazz));
            }

            if (dependencies.Count > 0) {
                IDictionary<TypeInfo, IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>> recognizerDependencies = new Dictionary<TypeInfo, IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>>();
                foreach (var dependency in dependencies) {
                    var recognizerType = dependency.Item1.Recognizer.GetTypeInfo();
                    if (!recognizerDependencies.TryGetValue(recognizerType, out var list)) {
                        list = new List<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>();
                        recognizerDependencies[recognizerType] = list;
                    }
                    list.Add(dependency);
                }

                foreach (var entry in recognizerDependencies) {
                    //processingEnv.getMessager().printMessage(Diagnostic.Kind.NOTE, String.format("ANTLR 4: Validating {0} dependencies on rules in {1}.", entry.getValue().size(), entry.getKey().toString()));
                    CheckDependencies(entry.Value, entry.Key);
                }
            }

            MarkChecked(assembly);
        }

        private static IEnumerable<TypeInfo> GetTypesToCheck(Assembly assembly) {
            return assembly.DefinedTypes;
        }

        private static bool IsChecked(Assembly assembly) {
            lock (checkedAssemblies) {
                return checkedAssemblies.Contains(assembly.FullName);
            }
        }

        private static void MarkChecked(Assembly assembly) {
            lock (checkedAssemblies) {
                checkedAssemblies.Add(assembly.FullName);
            }
        }

        private static void CheckDependencies(IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>> dependencies, TypeInfo recognizerType) {
            var ruleNames = GetRuleNames(recognizerType);
            var ruleVersions = GetRuleVersions(recognizerType, ruleNames);
            var relations = ExtractRuleRelations(recognizerType);
            var errors = new StringBuilder();
            foreach (var dependency in dependencies) {
                if (!dependency.Item1.Recognizer.GetTypeInfo().IsAssignableFrom(recognizerType)) {
                    continue;
                }
                // this is the rule in the dependency set with the highest version number
                var effectiveRule = dependency.Item1.Rule;
                if (effectiveRule < 0 || effectiveRule >= ruleVersions.Length) {
                    var message = string.Format("Rule dependency on unknown rule {0}@{1} in {2}", dependency.Item1.Rule, dependency.Item1.Version, dependency.Item1.Recognizer.ToString());
                    errors.AppendLine(dependency.Item2.ToString());
                    errors.AppendLine(message);
                    continue;
                }
                var dependents = Dependents.Self | dependency.Item1.Dependents;
                ReportUnimplementedDependents(errors, dependency, dependents);
                var @checked = new BitSet();
                var highestRequiredDependency = CheckDependencyVersion(errors, dependency, ruleNames, ruleVersions, effectiveRule, null);
                if ((dependents & Dependents.Parents) != 0) {
                    var parents = relations.parents[dependency.Item1.Rule];
                    for (var parent = parents.NextSetBit(0); parent >= 0; parent = parents.NextSetBit(parent + 1)) {
                        if (parent < 0 || parent >= ruleVersions.Length || @checked.Get(parent)) {
                            continue;
                        }
                        @checked.Set(parent);
                        var required = CheckDependencyVersion(errors, dependency, ruleNames, ruleVersions, parent, "parent");
                        highestRequiredDependency = Math.Max(highestRequiredDependency, required);
                    }
                }
                if ((dependents & Dependents.Children) != 0) {
                    var children = relations.children[dependency.Item1.Rule];
                    for (var child = children.NextSetBit(0); child >= 0; child = children.NextSetBit(child + 1)) {
                        if (child < 0 || child >= ruleVersions.Length || @checked.Get(child)) {
                            continue;
                        }
                        @checked.Set(child);
                        var required = CheckDependencyVersion(errors, dependency, ruleNames, ruleVersions, child, "child");
                        highestRequiredDependency = Math.Max(highestRequiredDependency, required);
                    }
                }
                if ((dependents & Dependents.Ancestors) != 0) {
                    var ancestors = relations.GetAncestors(dependency.Item1.Rule);
                    for (var ancestor = ancestors.NextSetBit(0); ancestor >= 0; ancestor = ancestors.NextSetBit(ancestor + 1)) {
                        if (ancestor < 0 || ancestor >= ruleVersions.Length || @checked.Get(ancestor)) {
                            continue;
                        }
                        @checked.Set(ancestor);
                        var required = CheckDependencyVersion(errors, dependency, ruleNames, ruleVersions, ancestor, "ancestor");
                        highestRequiredDependency = Math.Max(highestRequiredDependency, required);
                    }
                }
                if ((dependents & Dependents.Descendants) != 0) {
                    var descendants = relations.GetDescendants(dependency.Item1.Rule);
                    for (var descendant = descendants.NextSetBit(0); descendant >= 0; descendant = descendants.NextSetBit(descendant + 1)) {
                        if (descendant < 0 || descendant >= ruleVersions.Length || @checked.Get(descendant)) {
                            continue;
                        }
                        @checked.Set(descendant);
                        var required = CheckDependencyVersion(errors, dependency, ruleNames, ruleVersions, descendant, "descendant");
                        highestRequiredDependency = Math.Max(highestRequiredDependency, required);
                    }
                }
                var declaredVersion = dependency.Item1.Version;
                if (declaredVersion > highestRequiredDependency) {
                    var message = string.Format("Rule dependency version mismatch: {0} has maximum dependency version {1} (expected {2}) in {3}", ruleNames[dependency.Item1.Rule], highestRequiredDependency, declaredVersion, dependency.Item1.Recognizer.ToString());
                    errors.AppendLine(dependency.Item2.ToString());
                    errors.AppendLine(message);
                }
            }
            if (errors.Length > 0) {
                throw new InvalidOperationException(errors.ToString());
            }
        }

        private static readonly Dependents ImplementedDependents = Dependents.Self | Dependents.Parents | Dependents.Children | Dependents.Ancestors | Dependents.Descendants;

        private static void ReportUnimplementedDependents(StringBuilder errors, Tuple<RuleDependencyAttribute, ICustomAttributeProvider> dependency, Dependents dependents) {
            var unimplemented = dependents;
            unimplemented &= ~ImplementedDependents;
            if (unimplemented != Dependents.None) {
                var message = string.Format("Cannot validate the following dependents of rule {0}: {1}", dependency.Item1.Rule, unimplemented);
                errors.AppendLine(message);
            }
        }

        private static int CheckDependencyVersion(StringBuilder errors, Tuple<RuleDependencyAttribute, ICustomAttributeProvider> dependency, string[] ruleNames, int[] ruleVersions, int relatedRule, string relation) {
            var ruleName = ruleNames[dependency.Item1.Rule];
            string path;
            if (relation == null) {
                path = ruleName;
            } else {
                var mismatchedRuleName = ruleNames[relatedRule];
                path = string.Format("rule {0} ({1} of {2})", mismatchedRuleName, relation, ruleName);
            }
            var declaredVersion = dependency.Item1.Version;
            var actualVersion = ruleVersions[relatedRule];
            if (actualVersion > declaredVersion) {
                var message = string.Format("Rule dependency version mismatch: {0} has version {1} (expected <= {2}) in {3}", path, actualVersion, declaredVersion, dependency.Item1.Recognizer.ToString());
                errors.AppendLine(dependency.Item2.ToString());
                errors.AppendLine(message);
            }
            return actualVersion;
        }

        private static int[] GetRuleVersions(TypeInfo recognizerClass, string[] ruleNames) {
            var versions = new int[ruleNames.Length];
            var fields = recognizerClass.DeclaredFields;
            foreach (var field in fields) {
                var isStatic = field.IsStatic;
                var isInteger = field.FieldType == typeof(int);
                if (isStatic && isInteger && field.Name.StartsWith("RULE_")) {
                    try {
                        var name = field.Name["RULE_".Length..];
                        if (name.Length == 0 || !char.IsLower(name[0])) {
                            continue;
                        }
                        var index = (int)field.GetValue(null);
                        if (index < 0 || index >= versions.Length) {
                            var @params = new object[] { index, field.Name, recognizerClass.Name };
#if false
                            Logger.Log(Level.Warning, "Rule index {0} for rule ''{1}'' out of bounds for recognizer {2}.", @params);
#endif
                            continue;
                        }
                        var ruleMethod = GetRuleMethod(recognizerClass, name);
                        if (ruleMethod == null) {
                            var @params = new object[] { name, recognizerClass.Name };
#if false
                            Logger.Log(Level.Warning, "Could not find rule method for rule ''{0}'' in recognizer {1}.", @params);
#endif
                            continue;
                        }
                        var ruleVersion = ruleMethod.GetCustomAttribute<RuleVersionAttribute>();
                        var version = ruleVersion != null ? ruleVersion.Version : 0;
                        versions[index] = version;
                    } catch (ArgumentException) {
#if false
                        Logger.Log(Level.Warning, null, ex);
#else
                        throw;
#endif
                    } catch (MemberAccessException) {
#if false
                        Logger.Log(Level.Warning, null, ex);
#else
                        throw;
#endif
                    }
                }
            }
            return versions;
        }

        private static MethodInfo GetRuleMethod(TypeInfo recognizerClass, string name) {
            var declaredMethods = recognizerClass.DeclaredMethods;
            foreach (var method in declaredMethods) {
                if (method.Name.Equals(name) && method.GetCustomAttribute<RuleVersionAttribute>() != null) {
                    return method;
                }
            }
            return null;
        }

        private static string[] GetRuleNames(TypeInfo recognizerClass) {
            var ruleNames = recognizerClass.DeclaredFields.First(i => i.Name == "ruleNames");
            return (string[])ruleNames.GetValue(null);
        }

        public static IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>> GetDependencies(TypeInfo clazz) {
            IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>> result = new List<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>>();

            GetElementDependencies(AsCustomAttributeProvider(clazz), result);
            foreach (var ctor in clazz.DeclaredConstructors) {
                GetElementDependencies(AsCustomAttributeProvider(ctor), result);
                foreach (var parameter in ctor.GetParameters()) {
                    GetElementDependencies(AsCustomAttributeProvider(parameter), result);
                }
            }

            foreach (var field in clazz.DeclaredFields) {
                GetElementDependencies(AsCustomAttributeProvider(field), result);
            }

            foreach (var method in clazz.DeclaredMethods) {
                GetElementDependencies(AsCustomAttributeProvider(method), result);

                if (method.ReturnParameter != null) {
                    GetElementDependencies(AsCustomAttributeProvider(method.ReturnParameter), result);
                }

                foreach (var parameter in method.GetParameters()) {
                    GetElementDependencies(AsCustomAttributeProvider(parameter), result);
                }
            }

            return result;
        }

        private static void GetElementDependencies(ICustomAttributeProvider annotatedElement, IList<Tuple<RuleDependencyAttribute, ICustomAttributeProvider>> result) {
            foreach (RuleDependencyAttribute dependency in annotatedElement.GetCustomAttributes(typeof(RuleDependencyAttribute), true)) {
                result.Add(Tuple.Create(dependency, annotatedElement));
            }
        }

        private static RuleDependencyChecker.RuleRelations ExtractRuleRelations(TypeInfo recognizer) {
            var serializedATN = GetSerializedATN(recognizer);
            if (serializedATN == null) {
                return null;
            }
            var atn = new ATNDeserializer().Deserialize(serializedATN);
            var relations = new RuleDependencyChecker.RuleRelations(atn.ruleToStartState.Length);
            foreach (var state in atn.states) {
                if (!state.epsilonOnlyTransitions) {
                    continue;
                }
                foreach (var transition in state.transitions) {
                    if (transition.TransitionType != TransitionType.RULE) {
                        continue;
                    }
                    var ruleTransition = (RuleTransition)transition;
                    relations.AddRuleInvocation(state.ruleIndex, ruleTransition.target.ruleIndex);
                }
            }
            return relations;
        }

        private static int[] GetSerializedATN(TypeInfo recognizerClass) {
            var serializedAtnField = recognizerClass.DeclaredFields.First(i => i.Name == "_serializedATN");
            if (serializedAtnField != null) {
                return (int[])serializedAtnField.GetValue(null);
            }

            if (recognizerClass.BaseType != null) {
                return GetSerializedATN(recognizerClass.BaseType.GetTypeInfo());
            }

            return null;
        }

        private sealed class RuleRelations {
            public readonly BitSet[] parents;

            public readonly BitSet[] children;

            public RuleRelations(int ruleCount) {
                parents = new BitSet[ruleCount];
                for (var i = 0; i < ruleCount; i++) {
                    parents[i] = new BitSet();
                }
                children = new BitSet[ruleCount];
                for (var i_1 = 0; i_1 < ruleCount; i_1++) {
                    children[i_1] = new BitSet();
                }
            }

            public bool AddRuleInvocation(int caller, int callee) {
                if (caller < 0) {
                    // tokens rule
                    return false;
                }
                if (children[caller].Get(callee)) {
                    // already added
                    return false;
                }
                children[caller].Set(callee);
                parents[callee].Set(caller);
                return true;
            }

            public BitSet GetAncestors(int rule) {
                var ancestors = new BitSet();
                ancestors.Or(parents[rule]);
                while (true) {
                    var cardinality = ancestors.Cardinality();
                    for (var i = ancestors.NextSetBit(0); i >= 0; i = ancestors.NextSetBit(i + 1)) {
                        ancestors.Or(parents[i]);
                    }
                    if (ancestors.Cardinality() == cardinality) {
                        // nothing changed
                        break;
                    }
                }
                return ancestors;
            }

            public BitSet GetDescendants(int rule) {
                var descendants = new BitSet();
                descendants.Or(children[rule]);
                while (true) {
                    var cardinality = descendants.Cardinality();
                    for (var i = descendants.NextSetBit(0); i >= 0; i = descendants.NextSetBit(i + 1)) {
                        descendants.Or(children[i]);
                    }
                    if (descendants.Cardinality() == cardinality) {
                        // nothing changed
                        break;
                    }
                }
                return descendants;
            }
        }

        private RuleDependencyChecker() {
        }

        public interface ICustomAttributeProvider {
            object[] GetCustomAttributes(Type attributeType, bool inherit);
        }

        protected static ICustomAttributeProvider AsCustomAttributeProvider(TypeInfo type) {
            return new TypeCustomAttributeProvider(type);
        }

        protected static ICustomAttributeProvider AsCustomAttributeProvider(MethodBase method) {
            return new MethodBaseCustomAttributeProvider(method);
        }

        protected static ICustomAttributeProvider AsCustomAttributeProvider(ParameterInfo parameter) {
            return new ParameterInfoCustomAttributeProvider(parameter);
        }

        protected static ICustomAttributeProvider AsCustomAttributeProvider(FieldInfo field) {
            return new FieldInfoCustomAttributeProvider(field);
        }

        protected sealed class TypeCustomAttributeProvider : ICustomAttributeProvider {
            private readonly TypeInfo _provider;

            public TypeCustomAttributeProvider(TypeInfo provider) {
                _provider = provider;
            }

            public object[] GetCustomAttributes(Type attributeType, bool inherit) {
                return _provider.GetCustomAttributes(attributeType, inherit).ToArray();
            }
        }

        protected sealed class MethodBaseCustomAttributeProvider : ICustomAttributeProvider {
            private readonly MethodBase _provider;

            public MethodBaseCustomAttributeProvider(MethodBase provider) {
                _provider = provider;
            }

            public object[] GetCustomAttributes(Type attributeType, bool inherit) {
                return _provider.GetCustomAttributes(attributeType, inherit).ToArray();
            }
        }

        protected sealed class ParameterInfoCustomAttributeProvider : ICustomAttributeProvider {
            private readonly ParameterInfo _provider;

            public ParameterInfoCustomAttributeProvider(ParameterInfo provider) {
                _provider = provider;
            }

            public object[] GetCustomAttributes(Type attributeType, bool inherit) {
                return _provider.GetCustomAttributes(attributeType, inherit).ToArray();
            }
        }

        protected sealed class FieldInfoCustomAttributeProvider : ICustomAttributeProvider {
            private readonly FieldInfo _provider;

            public FieldInfoCustomAttributeProvider(FieldInfo provider) {
                _provider = provider;
            }

            public object[] GetCustomAttributes(Type attributeType, bool inherit) {
                return _provider.GetCustomAttributes(attributeType, inherit).ToArray();
            }
        }
    }
}
