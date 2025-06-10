using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEditor.Compilation;
using UnityEngine;

namespace FairyGUIEditor
{
    public static class ScriptCompiler
    {
        [MenuItem("Tools/CompileScript")]
        public static void CompileAndExport()
        {
            InternalCompile();
        }

        [MenuItem("Tools/AdvanceCompile")]
        public static void AdvanceCompile()
        {
            string outputDir = Path.Combine(Application.dataPath, "..", "ScriptBuild", "Release");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            string dllPath = Path.Combine(outputDir, "FairyGUI.dll");

            string[] scripts = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);

            var builder = new AssemblyBuilder(dllPath, scripts);

            builder.compilerOptions = new ScriptCompilerOptions
            {
                ApiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0,
                CodeOptimization = CodeOptimization.Release,
                AllowUnsafeCode = false
            };

            builder.buildFinished += (path, messages) =>
            {
                string errorMsg = "";
                bool hasError = messages.Any(m =>
                {
                    if (m.type == CompilerMessageType.Error)
                    {
                        errorMsg = m.message;
                        hasError = true;
                        return true;
                    }
                    else
                        return false;
                });
                if (hasError)
                {
                    Error("Compilation failed " + errorMsg);
                    ExitWhenBatchMode(1);
                }
                else
                {
                    Log($"Release Dll built: {path}");
                    ExitWhenBatchMode(0);
                }
            };

            if (!builder.Build())
            {
                Error("Failed to start compilation!");
                ExitWhenBatchMode(1);
            }
        }

        private static void InternalCompile()
        {
            Log("Script compile start");
            AssetDatabase.Refresh();
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            CompilationPipeline.RequestScriptCompilation();
            Log("Script compile end");
        }

        private static void OnCompilationFinished(object _)
        {
            Log("Script compilation finished");

            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            var assemblyPaths = CompilationPipeline.GetAssemblies()
                .Where(asm => asm.name.StartsWith("FairyGUI"))
                .Select(asm => asm.outputPath);
            string outputDir = Path.Combine(Application.dataPath, "..", "Dlls");
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            if (assemblyPaths.Count() > 0)
            {
                foreach (string dllPath in assemblyPaths)
                {
                    string name = Path.GetFileName(dllPath);
                    string targetPath = Path.Combine(outputDir, name);
                    Log($"Copy dll from [{dllPath}] to [{targetPath}]");
                    if (File.Exists(targetPath))
                        File.Delete(targetPath);
                    File.Copy(dllPath, targetPath);
                    Log("Done!");
                }
                Log("All done!");
            }
            else
            {
                Warn("No assembly file matching!");
            }

            if (Application.isBatchMode)
            {
                Log("Exit Unity batchmode");
                EditorApplication.Exit(0);
            }
        }

        private static void ExitWhenBatchMode(int code)
        {
            if (Application.isBatchMode)
                EditorApplication.Exit(code);
        }

        private static void Log(string msg)
        {
            if (Application.isBatchMode)
                System.Console.WriteLine(msg);
            else
                Debug.Log(msg);
        }

        private static void Warn(string msg)
        {
            if (Application.isBatchMode)
                System.Console.WriteLine($"Warning:{msg}");
            else
                Debug.LogWarning(msg);
        }

        private static void Error(string msg)
        {
            if (Application.isBatchMode)
                System.Console.WriteLine(msg);
            else
                Debug.LogError(msg);
        }
    }
}