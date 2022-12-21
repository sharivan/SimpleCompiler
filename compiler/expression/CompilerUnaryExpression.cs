using compiler.types;
using assembler;

namespace compiler
{
    public partial class Compiler
    {
        private AbstractType CompileAssignableUnaryExpression(Context context, Assembler assembler, UnaryExpression expression, out bool isPointerDeference)
        {
            Expression operand = expression.Operand;
            AbstractType operandType = CompileExpression(context, assembler, operand, out Variable tempVar);
            if (expression.Operation != UnaryOperation.POINTER_INDIRECTION)
                throw new CompilerException(operand.Interval, "A expressão do lado esquerdo não é atribuível.");

            if (operandType is not PointerType ptr)
                throw new CompilerException(operand.Interval, "Indireção de ponteiros só pode ser feita com tipos de ponteiros.");

            tempVar?.Release();

            isPointerDeference = true;
            return ptr.Type;
        }

        private AbstractType CompileUnaryExpression(Context context, Assembler assembler, UnaryExpression expression)
        {
            AbstractType result = null;
            Expression operand = expression.Operand;
            switch (expression.Operation)
            {
                case UnaryOperation.NEGATION:
                {
                    result = CompileExpression(context, assembler, operand, out _);
                    if (result is not PrimitiveType pt)
                        throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{result}'.");

                    switch (pt.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.CHAR:
                            throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{pt}'.");

                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitNeg();
                            break;

                        case Primitive.LONG:
                            assembler.EmitNeg64();
                            break;

                        case Primitive.FLOAT:
                            assembler.EmitFNeg();
                            break;

                        case Primitive.DOUBLE:
                            assembler.EmitFNeg64();
                            break;
                    }

                    break;
                }

                case UnaryOperation.BITWISE_NOT:
                {
                    result = CompileExpression(context, assembler, operand, out _);
                    if (result is not PrimitiveType pt)
                        throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{result}'.");

                    switch (pt.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.CHAR:
                        case Primitive.FLOAT:
                        case Primitive.DOUBLE:
                            throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{pt}'.");

                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitNot();
                            break;

                        case Primitive.LONG:
                            assembler.EmitNot64();
                            break;
                    }

                    break;
                }

                case UnaryOperation.LOGICAL_NOT:
                {
                    AbstractType operandType = CompileExpression(context, assembler, operand, out _);
                    if (operandType is not PrimitiveType pt)
                        throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{operandType}'.");

                    switch (pt.Primitive)
                    {
                        case Primitive.BOOL:
                            assembler.EmitNot();
                            break;

                        default:
                            throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{pt}'.");
                    }

                    result = PrimitiveType.BOOL;
                    break;
                }

                case UnaryOperation.POINTER_INDIRECTION:
                {
                    AbstractType operandType = CompileExpression(context, assembler, operand, out _);
                    if (operandType is not PointerType ptr)
                        throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{operandType}'.");

                    AbstractType ptrType = ptr.Type;
                    if (ptrType == null)
                        throw new CompilerException(operand.Interval, "Indireção de ponteiros não é valida para literais nulos.");

                    if (ptrType is PrimitiveType pt)
                        switch (pt.Primitive)
                        {
                            case Primitive.VOID:
                                throw new CompilerException(operand.Interval, "Indireção em ponteiros do tipo void não pode ser feita.");

                            case Primitive.BOOL:
                            case Primitive.BYTE:
                                assembler.EmitLoadPointer8();
                                break;

                            case Primitive.CHAR:
                            case Primitive.SHORT:
                                assembler.EmitLoadPointer16();
                                break;

                            case Primitive.INT:
                            case Primitive.FLOAT:
                                assembler.EmitLoadPointer32();
                                break;

                            case Primitive.LONG:
                            case Primitive.DOUBLE:
                                assembler.EmitLoadPointer64();
                                break;
                        }
                    else if (ptrType is ArrayType at)
                    {
                        // TODO Implementar
                        throw new CompilerException(operand.Interval, $"Operação não implementada para o tipo '{at}'.");
                    }
                    else if (ptrType is StructType st)
                    {
                        // TODO Implementar
                        throw new CompilerException(operand.Interval, $"Operação não implementada para o tipo '{st}'.");
                    }
                    else if (ptrType is PointerType)
                        assembler.EmitLoadPointerPtr();

                    result = ptrType;
                    break;
                }

                case UnaryOperation.PRE_INCREMENT: // ++x <=> x = x + 1
                {
                    Assembler tempAssembler = new();
                    AbstractType operandType = CompileAssignableExpression(context, tempAssembler, operand, out Variable storeVar, out bool isPointerDeference);

                    if (storeVar == null)
                    {
                        assembler.Emit(tempAssembler);
                        assembler.Emit(tempAssembler);

                        if (isPointerDeference)
                            CompileLoadPointer(assembler, operandType, operand.Interval);
                        else
                            CompileLoadStack(assembler, operandType, operand.Interval);
                    }
                    else
                        CompileLoad(assembler, storeVar, operand.Interval);

                    switch (operandType)
                    {
                        case PrimitiveType pt:
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{pt}'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer8();
                                        else
                                            assembler.EmitStoreStack8();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);

                                    break;

                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer16();
                                        else
                                            assembler.EmitStoreStack16();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitAdd();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitAdd64();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFAdd();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFAdd64();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            if (storeVar == null)
                            {
                                assembler.Emit(tempAssembler);

                                if (isPointerDeference)
                                    CompileLoadPointer(assembler, operandType, operand.Interval);
                                else
                                    CompileLoadStack(assembler, operandType, operand.Interval);
                            }
                            else
                                CompileLoad(assembler, storeVar, operand.Interval);

                            result = operandType;
                            break;

                        case PointerType:
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitPtrAdd();

                            if (storeVar == null)
                                if (isPointerDeference)
                                    assembler.EmitStorePointerPtr();
                                else
                                    assembler.EmitStoreStackPtr();
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobalPtr(storeVar.Offset);
                            else
                                assembler.EmitStoreLocalPtr(storeVar.Offset);

                            result = operandType;
                            break;

                        default:
                            throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{operandType}'.");
                    }

                    break;
                }

                case UnaryOperation.PRE_DECREMENT:
                {
                    Assembler tempAssembler = new();
                    AbstractType operandType = CompileAssignableExpression(context, tempAssembler, operand, out Variable storeVar, out bool isPointerDeference);

                    if (storeVar == null)
                    {
                        assembler.Emit(tempAssembler);
                        assembler.Emit(tempAssembler);

                        if (isPointerDeference)
                            CompileLoadPointer(assembler, operandType, operand.Interval);
                        else
                            CompileLoadStack(assembler, operandType, operand.Interval);
                    }
                    else
                        CompileLoad(assembler, storeVar, operand.Interval);

                    switch (operandType)
                    {
                        case PrimitiveType pt:
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{pt}'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer8();
                                        else
                                            assembler.EmitStoreStack8();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);

                                    break;

                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer16();
                                        else
                                            assembler.EmitStoreStack16();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitSub64();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFSub();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFSub64();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            if (storeVar == null)
                            {
                                assembler.Emit(tempAssembler);

                                if (isPointerDeference)
                                    CompileLoadPointer(assembler, operandType, operand.Interval);
                                else
                                    CompileLoadStack(assembler, operandType, operand.Interval);
                            }
                            else
                                CompileLoad(assembler, storeVar, operand.Interval);

                            result = operandType;
                            break;

                        case PointerType:
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitPtrSub();

                            if (storeVar == null)
                                if (isPointerDeference)
                                    assembler.EmitStorePointerPtr();
                                else
                                    assembler.EmitStoreStackPtr();
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobalPtr(storeVar.Offset);
                            else
                                assembler.EmitStoreLocalPtr(storeVar.Offset);

                            result = operandType;
                            break;

                        default:
                            throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{operandType}'.");
                    }

                    break;
                }

