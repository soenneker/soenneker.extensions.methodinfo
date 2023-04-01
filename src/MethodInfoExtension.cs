using System.Linq;

namespace Soenneker.Extensions.MethodInfo;

public static class MethodInfoExtension
{
    /// <summary>
    /// Decently heavy, use in logging situations etc.. TODO: StringBuilder?
    /// </summary>
    public static string GetSignature(this System.Reflection.MethodInfo? methodInfo)
    {
        if (methodInfo == null)
            return "";

        var result = "";

        if (methodInfo.IsPrivate)
            result += "private";
        else if (methodInfo.IsPublic)
            result += "public ";

        if (methodInfo.IsAbstract)
            result += "abstract ";

        if (methodInfo.IsStatic)
            result += "static ";

        if (methodInfo.IsVirtual)
            result += "virtual ";

        result += methodInfo.ReturnType.Name + " ";

        result += methodInfo.Name + "(";

        string[] param = methodInfo.GetParameters()
            .Select(p => $"{p.ParameterType.Name} {p.Name}")
            .ToArray();

        result += string.Join(", ", param);

        result += ')';

        return result;
    }
}