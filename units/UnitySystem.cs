using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

using VM;

using static VM.VirtualMachine;

namespace Units;

public class UnitySystem
{
    public static readonly Dictionary<string, ExternalFunctionHandler> FUNCTIONS;

    private static readonly int COPY_MEMORY_PARAM_SIZE = 2 * POINTER_SIZE + sizeof(int);
    private static readonly int STRING_LENGTH_PARAM_SIZE = POINTER_SIZE + sizeof(int);
    private static readonly int COPY_STRING_PARAM_SIZE = 2 * POINTER_SIZE;
    private static readonly int CONCATENATE_STRING_PARAM_SIZE = 3 * POINTER_SIZE;
    private static readonly int STRING_TO_INT_PARAM_SIZE = sizeof(int) + 2 * POINTER_SIZE;
    private static readonly int INT_TO_STRING_PARAM_SIZE = sizeof(int) + POINTER_SIZE;
    private static readonly int LONG_TO_STRING_PARAM_SIZE = sizeof(long) + POINTER_SIZE;
    private static readonly int ALLOC_PARAM_SIZE = sizeof(int) + POINTER_SIZE;
    private static readonly int FREE_PARAM_SIZE = POINTER_SIZE;
    private static readonly int NEW_STRING_PARAM_SIZE = OBJECT_SIZE + POINTER_SIZE;
    private static readonly int NEW_STRING2_PARAM_SIZE = 2 * POINTER_SIZE;
    private static readonly int COPY_STRING2_PARAM_SIZE = OBJECT_SIZE + POINTER_SIZE;
    private static readonly int STRING_LENGTH2_PARAM_SIZE = sizeof(int) + OBJECT_SIZE;
    private static readonly int CONCATENATE_STRING2_PARAM_SIZE = 3 * OBJECT_SIZE;
    private static readonly int CONCATENATE_STRING3_PARAM_SIZE = POINTER_SIZE + 2 * OBJECT_SIZE;
    private static readonly int STRING_STORE_PARAM_SIZE = POINTER_SIZE + OBJECT_SIZE;
    private static readonly int STRING_ADDREF_PARAM_SIZE = OBJECT_SIZE;
    private static readonly int STRING_RELEASE_PARAM_SIZE = POINTER_SIZE + sizeof(int);
    private static readonly int STRING_ARRAY_RELEASE_PARAM_SIZE = POINTER_SIZE + 2 * sizeof(int);

    public static readonly int POINTER_COUNT = POINTER_SIZE / sizeof(int);
    public static readonly int STRING_COUNT = POINTER_COUNT;

