using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Configuration;
using HarmonyLib;
using Mono.Cecil;
using ShopkeeperDiscounts;

int checks = 0;
void Check(bool condition, string message)
{
    checks++;
    if (!condition) throw new InvalidOperationException(message);
}

for (int chance = 0; chance <= 100; chance++)
{
    int successes = Enumerable.Range(0, 100).Count(roll => DiscountRules.Succeeds(chance, roll));
    Check(successes == chance, $"Incorrect success rate: {chance}%.");
}
Check(!DiscountRules.Succeeds(100, -1) && !DiscountRules.Succeeds(100, 100), "Invalid random draw accepted.");
Check(!DiscountRules.Succeeds(-10, 0) && DiscountRules.Succeeds(110, 99), "Chance bounds failed.");

string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
string scratch = Path.Combine(root, "artifacts", "check-config-" + Guid.NewGuid().ToString("N") + ".cfg");
Directory.CreateDirectory(Path.GetDirectoryName(scratch)!);
try
{
    var config = new ConfigFile(scratch, false);
    var settings = new DiscountSettings(config);
    Check(config.Count == 2, "REPOConfig must discover exactly the two requested settings.");
    Check(settings.ChancePercent.DefaultValue.Equals(70) && settings.AttemptsPerShop.DefaultValue.Equals(5), "Unexpected defaults.");
    settings.ChancePercent.Value = -1;
    Check(settings.ChancePercent.Value == 0, "Chance below zero was not clamped.");
    settings.ChancePercent.Value = 101;
    Check(settings.ChancePercent.Value == 100, "Chance above 100 was not clamped.");
    settings.AttemptsPerShop.Value = -1;
    Check(settings.AttemptsPerShop.Value == 0, "Negative attempt limit was not clamped.");
    settings.AttemptsPerShop.Value = 11;
    Check(settings.AttemptsPerShop.Value == 10, "Attempt limit above 10 was not clamped.");
    config.Save();
    var reloaded = new DiscountSettings(new ConfigFile(scratch, false));
    Check(reloaded.ChancePercent.Value == 100 && reloaded.AttemptsPerShop.Value == 10, "Settings did not persist.");
}
finally { if (File.Exists(scratch)) File.Delete(scratch); }

var counter = typeof(CounterFixture).GetField(nameof(CounterFixture.Used))!;
var getter = typeof(CounterFixture).GetMethod(nameof(CounterFixture.Limit))!;
var fixture = new DynamicMethod("DiscountLimit", typeof(bool), new[] { typeof(CounterFixture) });
var generator = fixture.GetILGenerator();
var label = generator.DefineLabel();
var nativeCode = new[] {
    new CodeInstruction(OpCodes.Ldarg_0), new CodeInstruction(OpCodes.Ldfld, counter),
    new CodeInstruction(OpCodes.Ldc_I4_3), new CodeInstruction(OpCodes.Clt), new CodeInstruction(OpCodes.Ret)
};
nativeCode[2].labels.Add(label);
var rewritten = DiscountIlRewriter.AttemptLimit(nativeCode, counter, getter, "fixture").ToList();
Check(rewritten[2].labels.Contains(label), "Rewriting lost branch labels.");
foreach (var instruction in rewritten)
{
    foreach (var branchLabel in instruction.labels) generator.MarkLabel(branchLabel);
    if (instruction.operand is FieldInfo field) generator.Emit(instruction.opcode, field);
    else if (instruction.operand is MethodInfo method) generator.Emit(instruction.opcode, method);
    else generator.Emit(instruction.opcode);
}
var canAttempt = (Func<CounterFixture, bool>)fixture.CreateDelegate(typeof(Func<CounterFixture, bool>));
for (int limit = 0; limit <= 10; limit++)
for (int used = 0; used <= 12; used++)
{
    CounterFixture.Maximum = limit;
    Check(canAttempt(new CounterFixture { Used = used }) == (used < limit), "Rewritten native attempt comparison failed.");
}
CounterFixture.Maximum = 8;
var ongoing = new CounterFixture { Used = 6 };
Check(canAttempt(ongoing), "Six attempts incorrectly exhausted a larger limit.");
CounterFixture.Maximum = 2;
Check(!canAttempt(ongoing) && ongoing.Used == 6, "Reducing the limit refunded spent attempts.");
CounterFixture.Maximum = 10;
Check(canAttempt(ongoing) && ongoing.Used == 6, "Raising the limit reset spent attempts.");
bool rejected = false;
try { DiscountIlRewriter.AttemptLimit(new[] { new CodeInstruction(OpCodes.Ret) }, counter, getter, "changed-game").ToList(); }
catch (InvalidOperationException) { rejected = true; }
Check(rejected, "Changed native IL was silently accepted.");

