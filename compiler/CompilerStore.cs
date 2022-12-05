using compiler.types;
using assembler;

namespace compiler
{
    public partial class Compiler
    {
        private void CompileStoreStack(Assembler assembler, Assembler leftAssembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            if (type is StructType)
            {
                // TODO Implementar
                throw new CompilerException(interval, $"Operação não implementada para o tipo '{type}'.");
            }

            if (type is ArrayType)
            {
                // TODO Implementar
                throw new CompilerException(interval, $"Operação não implementada para o tipo '{type}'.");
            }

            if (type is PointerType)
            {
                assembler.EmitStoreStackPtr();
                return;
            }

            if (type is StringType)
            {
                leftAssembler.EmitResidentToHostAddress();
                Function f = unitySystem.FindFunction("AtribuiTexto");
                int index = GetOrAddExternalFunction(f.Name, f.ParameterSize);
                assembler.EmitExternCall(index);
                return;
            }

            throw new CompilerException(interval, $"Tipo desconhecido: '{type}'.");
        }

#pragma warning disable IDE0060 // Remover o parâmetro não utilizado
        private void CompileStorePointer(Assembler assembler, Assembler leftAssembler, AbstractType type, SourceInterval interval)
#pragma warning restore IDE0060 // Remover o parâmetro não utilizado
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        assembler.EmitStorePointer16();
                        return;

                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            if (type is StructType)
            {
                // TODO Implementar
                throw new CompilerException(interval, $"Operação não implementada para o tipo '{type}'.");
            }

            if (type is ArrayType)
            {
                // TODO Implementar
                throw new CompilerException(interval, $"Operação não implementada para o tipo '{type}'.");
            }

            if (type is PointerType)
            {
                assembler.EmitStorePointerPtr();
                return;
            }

            if (type is StringType)
            {
                // TODO Isto está certo?
                leftAssembler.EmitResidentToHostAddress();
                Function f = unitySystem.FindFunction("AtribuiTexto");
                int index = GetOrAddExternalFunction(f.Name, f.ParameterSize);
                assembler.EmitExternCall(index);
                return;
            }

            throw new CompilerException(interval, $"Tipo desconhecido: '{type}'.");
        }

        private void CompileStoreGlobal(Assembler assembler, Assembler leftAssembler, AbstractType type, int offset, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitStoreGlobal8(offset);
                        return;

                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        assembler.EmitStoreGlobal16(offset);
                        return;

                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitStoreGlobal32(offset);
                        return;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitStoreGlobal64(offset);
                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitStoreGlobalPtr(offset);
                return;
            }

            if (type is StringType)
            {
                leftAssembler.EmitLoadGlobalHostAddress(offset);
                Function f = unitySystem.FindFunction("AtribuiTexto");
                int index = GetOrAddExternalFunction(f.Name, f.ParameterSize);
                assembler.EmitExternCall(index);
                return;
            }

