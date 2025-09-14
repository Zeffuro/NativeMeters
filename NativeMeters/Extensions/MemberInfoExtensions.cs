using System.Reflection;

namespace NativeMeters.Extensions;

public static class MemberInfoExtensions {
    public static T? GetValue<T>(this MemberInfo memberInfo, object forObject) {
        return memberInfo.MemberType switch {
            MemberTypes.Field => (T?)((FieldInfo)memberInfo).GetValue(forObject),
            MemberTypes.Property => (T?)((PropertyInfo)memberInfo).GetValue(forObject),
            _ => default,
        };
    }

    public static void SetValue<T>(this MemberInfo memberInfo, object forObject, T value) {
        switch (memberInfo.MemberType) {
            case MemberTypes.Field:
                ((FieldInfo)memberInfo).SetValue(forObject, value);
                break;
            case MemberTypes.Property:
                ((PropertyInfo)memberInfo).SetValue(forObject, value);
                break;
        }
    }
}