                case UnaryOperation.POST_INCREMENT:
                {
                    Assembler tempAssembler = new();
                    AbstractType operandType = CompileAssignableExpression(context, tempAssembler, operand, out Variable storeVar, out bool isPointerDeference);

                    if (storeVar == null)
                    {
                        Assembler tempAssembler2 = new();
                        tempAssembler2.Emit(tempAssembler);

                        if (isPointerDeference)
                            CompileLoadPointer(tempAssembler2, operandType, operand.Interval);
                        else
                            CompileLoadStack(tempAssembler2, operandType, operand.Interval);

                        assembler.Emit(tempAssembler2);

                        assembler.Emit(tempAssembler);
                        assembler.Emit(tempAssembler2);
                    }
                    else
                    {
                        Assembler tempAssembler2 = new();
                        CompileLoad(tempAssembler2, storeVar, operand.Interval);

                        assembler.Emit(tempAssembler2);
                        assembler.Emit(tempAssembler2);
                    }

                    if (operandType is not PrimitiveType pt)
                        throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{operandType}'.");

                    switch (pt.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.CHAR:
                            throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{pt}'.");

                        case Primitive.BYTE:
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitAdd();

                            if (storeVar == null)
                                if (isPointerDeference)
                                    assembler.EmitStorePointer8();
                                else
                                    assembler.EmitStoreStack8();
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal8(storeVar.Offset);
                            else
                                assembler.EmitStoreLocal8(storeVar.Offset);

                            break;

                        case Primitive.SHORT:
                            assembler.EmitLoadConst((short) 1);
                            assembler.EmitAdd();

                            if (storeVar == null)
                                if (isPointerDeference)
                                    assembler.EmitStorePointer16();
                                else
                                    assembler.EmitStoreStack16();
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal16(storeVar.Offset);
                            else
                                assembler.EmitStoreLocal16(storeVar.Offset);

                            break;

                        case Primitive.INT:
                            assembler.EmitLoadConst(1);
                            assembler.EmitAdd();

                            if (storeVar == null)
                                if (isPointerDeference)
                                    assembler.EmitStorePointer32();
                                else
                                    assembler.EmitStoreStack32();
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal32(storeVar.Offset);
                            else
                                assembler.EmitStoreLocal32(storeVar.Offset);

                            break;

                        case Primitive.LONG:
                            assembler.EmitLoadConst(1L);
                            assembler.EmitAdd64();

                            if (storeVar == null)
                                if (isPointerDeference)
                                    assembler.EmitStorePointer64();
                                else
                                    assembler.EmitStoreStack64();
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal64(storeVar.Offset);
                            else
                                assembler.EmitStoreLocal64(storeVar.Offset);

                            break;

                        case Primitive.FLOAT:
                            assembler.EmitLoadConst(1F);
                            assembler.EmitFAdd();

                            if (storeVar == null)
                                if (isPointerDeference)
                                    assembler.EmitStorePointer32();
                                else
                                    assembler.EmitStoreStack32();
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal32(storeVar.Offset);
                            else
                                assembler.EmitStoreLocal32(storeVar.Offset);

                            break;

                        case Primitive.DOUBLE:
                            assembler.EmitLoadConst(1.0);
                            assembler.EmitFAdd64();

                            if (storeVar == null)
                                if (isPointerDeference)
                                    assembler.EmitStorePointer64();
                                else
                                    assembler.EmitStoreStack64();
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobal64(storeVar.Offset);
                            else
                                assembler.EmitStoreLocal64(storeVar.Offset);

                            break;
                    }

                    if (operandType is PointerType)
                    {
                        assembler.EmitLoadConst((byte) 1);
                        assembler.EmitPtrAdd();

                        if (storeVar == null)
                            if (isPointerDeference)
                                assembler.EmitStorePointerPtr();
                            else
                                assembler.EmitStoreStackPtr();
                        else if (storeVar is GlobalVariable)
                            assembler.EmitStoreGlobalPtr(storeVar.Offset);
                        else
                            assembler.EmitStoreLocalPtr(storeVar.Offset);
                    }

                    result = operandType;
                    break;
                }

