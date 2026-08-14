using ANU.IngameDebug.Console.CommandLinePreprocessors;
using ANU.IngameDebug.Utils;
using System.Linq;

namespace ANU.IngameDebug.Console {
    public class NamedParametersPreprocessor : ICommandInputPreprocessor, IInjectDebugConsoleContext {
        public int Priority => 100;

        public IReadOnlyDebugConsoleProcessor Context { get; set; }

        public string Preprocess(string input) {
            var namedParameterGroup = input.GetFirstNamedParameter();

            var inputToAddParameterNames = input;
            var concatInput = "";

            if (namedParameterGroup.Success) {
                inputToAddParameterNames = input[..namedParameterGroup.Index];
                concatInput = input[namedParameterGroup.Index..];
            }

            var commandLine = inputToAddParameterNames.SplitCommandLine();

            var commandName = commandLine.First();

            if (!Context.Commands.Commands.TryGetValue(commandName, out var command)) {
                return input;
            }

            var namedParameters = commandLine.Skip(1).Zip(command.Options, (p, o) => {
                return o.OptionValueType == NDesk.Options.OptionValueType.None
                    ? p
                    : $"--{o.GetNames().First()}=\"{p}\"";
            });

            input = string.Join(" ", namedParameters.Prepend(commandName).Append(concatInput));

            return input;
        }
    }
}
