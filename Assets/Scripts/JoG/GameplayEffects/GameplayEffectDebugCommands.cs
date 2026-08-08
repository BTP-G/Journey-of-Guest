using ANU.IngameDebug.Console;
using Cysharp.Text;
using Xoderony.GameplayEffects;
using Xoderony.Logging;

[assembly: RegisterDebugCommandTypes(typeof(JoG.GameplayEffects.GameplayEffectDebugCommands))]

namespace JoG.GameplayEffects {

    [DebugCommandPrefix("effect")]
    public static class GameplayEffectDebugCommands {

        [DebugCommand]
        public static void PrintDefinitions() {
            using var builder = ZString.CreateStringBuilder(true);
            foreach (var definition in GameplayEffectDefinitionRegistry.Shared.Definitions) {
                builder.Append("id: ");
                builder.Append(definition.Id);
                builder.Append("; definition name: ");
                builder.AppendLine(definition.name);
            }
            GameplayEffectDefinitionRegistry.Shared.Log(builder.ToString());
        }
    }
}
