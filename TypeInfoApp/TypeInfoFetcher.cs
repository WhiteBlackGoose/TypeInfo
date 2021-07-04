using HonkSharp.Fluency;
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
        public static string GetInfoAboutType(string typename, string indentation = "")
            => Type.GetType(typename)
                ?.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .OrderBy(
                    c => c switch
                    {
                        FieldInfo => 0,
                        MethodInfo => 1,
                        TypeInfo => 2,
                        _ => 3
                    }
                )
                .Select(c => 
                    c switch
                    {
                        FieldInfo fi => $"IsStatic: {fi.IsStatic} | IsPublic: {fi.IsPublic} | IsPrivate: {fi.IsPrivate} | Field {fi.Name} of type {fi.FieldType}",
                        MethodInfo mi =>
                            $"IsStatic: {mi.IsStatic} | IsPublic: {mi.IsPublic} | IsPrivate: {mi.IsPrivate} | " +
                            mi
                            .GetParameters()
                            .Select(c => $"{c.Name} of type {c.ParameterType}")
                            .Pipe<IEnumerable<string>, string>($"\n    {indentation}".Join)
                            .Pipe(argInfo => 
                                $"Method {mi.Name} of return type of {mi.ReturnType} and arguments \n    {argInfo}"
                            ),
                        TypeInfo ti =>
                            $"IsPublic: {ti.IsNestedPublic} | IsPrivate: {ti.IsNestedPrivate} | " +
                            $"Nested type {ti.Name}:\n" + GetInfoAboutType(ti.Name, indentation + "    "),
                        _ => "Oh no!"
                    }
                )
                .Pipe<IEnumerable<string>, string>($"\n\n{indentation}".Join);
    }
}
