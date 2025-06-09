using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
            
            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation();

            EditorApplication.CallbackFunction delayCall = () =>
            {
                Debug.Log("Script compilation completed!");

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
                        Debug.Log($"Copy dll from [{dllPath}] to [{targetPath}]");
                        File.Copy(dllPath, targetPath);
                        Debug.Log("Done!");
                    }
                    Debug.Log("All done!");
                }
                else
                {
                    Debug.LogWarning("No assembly file matching!");
                }
            };
            EditorApplication.delayCall += delayCall;
        }
    }
}