using System.Reflection;
using System.Runtime.Serialization;

public static class EnumExtensions
{
    public static string GetEnumMemberValue(this Enum value)
    {
        var member = value.GetType()
            .GetMember(value.ToString())
            .FirstOrDefault();

        var attribute = member?
            .GetCustomAttribute<EnumMemberAttribute>();

        return attribute?.Value ?? value.ToString();
    }
}
