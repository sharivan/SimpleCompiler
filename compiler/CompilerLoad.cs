using compiler.types;
using assembler;

namespace compiler
{
    public partial class Compiler
    {
        private void CompileLoadStack(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            switch (type)
            {
                case PrimitiveType p:
                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                            assembler.EmitLoadStack8();
                            return;

                        case Primitive.CHAR:
                        case Primitive.SHORT:
                            assembler.EmitLoadStack16();
                            return;

                        case Primitive.INT:
                        case Primitive.FLOAT:
                            assembler.EmitLoadStack32();
                            return;

                        case Primitive.LONG:
                        case Primitive.DOUBLE:
                            assembler.EmitLoadStack64();
                            return;
                    }

                    break;

                case StructType:
                    // TODO Implementar
                    throw new CompilerException(interval, $"Operação não implementada para o tipo '{type}'.");

                case ArrayType:
                    // TODO Implementar
                    throw new CompilerException(interval, $"Operação não implementada para o tipo '{type}'.");

                case PointerType or StringType:
                    assembler.EmitLoadStackPtr();
                    return;
            }

            throw new CompilerException(interval, $"Tipo desconhecido: '{type}'.");
        }

        private void CompileLoadPointer(Assembler assembler, AbstractType type, SourceInterval interval)
        {
            switch (type)
            {
                case PrimitiveType p:
                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                            assembler.EmitLoadPointer8();
                            return;

                        case Primitive.CHAR:
                        case Primitive.SHORT:
                            assembler.EmitLoadPointer16();
                            return;

                        case Primitive.INT:
                        case Primitive.FLOAT:
                            assembler.EmitLoadPointer32();
                            return;

                        case Primitive.LONG:
                        case Primitive.DOUBLE:
                            assembler.EmitLoadPointer64();
                            return;
                    }

                    break;

                case StructType:
                    // TODO Implementar
                    throw new CompilerException(interval, $"Operação não implementada para o tipo '{type}'.");

                case ArrayType:
                    // TODO Implementar
                    throw new CompilerException(interval, $"Operação não implementada para o tipo '{type}'.");

                case PointerType or StringType:
                    assembler.EmitLoadPointerPtr();
                    return;
            }

            throw new CompilerException(interval, $"Tipo desconhecido: '{type}'.");
        }

        private void CompileLoad(Assembler assembler, Variable loadVar, SourceInterval interval)
        {
            AbstractType type = loadVar.Type;
            switch (type)
            {
                case PrimitiveType p:
                {
                    switch (p.Primitive)
                    {
                        case Primitive.BOOL:
                        case Primitive.BYTE:
                            if (loadVar is GlobalVariable)
                                assembler.EmitLoadGlobal8(unity.GlobalStartOffset + loadVar.Offset);
                            else if (loadVar is Parameter param && param.ByRef)
                            {
                                assembler.EmitLoadLocalPtr(loadVar.Offset);
                                assembler.EmitLoadPointer8();
                            }
                            else
                                assembler.EmitLoadLocal8(loadVar.Offset);

                            return;

                        case Primitive.CHAR:
                        case Primitive.SHORT:
                            if (loadVar is GlobalVariable)
                                assembler.EmitLoadGlobal16(unity.GlobalStartOffset + loadVar.Offset);
                            else if (loadVar is Parameter param && param.ByRef)
                            {
                                assembler.EmitLoadLocalPtr(loadVar.Offset);
                                assembler.EmitLoadPointer16();
                            }
                            else
                                assembler.EmitLoadLocal16(loadVar.Offset);

                            return;

                        case Primitive.INT:
                        case Primitive.FLOAT:
                            if (loadVar is GlobalVariable)
                                assembler.EmitLoadGlobal32(unity.GlobalStartOffset + loadVar.Offset);
                            else if (loadVar is Parameter param && param.ByRef)
                            {
                                assembler.EmitLoadLocalPtr(loadVar.Offset);
                                assembler.EmitLoadPointer32();
                            }
                            else
                                assembler.EmitLoadLocal32(loadVar.Offset);

                            return;

                        case Primitive.LONG:
                        case Primitive.DOUBLE:
                            if (loadVar is GlobalVariable)
                                assembler.EmitLoadGlobal64(unity.GlobalStartOffset + loadVar.Offset);
                            else if (loadVar is Parameter param && param.ByRef)
                            {
                                assembler.EmitLoadLocalPtr(loadVar.Offset);
                                assembler.EmitLoadPointer64();
                            }
                            else
                                assembler.EmitLoadLocal64(loadVar.Offset);

                            return;
                    }

                    break;
                }

                case PointerType or StringType:
                {
                    if (loadVar is GlobalVariable)
                        assembler.EmitLoadGlobalPtr(unity.GlobalStartOffset + loadVar.Offset);
                    else if (loadVar is Parameter param && param.ByRef)
                    {
                        assembler.EmitLoadLocalPtr(loadVar.Offset);
                        assembler.EmitLoadPointerPtr();
                    }
                    else
                        assembler.EmitLoadLocalPtr(loadVar.Offset);

                    return;
                }
            }

            throw new CompilerException(interval, $"Tipo desconhecido: '{type}'.");
        }
    }
}
