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

        public static IEnumerable<MemberInfo> GetMembers(string name)
            => Type.GetType(name)?.GetMembers(AnyMember) ?? Enumerable.Empty<MemberInfo>();


        const int MaxStringLength = 25;
        public static string CutString(this string s)
            => s switch
            {
                { Length: < MaxStringLength } shortEnough => shortEnough,
                _ => $"{s[0..MaxStringLength]}\n{s[MaxStringLength..].CutString()}"
            };
    }
}
