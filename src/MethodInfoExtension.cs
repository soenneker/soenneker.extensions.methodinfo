using System;
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
    public static string GetSignature(this System.Reflection.MethodInfo? methodInfo)
    {
        if (methodInfo == null)
            return "";

        var sb = new StringBuilder();

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
        if (methodInfo.IsVirtual && !methodInfo.IsAbstract)
            sb.Append("virtual ");

        // Append return type and method name
        sb.Append(methodInfo.ReturnType.Name);
        sb.Append(' ');
        sb.Append(methodInfo.Name);
        sb.Append('(');

        // Append parameters
        ParameterInfo[] parameters = methodInfo.GetParameters();

        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterInfo param = parameters[i];
            sb.Append(param.ParameterType.Name);
            sb.Append(' ');
            sb.Append(param.Name);

            if (i < parameters.Length - 1)
                sb.Append(", ");
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
    public static string ToOriginalMemberName(this System.Reflection.MethodInfo methodInfo)
    {
        string methodName = methodInfo.Name;

        // Check if it's a property getter or setter and remove "get_" or "set_"
        if (methodInfo.IsSpecialName)
        {
            ReadOnlySpan<char> methodSpan = methodName.AsSpan();
            int underscoreIndex = methodSpan.IndexOf('_');

            if (underscoreIndex >= 0 && underscoreIndex < methodSpan.Length - 1)
            {
                methodName = methodSpan.Slice(underscoreIndex + 1).ToString();
            }
        }

        return methodName;
    }
}