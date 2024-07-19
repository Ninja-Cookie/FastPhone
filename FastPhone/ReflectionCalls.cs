using System.Reflection;

namespace FastPhone
{
    internal static class ReflectionCalls
    {
        public static BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;

        public static void InvokeMethod(object reference, string method, params object[] parms)
        {
            reference.GetType().GetMethod(method, flags).Invoke(reference, parms);
        }

        public static void SetFieldValue<T>(object reference, string field, T value)
        {
            reference.GetType().GetField(field, flags).SetValue(reference, value);
        }

        public static T GetFieldValue<T>(object reference, string field)
        {
            return (T)reference.GetType().GetField(field, flags).GetValue(reference);
        }
    }
}
