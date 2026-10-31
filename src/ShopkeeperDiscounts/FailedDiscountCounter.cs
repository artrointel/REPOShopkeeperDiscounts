using System;

namespace ShopkeeperDiscounts;

// One instance per physical shop item, shared by all players using that item.
internal sealed class FailedDiscountCounter
{
    private int _lastVerdictSequence;
    internal int Failures { get; private set; }
    internal bool BreakAllowed { get; private set; }

    internal void ObserveVerdict(int sequence, bool failed, int breakAfterFailures)
    {
        // Native TimesShown increases once per completed verdict. Replayed
        // callbacks must not count one rejection more than once.
        if (sequence <= _lastVerdictSequence)
        {
            return;
        }
        _lastVerdictSequence = sequence;
        BreakAllowed = false;
        if (!failed)
        {
            return;
        }
        if (Failures < int.MaxValue)
        {
            Failures++;
        }
        // Freeze the setting for this verdict, before the delayed native toss
        // and rejection effect. Success never refunds accumulated failures.
        int threshold = Math.Max(1, Math.Min(5, breakAfterFailures));
        BreakAllowed = Failures >= threshold;
    }
}