            throw new CompilerException(interval, $"Tipo desconhecido: '{type}'.");
        }

        private void CompileStoreLocal(Assembler assembler, Assembler leftAssembler, AbstractType type, int offset, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitStoreLocal8(offset);
                        return;

                    case Primitive.CHAR:
                    case Primitive.SHORT:
                        assembler.EmitStoreLocal16(offset);
                        return;

                    case Primitive.INT:
                    case Primitive.FLOAT:
                        assembler.EmitStoreLocal32(offset);
                        return;

                    case Primitive.LONG:
                    case Primitive.DOUBLE:
                        assembler.EmitStoreLocal64(offset);
                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitStoreLocalPtr(offset);
                return;
            }

            if (type is StringType)
            {
                leftAssembler.EmitLoadLocalHostAddress(offset);
                Function f = unitySystem.FindFunction("AtribuiTexto");
                int index = GetOrAddExternalFunction(f.Name, f.ParameterSize);
                assembler.EmitExternCall(index);
                return;
            }

            throw new CompilerException(interval, $"Tipo desconhecido: '{type}'.");
        }

        private void CompileStore(Assembler assembler, Assembler leftAssembler, Variable storeVar, SourceInterval interval)
        {
            if (storeVar is GlobalVariable)
                CompileStoreGlobal(assembler, leftAssembler, storeVar.Type, unity.GlobalStartOffset + storeVar.Offset, interval);
            else
                CompileStoreLocal(assembler, leftAssembler, storeVar.Type, storeVar.Offset, interval);
        }

        private void CompileStoreStackAdd(Assembler assembler, Assembler leftAssembler, AbstractType type, SourceInterval interval)
        {
            switch (type)
            {
                case PrimitiveType p:
                    switch (p.Primitive)
                    {
                        case Primitive.BYTE:
                            assembler.EmitAdd();
                            assembler.EmitStoreStack8();
                            break;

                        case Primitive.SHORT:
                            assembler.EmitAdd();
                            assembler.EmitStoreStack16();
                            break;

                        case Primitive.INT:
                            assembler.EmitAdd();
                            assembler.EmitStoreStack32();
                            return;

                        case Primitive.LONG:
                            assembler.EmitAdd64();
                            assembler.EmitStoreStack64();
                            return;

                        case Primitive.FLOAT:
                            assembler.EmitFAdd();
                            assembler.EmitStoreStack32();
                            return;

                        case Primitive.DOUBLE:
                            assembler.EmitFAdd64();
                            assembler.EmitStoreStack64();
                            return;
                    }

                    break;

                case PointerType:
                    assembler.EmitAdd();
                    assembler.EmitStoreStackPtr();
                    return;

                case StringType:
                {
                    leftAssembler.EmitResidentToHostAddress();

                    Function func = unitySystem.FindFunction("ConcatenaTextos2");
                    int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                    assembler.EmitExternCall(index);
                    return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerAdd(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            switch (type)
            {
                case PrimitiveType p:
                    switch (p.Primitive)
                    {
                        case Primitive.BYTE:
                            assembler.EmitAdd();
                            assembler.EmitStorePointer8();
                            break;

                        case Primitive.SHORT:
                            assembler.EmitAdd();
                            assembler.EmitStorePointer16();
                            break;

                        case Primitive.INT:
                            assembler.EmitAdd();
                            assembler.EmitStorePointer32();
                            return;

                        case Primitive.LONG:
                            assembler.EmitAdd64();
                            assembler.EmitStorePointer64();
                            return;

                        case Primitive.FLOAT:
                            assembler.EmitFAdd();
                            assembler.EmitStorePointer32();
                            return;

                        case Primitive.DOUBLE:
                            assembler.EmitFAdd64();
                            assembler.EmitStorePointer64();
                            return;
                    }

                    break;

                case PointerType:
                    assembler.EmitAdd();
                    assembler.EmitStorePointerPtr();
                    return;

                case StringType:
                    // TODO Implementar
                    throw new CompilerException(interval, $"Operação não implementada para o tipo '{type}'.");
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreAdd(Assembler assembler, Assembler leftAssembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            switch (type)
            {
                case PrimitiveType p:
                    switch (p.Primitive)
                    {
                        case Primitive.BYTE:
                            assembler.EmitAdd();

                            if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                            else
                                assembler.EmitStoreLocal8(storeVar.Offset);

                            return;

                        case Primitive.SHORT:
                            assembler.EmitAdd();

                            if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                            else
                                assembler.EmitStoreLocal16(storeVar.Offset);

                            return;

                        case Primitive.INT:
                            assembler.EmitAdd();

                            if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                            else
                                assembler.EmitStoreLocal32(storeVar.Offset);

                            return;

                        case Primitive.LONG:
                            assembler.EmitAdd64();

                            if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                            else
                                assembler.EmitStoreLocal64(storeVar.Offset);

                            return;

                        case Primitive.FLOAT:
                            assembler.EmitFAdd();

                            if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                            else
                                assembler.EmitStoreLocal32(storeVar.Offset);

                            return;

                        case Primitive.DOUBLE:
                            assembler.EmitFAdd64();

                            if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                            else
                                assembler.EmitStoreLocal64(storeVar.Offset);

                            return;
                    }

                    break;

                case PointerType:
                    assembler.EmitAdd();

                    if (storeVar is GlobalVariable)
                        assembler.EmitStoreGlobalPtr(unity.GlobalStartOffset + storeVar.Offset);
                    else
                        assembler.EmitStoreLocalPtr(storeVar.Offset);

                    return;

                case StringType:
                {
                    if (storeVar is GlobalVariable)
                        leftAssembler.EmitLoadGlobalHostAddress(unity.GlobalStartOffset + storeVar.Offset);
                    else
                        leftAssembler.EmitLoadLocalHostAddress(storeVar.Offset);

                    Function func = unitySystem.FindFunction("ConcatenaTextos2");
                    int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                    assembler.EmitExternCall(index);
                    return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackSub(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitSub();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitSub();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitSub();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitSub64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFSub();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFSub64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitSub();
                assembler.EmitStoreStackPtr();
                return;
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerSub(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitSub();
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitSub();
                        assembler.EmitStorePointer16();
                        return;

                    case Primitive.INT:
                        assembler.EmitSub();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitSub64();
                        assembler.EmitStorePointer64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFSub();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFSub64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitSub();
                assembler.EmitStorePointerPtr();
                return;
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreSub(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitSub();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitSub();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitSub();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitSub64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFSub();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFSub64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            if (type is PointerType)
            {
                assembler.EmitSub();

                if (storeVar is GlobalVariable)
                    assembler.EmitStoreGlobalPtr(unity.GlobalStartOffset + storeVar.Offset);
                else
                    assembler.EmitStoreLocalPtr(storeVar.Offset);

                return;
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackMul(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMul();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitMul();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitMul();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitMul64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFMul();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFMul64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerMul(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMul();
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitMul();
                        assembler.EmitStorePointer16();
                        return;

                    case Primitive.INT:
                        assembler.EmitMul();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitMul64();
                        assembler.EmitStorePointer64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFMul();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFMul64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreMul(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMul();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitMul();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitMul();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitMul64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFMul();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFMul64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackDiv(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitDiv();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitDiv();
                        assembler.EmitStoreStack16();

                        return;
                    case Primitive.INT:
                        assembler.EmitDiv();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitDiv64();
                        assembler.EmitStoreStack64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFDiv();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFDiv64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerDiv(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitDiv();
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitDiv();
                        assembler.EmitStorePointer16();

                        return;
                    case Primitive.INT:
                        assembler.EmitDiv();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitDiv64();
                        assembler.EmitStorePointer64();
                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFDiv();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFDiv64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreDiv(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitDiv();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitDiv();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;
                    case Primitive.INT:
                        assembler.EmitDiv();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitDiv64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;

                    case Primitive.FLOAT:
                        assembler.EmitFDiv();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.DOUBLE:
                        assembler.EmitFDiv64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackMod(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMod();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitMod();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitMod();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitMod64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerMod(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMod();
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitMod();
                        assembler.EmitStorePointer16();
                        return;

                    case Primitive.INT:
                        assembler.EmitMod();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitMod64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreMod(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitMod();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitMod();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitMod();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitMod64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackAnd(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitAnd();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitAnd();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitAnd();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitAnd64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerAnd(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitAnd();
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitAnd();
                        assembler.EmitStorePointer16();
                        return;

                    case Primitive.INT:
                        assembler.EmitAnd();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitAnd64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreAnd(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitAnd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitAnd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitAnd();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitAnd64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackOr(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitOr();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitOr();
                        assembler.EmitStoreStack16();

                        return;
                    case Primitive.INT:
                        assembler.EmitOr();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitOr64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerOr(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitOr();
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitOr();
                        assembler.EmitStorePointer16();

                        return;
                    case Primitive.INT:
                        assembler.EmitOr();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitOr64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreOr(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitOr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitOr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitOr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitOr64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackXor(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitXor();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitXor();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitXor();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitXor64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerXor(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitXor();
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitXor();
                        assembler.EmitStorePointer16();
                        return;

                    case Primitive.INT:
                        assembler.EmitXor();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitXor64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreXor(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BOOL:
                    case Primitive.BYTE:
                        assembler.EmitXor();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitXor();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;
                    case Primitive.INT:
                        assembler.EmitXor();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitXor64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackShiftLeft(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShl();
                        assembler.EmitStoreStack8();

                        return;
                    case Primitive.SHORT:
                        assembler.EmitShl();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitShl();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitShl64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerShiftLeft(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShl();
                        assembler.EmitStorePointer8();

                        return;
                    case Primitive.SHORT:
                        assembler.EmitShl();
                        assembler.EmitStorePointer16();
                        return;

                    case Primitive.INT:
                        assembler.EmitShl();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitShl64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreShiftLeft(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShl();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitShl();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitShl();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitShl64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackShiftRight(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShr();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitShr();
                        assembler.EmitStoreStack16();
                        return;

                    case Primitive.INT:
                        assembler.EmitShr();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitShr64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerShiftRight(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShr();
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitShr();
                        assembler.EmitStorePointer16();
                        return;

                    case Primitive.INT:
                        assembler.EmitShr();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitShr64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreShiftRight(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitShr64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreStackUnsignedShiftRight(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitUShr();
                        assembler.EmitStoreStack8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitUShr();
                        assembler.EmitStoreStack16();

                        return;
                    case Primitive.INT:
                        assembler.EmitUShr();
                        assembler.EmitStoreStack32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitUShr64();
                        assembler.EmitStoreStack64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStorePointerUnsignedShiftRight(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitUShr();
                        assembler.EmitStorePointer8();
                        return;

                    case Primitive.SHORT:
                        assembler.EmitUShr();
                        assembler.EmitStorePointer16();

                        return;
                    case Primitive.INT:
                        assembler.EmitUShr();
                        assembler.EmitStorePointer32();
                        return;

                    case Primitive.LONG:
                        assembler.EmitUShr64();
                        assembler.EmitStorePointer64();
                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreUnsignedShiftRight(Assembler assembler, Variable storeVar, SourceInterval interval)
        {
            AbstractType type = storeVar.Type;
            if (type is PrimitiveType p)
            {
                switch (p.Primitive)
                {
                    case Primitive.BYTE:
                        assembler.EmitUShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal8(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal8(storeVar.Offset);

                        return;

                    case Primitive.SHORT:
                        assembler.EmitUShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal16(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal16(storeVar.Offset);

                        return;

                    case Primitive.INT:
                        assembler.EmitUShr();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal32(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal32(storeVar.Offset);

                        return;

                    case Primitive.LONG:
                        assembler.EmitUShr64();

                        if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobal64(unity.GlobalStartOffset + storeVar.Offset);
                        else
                            assembler.EmitStoreLocal64(storeVar.Offset);

                        return;
                }
            }

            throw new CompilerException(interval, $"Operação inválida para o tipo '{type}'.");
        }

        private void CompileStoreExpression(Context context, Assembler assembler, BinaryOperation operation, Expression leftOperand, Expression rightOperand)
        {
            AbstractType leftType;
            AbstractType rightType;
            Assembler leftAssembler;
            Assembler preCastAssembler;
            Assembler rightAssembler;
            Assembler storeAssembler;
            Variable storeVar;
            LocalVariable tempVar;
            LocalVariable castTempVar;
            bool isPointerDeference;

            if (operation == BinaryOperation.STORE)
            {
                leftAssembler = new();
                leftType = CompileAssignableExpression(context, leftAssembler, leftOperand, out storeVar, out isPointerDeference);

                preCastAssembler = new();
                rightAssembler = new();
                rightType = CompileExpression(context, rightAssembler, rightOperand, out tempVar);
                CompileCast(context, rightAssembler, preCastAssembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);

                storeAssembler = new();
                if (storeVar != null)
                {
                    leftAssembler.Clear();
                    CompileStore(storeAssembler, leftAssembler, storeVar, leftOperand.Interval);
                }
                else if (isPointerDeference)
                    CompileStorePointer(storeAssembler, leftAssembler, leftType, leftOperand.Interval);
                else
                    CompileStoreStack(storeAssembler, leftAssembler, leftType, leftOperand.Interval);

                assembler.Emit(leftAssembler);
                assembler.Emit(preCastAssembler);
                assembler.Emit(rightAssembler);
                assembler.Emit(storeAssembler);

                tempVar?.Release();
                castTempVar?.Release();

                return;
            }

            leftAssembler = new();
            Assembler loadAssembler = new();

            leftType = CompileAssignableExpression(context, leftAssembler, leftOperand, out storeVar, out isPointerDeference);

            if (storeVar == null)
            {
                loadAssembler.Emit(leftAssembler);

                if (isPointerDeference)
                    CompileLoadPointer(loadAssembler, leftType, leftOperand.Interval);
                else
                    CompileLoadStack(loadAssembler, leftType, leftOperand.Interval);
            }
            else
            {
                leftAssembler.Clear();
                CompileLoad(loadAssembler, storeVar, leftOperand.Interval);
            }

            rightAssembler = new();
            rightType = CompileExpression(context, rightAssembler, rightOperand, out tempVar);

            preCastAssembler = new();
            CompileCast(context, rightAssembler, preCastAssembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);

            storeAssembler = new();
            switch (operation)
            {
                case BinaryOperation.STORE_OR:
                {
                    if (storeVar == null)
                        CompileStoreStackOr(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerOr(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreOr(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_XOR:
                {
                    if (storeVar == null)
                        CompileStoreStackXor(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerXor(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreXor(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_AND:
                {
                    if (storeVar == null)
                        CompileStoreStackAnd(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerAnd(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreAnd(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_SHIFT_LEFT:
                {
                    if (storeVar == null)
                        CompileStoreStackShiftLeft(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerShiftLeft(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreShiftLeft(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_SHIFT_RIGHT:
                {
                    if (storeVar == null)
                        CompileStoreStackShiftRight(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerShiftRight(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreShiftRight(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_UNSIGNED_SHIFT_RIGHT:
                {
                    if (storeVar == null)
                        CompileStoreStackUnsignedShiftRight(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerUnsignedShiftRight(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreUnsignedShiftRight(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_ADD:
                {
                    if (storeVar == null)
                        CompileStoreStackAdd(storeAssembler, leftAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerAdd(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreAdd(storeAssembler, leftAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_SUB:
                {
                    if (storeVar == null)
                        CompileStoreStackSub(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerSub(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreSub(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_MUL:
                {
                    if (storeVar == null)
                        CompileStoreStackMul(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerMul(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreMul(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_DIV:
                {
                    if (storeVar == null)
                        CompileStoreStackDiv(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerDiv(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreDiv(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                case BinaryOperation.STORE_MOD:
                {
                    if (storeVar == null)
                        CompileStoreStackMod(storeAssembler, leftType, leftOperand.Interval);
                    else if (isPointerDeference)
                        CompileStorePointerMod(storeAssembler, leftType, leftOperand.Interval);
                    else
                        CompileStoreMod(storeAssembler, storeVar, leftOperand.Interval);

                    break;
                }

                default:
                    throw new CompilerException(leftOperand.Interval, $"Operador '{operation}' desconhecido.");
            }

            tempVar?.Release();
            castTempVar?.Release();

            assembler.Emit(leftAssembler);
            assembler.Emit(loadAssembler);
            assembler.Emit(preCastAssembler);
            assembler.Emit(rightAssembler);
            assembler.Emit(storeAssembler);
        }
    }
}