    public static void CopyMemory(VirtualMachine vm)
    {
        IntPtr src = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, 0);
        IntPtr dst = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, POINTER_COUNT);
        int len = vm.LoadParam(COPY_MEMORY_PARAM_SIZE, 2 * POINTER_COUNT);

        MovePointerBlock(src, dst, len);
    }

    public static void StringLength(VirtualMachine vm)
    {
        IntPtr str = vm.LoadParamPtr(STRING_LENGTH_PARAM_SIZE, 1);

        string s = ReadPointerString(str);

        vm.SetParam(STRING_LENGTH_PARAM_SIZE, 0, s.Length);
    }

    public static void CopyString(VirtualMachine vm)
    {
        IntPtr src = vm.LoadParamPtr(COPY_STRING_PARAM_SIZE, 0);
        IntPtr dst = vm.LoadParamPtr(COPY_STRING_PARAM_SIZE, POINTER_COUNT);

        string s = ReadPointerString(src);

        WritePointer(dst, s);
    }

    public static void ConcatenateStrings(VirtualMachine vm)
    {
        IntPtr dst = vm.LoadParamPtr(CONCATENATE_STRING_PARAM_SIZE, 0);
        IntPtr src1 = vm.LoadParamPtr(CONCATENATE_STRING_PARAM_SIZE, POINTER_COUNT);
        IntPtr src2 = vm.LoadParamPtr(CONCATENATE_STRING_PARAM_SIZE, 2 * POINTER_COUNT);

        string s1 = ReadPointerString(src1);
        string s2 = ReadPointerString(src2);

        WritePointer(dst, s1 + s2);
    }

    public static void CompareStyrings(VirtualMachine vm)
    {
        IntPtr str1 = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, 1);
        IntPtr str2 = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, 1 + POINTER_COUNT);

        string s1 = ReadPointerString(str1);
        string s2 = ReadPointerString(str2);

        vm.SetParam(COPY_MEMORY_PARAM_SIZE, 0, s1 == s2 ? 1 : 0);
    }

    public static void StringToInt(VirtualMachine vm)
    {
        IntPtr src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
        IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

        string s = ReadPointerString(src);

        try
        {
            int value = int.Parse(s, CultureInfo.InvariantCulture);
            vm.WritePointer(dst, value);
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
        }
        catch (Exception)
        {
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
        }
    }

    public static void StringToLong(VirtualMachine vm)
    {
        IntPtr src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
        IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

        string s = ReadPointerString(src);

        try
        {
            long value = long.Parse(s, CultureInfo.InvariantCulture);
            vm.WritePointer(dst, value);
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
        }
        catch (Exception)
        {
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
        }
    }

    public static void StringToFloat(VirtualMachine vm)
    {
        IntPtr src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
        IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

        string s = ReadPointerString(src);

        try
        {
            float value = float.Parse(s, CultureInfo.InvariantCulture);
            vm.WritePointer(dst, value);
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
        }
        catch (Exception)
        {
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
        }
    }

    public static void StringToDouble(VirtualMachine vm)
    {
        IntPtr src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
        IntPtr dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

        string s = ReadPointerString(src);

        try
        {
            double value = double.Parse(s, CultureInfo.InvariantCulture);
            WritePointer(dst, value);
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
        }
        catch (Exception)
        {
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
        }
    }

    public static void IntToString(VirtualMachine vm)
    {
        int src = vm.LoadParam(INT_TO_STRING_PARAM_SIZE, 0);
        IntPtr dst = vm.LoadParamPtr(INT_TO_STRING_PARAM_SIZE, 1);

        string s = src.ToString(CultureInfo.InvariantCulture);
        WritePointer(dst, s);
    }

    public static void LongToString(VirtualMachine vm)
    {
        long src = vm.LoadParamLong(LONG_TO_STRING_PARAM_SIZE, 0);
        IntPtr dst = vm.LoadParamPtr(LONG_TO_STRING_PARAM_SIZE, 2);

        string s = src.ToString(CultureInfo.InvariantCulture);
        WritePointer(dst, s);
    }

    public static void FloatToString(VirtualMachine vm)
    {
        float src = vm.LoadParamFloat(INT_TO_STRING_PARAM_SIZE, 0);
        IntPtr dst = vm.LoadParamPtr(INT_TO_STRING_PARAM_SIZE, 1);

        string s = src.ToString(CultureInfo.InvariantCulture);
        WritePointer(dst, s);
    }

    public static void DoubleToString(VirtualMachine vm)
    {
        double src = vm.LoadParamDouble(LONG_TO_STRING_PARAM_SIZE, 0);
        IntPtr dst = vm.LoadParamPtr(LONG_TO_STRING_PARAM_SIZE, 2);

        string s = src.ToString(CultureInfo.InvariantCulture);
        WritePointer(dst, s);
    }

    public static void Alloc(VirtualMachine vm)
    {
        int len = vm.LoadParam(ALLOC_PARAM_SIZE, POINTER_COUNT);

        try
        {
            IntPtr result = Marshal.AllocHGlobal(len);
            vm.SetParam(ALLOC_PARAM_SIZE, 0, result);
        }
        catch (OutOfMemoryException)
        {
            vm.SetParam(ALLOC_PARAM_SIZE, 0, IntPtr.Zero);
        }
    }

    public static void Free(VirtualMachine vm)
    {
        IntPtr ptr = vm.LoadParamPtr(FREE_PARAM_SIZE, 0);
        Marshal.FreeHGlobal(ptr);
    }

    public static void NewString(VirtualMachine vm)
    {
        IntPtr str = vm.LoadParamPtr(NEW_STRING_PARAM_SIZE, POINTER_COUNT);
        string s = ReadPointerString(str);

        IntPtr result = vm.NewString(s);

        vm.SetParam(NEW_STRING_PARAM_SIZE, 0, result);
    }

    public static void NewString2(VirtualMachine vm)
    {
        IntPtr str = vm.LoadParamPtr(NEW_STRING2_PARAM_SIZE, POINTER_COUNT);
        string s = ReadPointerString(str);

        IntPtr result = vm.NewString(s);

        IntPtr dstAddr = vm.LoadParamPtr(NEW_STRING2_PARAM_SIZE, 0);

        IntPtr dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void CopyString2(VirtualMachine vm)
    {
        IntPtr src = vm.LoadParamPtr(COPY_STRING2_PARAM_SIZE, 0);
        string s = src != IntPtr.Zero ? ReadPointerString(src) : "";

        IntPtr dst = vm.SetStringLength(src, s.Length);
        vm.SetParam(COPY_STRING2_PARAM_SIZE, STRING_COUNT, dst);
    }

    public static void StringLength2(VirtualMachine vm)
    {
        IntPtr src = vm.LoadParamPtr(STRING_LENGTH2_PARAM_SIZE, 1);
        int len = VirtualMachine.StringLength(src);
        vm.SetParam(STRING_LENGTH2_PARAM_SIZE, 0, len);
    }

    public static void ConcatenateStrings2(VirtualMachine vm)
    {
        IntPtr str1 = vm.LoadParamPtr(CONCATENATE_STRING2_PARAM_SIZE, STRING_COUNT);
        IntPtr str2 = vm.LoadParamPtr(CONCATENATE_STRING2_PARAM_SIZE, 2 * STRING_COUNT);

        string s1 = str1 != IntPtr.Zero ? ReadPointerString(str1) : "";
        string s2 = str2 != IntPtr.Zero ? ReadPointerString(str2) : "";
        string s = s1 + s2;

        IntPtr result = vm.NewString(s);

        vm.SetParam(CONCATENATE_STRING2_PARAM_SIZE, 0, result);
    }

    public static void ConcatenateStrings3(VirtualMachine vm)
    {
        IntPtr str1 = vm.LoadParamPtr(CONCATENATE_STRING3_PARAM_SIZE, POINTER_COUNT);
        IntPtr str2 = vm.LoadParamPtr(CONCATENATE_STRING3_PARAM_SIZE, POINTER_COUNT + STRING_COUNT);

        string s1 = str1 != IntPtr.Zero ? ReadPointerString(str1) : "";
        string s2 = str2 != IntPtr.Zero ? ReadPointerString(str2) : "";
        string s = s1 + s2;

        IntPtr result = vm.NewString(s);

        IntPtr dstAddr = vm.LoadParamPtr(CONCATENATE_STRING3_PARAM_SIZE, 0);

        IntPtr dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void StringStore(VirtualMachine vm)
    {
        IntPtr dstAddr = vm.LoadParamPtr(STRING_STORE_PARAM_SIZE, 0);
        IntPtr dst = ReadPointerPtr(dstAddr);
        IntPtr src = vm.LoadParamPtr(STRING_STORE_PARAM_SIZE, POINTER_COUNT);

        VirtualMachine.ObjectAddRef(src);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, src);
    }

    public static void StringAddRef(VirtualMachine vm)
    {
        IntPtr str = vm.LoadParamPtr(STRING_ADDREF_PARAM_SIZE, 0);
        VirtualMachine.ObjectAddRef(str);
    }

    public static void StringRelease(VirtualMachine vm)
    {
        IntPtr strAddr = vm.LoadParamPtr(STRING_RELEASE_PARAM_SIZE, 0);
        bool setNull = vm.LoadParam(STRING_RELEASE_PARAM_SIZE, POINTER_COUNT) != 0;
        IntPtr str = ReadPointerPtr(strAddr);
        str = vm.ObjectRelease(str);
        WritePointer(strAddr, setNull ? IntPtr.Zero : str);
    }

    public static void StringArrayRelease(VirtualMachine vm)
    {
        IntPtr ptr = vm.LoadParamPtr(STRING_ARRAY_RELEASE_PARAM_SIZE, 0);
        int count = vm.LoadParam(STRING_ARRAY_RELEASE_PARAM_SIZE, POINTER_COUNT);
        bool setNull = vm.LoadParam(STRING_RELEASE_PARAM_SIZE, POINTER_COUNT + 1) != 0;
        vm.ObjectArrayRelease(ptr, count, setNull);
    }

    static UnitySystem()
    {
        FUNCTIONS = new Dictionary<string, ExternalFunctionHandler>
        {
            { "CopiaMemória", CopyMemory },
            { "ComprimentoString", StringLength },
            { "CopiaString", CopyString },
            { "ConcatenaStrings", ConcatenateStrings },
            { "CompareStrings", CompareStyrings },
            { "StringParaInt", StringToInt },
            { "StringParaLong", StringToLong },
            { "StringParaFloat", StringToFloat },
            { "StringParaReal", StringToDouble },
            { "IntParaString", IntToString },
            { "LongParaString", LongToString },
            { "FloatParaString", FloatToString },
            { "RealParaString", DoubleToString },
            { "AlocarMemória", Alloc },
            { "DesalocarMemória", Free },
            { "NovoTexto", NewString },
            { "NovoTexto2", NewString2 },
            { "CopiaTexto", CopyString2 },
            { "ComprimentoTexto", StringLength2 },
            { "ConcatenaTextos", ConcatenateStrings2 },
            { "ConcatenaTextos2", ConcatenateStrings3 },
            { "AtribuiTexto", StringStore },
            { "IncrementaReferenciaTexto", StringAddRef },
            { "DecrementaReferenciaTexto", StringRelease },
            { "DecrementaReferenciaArrayTexto", StringArrayRelease }
        };
    }
}
