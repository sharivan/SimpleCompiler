using SimpleCompiler.VM;
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
    private static readonly int BOOL_TO_STRING_PARAM_SIZE = POINTER_SIZE + sizeof(int);
    private static readonly int CHAR_TO_STRING_PARAM_SIZE = POINTER_SIZE + sizeof(int);
    private static readonly int INT_TO_STRING2_PARAM_SIZE = POINTER_SIZE + sizeof(int);
    private static readonly int LONG_TO_STRING2_PARAM_SIZE = POINTER_SIZE + sizeof(long);
    private static readonly int FLOAT_TO_STRING2_PARAM_SIZE = POINTER_SIZE + sizeof(float);
    private static readonly int DOUBLE_TO_STRING2_PARAM_SIZE = POINTER_SIZE + sizeof(double);
    private static readonly int STRING_TO_BOOL_PARAM_SIZE = sizeof(int) + POINTER_SIZE;
    private static readonly int STRING_TO_CHAR_PARAM_SIZE = sizeof(int) + POINTER_SIZE;
    private static readonly int STRING_TO_INT2_PARAM_SIZE = sizeof(int) + POINTER_SIZE;
    private static readonly int STRING_TO_LONG2_PARAM_SIZE = sizeof(long) + POINTER_SIZE;
    private static readonly int STRING_TO_FLOAT2_PARAM_SIZE = sizeof(float) + POINTER_SIZE;
    private static readonly int STRING_TO_DOUBLE2_PARAM_SIZE = sizeof(double) + POINTER_SIZE;
    private static readonly int LAST_ERROR_PARAM_SIZE = sizeof(int);

    public static readonly int POINTER_COUNT = POINTER_SIZE / sizeof(int);
    public static readonly int STRING_COUNT = POINTER_COUNT;

    public static void CopyMemory(VirtualMachine vm)
    {
        var src = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, 0);
        var dst = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, POINTER_COUNT);
        int len = vm.LoadParam(COPY_MEMORY_PARAM_SIZE, 2 * POINTER_COUNT);

        MovePointerBlock(src, dst, len);
    }

    public static void StringLength(VirtualMachine vm)
    {
        var str = vm.LoadParamPtr(STRING_LENGTH_PARAM_SIZE, 1);

        string s = ReadPointerString(str);

        vm.SetParam(STRING_LENGTH_PARAM_SIZE, 0, s.Length);
    }

    public static void CopyString(VirtualMachine vm)
    {
        var src = vm.LoadParamPtr(COPY_STRING_PARAM_SIZE, 0);
        var dst = vm.LoadParamPtr(COPY_STRING_PARAM_SIZE, POINTER_COUNT);

        string s = ReadPointerString(src);

        WritePointer(dst, s);
    }

    public static void ConcatenateStrings(VirtualMachine vm)
    {
        var dst = vm.LoadParamPtr(CONCATENATE_STRING_PARAM_SIZE, 0);
        var src1 = vm.LoadParamPtr(CONCATENATE_STRING_PARAM_SIZE, POINTER_COUNT);
        var src2 = vm.LoadParamPtr(CONCATENATE_STRING_PARAM_SIZE, 2 * POINTER_COUNT);

        string s1 = ReadPointerString(src1);
        string s2 = ReadPointerString(src2);

        WritePointer(dst, s1 + s2);
    }

    public static void CompareStyrings(VirtualMachine vm)
    {
        var str1 = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, 1);
        var str2 = vm.LoadParamPtr(COPY_MEMORY_PARAM_SIZE, 1 + POINTER_COUNT);

        string s1 = ReadPointerString(str1);
        string s2 = ReadPointerString(str2);

        vm.SetParam(COPY_MEMORY_PARAM_SIZE, 0, s1 == s2 ? 1 : 0);
    }

    public static void StringToInt(VirtualMachine vm)
    {
        var src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
        var dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

        string s = ReadPointerString(src);

        try
        {
            vm.LastError = Error.NONE;
            int value = int.Parse(s, CultureInfo.InvariantCulture);
            vm.WritePointer(dst, value);
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
        }
        catch (Exception)
        {
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
            vm.LastError = Error.INVALID_CONVERSION;
        }
    }

    public static void StringToLong(VirtualMachine vm)
    {
        var src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
        var dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

        string s = ReadPointerString(src);

        try
        {
            vm.LastError = Error.NONE;
            long value = long.Parse(s, CultureInfo.InvariantCulture);
            vm.WritePointer(dst, value);
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
        }
        catch (Exception)
        {
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
            vm.LastError = Error.INVALID_CONVERSION;
        }
    }

    public static void StringToFloat(VirtualMachine vm)
    {
        var src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
        var dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

        string s = ReadPointerString(src);

        try
        {
            vm.LastError = Error.NONE;
            float value = float.Parse(s, CultureInfo.InvariantCulture);
            vm.WritePointer(dst, value);
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
        }
        catch (Exception)
        {
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
            vm.LastError = Error.INVALID_CONVERSION;
        }
    }

    public static void StringToDouble(VirtualMachine vm)
    {
        var src = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1);
        var dst = vm.LoadParamPtr(STRING_TO_INT_PARAM_SIZE, 1 + POINTER_COUNT);

        string s = ReadPointerString(src);

        try
        {
            vm.LastError = Error.NONE;
            double value = double.Parse(s, CultureInfo.InvariantCulture);
            WritePointer(dst, value);
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 1);
        }
        catch (Exception)
        {
            vm.SetParam(STRING_TO_INT_PARAM_SIZE, 0, 0);
            vm.LastError = Error.INVALID_CONVERSION;
        }
    }

    public static void IntToString(VirtualMachine vm)
    {
        int src = vm.LoadParam(INT_TO_STRING_PARAM_SIZE, 0);
        var dst = vm.LoadParamPtr(INT_TO_STRING_PARAM_SIZE, 1);

        string s = src.ToString(CultureInfo.InvariantCulture);
        WritePointer(dst, s);
    }

    public static void LongToString(VirtualMachine vm)
    {
        long src = vm.LoadParamLong(LONG_TO_STRING_PARAM_SIZE, 0);
        var dst = vm.LoadParamPtr(LONG_TO_STRING_PARAM_SIZE, 2);

        string s = src.ToString(CultureInfo.InvariantCulture);
        WritePointer(dst, s);
    }

    public static void FloatToString(VirtualMachine vm)
    {
        float src = vm.LoadParamFloat(INT_TO_STRING_PARAM_SIZE, 0);
        var dst = vm.LoadParamPtr(INT_TO_STRING_PARAM_SIZE, 1);

        string s = src.ToString(CultureInfo.InvariantCulture);
        WritePointer(dst, s);
    }

    public static void DoubleToString(VirtualMachine vm)
    {
        double src = vm.LoadParamDouble(LONG_TO_STRING_PARAM_SIZE, 0);
        var dst = vm.LoadParamPtr(LONG_TO_STRING_PARAM_SIZE, 2);

        string s = src.ToString(CultureInfo.InvariantCulture);
        WritePointer(dst, s);
    }

    public static void Alloc(VirtualMachine vm)
    {
        int len = vm.LoadParam(ALLOC_PARAM_SIZE, POINTER_COUNT);

        try
        {
            vm.LastError = Error.NONE;
            var result = Marshal.AllocHGlobal(len);
            vm.SetParam(ALLOC_PARAM_SIZE, 0, result);
        }
        catch (OutOfMemoryException)
        {
            vm.SetParam(ALLOC_PARAM_SIZE, 0, IntPtr.Zero);
            vm.LastError = Error.OUT_OF_MEMORY;
        }
    }

    public static void Free(VirtualMachine vm)
    {
        var ptr = vm.LoadParamPtr(FREE_PARAM_SIZE, 0);
        Marshal.FreeHGlobal(ptr);
    }

    public static void NewString(VirtualMachine vm)
    {
        var str = vm.LoadParamPtr(NEW_STRING_PARAM_SIZE, POINTER_COUNT);
        string s = ReadPointerString(str);

        var result = vm.NewString(s);

        vm.SetParam(NEW_STRING_PARAM_SIZE, 0, result);
    }

    public static void NewString2(VirtualMachine vm)
    {
        var str = vm.LoadParamPtr(NEW_STRING2_PARAM_SIZE, POINTER_COUNT);
        string s = ReadPointerString(str);

        var result = vm.NewString(s);

        var dstAddr = vm.LoadParamPtr(NEW_STRING2_PARAM_SIZE, 0);

        var dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void CopyString2(VirtualMachine vm)
    {
        var src = vm.LoadParamPtr(COPY_STRING2_PARAM_SIZE, 0);
        string s = src != IntPtr.Zero ? ReadPointerString(src) : "";

        var dst = vm.SetStringLength(src, s.Length);
        vm.SetParam(COPY_STRING2_PARAM_SIZE, STRING_COUNT, dst);
    }

    public static void StringLength2(VirtualMachine vm)
    {
        var src = vm.LoadParamPtr(STRING_LENGTH2_PARAM_SIZE, 1);
        int len = VirtualMachine.StringLength(src);
        vm.SetParam(STRING_LENGTH2_PARAM_SIZE, 0, len);
    }

    public static void ConcatenateStrings2(VirtualMachine vm)
    {
        var str1 = vm.LoadParamPtr(CONCATENATE_STRING2_PARAM_SIZE, STRING_COUNT);
        var str2 = vm.LoadParamPtr(CONCATENATE_STRING2_PARAM_SIZE, 2 * STRING_COUNT);

        string s1 = str1 != IntPtr.Zero ? ReadPointerString(str1) : "";
        string s2 = str2 != IntPtr.Zero ? ReadPointerString(str2) : "";
        string s = s1 + s2;

        var result = vm.NewString(s);

        vm.SetParam(CONCATENATE_STRING2_PARAM_SIZE, 0, result);
    }

    public static void ConcatenateStrings3(VirtualMachine vm)
    {
        var str1 = vm.LoadParamPtr(CONCATENATE_STRING3_PARAM_SIZE, POINTER_COUNT);
        var str2 = vm.LoadParamPtr(CONCATENATE_STRING3_PARAM_SIZE, POINTER_COUNT + STRING_COUNT);

        string s1 = str1 != IntPtr.Zero ? ReadPointerString(str1) : "";
        string s2 = str2 != IntPtr.Zero ? ReadPointerString(str2) : "";
        string s = s1 + s2;

        var result = vm.NewString(s);

        var dstAddr = vm.LoadParamPtr(CONCATENATE_STRING3_PARAM_SIZE, 0);

        var dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void StringStore(VirtualMachine vm)
    {
        var dstAddr = vm.LoadParamPtr(STRING_STORE_PARAM_SIZE, 0);
        var dst = ReadPointerPtr(dstAddr);
        var src = vm.LoadParamPtr(STRING_STORE_PARAM_SIZE, POINTER_COUNT);

        VirtualMachine.ObjectAddRef(src);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, src);
    }

    public static void StringAddRef(VirtualMachine vm)
    {
        var str = vm.LoadParamPtr(STRING_ADDREF_PARAM_SIZE, 0);
        ObjectAddRef(str);
    }

    public static void StringRelease(VirtualMachine vm)
    {
        var strAddr = vm.LoadParamPtr(STRING_RELEASE_PARAM_SIZE, 0);
        bool setNull = vm.LoadParam(STRING_RELEASE_PARAM_SIZE, POINTER_COUNT) != 0;
        var str = ReadPointerPtr(strAddr);
        str = vm.ObjectRelease(str);
        WritePointer(strAddr, setNull ? IntPtr.Zero : str);
    }

    public static void StringArrayRelease(VirtualMachine vm)
    {
        var ptr = vm.LoadParamPtr(STRING_ARRAY_RELEASE_PARAM_SIZE, 0);
        int count = vm.LoadParam(STRING_ARRAY_RELEASE_PARAM_SIZE, POINTER_COUNT);
        bool setNull = vm.LoadParam(STRING_RELEASE_PARAM_SIZE, POINTER_COUNT + 1) != 0;
        vm.ObjectArrayRelease(ptr, count, setNull);
    }

    public static void BoolToString(VirtualMachine vm)
    {
        bool flag = vm.LoadParam(BOOL_TO_STRING_PARAM_SIZE, 1) != 0;
        string s = flag ? "verdade" : "falso";
        var result = vm.NewString(s);

        var dstAddr = vm.LoadParamPtr(BOOL_TO_STRING_PARAM_SIZE, 0);

        var dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void CharToString(VirtualMachine vm)
    {
        char c = (char) vm.LoadParam(CHAR_TO_STRING_PARAM_SIZE, 1);
        string s = new(c, 1);
        var result = vm.NewString(s);

        var dstAddr = vm.LoadParamPtr(CHAR_TO_STRING_PARAM_SIZE, 0);

        var dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void IntToString2(VirtualMachine vm)
    {
        int num = vm.LoadParam(INT_TO_STRING2_PARAM_SIZE, 1);
        string s = num.ToString(CultureInfo.InvariantCulture);
        var result = vm.NewString(s);

        var dstAddr = vm.LoadParamPtr(INT_TO_STRING2_PARAM_SIZE, 0);

        var dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void LongToString2(VirtualMachine vm)
    {
        long num = vm.LoadParamLong(LONG_TO_STRING2_PARAM_SIZE, 1);
        string s = num.ToString(CultureInfo.InvariantCulture);
        var result = vm.NewString(s);

        var dstAddr = vm.LoadParamPtr(LONG_TO_STRING2_PARAM_SIZE, 0);

        var dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void FloatToString2(VirtualMachine vm)
    {
        float num = vm.LoadParamFloat(FLOAT_TO_STRING2_PARAM_SIZE, 1);
        string s = num.ToString(CultureInfo.InvariantCulture);
        var result = vm.NewString(s);

        var dstAddr = vm.LoadParamPtr(FLOAT_TO_STRING2_PARAM_SIZE, 0);

        var dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void DoubleToString2(VirtualMachine vm)
    {
        double num = vm.LoadParamDouble(DOUBLE_TO_STRING2_PARAM_SIZE, 1);
        string s = num.ToString(CultureInfo.InvariantCulture);
        var result = vm.NewString(s);

        var dstAddr = vm.LoadParamPtr(DOUBLE_TO_STRING2_PARAM_SIZE, 0);

        var dst = ReadPointerPtr(dstAddr);
        vm.ObjectRelease(dst);

        WritePointer(dstAddr, result);
    }

    public static void StringToBool(VirtualMachine vm)
    {
        var txt = vm.LoadParamPtr(STRING_TO_BOOL_PARAM_SIZE, 1);
        string s = txt != IntPtr.Zero ? ReadPointerString(txt) : null;

        vm.LastError = Error.NONE;
        bool result = false;

        if (s == null)
        {
            vm.LastError = Error.INVALID_CONVERSION;
        }
        else
        {
            s = s.Trim();
            if (s.Equals("verdade", StringComparison.InvariantCultureIgnoreCase) ||
                s.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
                s.Equals("1", StringComparison.InvariantCultureIgnoreCase))
                result = true;
            else if (s.Equals("falso", StringComparison.InvariantCultureIgnoreCase) ||
                s.Equals("false", StringComparison.InvariantCultureIgnoreCase) ||
                s.Equals("0", StringComparison.InvariantCultureIgnoreCase))
                result = false;
            else
                vm.LastError = Error.INVALID_CONVERSION;
        }

        vm.SetParam(STRING_TO_BOOL_PARAM_SIZE, 0, result ? 1 : 0);
    }

    public static void StringToChar(VirtualMachine vm)
    {
        var txt = vm.LoadParamPtr(STRING_TO_CHAR_PARAM_SIZE, 1);
        string s = txt != IntPtr.Zero ? ReadPointerString(txt) : null;

        vm.LastError = Error.NONE;
        char result = '\0';

        if (s == null)
        {
            vm.LastError = Error.INVALID_CONVERSION;
        }
        else
        {
            if (s.Length == 1)
                result = s[0];
            else
                vm.LastError = Error.INVALID_CONVERSION;
        }

        vm.SetParam(STRING_TO_CHAR_PARAM_SIZE, 0, result);
    }

    public static void StringToInt2(VirtualMachine vm)
    {
        var txt = vm.LoadParamPtr(STRING_TO_INT2_PARAM_SIZE, 1);
        string s = txt != IntPtr.Zero ? ReadPointerString(txt) : null;

        vm.LastError = Error.NONE;
        int result = 0;

        if (s == null)
        {
            vm.LastError = Error.INVALID_CONVERSION;
        }
        else
        {
            try
            {
                result = int.Parse(s, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                vm.LastError = Error.INVALID_CONVERSION;
            }
        }

        vm.SetParam(STRING_TO_INT2_PARAM_SIZE, 0, result);
    }

    public static void StringToLong2(VirtualMachine vm)
    {
        var txt = vm.LoadParamPtr(STRING_TO_LONG2_PARAM_SIZE, 2);
        string s = txt != IntPtr.Zero ? ReadPointerString(txt) : null;

        vm.LastError = Error.NONE;
        long result = 0;

        if (s == null)
        {
            vm.LastError = Error.INVALID_CONVERSION;
        }
        else
        {
            try
            {
                result = long.Parse(s, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                vm.LastError = Error.INVALID_CONVERSION;
            }
        }

        vm.SetParam(STRING_TO_LONG2_PARAM_SIZE, 0, result);
    }

    public static void StringToFloat2(VirtualMachine vm)
    {
        var txt = vm.LoadParamPtr(STRING_TO_FLOAT2_PARAM_SIZE, 1);
        string s = txt != IntPtr.Zero ? ReadPointerString(txt) : null;

        vm.LastError = Error.NONE;
        float result = 0;

        if (s == null)
        {
            vm.LastError = Error.INVALID_CONVERSION;
        }
        else
        {
            try
            {
                result = float.Parse(s, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                vm.LastError = Error.INVALID_CONVERSION;
            }
        }

        vm.SetParam(STRING_TO_FLOAT2_PARAM_SIZE, 0, result);
    }

    public static void StringToDouble2(VirtualMachine vm)
    {
        var txt = vm.LoadParamPtr(STRING_TO_DOUBLE2_PARAM_SIZE, 2);
        string s = txt != IntPtr.Zero ? ReadPointerString(txt) : null;

        vm.LastError = Error.NONE;
        double result = 0;

        if (s == null)
        {
            vm.LastError = Error.INVALID_CONVERSION;
        }
        else
        {
            try
            {
                result = double.Parse(s, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                vm.LastError = Error.INVALID_CONVERSION;
            }
        }

        vm.SetParam(STRING_TO_DOUBLE2_PARAM_SIZE, 0, result);
    }

    public static void LastError(VirtualMachine vm)
    {
        vm.SetParam(LAST_ERROR_PARAM_SIZE, 0, (int) vm.LastError);
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
            { "DecrementaReferenciaArrayTexto", StringArrayRelease },
            { "BoolParaTexto", BoolToString },
            { "CharParaTexto", CharToString },
            { "IntParaTexto", IntToString2 },
            { "LongParaTexto", LongToString2 },
            { "FloatParaTexto", FloatToString2 },
            { "RealParaTexto", DoubleToString2 },
            { "TextoParaBool", StringToBool },
            { "TextoParaChar", StringToChar },
            { "TextoParaInt", StringToInt2 },
            { "TextoParaLong", StringToLong2 },
            { "TextoParaFloat", StringToFloat2 },
            { "TextoParaReal", StringToDouble2 },
            { "UltimoErro", LastError }
        };
    }
}