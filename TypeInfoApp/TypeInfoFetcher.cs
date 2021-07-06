using HonkSharp.Fluency;
using HonkSharp.Functional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TypeInfoApp
{
    public static class TypeInfoFetcher
    {
        public static readonly BindingFlags AnyMember = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static string GetVisibilityModifier(this FieldInfo fi)
            => (fi.IsPrivate, fi.IsPublic) switch
            {
                (true, _) => "private",
                (_, true) => "public",
                _ => "internal"
            };

        public static string GetVisibilityModifier(this MethodInfo fi)
            => (fi.IsPrivate, fi.IsPublic) switch
            {
                (true, _) => "private",
                (_, true) => "public",
                _ => "internal"
            };

        public static string GetAccessorsInfo(this PropertyInfo pi)
            => (pi.GetMethod, pi.SetMethod) switch
            {
                (null, null) => "-",
                (not null, null) => "get",
                (null, not null) => "set",
                (not null, not null) => "get/set"
            };

        public static string GetAllInfo<T>(T fi)
            => typeof(T)
                .GetProperties(AnyMember)
                .Select(p => 
                    p.Name 
                    + ": "
                    + p.Dangerous()
                        .Try<Exception, string>(p => p.GetValue(fi)?.ToString() ?? "")
                        .Switch(
                            valid => valid,
                            failure => $"Exception ocurred: {failure.Reason.Message}"
                        )
                )
                .Pipe<IEnumerable<string>, string>("\n".Join);

        private static readonly Type[] allTypes =
            AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(c => c.GetTypes())
            .SelectMany(c => c)
            .ToArray();

        public static IEnumerable<Type> GetAllTypesRoughly(string name)
            => Type.GetType(name) is { } foundUnique
                ? new [] { foundUnique }
                : allTypes.Where(t => t.AssemblyQualifiedName.Contains(name, StringComparison.OrdinalIgnoreCase));

        public static IEnumerable<MemberInfo>? GetMembers(string name)
            => Type.GetType(name)?.GetMembers(AnyMember);

        public static bool IsSingleElement<T>(this IEnumerable<T> collection, out T res)
        {
            var enumerator = collection.GetEnumerator();
            if (enumerator.MoveNext())
            {
                res = enumerator.Current;
                return enumerator.MoveNext() == false; // it should be false after the first iter
            }
            res = default!;
            return false;
        }

        const int MaxStringLength = 25;
        public static string CutString(this string s, int maxLength = MaxStringLength)
        {
            if (s.Length <= maxLength)
                return s;
            if (s.IndexOf('.') is var dotIndex and not -1)
                return CutString(s[..dotIndex], maxLength) + "\n." + CutString(s[(dotIndex + 1)..], maxLength);
            if (s.IndexOf('[') is var bracketIndex and not -1)
                return CutString(s[..bracketIndex], maxLength) + "\n[" + CutString(s[(bracketIndex + 1)..], maxLength);
            return s[..maxLength] + "\n" + CutString(s[..maxLength], maxLength);
        }
    }
}
