using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace FairyGUIEditor
{
    public static class ScriptCompiler
    {
        [MenuItem("Tools/CompileScrip")]
        public static void CompileAndExport()
        {

            Log("CompileAndExport start");
            AssetDatabase.Refresh();
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            CompilationPipeline.RequestScriptCompilation();
            if (EditorApplication.isCompiling)
            {
                Task.Delay(2000).Wait();
            }
            Log("CompileAndExport end");
        }

        private static void OnCompilationFinished(object obj)
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
                EditorApplication.Exit(0);
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

        private static void Error(Exception exception)
        {
            if (Application.isBatchMode)
                System.Console.Write($"Error:{exception.Message}[{exception.ToString()}]");
            else
                Debug.LogError($"Error:{exception.Message}[{exception.ToString()}]");
        }
    }
}