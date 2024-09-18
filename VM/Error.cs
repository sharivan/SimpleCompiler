namespace SimpleCompiler.VM;

public enum Error
{
    NONE = 0,
    ARITHMETIC_OVERFLOW,
    DIVISION_BY_ZERO,
    STACK_OVERFLOW,
    OUT_OF_MEMORY,
    INVALID_CONVERSION
}