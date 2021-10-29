# SimpleCompiler
Um compilador simples que foi desenvolvido em um único dia, durante uma live de devstream em meu canal na twitch no dia 28/10/2021.

IDE de desenvolvimento: Microsoft Visual Studio Community 2019
Linguagem: C#

Uma linguagem gramática nova foi criada para ser aceita por este compilador, a sintaxe de tal gramática é bastante semelhante com C e possui declarações parecidas com Action Script 2.0. Diversas palavras chaves da linguagem C foram traduzidas para o portugues e inseridas como palavras chave para a gramática desta nova linguagem.

A seguir, temos um exemplo simples de um programa escrito nessa linguagem:

programa Programa1
{
	estrutura Vetor
	{
		x:real;
		y:real;
	}

	função f(x:real):real
	{
		retorne x * x;
	}
	
	função g(x:int, y:int):int
	{
		retorne x + y;
	}
	
	{
		declare x:int = 1;
		declare y:int = 2;
		declare z:real = 3.3;
		
		declare w:int = g(x, y);
		declare t:real = f(z);
		
		escreva w;
		escreva t;
	}
}

A compilação desse código utilizando o compilador simples desse projeto irá gerar um código intermediário que utiliza um conjunto de instruções do tipo bytecode, no qual cada opcode possui apenas um byte.

Ainda existem diversos bugs a serem corrigidos nos quais posso destacara alguns:

- O compilador não gera as instruções corretas para atribuição de tipos compatíveis, porém diferentes. Exemplo, se x for float, a instrução x = 1 não fará a conversão do inteiro 1 para ponto flutuante, gerando um valor incorreto para a atribuição em x.

- Algumas estruturas de fluxo ainda não foram implementadas, como os ifs e estrutura de loopings.

- Ainda não existe suporte total a valores booleanos, embora exista o tipo bool e as palavras chaves 'verdadeiro' e 'falso'.

- A instrução de leitura não faz nada além de atribuir valores nulos às variáveis passadas como parâmetro.

- Embora seja possívei declarar estruturas (structs), o compilador ainda não é capaz de tratar expressões envolvendo acesso de membros de estruturas e nem atribuições de valores para membros de estruturas.

Alguns recursos ainda serão inseridos em breve à linguagem:

- Suporte a ponteiros, incluindo o suporte ao literal null.

- Suporte a alocação dinâmica.

- Suporte a arrays estáticos e dinâmicos.

- Suporte a strings com contagem por referência, da mesma forma como no Delphi.

- Suporte a bibliotecas, incluindo uma biblioteca padrão contendo todas as funções essenciais.

- Suporte a literais dos tipos byte, char, short, long e float.

- Suporte a literais do tipo string.