// Read actual installed game IL using Harmony's own method reader and run the
// production rewriter over it. No Unity object or game method is executed.
var nativeAttempts = AccessTools.DeclaredField(typeof(ShopKeeper), "itemDiscountAttempts");
foreach (string name in new[] { "ItemPresentationTargetValid", "ItemNoticeStart", "TryStartItemDiscount", "ItemDenyPhysics" })
{
    var method = AccessTools.DeclaredMethod(typeof(ShopKeeper), name);
    var instructions = PatchProcessor.GetOriginalInstructions(method, new DynamicMethod("ReadNativeIL", typeof(void), Type.EmptyTypes).GetILGenerator());
    var changed = DiscountIlRewriter.AttemptLimit(instructions, nativeAttempts, getter, name).ToList();
    Check(changed.Count(i => i.Calls(getter)) == 1, $"Native limit patch failed: {name}");
    Check(changed.Count == instructions.Count, "Limit rewrite changed native control-flow length.");
}
var verdict = AccessTools.DeclaredMethod(typeof(ShopKeeper), "ItemVerdictPositiveGet");
var range = AccessTools.DeclaredMethod(typeof(UnityEngine.Random), "Range", new[] { typeof(int), typeof(int) });
var draw = typeof(CounterFixture).GetMethod(nameof(CounterFixture.Draw))!;
var verdictOriginal = PatchProcessor.GetOriginalInstructions(verdict, new DynamicMethod("ReadNativeIL", typeof(void), Type.EmptyTypes).GetILGenerator());
var verdictChanged = DiscountIlRewriter.Verdict(verdictOriginal, range, draw).ToList();
Check(verdictChanged.Count(i => i.Calls(draw)) == 1, "Verdict draw was not replaced exactly once.");
Check(verdictChanged.Any(i => i.operand is MethodInfo method && method.Name == "ItemShownSet"), "Native item reaction bookkeeping was removed.");

string managed = Path.GetDirectoryName(typeof(ShopKeeper).Assembly.Location)!;
using var resolver = new DefaultAssemblyResolver();
resolver.AddSearchDirectory(managed);
var parameters = new ReaderParameters { AssemblyResolver = resolver };
using var game = AssemblyDefinition.ReadAssembly(Path.Combine(managed, "Assembly-CSharp.dll"), parameters);
var keeper = game.MainModule.Types.Single(t => t.Name == "ShopKeeper");
var rpc = keeper.Methods.Single(m => m.Name == "UpdateStateRPC");
Check(rpc.Parameters.Count == 9 && rpc.CustomAttributes.Any(a => a.AttributeType.Name == "PunRPC"), "Native state RPC contract changed.");
Check(rpc.Body.Instructions.Count(i => i.OpCode.Code == Mono.Cecil.Cil.Code.Stfld &&
    i.Operand is FieldReference field && field.Name == "itemDiscountAttempts") == 1, "Native counter clamp contract changed.");
foreach (string nativeMethod in new[] { "ItemDiscountApply", "ShopKeeperStateSend", "ItemShownSet" })
    Check(keeper.Methods.Single(m => m.Name == nativeMethod).Body.Instructions.Any(i =>
        i.Operand is MethodReference method && method.Name == "RPC"), $"Native outcome synchronization missing: {nativeMethod}");
foreach (string configuration in new[] { "Debug", "Release" })
{
    using var plugin = AssemblyDefinition.ReadAssembly(Path.Combine(root, "src/ShopkeeperDiscounts/bin", configuration, "netstandard2.1/ShopkeeperDiscounts.dll"), parameters);
    Check(!plugin.MainModule.Types.SelectMany(t => t.Methods).Any(m => m.CustomAttributes.Any(a => a.AttributeType.Name == "PunRPC")), "Custom RPC introduced.");
    Check(!plugin.MainModule.GetTypeReferences().Any(t => t.Name == "PhotonStream"), "Custom stream code introduced.");
    foreach (MemberReference reference in plugin.MainModule.GetMemberReferences())
    {
        if (reference.DeclaringType.Scope.Name != "Assembly-CSharp" && !reference.DeclaringType.Scope.Name.StartsWith("UnityEngine")) continue;
        Check(reference switch {
            MethodReference method => method.Resolve() != null,
            FieldReference field => field.Resolve() != null,
            _ => true
        }, $"Missing installed-game API: {reference.FullName}");
    }
    var hostPatch = plugin.MainModule.Types.Single(t => t.Name == "HostAttemptCounterPatch");
    Check(hostPatch.Methods.Any(m => m.Name == "Prefix") && hostPatch.Methods.Any(m => m.Name == "Postfix"), "Host counter preservation missing.");
}
// Execute the production postfix against an uninitialized managed wrapper. This
// touches only an int field: no Unity object constructor or native engine call.
var production = Assembly.LoadFrom(Path.Combine(root, "src/ShopkeeperDiscounts/bin/Release/netstandard2.1/ShopkeeperDiscounts.dll"));
var preserve = production.GetType("ShopkeeperDiscounts.HostAttemptCounterPatch")!
    .GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic)!;
var wrapper = (ShopKeeper)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(ShopKeeper));
for (int used = 0; used <= 10; used++)
{
    nativeAttempts.SetValue(wrapper, Math.Min(3, used));
    preserve.Invoke(null, new object[] { wrapper, used });
    Check((int)nativeAttempts.GetValue(wrapper)! == used, "Production postfix lost authoritative attempts above three.");
}
nativeAttempts.SetValue(wrapper, 3);
preserve.Invoke(null, new object[] { wrapper, -1 });
Check((int)nativeAttempts.GetValue(wrapper)! == 3, "Non-host counter was modified.");
Console.WriteLine($"Passed {checks} checks: exact chance boundaries, native BepInEx ranges/persistence, executable limit rewriting, actual game IL, native synchronization and assembly contracts.");

public class CounterFixture
{
    public int Used;
    public static int Maximum;
    public static int Limit() => Maximum;
    public static int Draw(int minimum, int maximum) => 0;
}
