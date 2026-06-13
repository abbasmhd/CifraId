using System.Text;

namespace CifraId.Encoding;

/// <summary>
/// Internal Hashids-style reversible obfuscation algorithm.
/// Produces short alphanumeric hashes from non-negative integers
/// using salt-based alphabet shuffling.
/// </summary>
internal sealed class HashidsAlgorithm
{
    private const int MinAlphabetLength = 16;
    private const double SeparatorDiv = 3.3;
    private const double GuardDiv = 12.0;

    private readonly string _salt;
    private readonly string _alphabet;
    private readonly string _separators;
    private readonly string _guards;
    private readonly int _minHashLength;

    public HashidsAlgorithm(string salt, int minHashLength, string alphabet, string separators)
    {
        _salt = salt ?? string.Empty;
        _minHashLength = Math.Max(0, minHashLength);

        var seen = new HashSet<char>();
        var uniqueAlphabet = new StringBuilder();
        foreach (var c in alphabet)
        {
            if (seen.Add(c))
            {
                uniqueAlphabet.Append(c);
            }
        }

        if (uniqueAlphabet.Length < MinAlphabetLength)
        {
            throw new ArgumentException(
                $"Alphabet must contain at least {MinAlphabetLength} unique characters.",
                nameof(alphabet));
        }

        var currentAlphabet = uniqueAlphabet.ToString();

        var filteredSeps = new StringBuilder();
        var sepSeen = new HashSet<char>();
        foreach (var c in separators)
        {
            if (currentAlphabet.Contains(c) && sepSeen.Add(c))
            {
                filteredSeps.Append(c);
            }
        }

        var alphaBuilder = new StringBuilder();
        foreach (var c in currentAlphabet)
        {
            if (!filteredSeps.ToString().Contains(c))
            {
                alphaBuilder.Append(c);
            }
        }

        currentAlphabet = alphaBuilder.ToString();
        var currentSeparators = ConsistentShuffle(filteredSeps.ToString(), _salt);

        if (currentSeparators.Length == 0 ||
            (float)currentAlphabet.Length / currentSeparators.Length > SeparatorDiv)
        {
            var sepsLen = (int)Math.Ceiling(currentAlphabet.Length / SeparatorDiv);
            if (sepsLen == 1)
            {
                sepsLen = 2;
            }

            if (sepsLen > currentSeparators.Length)
            {
                var diff = sepsLen - currentSeparators.Length;
                currentSeparators += currentAlphabet[..diff];
                currentAlphabet = currentAlphabet[diff..];
            }
            else
            {
                currentSeparators = currentSeparators[..sepsLen];
            }
        }

        currentAlphabet = ConsistentShuffle(currentAlphabet, _salt);

        var guardCount = (int)Math.Ceiling(currentAlphabet.Length / GuardDiv);
        string guards;

        if (currentAlphabet.Length < 3)
        {
            guards = currentSeparators[..guardCount];
            currentSeparators = currentSeparators[guardCount..];
        }
        else
        {
            guards = currentAlphabet[..guardCount];
            currentAlphabet = currentAlphabet[guardCount..];
        }

        _alphabet = currentAlphabet;
        _separators = currentSeparators;
        _guards = guards;
    }

    public string Encode(params long[] numbers)
    {
        if (numbers.Length == 0)
        {
            return string.Empty;
        }

        foreach (var n in numbers)
        {
            if (n < 0)
            {
                return string.Empty;
            }
        }

        long numberHashInt = 0;
        for (var i = 0; i < numbers.Length; i++)
        {
            numberHashInt += numbers[i] % (i + 100);
        }

        var alphabet = _alphabet;
        var lottery = alphabet[(int)(numberHashInt % alphabet.Length)];
        var result = new StringBuilder();
        result.Append(lottery);

        for (var i = 0; i < numbers.Length; i++)
        {
            var number = numbers[i];
            var alphabetSalt = $"{lottery}{_salt}{alphabet}";
            alphabet = ConsistentShuffle(alphabet, alphabetSalt[..alphabet.Length]);
            var last = HashNumber(number, alphabet);
            result.Append(last);

            if (i + 1 < numbers.Length)
            {
                number %= last[0] + i;
                result.Append(_separators[(int)(number % _separators.Length)]);
            }
        }

        if (result.Length < _minHashLength)
        {
            var guardIndex = (int)((numberHashInt + result[0]) % _guards.Length);
            result.Insert(0, _guards[guardIndex]);

            if (result.Length < _minHashLength)
            {
                guardIndex = (int)((numberHashInt + result[2]) % _guards.Length);
                result.Append(_guards[guardIndex]);
            }
        }

        var halfLength = alphabet.Length / 2;
        while (result.Length < _minHashLength)
        {
            alphabet = ConsistentShuffle(alphabet, alphabet);
            result.Insert(0, alphabet[halfLength..]);
            result.Append(alphabet[..halfLength]);

            var excess = result.Length - _minHashLength;
            if (excess > 0)
            {
                var startPos = excess / 2;
                var resultStr = result.ToString();
                result.Clear();
                result.Append(resultStr.Substring(startPos, _minHashLength));
            }
        }

        return result.ToString();
    }

