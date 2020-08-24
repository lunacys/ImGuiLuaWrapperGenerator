using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using ImGuiNET;

namespace ImGuiWrapperGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                WriteError("ERROR: Not enough arguments. You must provide namespace name and output path separated by whitespace.\n" +
                           "Example: ImGuiWrapperGenerator MyNameSpace \"C:\\Program Files\\MyProgram\\ImGuiWrapper.cs\"");
                return;
            }
            if (args.Length > 2)
            {
                WriteError("ERROR: Too many arguments. You must provide exactly 2 arguments: namespace name and output path separated by whitespace.\n" +
                           "Example: ImGuiWrapperGenerator MyNameSpace \"C:\\Program Files\\MyProgram\\ImGuiWrapper.cs\"");
                return;
            }

            var namespaceName = args[0];
            var output = args[1];

            Console.WriteLine("Starting");

            try
            {
                Process(typeof(ImGui), namespaceName, output);
            }
            catch (NotImplementedException e)
            {
                WriteError("Method not supported: " + e.Message);
                return;
            }
            catch (Exception e)
            {
                WriteError(e.Message);
                return;
            }
        }

        static void Process(Type input, string namespaceName, string output, bool allowPointers = false)
        {
            if (allowPointers)
                throw new NotImplementedException();

            string result = GetBeginning(namespaceName);

            Console.WriteLine("Scanning type " + input.FullName);
            var allMethods = input.GetMethods();
            Console.WriteLine($"Found {allMethods.Length} methods, processing..");

            foreach (var methodInfo in allMethods)
            {
                if (methodInfo.IsPublic && methodInfo.Name != "GetType" && methodInfo.Name != "ToString" && methodInfo.Name != "Equals" && methodInfo.Name != "GetHashCode")
                {
                    if (methodInfo.ReturnType.IsPointer && !allowPointers)
                        continue;

                    if (methodInfo.ReturnType == typeof(void))
                    {
                        result += $"\t\tpublic static void {methodInfo.Name}{GetParamsForMethod(methodInfo)}";
                    }
                    else
                    {
                        result += $"\t\tpublic static {methodInfo.ReturnType.FullName} {methodInfo.Name}{GetParamsForMethod(methodInfo)}";
                    }

                    result += $" => {GetBodyForMethod(input.Name, methodInfo)};{Environment.NewLine}";
                }
            }

            result += GetEnding();

            Console.WriteLine("Done processing methods, writing output to the file " + output);
            using (var sw = new StreamWriter(output, false))
            {
                sw.WriteLine(result);
            }

            Console.WriteLine("Done");
        }

        static string GetBeginning(string namespaceName)
        {
            return $"using ImGuiNET;{Environment.NewLine}" +
                   $"{Environment.NewLine}" +
                   $"namespace {namespaceName}{Environment.NewLine}" +
                   $"{{{Environment.NewLine}" +
                   $"\t[MoonSharp.Interpreter.MoonSharpUserData]{Environment.NewLine}" +
                   $"\tpublic static class ImGuiWrapper{Environment.NewLine}" +
                   $"\t{{{Environment.NewLine}";
        }

        static string GetParamsForMethod(MethodInfo methodInfo, bool appendParamTypes = true)
        {
            string result = "(";

            var methodParams = methodInfo.GetParameters();

            for (var i = 0; i < methodParams.Length; i++)
            {
                var parameterInfo = methodParams[i];
                if (parameterInfo.ParameterType.IsByRef)
                {
                    var appendix = parameterInfo.IsOut ? "out" : "ref";
                    
                    if (appendParamTypes)
                    {
                        var fullName = parameterInfo.ParameterType.FullName;
                        result += $"{appendix} {fullName.Remove(fullName.Length - 1, 1)} {PrettifyParamName(parameterInfo.Name)}";
                    }
                    else
                    {
                        result += $"{appendix} {PrettifyParamName(parameterInfo.Name)}";
                    }
                }
                else
                {
                    var name = PrettifyParamName(parameterInfo.Name);

                    if (name == "ref")
                    {
                        name = "@ref";
                    }
                    else if (name == "in")
                    {
                        name = "@in";
                    }

                    if (appendParamTypes)
                    {
                        result += $"{parameterInfo.ParameterType.FullName} {name}";
                    }
                    else
                    {
                        result += $"{name}";
                    }
                }

                if (i == methodParams.Length - 1)
                { }
                else
                {
                    result += ", ";
                }
            }

            return result + ")";
        }

        static string PrettifyParamName(string name)
        {
            if (!name.Contains('_'))
                return name;

            var result = "";

            var words = name.Split('_');

            for (var i = 0; i < words.Length; i++)
            {
                var word = words[i];

                if (i == 0)
                {
                    result += word;
                }
                else
                {
                    result += FirstCharToUpper(word);
                }
            }

            return result;
        }

        static string GetBodyForMethod(string typeName, MethodInfo methodInfo)
        {
            string result = "";

            result += $"{typeName}.{methodInfo.Name}{GetParamsForMethod(methodInfo, false)}";
          
            return result;
        }

        static string GetEnding()
        {
            return $"\t}}{Environment.NewLine}" + "}";
        }

        public static string FirstCharToUpper(string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => input.First().ToString().ToUpper() + input.Substring(1)
            };

        static void WriteError(string msg)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(msg);
            Console.ForegroundColor = oldColor;
        }
    }
}
