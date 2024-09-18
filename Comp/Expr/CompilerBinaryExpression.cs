using Asm;

using Comp.Types;

namespace Comp;

public partial class Compiler
{
    private AbstractType CompileBinaryExpression(Context context, Assembler assembler, BinaryExpression expression, out Variable tempVar)
    {
        tempVar = null;

        var leftOperand = expression.LeftOperand;
        var rightOperand = expression.RightOperand;
        AbstractType result;

        if (expression.Operation <= BinaryOperation.STORE_MOD)
        {
            CompileStoreExpression(context, assembler, expression.Operation, leftOperand, rightOperand);
            result = PrimitiveType.VOID;
        }
        else
        {
            Assembler leftAssembler = new();

            var leftType = CompileExpression(context, leftAssembler, leftOperand, out _);

            Assembler rightAssembler = new();

            var rightType = CompileExpression(context, rightAssembler, rightOperand, out _);

            switch (expression.Operation)
            {
                case BinaryOperation.LOGICAL_OR:
                {
                    if (!PrimitiveType.IsPrimitiveBool(leftType))
                        throw new CompilerException(leftOperand.Interval, "Operação não definida entre tipos não booleanos.");

                    if (!PrimitiveType.IsPrimitiveBool(rightType))
                        throw new CompilerException(rightOperand.Interval, "Operação não definida entre tipos não booleanos.");

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);
                    assembler.EmitOr();
                    result = PrimitiveType.BOOL;
                    break;
                }

                case BinaryOperation.LOGICAL_XOR:
                {
                    if (!PrimitiveType.IsPrimitiveBool(leftType))
                        throw new CompilerException(leftOperand.Interval, "Operação não definida entre tipos não booleanos.");

                    if (!PrimitiveType.IsPrimitiveBool(rightType))
                        throw new CompilerException(rightOperand.Interval, "Operação não definida entre tipos não booleanos.");

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);
                    assembler.EmitXor();
                    result = PrimitiveType.BOOL;
                    break;
                }

                case BinaryOperation.LOGICAL_AND:
                {
                    if (!PrimitiveType.IsPrimitiveBool(leftType))
                        throw new CompilerException(leftOperand.Interval, "Operação não definida entre tipos não booleanos.");

                    if (!PrimitiveType.IsPrimitiveBool(rightType))
                        throw new CompilerException(rightOperand.Interval, "Operação não definida entre tipos não booleanos.");

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);
                    assembler.EmitAnd();
                    result = PrimitiveType.BOOL;
                    break;
                }

                case BinaryOperation.SHIFT_LEFT:
                {
                    if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                        throw new CompilerException(rightOperand.Interval, $"Tipo inválido para o operando direito: '{rightType}'.");

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);

                    if (PrimitiveType.IsUpTo32BitsInt(leftType))
                        assembler.EmitShl();
                    else if (PrimitiveType.Is64BitsInt(leftType))
                        assembler.EmitShl64();
                    else
                        throw new CompilerException(leftOperand.Interval, $"Tipo inválido para o operando esquerdo: '{leftType}'.");

