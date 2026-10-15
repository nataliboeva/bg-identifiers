namespace BgIdentifiers.Internal;

/// <summary>
/// Small helpers for working with ASCII digit spans. Callers are expected to
/// check <see cref="AreAll"/> before using the other members.
/// </summary>
internal static class Digits
{
    public static bool AreAll(ReadOnlySpan<char> value) =>
        !value.IsEmpty && !value.ContainsAnyExceptInRange('0', '9');

    public static int ValueAt(ReadOnlySpan<char> value, int index) => value[index] - '0';

    public static int TwoDigitNumber(ReadOnlySpan<char> value, int start) =>
        (ValueAt(value, start) * 10) + ValueAt(value, start + 1);

    public static int WeightedSum(ReadOnlySpan<char> digits, ReadOnlySpan<int> weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
        {
            sum += ValueAt(digits, i) * weights[i];
        }

        return sum;
    }
}
