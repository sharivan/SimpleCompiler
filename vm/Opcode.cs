namespace vm;

public enum Opcode
{
    NOP, // nenhuma operação (no operation)
    LC8, // carrega uma constante de 8 bits para o topo da pilha
    LC16, // carrega uma constante de 16 bits para o topo da pilha
    LC32, // carrega uma constante de 32 bits para o topo da pilha
    LC64, // carrega uma constante de 64 bits para o topo da pilha
    LCPTR, // carrega uma constante do tipo ponteiro para o topo da pilha
    LIP, // carrega o ip para o topo da pilha
    LSP, // carrega o sp para o topo da pilha
    LBP, // carrega o bp para o topo da pilha
    SIP, // altera o ip
    SSP, // altera o sp
    SBP, // altera o bp
    ADDSP, // adiciona o sp
    SUBSP, // subtrai o sp
    LHA, // carrega o endereço efetivo de um endereço residente armazenado no topo da pilha (load host address)
    LGHA, // carrega o endereço efetivo de uma variável global no topo da pilha (load global host address)
    LLRA, // carrega o endereço de uma variável local no topo da pilha (load local resident address)
    LLHA, // carrega o endereço efetivo de uma variável local no topo da pilha (load local host address)
    RHA, // converte o endereço residente armazenado no topo da pilha para o endereço efetivo (resident to host address)
    HRA, // converte o endereço efetivo armazenado no topo da pilha para o endereço residente (host to residente address)
    LS8, // carrega da pilha (8 bits)
    LS16, // carrega da pilha (16 bits)
    LS32, // carrega da pilha (32 bits)
    LS64, // carrega da pilha (64 bits)
    LSPTR, // carrega da pilha (ponteiro)
    SS8, // store to stack (8 bits)
    SS16, // store to stack (16 bits)
    SS32, // store to stack (32 bits)
    SS64, // store to stack (64 bits)
    SSPTR, // store to stack (ponteiro)
    LG8, // carrega uma variável global (8 bits)
    LG16, // carrega uma variável global (16 bits)
    LG32, // carrega uma variável global (32 bits)
    LG64, // carrega uma variável global (64 bits)
    LGPTR, // carrega uma variável global (ponteiro)
    LL8, // carrega uma variável local (8 bits)
    LL16, // carrega uma variável local (16 bits)
    LL32, // carrega uma variável local (32 bits)
    LL64, // carrega uma variável local (64 bits)
    LLPTR, // carrega uma variável local (ponteiro)
    SG8, // altera uma variável global (8 bits)
    SG16, // altera uma variável global (16 bits)
    SG32, // altera uma variável global (32 bits)
    SG64, // altera uma variável global (64 bits)
    SGPTR, // altera uma variável global (ponteiro)
    SL8, // altera uma variável local (8 bits)
    SL16, // altera uma variável local (16 bits)
    SL32, // altera uma variável local (32 bits)
    SL64, // altera uma variável local (64 bits)
    SLPTR, // altera uma variável local (ponteiro)
    LPTR8, // carrega o valor de um ponteiro de byte
    LPTR16, // carrega o valor de um ponteiro de short
    LPTR32, // carrega o valor de um ponteiro de int
    LPTR64, // carrega o valor de um ponteiro de long
    LPTRPTR, // carrega o valor de um ponteiro de ponteiro
    SPTR8, // altera o conteúdo de um ponteiro de byte
    SPTR16, // altera o conteúdo de um ponteiro de short
    SPTR32, // altera o conteúdo de um ponteiro de int
    SPTR64, // altera o conteúdo de um ponteiro de long
    SPTRPTR, // altera o conteúdo de um ponteiro de ponteiro
    ADD, // soma
    ADD64, // soma (64 bits)
    SUB, // subtração
    SUB64, // subtração (64 bits)
    MUL, // multiplicação
    MUL64, // multiplicação (64 bits)
    DIV, // divisão inteira
    DIV64, // divisão inteira (64 bits)
    MOD, // resto da divisão inteira
    MOD64, // resto da divisão inteira (64 bits)
    NEG, // negação
    NEG64, // negação (64 bits)
    FADD, // adição de floats
    FADD64, // adição de doubles
    FSUB, // subtração de floats
    FSUB64, // subtração de doubles
    FMUL, // multiplicação de floats
    FMUL64, // multiplicação de doubles
    FDIV, // divisão de floats
    FDIV64, // divisão de doubles
    FNEG, // negação de float
    FNEG64, // negação de double
    I32I64, // conversão de int para long
    I64I32, // conversão de long para int
    I32F32, // conversão de int para float
    I32F64, // conversão de int para double
    I64F64, // conversão de long para double
    F32F64, // conversão de float para double
    F32I32, // conversão de float para int
    F32I64, // conveersão de float para long
    F64I64, // conversão de double para long
    F64F32, // conversão de double para float
    I32PTR, // conversão de int para ponteiro
    I64PTR, // conversão de long para ponteiro
    PTRI32, // conversão de ponteiro para int
    PTRI64, // conversão de ponteiro para long
    AND, // operação and bit a bit com operandos do tipo int
    AND64, // operação and bit a bit com operandos do tipo long
    OR, // operação or bit a bit com operandos do tipo int
    OR64, // operação or bit a bit com operandos do tipo long
    XOR, // operação xor bit a bit com operandos do tipo int
    XOR64, // operação xor bit a bit com operandos do tipo long
    NOT, // operação not bit a bit com operando do tipo int
    NOT64, // operação not bit a bit com operando do tipo long
    SHL, // deslocalmento de bits para a esquerda (shift left)
    SHL64, // deslocalmento de bits para a esquerda (64 bits)
    SHR, // deslocalmento de bits para a direita (preservando o sinal) (shift right)
    SHR64, // deslocalmento de bits para a direita (preservando o sinal) (64 bits)
    USHR, // deslocalmento de bits para a direita (não preservando o sinal) (unsigned shift right)
    USHR64, // deslocalmento de bits para a direita (não preservando o sinal) (64 bits)
    PTRADD, // adiciona um ponteiro a um int
    PTRADD64, // adiciona um ponteiro a um long
    PTRSUB, // subtrai um ponteiro de um int
    PTRSUB64, // subtrai um ponteiro de um long
    CMPE, // compare se dois inteiros são iguais
    CMPNE, // compare se dois inteiros são diferentes
    CMPG, // compare se um inteiro é maior que outro inteiro
    CMPGE, // compare se um inteiro é maior ou igual a outro inteiro
    CMPL, // compare se um inteiro é menor que outro inteiro
    CMPLE, // compare se um inteiro é menor ou igual a outro inteiro
    CMPE64, // compare se dois inteiros de 64 bits são iguais
    CMPNE64, // compare se dois inteiros de 64 bits são diferentes
    CMPG64, // compare se um inteiro de 64 bits é maior que outro inteiro de 64 bits
    CMPGE64, // compare se um inteiro de 64 bits é maior ou igual a outro inteiro de 64 bits
    CMPL64, // compare se um inteiro de 64 bits é menor que outro inteiro de 64 bits
    CMPLE64, // compare se um inteiro de 64 bits é menor ou igual a outro inteiro de 64 bits
    CMPEPTR, // compare se dois ponteiros são iguais
    CMPNEPTR, // compare se dois ponteiros são diferentes
    CMPGPTR, // compare se um ponteiros é maior que outro ponteiros
    CMPGEPTR, // compare se um ponteiros é maior ou igual a outro ponteiros
    CMPLPTR, // compare se um ponteiros é menor que outro ponteiros
    CMPLEPTR, // compare se um ponteiros é menor ou igual a outro ponteiros
    FCMPE, // compare se dois floats são iguais
    FCMPNE, // compare se dois floats são diferentes
    FCMPG, // compare se um float é maior que outro float
    FCMPGE, // compare se um float é maior ou igual a outro float
    FCMPL, // compare se um float é menor que outro float
    FCMPLE, // compare se um float é menor ou igual a outro float
    FCMPE64, // compare se dois doubles são iguais
    FCMPNE64, // compare se dois doubles são diferentes
    FCMPG64, // compare se um double é maior que outro double
    FCMPGE64, // compare se um double é maior ou igual a outro double
    FCMPL64, // compare se um double é menor que outro double
    FCMPLE64, // compare se um double é menor ou igual a outro double
    JMP, // desvio incondicional
    JT, // desvio condicional (se verdadeiro)
    JF, // desvio condicional (se falso)
    POP, // decrementa sp em 4
    POP2, // decrementa sp em 8
    POPN, // decrementa sp em n
    DUP, // duplica o topo da pilha
    DUP64, // duplica o topo da pilha (64 bits)
    DUPPTR, // duplica o topo da pilha (ponteiro)
    DUPN, // duplica o topo da pilha n vezes
    DUP64N, // duplica o topo da pilha n vezes (64 bits)
    DUPPTRN, // duplica o topo da pilha n vezes (ponteiro)
    CALL, // chama uma função
    ICALL, // chama uma função de forma indireta
    ECALL, // chama uma função externa
    RET, // retorne de uma função para o seu chamador
    RETN, // retorne de uma função para o seu chamador, subtraindo sp de n
    SCANB, // escaneia um bool da entrada externa
    SCAN8, // escaneia um byte da entrada externa
    SCANC, // escaneia um char da entrada externa
    SCAN16, // escaneia um short da entrada externa
    SCAN32, // escaneia um inteiro da entrada externa
    SCAN64, // escaneia um long da entrada externa		
    FSCAN, // escaneia um float da entrada externa
    FSCAN64, // escaneia um double da entrada externa
    SCANSTR, // escaneia uma string estática na entrada externa
    DSCANSTR, // escaneia uma string dinâmica na entrada externa
    PRINTB, // imprime um bool na saída externa
    PRINTC, // imprime um caractere na saída externa
    PRINT32, // imprime um inteiro na saída externa
    PRINT64, // imprime um long na saída externa
    FPRINT, // imprime um float na saída externa
    FPRINT64, // imprime um double na saída externa
    PRINTSTR, // imprime uma string na saída externa
    HALT, // encerra o programa
    BREAK // pausa a execução do programa (breakpoint)
}
