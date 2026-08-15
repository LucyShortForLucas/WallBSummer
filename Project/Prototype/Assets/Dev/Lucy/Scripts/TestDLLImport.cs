using System.Runtime.InteropServices;
using UnityEngine;

public class TestDLLImport 
{
    private const string DllName = "NativePlugin_unityExport"; // no .dll extension, no lib prefix

    [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
    public static extern int add(int a, int b);
}