                    result = leftType;
                    break;
                }

                case BinaryOperation.SHIFT_RIGHT:
                {
                    if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                        throw new CompilerException(rightOperand.Interval, $"Tipo inválido para o operando direito: '{rightType}'.");

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);

                    if (PrimitiveType.IsUpTo32BitsInt(leftType))
                        assembler.EmitShr();
                    else if (PrimitiveType.Is64BitsInt(leftType))
                        assembler.EmitShr64();
                    else
                        throw new CompilerException(leftOperand.Interval, $"Tipo inválido para o operando esquerdo: '{leftType}'.");

                    result = leftType;
                    break;
                }

                case BinaryOperation.UNSIGNED_SHIFT_RIGHT:
                {
                    if (!PrimitiveType.IsUpTo32BitsInt(rightType))
                        throw new CompilerException(rightOperand.Interval, $"Tipo inválido para o operando direito: '{rightType}'.");

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);

                    if (PrimitiveType.IsUpTo32BitsInt(leftType))
                        assembler.EmitUShr();
                    else if (PrimitiveType.Is64BitsInt(leftType))
                        assembler.EmitUShr64();
                    else
                        throw new CompilerException(leftOperand.Interval, $"Tipo inválido para o operando esquerdo: '{leftType}'.");

                    result = leftType;
                    break;
                }

                case BinaryOperation.EQUALS:
                {
                    if (PrimitiveType.IsPrimitiveBool(leftType) && PrimitiveType.IsPrimitiveBool(rightType))
                    {
                        assembler.Emit(leftAssembler);
                        assembler.EmitLoadConst((byte) 1);
                        assembler.EmitAnd();
                        assembler.Emit(rightAssembler);
                        assembler.EmitLoadConst((byte) 1);
                        assembler.EmitAnd();
                        assembler.EmitCompareEquals();

                        result = PrimitiveType.BOOL;
                        break;
                    }

                    if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                    {
                        PrimitiveType resultType = null;
                        Variable castTempVar = null;
                        if (leftType.CoerceWith(rightType, false))
                        {
                            resultType = (PrimitiveType) rightType;
                            CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                        }
                        else if (rightType.CoerceWith(leftType, false))
                        {
                            resultType = (PrimitiveType) leftType;
                            CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                        }

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        switch (resultType.Primitive)
                        {
                            case Primitive.BYTE:
                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitCompareEquals();
                                break;

                            case Primitive.LONG:
                                assembler.EmitCompareEquals64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFCompareEquals();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFCompareEquals64();
                                break;
                        }

                        castTempVar?.Release();

                        result = PrimitiveType.BOOL;
                    }
                    else if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitCompareEquals();

                        result = PrimitiveType.BOOL;
                    }
                    else if (leftType is PointerType && rightType is PointerType)
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitComparePointerEquals();

                        result = PrimitiveType.BOOL;
                    }
                    else if (leftType == rightType)
                    {
                        // TODO Implementar
                        throw new CompilerException(expression.Interval, $"Operação não implementada para os tipos '{leftType}' e '{rightType}'.");
                    }
                    else
                    {
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    break;
                }

                case BinaryOperation.NOT_EQUALS:
                {
                    if (PrimitiveType.IsPrimitiveBool(leftType) && PrimitiveType.IsPrimitiveBool(rightType))
                    {
                        assembler.Emit(leftAssembler);
                        assembler.EmitLoadConst((byte) 1);
                        assembler.EmitAnd();
                        assembler.Emit(rightAssembler);
                        assembler.EmitLoadConst((byte) 1);
                        assembler.EmitAnd();
                        assembler.EmitCompareNotEquals();

                        result = PrimitiveType.BOOL;
                    }
                    else if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                    {
                        PrimitiveType resultType = null;
                        Variable castTempVar = null;
                        if (leftType.CoerceWith(rightType, false))
                        {
                            resultType = (PrimitiveType) rightType;
                            CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                        }
                        else if (rightType.CoerceWith(leftType, false))
                        {
                            resultType = (PrimitiveType) leftType;
                            CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                        }

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        switch (resultType.Primitive)
                        {
                            case Primitive.BYTE:
                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitCompareNotEquals();
                                break;

                            case Primitive.LONG:
                                assembler.EmitCompareNotEquals64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFCompareNotEquals();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFCompareNotEquals64();
                                break;
                        }

                        castTempVar?.Release();

                        result = PrimitiveType.BOOL;
                    }
                    else if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitCompareNotEquals();

                        result = PrimitiveType.BOOL;
                    }
                    else if (leftType is PointerType && rightType is PointerType)
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitComparePointerNotEquals();

                        result = PrimitiveType.BOOL;
                    }
                    else if (leftType == rightType)
                    {
                        // TODO Implementar
                        throw new CompilerException(expression.Interval, $"Operação não implementada para os tipos '{leftType}' e '{rightType}'.");
                    }
                    else
                    {
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    break;
                }

                case BinaryOperation.GREATER:
                {
                    if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                    {
                        PrimitiveType resultType = null;
                        Variable castTempVar = null;
                        if (leftType.CoerceWith(rightType, false))
                        {
                            resultType = (PrimitiveType) rightType;
                            CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                        }
                        else if (rightType.CoerceWith(leftType, false))
                        {
                            resultType = (PrimitiveType) leftType;
                            CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                        }

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        switch (resultType.Primitive)
                        {
                            case Primitive.BYTE:
                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitCompareGreater();
                                break;

                            case Primitive.LONG:
                                assembler.EmitCompareGreater64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFCompareGreater();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFCompareGreater64();
                                break;
                        }

                        castTempVar?.Release();

                        result = PrimitiveType.BOOL;
                    }
                    else if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitCompareGreater();

                        result = PrimitiveType.BOOL;
                    }
                    else if (leftType is PointerType && rightType is PointerType)
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitComparePointerGreater();

                        result = PrimitiveType.BOOL;
                    }
                    else
                    {
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    break;
                }

                case BinaryOperation.GREATER_OR_EQUALS:
                {
                    if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                    {
                        PrimitiveType resultType = null;
                        Variable castTempVar = null;
                        if (leftType.CoerceWith(rightType, false))
                        {
                            resultType = (PrimitiveType) rightType;
                            CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                        }
                        else if (rightType.CoerceWith(leftType, false))
                        {
                            resultType = (PrimitiveType) leftType;
                            CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                        }

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        switch (resultType.Primitive)
                        {
                            case Primitive.BYTE:
                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitCompareGreaterOrEquals();
                                break;

                            case Primitive.LONG:
                                assembler.EmitCompareGreaterOrEquals64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFCompareGreaterOrEquals();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFCompareGreaterOrEquals64();
                                break;
                        }

                        castTempVar?.Release();

                        result = PrimitiveType.BOOL;
                    }
                    else if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitCompareGreaterOrEquals();

                        result = PrimitiveType.BOOL;
                    }
                    else if (leftType is PointerType && rightType is PointerType)
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitComparePointerGreaterOrEquals();

                        result = PrimitiveType.BOOL;
                    }
                    else
                    {
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    break;
                }

                case BinaryOperation.LESS:
                {
                    if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                    {
                        PrimitiveType resultType = null;
                        Variable castTempVar = null;
                        if (leftType.CoerceWith(rightType, false))
                        {
                            resultType = (PrimitiveType) rightType;
                            CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                        }
                        else if (rightType.CoerceWith(leftType, false))
                        {
                            resultType = (PrimitiveType) leftType;
                            CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                        }

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        switch (resultType.Primitive)
                        {
                            case Primitive.BYTE:
                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitCompareLess();
                                break;

                            case Primitive.LONG:
                                assembler.EmitCompareLess64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFCompareLess();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFCompareLess64();
                                break;
                        }

                        castTempVar?.Release();

                        result = PrimitiveType.BOOL;
                    }
                    else if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitCompareLess();

                        result = PrimitiveType.BOOL;
                    }
                    else if (leftType is PointerType && rightType is PointerType)
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitComparePointerLess();

                        result = PrimitiveType.BOOL;
                    }
                    else
                    {
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    break;
                }

                case BinaryOperation.LESS_OR_EQUALS:
                {
                    if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                    {
                        PrimitiveType resultType = null;
                        Variable castTempVar = null;
                        if (leftType.CoerceWith(rightType, false))
                        {
                            resultType = (PrimitiveType) rightType;
                            CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                        }
                        else if (rightType.CoerceWith(leftType, false))
                        {
                            resultType = (PrimitiveType) leftType;
                            CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                        }

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        switch (resultType.Primitive)
                        {
                            case Primitive.BYTE:
                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitCompareLessOrEquals();
                                break;

                            case Primitive.LONG:
                                assembler.EmitCompareLessOrEquals64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFCompareLessOrEquals();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFCompareLessOrEquals64();
                                break;
                        }

                        castTempVar?.Release();

                        result = PrimitiveType.BOOL;
                    }
                    else if (PrimitiveType.IsPrimitiveChar(leftType) && PrimitiveType.IsPrimitiveChar(rightType))
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitCompareLessOrEquals();

                        result = PrimitiveType.BOOL;
                    }
                    else if (leftType is PointerType && rightType is PointerType)
                    {
                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitComparePointerLessOrEquals();
                        result = PrimitiveType.BOOL;
                    }
                    else
                    {
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    break;
                }

                case BinaryOperation.BITWISE_OR:
                {
                    if (!PrimitiveType.IsPrimitiveInteger(leftType) || !PrimitiveType.IsPrimitiveInteger(rightType))
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");

                    PrimitiveType resultType = null;
                    Variable castTempVar = null;
                    if (leftType.CoerceWith(rightType, false))
                    {
                        resultType = (PrimitiveType) rightType;
                        CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                    }
                    else if (rightType.CoerceWith(leftType, false))
                    {
                        resultType = (PrimitiveType) leftType;
                        CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                    }

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);

                    switch (resultType.Primitive)
                    {
                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitOr();
                            break;

                        case Primitive.LONG:
                            assembler.EmitOr64();
                            break;
                    }

                    castTempVar?.Release();

                    result = resultType.Size < sizeof(int) ? PrimitiveType.INT : resultType;
                    break;
                }

                case BinaryOperation.BITWISE_XOR:
                {
                    if (!PrimitiveType.IsPrimitiveInteger(leftType) || !PrimitiveType.IsPrimitiveInteger(rightType))
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");

                    PrimitiveType resultType = null;
                    Variable castTempVar = null;
                    if (leftType.CoerceWith(rightType, false))
                    {
                        resultType = (PrimitiveType) rightType;
                        CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                    }
                    else if (rightType.CoerceWith(leftType, false))
                    {
                        resultType = (PrimitiveType) leftType;
                        CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                    }

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);

                    switch (resultType.Primitive)
                    {
                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitXor();
                            break;

                        case Primitive.LONG:
                            assembler.EmitXor64();
                            break;
                    }

                    castTempVar?.Release();

                    result = resultType.Size < sizeof(int) ? PrimitiveType.INT : resultType;
                    break;
                }

                case BinaryOperation.BITWISE_AND:
                {
                    if (!PrimitiveType.IsPrimitiveInteger(leftType) || !PrimitiveType.IsPrimitiveInteger(rightType))
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");

                    PrimitiveType resultType = null;
                    Variable castTempVar = null;
                    if (leftType.CoerceWith(rightType, false))
                    {
                        resultType = (PrimitiveType) rightType;
                        CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                    }
                    else if (rightType.CoerceWith(leftType, false))
                    {
                        resultType = (PrimitiveType) leftType;
                        CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                    }

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);

                    switch (resultType.Primitive)
                    {
                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitAnd();
                            break;

                        case Primitive.LONG:
                            assembler.EmitAnd64();
                            break;
                    }

                    castTempVar?.Release();

                    result = resultType.Size < sizeof(int) ? PrimitiveType.INT : resultType;
                    break;
                }

                case BinaryOperation.ADD:
                {
                    if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                    {
                        PrimitiveType resultType = null;
                        Variable castTempVar = null;
                        if (leftType.CoerceWith(rightType, false))
                        {
                            resultType = (PrimitiveType) rightType;
                            CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                        }
                        else if (rightType.CoerceWith(leftType, false))
                        {
                            resultType = (PrimitiveType) leftType;
                            CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                        }

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        switch (resultType.Primitive)
                        {
                            case Primitive.BYTE:
                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitAdd();
                                break;

                            case Primitive.LONG:
                                assembler.EmitAdd64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFAdd();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFAdd64();
                                break;
                        }

                        castTempVar?.Release();

                        result = resultType.Size < sizeof(int) ? PrimitiveType.INT : resultType;
                    }
                    else
                    {
                        switch (leftType)
                        {
                            case PrimitiveType p:
                            {
                                if (rightType is not PointerType and not StringType)
                                    throw new CompilerException(rightOperand.Interval, $"Operação inválida com o tipo '{rightType}'.");

                                if (rightType is PointerType rptr && !PrimitiveType.IsPrimitiveChar(rptr.Type))
                                    throw new CompilerException(rightOperand.Interval, $"Concatenação de strings não pode ser feita com um ponteiro do tipo '{rptr.Type}'.");

                                Assembler beforeLeftAssembler = new();
                                Assembler beforeRightAssembler = new();

                                var leftTempVar = context.AcquireTemporaryVariable(StringType.STRING, leftOperand.Interval);
                                beforeLeftAssembler.EmitLoadLocalHostAddress(leftTempVar.Offset);

                                Function func = p.Primitive switch
                                {
                                    Primitive.BOOL => unitySystem.FindFunction("BoolParaTexto"),
                                    Primitive.CHAR => unitySystem.FindFunction("CharParaTexto"),
                                    Primitive.BYTE or Primitive.SHORT or Primitive.INT => unitySystem.FindFunction("IntParaTexto"),
                                    Primitive.LONG => unitySystem.FindFunction("LongParaTexto"),
                                    Primitive.FLOAT => unitySystem.FindFunction("FloatParaTexto"),
                                    Primitive.DOUBLE => unitySystem.FindFunction("DoubleParaTexto"),
                                    _ => throw new CompilerException(leftOperand.Interval, "Operando inválido."),
                                };
                                int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                                leftAssembler.EmitExternCall(index);
                                leftAssembler.EmitLoadLocalPtr(leftTempVar.Offset);

                                CompileCast(context, rightAssembler, beforeRightAssembler, rightType, StringType.STRING, false, rightOperand.Interval, out var rightCastTempVar);

                                tempVar = context.AcquireTemporaryVariable(StringType.STRING, expression.Interval);
                                assembler.EmitLoadLocalHostAddress(tempVar.Offset);

                                assembler.Emit(beforeLeftAssembler);
                                assembler.Emit(leftAssembler);
                                assembler.Emit(beforeRightAssembler);
                                assembler.Emit(rightAssembler);

                                func = unitySystem.FindFunction("ConcatenaTextos2");
                                index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                                assembler.EmitExternCall(index);

                                leftTempVar?.Release();
                                rightCastTempVar?.Release();

                                assembler.EmitLoadLocalPtr(tempVar.Offset);

                                result = StringType.STRING;
                                break;
                            }

                            case PointerType ptr:
                            {
                                if (PrimitiveType.IsPrimitiveVoid(ptr.Type))
                                    throw new CompilerException(leftOperand.Interval, "Operação aritimética com ponteiros não permitida para ponteiros do tipo void.");

                                switch (rightType)
                                {
                                    case PrimitiveType rp: // Concatenação de uma string com um tipo primitivo, isso exige a conversão do primitivo para string para então realizar a concatenação
                                    {
                                        if (PrimitiveType.IsPrimitiveChar(ptr.Type))
                                        {
                                            Assembler beforeLeftAssembler = new();
                                            Assembler beforeRightAssembler = new();

                                            CompileCast(context, leftAssembler, beforeLeftAssembler, leftType, StringType.STRING, false, leftOperand.Interval, out var leftCastTempVar);

                                            var rightTempVar = context.AcquireTemporaryVariable(StringType.STRING, rightOperand.Interval);
                                            beforeRightAssembler.EmitLoadLocalHostAddress(rightTempVar.Offset);

                                            Function func = rp.Primitive switch
                                            {
                                                Primitive.BOOL => unitySystem.FindFunction("BoolParaTexto"),
                                                Primitive.CHAR => unitySystem.FindFunction("CharParaTexto"),
                                                Primitive.BYTE or Primitive.SHORT or Primitive.INT => unitySystem.FindFunction("IntParaTexto"),
                                                Primitive.LONG => unitySystem.FindFunction("LongParaTexto"),
                                                Primitive.FLOAT => unitySystem.FindFunction("FloatParaTexto"),
                                                Primitive.DOUBLE => unitySystem.FindFunction("DoubleParaTexto"),
                                                _ => throw new CompilerException(rightOperand.Interval, "Operando inválido."),
                                            };
                                            int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                                            rightAssembler.EmitExternCall(index);
                                            rightAssembler.EmitLoadLocalPtr(rightTempVar.Offset);

                                            tempVar = context.AcquireTemporaryVariable(StringType.STRING, expression.Interval);
                                            assembler.EmitLoadLocalHostAddress(tempVar.Offset);

                                            assembler.Emit(beforeLeftAssembler);
                                            assembler.Emit(leftAssembler);
                                            assembler.Emit(beforeRightAssembler);
                                            assembler.Emit(rightAssembler);

                                            func = unitySystem.FindFunction("ConcatenaTextos2");
                                            index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                                            assembler.EmitExternCall(index);

                                            leftCastTempVar?.Release();
                                            rightTempVar?.Release();

                                            assembler.EmitLoadLocalPtr(tempVar.Offset);

                                            result = StringType.STRING;
                                        }
                                        else
                                        {
                                            if (!PrimitiveType.IsPrimitiveInteger(rp))
                                                throw new CompilerException(rightOperand.Interval, $"Adição de ponteiros inválida com deslocamento do tipo '{rp}'.");

                                            assembler.Emit(leftAssembler);
                                            assembler.Emit(rightAssembler);
                                            assembler.EmitLoadConst(ptr.Type.Size);
                                            assembler.EmitMul();

                                            if (rp.Primitive == Primitive.INT)
                                                assembler.EmitPtrAdd();
                                            else
                                                assembler.EmitPtrAdd64();

                                            result = leftType;
                                        }

                                        break;
                                    }

                                    case PointerType rptr:
                                    {
                                        if (!PrimitiveType.IsPrimitiveChar(ptr.Type) && !PrimitiveType.IsPrimitiveChar(rptr.Type))
                                            throw new CompilerException(rightOperand.Interval, $"Adição de ponteiros inválida entre ponteiros dos tipos '{ptr.Type}' e '{rptr.Type}'.");

                                        Assembler beforeLeftAssembler = new();
                                        Assembler beforeRightAssembler = new();

                                        CompileCast(context, leftAssembler, beforeLeftAssembler, leftType, StringType.STRING, false, leftOperand.Interval, out var leftCastTempVar);
                                        CompileCast(context, rightAssembler, beforeRightAssembler, rightType, StringType.STRING, false, rightOperand.Interval, out var rightCastTempVar);

                                        tempVar = context.AcquireTemporaryVariable(StringType.STRING, expression.Interval);
                                        assembler.EmitLoadLocalHostAddress(tempVar.Offset);

                                        assembler.Emit(beforeLeftAssembler);
                                        assembler.Emit(leftAssembler);
                                        assembler.Emit(beforeRightAssembler);
                                        assembler.Emit(rightAssembler);

                                        var func = unitySystem.FindFunction("ConcatenaTextos2");
                                        int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                                        assembler.EmitExternCall(index);

                                        leftCastTempVar?.Release();
                                        rightCastTempVar?.Release();

                                        assembler.EmitLoadLocalPtr(tempVar.Offset);

                                        result = StringType.STRING;
                                        break;
                                    }

                                    case StringType:
                                    {
                                        Assembler beforeLeftAssembler = new();

                                        CompileCast(context, leftAssembler, beforeLeftAssembler, leftType, rightType, false, leftOperand.Interval, out var leftCastTempVar);

                                        tempVar = context.AcquireTemporaryVariable(StringType.STRING, expression.Interval);
                                        assembler.EmitLoadLocalHostAddress(tempVar.Offset);

                                        assembler.Emit(beforeLeftAssembler);
                                        assembler.Emit(leftAssembler);
                                        assembler.Emit(rightAssembler);

                                        var func = unitySystem.FindFunction("ConcatenaTextos2");
                                        int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                                        assembler.EmitExternCall(index);

                                        leftCastTempVar?.Release();

                                        assembler.EmitLoadLocalPtr(tempVar.Offset);

                                        result = rightType;
                                        break;
                                    }

                                    default:
                                        throw new CompilerException(rightOperand.Interval, "Operando direito de uma operação de deslocamento de ponteiros deve ser um inteiro.");
                                }

                                break;
                            }

                            case StringType:
                            {
                                Assembler beforeLeftAssembler = new();
                                Assembler beforeRightAssembler = new();
                                Variable rightCastTempVar = null;

                                CompileCast(context, leftAssembler, beforeLeftAssembler, leftType, StringType.STRING, false, leftOperand.Interval, out var leftCastTempVar);

                                switch (rightType)
                                {
                                    case PrimitiveType rp: // Concatenação de uma string com um tipo primitivo, isso exige a conversão do primitivo para string para então realizar a concatenação
                                    {
                                        var rightTempVar = context.AcquireTemporaryVariable(StringType.STRING, rightOperand.Interval);
                                        beforeRightAssembler.EmitLoadLocalHostAddress(rightTempVar.Offset);

                                        var func = rp.Primitive switch
                                        {
                                            Primitive.BOOL => unitySystem.FindFunction("BoolParaTexto"),
                                            Primitive.CHAR => unitySystem.FindFunction("CharParaTexto"),
                                            Primitive.BYTE or Primitive.SHORT or Primitive.INT => unitySystem.FindFunction("IntParaTexto"),
                                            Primitive.LONG => unitySystem.FindFunction("LongParaTexto"),
                                            Primitive.FLOAT => unitySystem.FindFunction("FloatParaTexto"),
                                            Primitive.DOUBLE => unitySystem.FindFunction("DoubleParaTexto"),
                                            _ => throw new CompilerException(rightOperand.Interval, "Operando inválido."),
                                        };
                                        int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                                        rightAssembler.EmitExternCall(index);
                                        rightAssembler.EmitLoadLocalPtr(rightTempVar.Offset);
                                        break;
                                    }

                                    case PointerType rptr:
                                    {
                                        if (!PrimitiveType.IsPrimitiveChar(rptr.Type))
                                            throw new CompilerException(rightOperand.Interval, $"Concatenação de strings não pode ser feita com um ponteiro do tipo '{rptr.Type}'.");

                                        CompileCast(context, rightAssembler, beforeRightAssembler, rightType, leftType, false, rightOperand.Interval, out rightCastTempVar);
                                        break;
                                    }

                                    default:
                                        throw new CompilerException(rightOperand.Interval, $"Concatenação de strings não pode ser feita com o tipo '{rightType}'.");
                                }

                                tempVar = context.AcquireTemporaryVariable(StringType.STRING, expression.Interval);
                                assembler.EmitLoadLocalHostAddress(tempVar.Offset);

                                assembler.Emit(beforeLeftAssembler);
                                assembler.Emit(leftAssembler);
                                assembler.Emit(beforeRightAssembler);
                                assembler.Emit(rightAssembler);

                                rightCastTempVar?.Release();

                                var concat = unitySystem.FindFunction("ConcatenaTextos2");
                                int concatIndex = GetOrAddExternalFunction(concat.Name, concat.ParameterSize);
                                assembler.EmitExternCall(concatIndex);

                                assembler.EmitLoadLocalPtr(tempVar.Offset);

                                result = leftType;
                                break;
                            }

                            default:
                            {
                                if (rightType is not StringType)
                                    throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");

                                Assembler beforeLefttAssembler = new();
                                Variable leftCastTempVar = null;
                                if (leftType is PointerType lptr)
                                {
                                    if (!PrimitiveType.IsPrimitiveChar(lptr.Type))
                                        throw new CompilerException(rightOperand.Interval, $"Concatenação de strings não pode ser feita com um ponteiro do tipo '{lptr.Type}'.");

                                    CompileCast(context, leftAssembler, beforeLefttAssembler, leftType, rightType, false, leftOperand.Interval, out leftCastTempVar);
                                }
                                else if (leftType is not StringType)
                                {
                                    throw new CompilerException(rightOperand.Interval, $"Concatenação de strings não pode ser feita com o tipo '{leftType}'.");
                                }

                                tempVar = context.AcquireTemporaryVariable(StringType.STRING, expression.Interval);
                                assembler.EmitLoadLocalHostAddress(tempVar.Offset);

                                assembler.Emit(beforeLefttAssembler);
                                assembler.Emit(leftAssembler);
                                assembler.Emit(rightAssembler);

                                leftCastTempVar?.Release();

                                var func = unitySystem.FindFunction("ConcatenaTextos2");
                                int index = GetOrAddExternalFunction(func.Name, func.ParameterSize);
                                assembler.EmitExternCall(index);

                                assembler.EmitLoadLocalPtr(tempVar.Offset);

                                result = rightType;
                                break;
                            }
                        }
                    }

                    break;
                }

                case BinaryOperation.SUB:
                {
                    if (PrimitiveType.IsPrimitiveNumber(leftType) && PrimitiveType.IsPrimitiveNumber(rightType))
                    {
                        PrimitiveType resultType = null;
                        Variable castTempVar = null;
                        if (leftType.CoerceWith(rightType, false))
                        {
                            resultType = (PrimitiveType) rightType;
                            CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                        }
                        else if (rightType.CoerceWith(leftType, false))
                        {
                            resultType = (PrimitiveType) leftType;
                            CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                        }

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);

                        switch (resultType.Primitive)
                        {
                            case Primitive.BYTE:
                            case Primitive.SHORT:
                            case Primitive.INT:
                                assembler.EmitSub();
                                break;

                            case Primitive.LONG:
                                assembler.EmitSub64();
                                break;

                            case Primitive.FLOAT:
                                assembler.EmitFSub();
                                break;

                            case Primitive.DOUBLE:
                                assembler.EmitFSub64();
                                break;
                        }

                        castTempVar?.Release();

                        result = resultType.Size < sizeof(int) ? PrimitiveType.INT : resultType;
                    }
                    else if (leftType is PointerType ptr)
                    {
                        if (rightType is not PrimitiveType rp || !PrimitiveType.IsPrimitiveInteger(rp))
                            throw new CompilerException(rightOperand.Interval, "Operando direito de uma operação de subtração envolvendo ponteiros deve ser um inteiro ou um ponteiro.");

                        if (PrimitiveType.IsPrimitiveVoid(ptr.Type))
                            throw new CompilerException(leftOperand.Interval, "Operação aritimética com ponteiros não permitida para ponteiros do tipo void.");

                        int size = ptr.Type.Size;

                        assembler.Emit(leftAssembler);
                        assembler.Emit(rightAssembler);
                        assembler.EmitLoadConst(size);
                        assembler.EmitMul();

                        if (rp.Primitive == Primitive.INT)
                            assembler.EmitPtrSub();
                        else
                            assembler.EmitPtrSub64();

                        result = leftType;
                    }
                    else
                    {
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    break;
                }

                case BinaryOperation.MUL:
                {
                    if (!PrimitiveType.IsPrimitiveNumber(leftType) || !PrimitiveType.IsPrimitiveNumber(rightType))
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");

                    PrimitiveType resultType = null;
                    Variable castTempVar = null;
                    if (leftType.CoerceWith(rightType, false))
                    {
                        resultType = (PrimitiveType) rightType;
                        CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                    }
                    else if (rightType.CoerceWith(leftType, false))
                    {
                        resultType = (PrimitiveType) leftType;
                        CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                    }

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);

                    switch (resultType.Primitive)
                    {
                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitMul();
                            break;

                        case Primitive.LONG:
                            assembler.EmitMul64();
                            break;

                        case Primitive.FLOAT:
                            assembler.EmitFMul();
                            break;

                        case Primitive.DOUBLE:
                            assembler.EmitFMul64();
                            break;

                        default:
                            throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    castTempVar?.Release();

                    result = resultType.Size < sizeof(int) ? PrimitiveType.INT : resultType;
                    break;
                }

                case BinaryOperation.DIV:
                {
                    if (!PrimitiveType.IsPrimitiveNumber(leftType) || !PrimitiveType.IsPrimitiveNumber(rightType))
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");

                    PrimitiveType resultType = null;
                    Variable castTempVar = null;
                    if (leftType.CoerceWith(rightType, false))
                    {
                        resultType = (PrimitiveType) rightType;
                        CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                    }
                    else if (rightType.CoerceWith(leftType, false))
                    {
                        resultType = (PrimitiveType) leftType;
                        CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                    }

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);

                    switch (resultType.Primitive)
                    {
                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitDiv();
                            break;

                        case Primitive.LONG:
                            assembler.EmitDiv64();
                            break;

                        case Primitive.FLOAT:
                            assembler.EmitFDiv();
                            break;

                        case Primitive.DOUBLE:
                            assembler.EmitFDiv64();
                            break;

                        default:
                            throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    castTempVar?.Release();

                    result = resultType.Size < sizeof(int) ? PrimitiveType.INT : resultType;
                    break;
                }

                case BinaryOperation.MOD:
                {
                    if (!PrimitiveType.IsPrimitiveInteger(leftType) || !PrimitiveType.IsPrimitiveInteger(rightType))
                        throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");

                    PrimitiveType resultType = null;
                    Variable castTempVar = null;
                    if (leftType.CoerceWith(rightType, false))
                    {
                        resultType = (PrimitiveType) rightType;
                        CompileCast(context, leftAssembler, assembler, leftType, rightType, false, leftOperand.Interval, out castTempVar);
                    }
                    else if (rightType.CoerceWith(leftType, false))
                    {
                        resultType = (PrimitiveType) leftType;
                        CompileCast(context, rightAssembler, assembler, rightType, leftType, false, rightOperand.Interval, out castTempVar);
                    }

                    assembler.Emit(leftAssembler);
                    assembler.Emit(rightAssembler);

                    switch (resultType.Primitive)
                    {
                        case Primitive.BYTE:
                        case Primitive.SHORT:
                        case Primitive.INT:
                            assembler.EmitMod();
                            break;

                        case Primitive.LONG:
                            assembler.EmitMod64();
                            break;

                        default:
                            throw new CompilerException(expression.Interval, $"Tipos imcompatíveis: '{leftType}' e '{rightType}'.");
                    }

                    castTempVar?.Release();

                    result = resultType.Size < sizeof(int) ? PrimitiveType.INT : resultType;
                    break;
                }

                default:
                    throw new CompilerException(expression.Interval, $"Operador '{expression.Operation}' desconhecido.");
            }
        }

        return result;
    }
}