                case UnaryOperation.POST_DECREMENT:
                {
                    Assembler tempAssembler = new();
                    AbstractType operandType = CompileAssignableExpression(context, tempAssembler, operand, out Variable storeVar, out bool isPointerDeference);

                    if (storeVar == null)
                    {
                        Assembler tempAssembler2 = new();

                        tempAssembler2.Emit(tempAssembler);

                        if (isPointerDeference)
                            CompileLoadPointer(tempAssembler2, operandType, operand.Interval);
                        else
                            CompileLoadStack(tempAssembler2, operandType, operand.Interval);

                        assembler.Emit(tempAssembler2);

                        assembler.Emit(tempAssembler);
                        assembler.Emit(tempAssembler2);
                    }
                    else
                    {
                        Assembler tempAssembler2 = new();
                        CompileLoad(tempAssembler2, storeVar, operand.Interval);

                        assembler.Emit(tempAssembler2);
                        assembler.Emit(tempAssembler2);
                    }

                    switch (operandType)
                    {
                        case PrimitiveType pt:
                            switch (pt.Primitive)
                            {
                                case Primitive.BOOL:
                                case Primitive.CHAR:
                                    throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{pt}'.");

                                case Primitive.BYTE:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer8();
                                        else
                                            assembler.EmitStoreStack8();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal8(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal8(storeVar.Offset);

                                    break;
                                case Primitive.SHORT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer16();
                                        else
                                            assembler.EmitStoreStack16();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal16(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal16(storeVar.Offset);

                                    break;

                                case Primitive.INT:
                                    assembler.EmitLoadConst((byte) 1);
                                    assembler.EmitSub();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.LONG:
                                    assembler.EmitLoadConst(1L);
                                    assembler.EmitSub64();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;

                                case Primitive.FLOAT:
                                    assembler.EmitLoadConst(1F);
                                    assembler.EmitFSub();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer32();
                                        else
                                            assembler.EmitStoreStack32();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal32(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal32(storeVar.Offset);

                                    break;

                                case Primitive.DOUBLE:
                                    assembler.EmitLoadConst(1.0);
                                    assembler.EmitFSub64();

                                    if (storeVar == null)
                                        if (isPointerDeference)
                                            assembler.EmitStorePointer64();
                                        else
                                            assembler.EmitStoreStack64();
                                    else if (storeVar is GlobalVariable)
                                        assembler.EmitStoreGlobal64(storeVar.Offset);
                                    else
                                        assembler.EmitStoreLocal64(storeVar.Offset);

                                    break;
                            }

                            result = operandType;
                            break;

                        case PointerType:
                            assembler.EmitLoadConst((byte) 1);
                            assembler.EmitPtrSub();

                            if (storeVar == null)
                                if (isPointerDeference)
                                    assembler.EmitStorePointerPtr();
                                else
                                    assembler.EmitStoreStackPtr();
                            else if (storeVar is GlobalVariable)
                                assembler.EmitStoreGlobalPtr(storeVar.Offset);
                            else
                                assembler.EmitStoreLocalPtr(storeVar.Offset);

                            result = operandType;
                            break;

                        default:
                            throw new CompilerException(operand.Interval, $"Operação não definida para o tipo '{operandType}'.");
                    }

                    break;
                }

                case UnaryOperation.POINTER_TO:
                {
                    AbstractType operandType = CompileAssignableExpression(context, assembler, operand, out _, out _, true);
                    result = operandType is ArrayType at ? new PointerType(at.Type, true) : (AbstractType) new PointerType(operandType);
                    break;
                }

                default:
                    throw new CompilerException(expression.Interval, $"Operador '{expression.Operation}' desconhecido.");
            }

            return result;
        }
    }
}