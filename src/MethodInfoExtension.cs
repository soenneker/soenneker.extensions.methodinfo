using System;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text;

namespace Soenneker.Extensions.MethodInfo;

/// <summary>
/// A collection of useful MethodInfo methods
/// </summary>
public static class MethodInfoExtension
{
    /// <summary>
    /// Generates a string representation of the method's signature, including its access modifiers,
    /// modifiers (abstract, static, virtual), return type, method name, and parameters.
    /// </summary>
    /// <param name="methodInfo">
    /// The <see cref="System.Reflection.MethodInfo"/> instance representing the method to generate the signature for.
    /// </param>
    /// <returns>
    /// A string that represents the signature of the specified method. If <paramref name="methodInfo"/> is <c>null</c>,
    /// an empty string is returned.
    /// </returns>
    [Pure]
    public static string GetSignature(this System.Reflection.MethodInfo? methodInfo)
    {
        if (methodInfo is null)
            return "";

        // Use StringBuilder with an estimated capacity to minimize resizing
        var sb = new StringBuilder(128);

        // Append access modifier
        if (methodInfo.IsPrivate)
            sb.Append("private ");
        else if (methodInfo.IsPublic)
            sb.Append("public ");

        // Append method modifiers
        if (methodInfo.IsAbstract)
            sb.Append("abstract ");

        if (methodInfo.IsStatic)
            sb.Append("static ");

        if (methodInfo is { IsVirtual: true, IsAbstract: false })
            sb.Append("virtual ");

        // Append return type and method name
        sb.Append(methodInfo.ReturnType.Name).Append(' ').Append(methodInfo.Name).Append('(');

        // Append parameters (avoiding unnecessary allocations)
        ParameterInfo[] parameters = methodInfo.GetParameters();

        if (parameters.Length > 0)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                ParameterInfo param = parameters[i];
                sb.Append(param.ParameterType.Name).Append(' ').Append(param.Name).Append(", ");
            }

            sb.Length -= 2; // Remove the last ", "
        }

        sb.Append(')');
        return sb.ToString();
    }

    /// <summary>
    /// Converts the <see cref="MethodInfo"/> instance's name to its original member name,
    /// removing prefixes like "get_" or "set_" if the method represents a property accessor.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> instance representing the method to extract the original name from.</param>
    /// <returns>
    /// A <see cref="string"/> containing the original member name, with any accessor prefixes removed.
    /// </returns>
    [Pure]
    public static string ToOriginalMemberName(this System.Reflection.MethodInfo methodInfo)
    {
        if (!methodInfo.IsSpecialName)
            return methodInfo.Name;

        ReadOnlySpan<char> methodSpan = methodInfo.Name.AsSpan();
        int underscoreIndex = methodSpan.IndexOf('_');

        // If the underscore is valid, slice the span without allocating a new string
        return underscoreIndex >= 0 && underscoreIndex < methodSpan.Length - 1 ? new string(methodSpan.Slice(underscoreIndex + 1)) : methodInfo.Name;
    }
}