using Antlr4.Runtime;
using NCalc.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace NCalc {
    public class Expression {
        public EvaluateOptions Options { get; set; }

        /// <summary>
        /// Textual representation of the expression to evaluate.
        /// </summary>
        protected string OriginalExpression;

        /// <summary>
        /// Get or set the culture info
        /// </summary>
        protected CultureInfo CultureInfo;

        public Expression(string expression) : this(expression, EvaluateOptions.None, CultureInfo.CurrentCulture) {
        }

        public Expression(string expression, CultureInfo cultureInfo) : this(expression, EvaluateOptions.None, cultureInfo) {
        }

        public Expression(string expression, EvaluateOptions options) : this(expression, options, CultureInfo.CurrentCulture) {
        }

        public Expression(string expression, EvaluateOptions options, CultureInfo cultureInfo) {
            if (string.IsNullOrEmpty(expression)) {
                throw new
                    ArgumentException("Expression can't be empty", "expression");
            }

            OriginalExpression = expression;
            Options = options;
            CultureInfo = cultureInfo;
        }

        public Expression(LogicalExpression expression) : this(expression, EvaluateOptions.None, CultureInfo.CurrentCulture) {
        }

        public Expression(LogicalExpression expression, EvaluateOptions options, CultureInfo cultureInfo) {
            if (expression == null) {
                throw new
                    ArgumentException("Expression can't be null", "expression");
            }

            ParsedExpression = expression;
            Options = options;
            CultureInfo = cultureInfo;
        }

        #region Cache management
        private static bool _cacheEnabled = true;
        private static Dictionary<string, WeakReference> _compiledExpressions = new Dictionary<string, WeakReference>();
        private static readonly ReaderWriterLock Rwl = new ReaderWriterLock();

        public static bool CacheEnabled {
            get { return _cacheEnabled; }
            set {
                _cacheEnabled = value;

                if (!CacheEnabled) {
                    // Clears cache
                    _compiledExpressions = new Dictionary<string, WeakReference>();
                }
            }
        }

        /// <summary>
        /// Removed unused entries from cached compiled expression
        /// </summary>
        private static void CleanCache() {
            var keysToRemove = new List<string>();

            try {
                Rwl.AcquireWriterLock(Timeout.Infinite);
                foreach (var de in _compiledExpressions) {
                    if (!de.Value.IsAlive) {
                        keysToRemove.Add(de.Key);
                    }
                }

                foreach (var key in keysToRemove) {
                    _compiledExpressions.Remove(key);
                    Trace.TraceInformation("Cache entry released: " + key);
                }
            } finally {
                Rwl.ReleaseReaderLock();
            }
        }

        #endregion

        public static LogicalExpression Compile(string expression, bool nocache) {
            LogicalExpression logicalExpression = null;

            if (_cacheEnabled && !nocache) {
                try {
                    Rwl.AcquireReaderLock(Timeout.Infinite);

                    if (_compiledExpressions.ContainsKey(expression)) {
                        Trace.TraceInformation("Expression retrieved from cache: " + expression);
                        var wr = _compiledExpressions[expression];
                        logicalExpression = wr.Target as LogicalExpression;

                        if (wr.IsAlive && logicalExpression != null) {
                            return logicalExpression;
                        }
                    }
                } finally {
                    Rwl.ReleaseReaderLock();
                }
            }

            if (logicalExpression == null) {
                var lexer = new NCalcLexer(new AntlrInputStream(expression));
                var errorListenerLexer = new ErrorListenerLexer();
                lexer.AddErrorListener(errorListenerLexer);

                var parser = new NCalcParser(new CommonTokenStream(lexer));
                var errorListenerParser = new ErrorListenerParser();
                parser.AddErrorListener(errorListenerParser);

                try {
                    logicalExpression = parser.ncalcExpression().retValue;
                } catch (Exception ex) {
                    var message = new StringBuilder(ex.Message);
                    if (errorListenerLexer.Errors.Any()) {
                        message.AppendLine();
                        message.AppendLine(string.Join(Environment.NewLine, errorListenerLexer.Errors.ToArray()));
                    }
                    if (errorListenerParser.Errors.Any()) {
                        message.AppendLine();
                        message.AppendLine(string.Join(Environment.NewLine, errorListenerParser.Errors.ToArray()));
                    }

                    throw new EvaluationException(message.ToString());
                }
                if (errorListenerLexer.Errors.Any()) {
                    throw new EvaluationException(string.Join(Environment.NewLine, errorListenerLexer.Errors.ToArray()));
                }
                if (errorListenerParser.Errors.Any()) {
                    throw new EvaluationException(string.Join(Environment.NewLine, errorListenerParser.Errors.ToArray()));
                }

                if (_cacheEnabled && !nocache) {
                    try {
                        Rwl.AcquireWriterLock(Timeout.Infinite);
                        _compiledExpressions[expression] = new WeakReference(logicalExpression);
                    } finally {
                        Rwl.ReleaseWriterLock();
                    }

                    CleanCache();

                    Trace.TraceInformation("Expression added to cache: " + expression);
                }
            }

            return logicalExpression;
        }

        /// <summary>
        /// Pre-compiles the expression in order to check syntax errors.
        /// If errors are detected, the Error property contains the message.
        /// </summary>
        /// <returns>True if the expression syntax is correct, otherwiser False</returns>
        public bool HasErrors() {
            try {
                ParsedExpression ??= Compile(OriginalExpression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);

                // In case HasErrors() is called multiple times for the same expression
                return ParsedExpression != null && Error != null;
            } catch (Exception e) {
                Error = e.Message;
                return true;
            }
        }

        public string Error { get; private set; }

        public LogicalExpression ParsedExpression { get; private set; }

        protected Dictionary<string, IEnumerator> ParameterEnumerators;
        protected Dictionary<string, object> ParametersBackup;

        public object Evaluate() {
            if (HasErrors()) {
                throw new EvaluationException(Error);
            }

            ParsedExpression ??= Compile(OriginalExpression, (Options & EvaluateOptions.NoCache) == EvaluateOptions.NoCache);

            var visitor = new EvaluationVisitor(Options, CultureInfo);
            visitor.EvaluateFunction += EvaluateFunction;
            visitor.EvaluateParameter += EvaluateParameter;
            visitor.Parameters = Parameters;

            // if array evaluation, execute the same expression multiple times
            if ((Options & EvaluateOptions.IterateParameters) == EvaluateOptions.IterateParameters) {
                var size = -1;
                ParametersBackup = new Dictionary<string, object>();
                foreach (var key in Parameters.Keys) {
                    ParametersBackup.Add(key, Parameters[key]);
                }

                ParameterEnumerators = new Dictionary<string, IEnumerator>();

                foreach (var parameter in Parameters.Values) {
                    if (parameter is IEnumerable) {
                        var localsize = 0;
                        foreach (var o in (IEnumerable)parameter) {
                            localsize++;
                        }

                        if (size == -1) {
                            size = localsize;
                        } else if (localsize != size) {
                            throw new EvaluationException("When IterateParameters option is used, IEnumerable parameters must have the same number of items");
                        }
                    }
                }

                foreach (var key in Parameters.Keys) {
                    if (Parameters[key] is IEnumerable parameter) {
                        ParameterEnumerators.Add(key, parameter.GetEnumerator());
                    }
                }

                var results = new List<object>();
                for (var i = 0; i < size; i++) {
                    foreach (var key in ParameterEnumerators.Keys) {
                        var enumerator = ParameterEnumerators[key];
                        enumerator.MoveNext();
                        Parameters[key] = enumerator.Current;
                    }

                    ParsedExpression.Accept(visitor);
                    results.Add(visitor.Result);
                }

                return results;
            }

            ParsedExpression.Accept(visitor);
            return visitor.Result;

        }

        public event EvaluateFunctionHandler EvaluateFunction;
        public event EvaluateParameterHandler EvaluateParameter;

        private Dictionary<string, object> _parameters;

        public Dictionary<string, object> Parameters {
            get { return _parameters ??= new Dictionary<string, object>(); }
            set { _parameters = value; }
        }
    }
}
