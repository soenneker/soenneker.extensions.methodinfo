using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Soenneker.Extensions.MethodInfo;

/// <summary>
/// A collection of useful MethodInfo methods
/// </summary>
public static class MethodInfoExtension
{
    private static readonly ConditionalWeakTable<System.Reflection.MethodInfo, string> _signatureCache = new();

    /// <summary>
    /// Generates a C#-style signature string for the specified method.
    /// </summary>
    /// <remarks>
    /// The generated signature includes:
    /// <list type="bullet">
    /// <item><description>Access modifier (<c>public</c> or <c>private</c>)</description></item>
    /// <item><description>Method modifiers (<c>abstract</c>, <c>static</c>, <c>virtual</c>)</description></item>
    /// <item><description>Return type name</description></item>
    /// <item><description>Method name</description></item>
    /// <item><description>Parameter list with type names and parameter names</description></item>
    /// </list>
    /// <para>
    /// This method uses an internal cache to avoid repeated reflection and string allocations
    /// when called multiple times for the same <see cref="System.Reflection.MethodInfo"/> instance.
    /// </para>
    /// </remarks>
    /// <param name="methodInfo">
    /// The <see cref="System.Reflection.MethodInfo"/> instance to generate a signature for.
    /// </param>
    /// <returns>
    /// A string containing the formatted method signature.
    /// If <paramref name="methodInfo"/> is <c>null</c>, an empty string is returned.
    /// </returns>
    [Pure]
    public static string GetSignature(this System.Reflection.MethodInfo? methodInfo)
    {
        if (methodInfo is null)
            return string.Empty;

        if (_signatureCache.TryGetValue(methodInfo, out string? cached))
            return cached;

        string sig = BuildSignature(methodInfo);
        _signatureCache.Add(methodInfo, sig);
        return sig;
    }

    /// <summary>
    /// Builds the method signature string without consulting or updating the cache.
    /// </summary>
    /// <remarks>
    /// This method performs reflection to retrieve parameter metadata and constructs
    /// the signature using a pre-sized <see cref="StringBuilder"/> to minimize allocations.
    /// <para>
    /// Callers should prefer <see cref="GetSignature(System.Reflection.MethodInfo?)"/> instead,
    /// which provides caching and avoids repeated work.
    /// </para>
    /// </remarks>
    /// <param name="methodInfo">
    /// The <see cref="System.Reflection.MethodInfo"/> to build the signature for.
    /// </param>
    /// <returns>
    /// A string containing the formatted method signature.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BuildSignature(System.Reflection.MethodInfo methodInfo)
    {
        // GetParameters() allocates the array; unavoidable without deeper caching.
        ParameterInfo[] parameters = methodInfo.GetParameters();

        // Slightly better starting capacity than a fixed 128 for small methods.
        // Rough estimate: base + per-param (type + space + name + ", ")
        int capacity = 48 + parameters.Length * 24;
        var sb = new StringBuilder(capacity);

        if (methodInfo.IsPrivate)
            sb.Append("private ");
        else if (methodInfo.IsPublic)
            sb.Append("public ");

        if (methodInfo.IsAbstract)
            sb.Append("abstract ");

        if (methodInfo.IsStatic)
            sb.Append("static ");

        if (methodInfo.IsVirtual && !methodInfo.IsAbstract)
            sb.Append("virtual ");

        sb.Append(methodInfo.ReturnType.Name);
        sb.Append(' ');
        sb.Append(methodInfo.Name);
        sb.Append('(');

        for (int i = 0; i < parameters.Length; i++)
        {
            if (i != 0)
                sb.Append(", ");

            ParameterInfo p = parameters[i];

            sb.Append(p.ParameterType.Name);
            sb.Append(' ');
            sb.Append(p.Name);
        }

        sb.Append(')');
        return sb.ToString();
    }

    /// <summary>
    /// Returns the original member name represented by the specified method.
    /// </summary>
    /// <remarks>
    /// This method is primarily intended for property and event accessor methods
    /// whose names are compiler-generated and prefixed with values such as:
    /// <list type="bullet">
    /// <item><description><c>get_</c></description></item>
    /// <item><description><c>set_</c></description></item>
    /// <item><description><c>add_</c></description></item>
    /// <item><description><c>remove_</c></description></item>
    /// </list>
    /// <para>
    /// For example, a method named <c>get_Value</c> will return <c>Value</c>.
    /// </para>
    /// <para>
    /// If the method is not a special name, the original method name is returned unchanged.
    /// </para>
    /// </remarks>
    /// <param name="methodInfo">
    /// The <see cref="System.Reflection.MethodInfo"/> instance to extract the original member name from.
    /// </param>
    /// <returns>
    /// The original member name with any accessor prefixes removed,
    /// or the method name unchanged if no prefix is present.
    /// </returns>
    [Pure]
    public static string ToOriginalMemberName(this System.Reflection.MethodInfo methodInfo)
    {
        // Fast path: not a special name => no allocation
        string name = methodInfo.Name;

        if (!methodInfo.IsSpecialName)
            return name;

        ReadOnlySpan<char> span = name.AsSpan();

        // Common special-name fast paths (property + event accessors)
        if (span.StartsWith("get_".AsSpan(), StringComparison.Ordinal) || span.StartsWith("set_".AsSpan(), StringComparison.Ordinal) ||
            span.StartsWith("add_".AsSpan(), StringComparison.Ordinal))
        {
            return span.Length > 4 ? new string(span[4..]) : name;
        }

        if (span.StartsWith("remove_".AsSpan(), StringComparison.Ordinal))
        {
            return span.Length > 7 ? new string(span[7..]) : name;
        }

        int underscoreIndex = span.IndexOf('_');
        return underscoreIndex >= 0 && underscoreIndex < span.Length - 1 ? new string(span[(underscoreIndex + 1)..]) : name;
    }
}