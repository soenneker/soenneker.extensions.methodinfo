using System.Linq;

namespace Soenneker.Extensions.MethodInfo;

public static class MethodInfoExtension
{
    /// <summary>
    /// Decently heavy, use in logging situations etc..
    /// </summary>
    public static string GetSignature(this System.Reflection.MethodInfo? mi)
    {
        if (mi == null)
            return "";

        var result = "";

        if (mi.IsPrivate)
            result += "private";
        else if (mi.IsPublic)
            result += "public ";
        if (mi.IsAbstract)
            result += "abstract ";
        if (mi.IsStatic)
            result += "static ";
        if (mi.IsVirtual)
            result += "virtual ";

        result += mi.ReturnType.Name + " ";

        result += mi.Name + "(";

        string[] param = mi.GetParameters()
            .Select(p => $"{p.ParameterType.Name} {p.Name}")
            .ToArray();

        result += string.Join(", ", param);

        result += ")";

        return result;
    }
}