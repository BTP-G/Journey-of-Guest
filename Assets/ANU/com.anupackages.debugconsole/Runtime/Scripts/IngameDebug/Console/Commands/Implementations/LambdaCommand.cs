using NDesk.Options;
using System;
using System.Collections.Generic;

namespace ANU.IngameDebug.Console.Commands.Implementations {
    public class LambdaCommand : ADebugCommand {
        private Func<Dictionary<Option, AvailableValuesHint>, OptionSet> _optionsGetter;
        private readonly Action _onParsed;

        public LambdaCommand(string name, string description, Func<Dictionary<Option, AvailableValuesHint>, OptionSet> optionsGetter, Action onParsed = null) : base(name, description) {
            _optionsGetter = optionsGetter;
            _onParsed = onParsed;
        }

        public LambdaCommand(string name, string description, Action noOptions)
            : this(name, description, valueHints => new OptionSet(), noOptions) { }

        protected override OptionSet CreateOptions(Dictionary<Option, AvailableValuesHint> valueHints) {
            return _optionsGetter.Invoke(InternalValueHints);
        }

        protected override ExecutionResult OnParsed() {
            _onParsed?.Invoke();
            return default;
        }
    }
}
