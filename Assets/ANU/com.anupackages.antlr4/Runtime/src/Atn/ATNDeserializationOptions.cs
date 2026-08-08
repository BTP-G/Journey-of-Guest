/* Copyright (c) 2012-2017 The ANTLR Project. All rights reserved.
 * Use of this file is governed by the BSD 3-clause license that
 * can be found in the LICENSE.txt file in the project root.
 */
using Antlr4.Runtime.Misc;
using System;

namespace Antlr4.Runtime.Atn {
    /// <author>Sam Harwell</author>
    public class ATNDeserializationOptions {
        private static readonly Antlr4.Runtime.Atn.ATNDeserializationOptions defaultOptions;

        static ATNDeserializationOptions() {
            defaultOptions = new Antlr4.Runtime.Atn.ATNDeserializationOptions();
            defaultOptions.MakeReadOnly();
        }

        private bool readOnly;

        private bool verifyATN;

        private bool generateRuleBypassTransitions;

        private bool optimize;

        public ATNDeserializationOptions() {
            verifyATN = true;
            generateRuleBypassTransitions = false;
            optimize = true;
        }

        public ATNDeserializationOptions(Antlr4.Runtime.Atn.ATNDeserializationOptions options) {
            verifyATN = options.verifyATN;
            generateRuleBypassTransitions = options.generateRuleBypassTransitions;
            optimize = options.optimize;
        }

        [NotNull]
        public static Antlr4.Runtime.Atn.ATNDeserializationOptions Default {
            get {
                return defaultOptions;
            }
        }

        public bool IsReadOnly {
            get {
                return readOnly;
            }
        }

        public void MakeReadOnly() {
            readOnly = true;
        }

        public bool VerifyAtn {
            get {
                return verifyATN;
            }
            set {
                var verifyATN = value;
                ThrowIfReadOnly();
                this.verifyATN = verifyATN;
            }
        }

        public bool GenerateRuleBypassTransitions {
            get {
                return generateRuleBypassTransitions;
            }
            set {
                var generateRuleBypassTransitions = value;
                ThrowIfReadOnly();
                this.generateRuleBypassTransitions = generateRuleBypassTransitions;
            }
        }

        public bool Optimize {
            get {
                return optimize;
            }
            set {
                var optimize = value;
                ThrowIfReadOnly();
                this.optimize = optimize;
            }
        }

        protected internal virtual void ThrowIfReadOnly() {
            if (IsReadOnly) {
                throw new InvalidOperationException("The object is read only.");
            }
        }
    }
}
