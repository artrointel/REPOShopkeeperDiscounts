using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace ShopkeeperDiscounts;

// Kept separate so the exact production rewrite can be tested against native IL.
internal static class DiscountIlRewriter
{
    internal static IEnumerable<CodeInstruction> AttemptLimit(
        IEnumerable<CodeInstruction> instructions, FieldInfo attempts, MethodInfo getter, string methodName)
    {
        List<CodeInstruction> code = instructions.Select(i => new CodeInstruction(i)).ToList();
        int replacements = 0;
        for (int i = 1; i < code.Count; i++)
        {
            if (code[i - 1].LoadsField(attempts) && code[i].LoadsConstant(3))
            {
                // Mutate in place to preserve branch labels and exception blocks.
                code[i].opcode = OpCodes.Call;
                code[i].operand = getter;
                replacements++;
            }
        }
        if (replacements != 1)
        {
            throw new InvalidOperationException($"Expected one native discount limit in {methodName}, found {replacements}. Game API changed.");
        }
        return code;
    }

    internal static IEnumerable<CodeInstruction> Verdict(
        IEnumerable<CodeInstruction> instructions, MethodInfo nativeRange, MethodInfo draw)
    {
        List<CodeInstruction> code = instructions.Select(i => new CodeInstruction(i)).ToList();
        int replacements = 0;
        for (int i = 2; i < code.Count; i++)
        {
            if (code[i].Calls(nativeRange) && code[i - 2].LoadsConstant(0) && code[i - 1].LoadsConstant(3))
            {
                code[i].operand = draw;
                replacements++;
            }
        }
        if (replacements != 1)
        {
            throw new InvalidOperationException($"Expected one native discount verdict draw, found {replacements}. Game API changed.");
        }
        return code;
    }
}
