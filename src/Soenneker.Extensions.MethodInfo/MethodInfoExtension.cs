using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Soenneker.Extensions.MethodInfo;

public static class MethodInfoExtension
{
    private static readonly ConditionalWeakTable<System.Reflection.MethodInfo, string> _signatureCache = new();

    /// <summary>
    /// Returns a string representation of the method signature for the specified method information.
    /// </summary>
    /// <remarks>The returned signature includes the method's name, parameters, and return type. The result is
    /// cached for performance when called repeatedly with the same method information.</remarks>
    /// <param name="methodInfo">The method information to generate the signature for. Can be null.</param>
    /// <returns>A string containing the method signature. Returns an empty string if <paramref name="methodInfo"/> is null.</returns>
    [Pure]
    public static string GetSignature(this System.Reflection.MethodInfo? methodInfo)
    {
        if (methodInfo is null)
            return string.Empty;

        return _signatureCache.GetValue(methodInfo, static mi => BuildSignature(mi));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string BuildSignature(System.Reflection.MethodInfo methodInfo)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();

        string access = GetAccessModifier(methodInfo); // may be ""
        string modifiers = GetModifiers(methodInfo); // may be ""

        string returnType = methodInfo.ReturnType.Name;
        string methodName = methodInfo.Name;

        // Pass 1: compute length
        int len = access.Length + modifiers.Length + returnType.Length + 1 + // space
                  methodName.Length + 1; // '('

        for (var i = 0; i < parameters.Length; i++)
        {
            if (i != 0)
                len += 2; // ", "

            ParameterInfo p = parameters[i];

            string pt = p.ParameterType.Name;
            string? pn = p.Name;

            len += pt.Length + 1; // type + space
            len += pn?.Length ?? 0; // name (can be null in rare cases)
        }

        len += 1; // ')'

        // Pass 2: create + fill
        return string.Create(len, new SignatureState(access, modifiers, returnType, methodName, parameters), static (span, state) =>
        {
            var pos = 0;

            pos = Write(span, pos, state.Access);
            pos = Write(span, pos, state.Modifiers);
            pos = Write(span, pos, state.ReturnType);
            span[pos++] = ' ';
            pos = Write(span, pos, state.MethodName);
            span[pos++] = '(';

            ParameterInfo[] ps = state.Parameters;

            for (var i = 0; i < ps.Length; i++)
            {
                if (i != 0)
                {
                    span[pos++] = ',';
                    span[pos++] = ' ';
                }

                ParameterInfo p = ps[i];

                string pt = p.ParameterType.Name;
                pos = Write(span, pos, pt);

                span[pos++] = ' ';

                string? pn = p.Name;

                if (!string.IsNullOrEmpty(pn))
                    pos = Write(span, pos, pn);
            }

            span[pos++] = ')';
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Write(Span<char> dest, int pos, string value)
    {
        value.AsSpan()
             .CopyTo(dest.Slice(pos));
        return pos + value.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetAccessModifier(System.Reflection.MethodInfo m)
    {
        if (m.IsPublic)
            return "public ";
        if (m.IsPrivate)
            return "private ";
        if (m.IsFamily)
            return "protected ";
        if (m.IsAssembly)
            return "internal ";
        if (m.IsFamilyOrAssembly)
            return "protected internal ";
        if (m.IsFamilyAndAssembly)
            return "private protected ";
        return string.Empty;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetModifiers(System.Reflection.MethodInfo m)
    {
        if (m.IsAbstract)
        {
            if (m.IsStatic)
                return "abstract static "; // extremely rare combo, but handle
            return "abstract ";
        }

        if (m.IsStatic)
            return "static ";

        if (m.IsVirtual && !m.IsFinal) // avoid printing "virtual" for sealed overrides
            return "virtual ";

        return string.Empty;
    }

    /// <summary>
    /// Returns the original member name associated with the specified method, removing special prefixes used for
    /// property and event accessors.
    /// </summary>
    /// <remarks>This method strips common prefixes such as "get_", "set_", "add_", and "remove_" from special
    /// method names generated for property and event accessors. If the method is not a special name, the original
    /// method name is returned unchanged.</remarks>
    /// <param name="methodInfo">The method information from which to extract the original member name. Can be null.</param>
    /// <returns>A string containing the original member name. Returns an empty string if <paramref name="methodInfo"/> is null.</returns>
    [Pure]
    public static string ToOriginalMemberName(this System.Reflection.MethodInfo? methodInfo)
    {
        if (methodInfo is null)
            return string.Empty;

        string name = methodInfo.Name;

        if (!methodInfo.IsSpecialName)
            return name;

        ReadOnlySpan<char> span = name.AsSpan();

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
        return (uint)underscoreIndex < (uint)(span.Length - 1) ? new string(span[(underscoreIndex + 1)..]) : name;
    }

    private readonly record struct SignatureState(string Access, string Modifiers, string ReturnType, string MethodName, ParameterInfo[] Parameters);
}