    public long[] Decode(string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return [];
        }

        var hashBreakdown = ExtractHashBody(hash);

        if (hashBreakdown.Length == 0)
        {
            return [];
        }

        var lottery = hashBreakdown[0];
        var hashBuffer = hashBreakdown[1..];

        var separatorRanges = Split(hashBuffer.AsSpan(), _separators.AsSpan());

        var alphabet = _alphabet;
        var result = new List<long>(separatorRanges.Count);

        foreach (var (start, length) in separatorRanges)
        {
            if (length == 0)
            {
                continue;
            }

            var subHash = hashBuffer.Substring(start, length);

            var alphabetSalt = $"{lottery}{_salt}{alphabet}";
            alphabet = ConsistentShuffle(alphabet, alphabetSalt[..alphabet.Length]);
            result.Add(UnhashNumber(subHash, alphabet));
        }

        if (result.Count == 0)
        {
            return [];
        }

        var verification = Encode(result.ToArray());
        if (verification != hash)
        {
            return [];
        }

        return result.ToArray();
    }

    private string ExtractHashBody(string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return string.Empty;
        }

        var guardRanges = Split(hash.AsSpan(), _guards.AsSpan());
        if (guardRanges.Count == 0)
        {
            return string.Empty;
        }

        var unguardedIndex = guardRanges.Count is 3 or 2 ? 1 : 0;
        var (start, length) = guardRanges[unguardedIndex];
        return hash.Substring(start, length);
    }

    private static List<(int Start, int Length)> Split(ReadOnlySpan<char> line, ReadOnlySpan<char> separators)
    {
        var ranges = new List<(int Start, int Length)>();
        var indexStart = 0;

        while (indexStart < line.Length)
        {
            var nextSeparatorIndex = line[indexStart..].IndexOfAny(separators);
            if (nextSeparatorIndex == 0)
            {
                indexStart++;
                continue;
            }

            if (nextSeparatorIndex == -1)
            {
                var remaining = line.Length - indexStart;
                if (remaining > 0)
                {
                    ranges.Add((indexStart, remaining));
                }

                break;
            }

            ranges.Add((indexStart, nextSeparatorIndex));
            indexStart += nextSeparatorIndex + 1;
        }

        return ranges;
    }

    private static string ConsistentShuffle(string alphabet, string salt)
    {
        if (string.IsNullOrEmpty(salt))
        {
            return alphabet;
        }

        var chars = alphabet.ToCharArray();
        var p = 0;

        for (int i = chars.Length - 1, v = 0; i > 0; i--, v++)
        {
            v %= salt.Length;
            var ascVal = (int)salt[v];
            p += ascVal;
            var j = (ascVal + v + p) % i;
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static string HashNumber(long input, string alphabet)
    {
        var result = new StringBuilder();

        do
        {
            result.Insert(0, alphabet[(int)(input % alphabet.Length)]);
            input /= alphabet.Length;
        } while (input > 0);

        return result.ToString();
    }

    private static long UnhashNumber(string input, string alphabet)
    {
        long number = 0;

        foreach (var c in input)
        {
            var pos = alphabet.IndexOf(c);
            if (pos < 0)
            {
                return -1;
            }

            number = number * alphabet.Length + pos;
        }

        return number;
    }
}
