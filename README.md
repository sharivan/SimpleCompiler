# SimpleCompiler
Um compilador simples que teve seu desenvolvimento iniciado a partir de 28/10/2021 em uma série de lives em meu canal na [twitch](https://www.twitch.tv/sharivanx), mas depois foi continuado em off. A reprise das lives podem ser conferidas nos seguintes links:

- Parte 1: https://www.youtube.com/watch?v=27Sksfc3psA
- Parte 2: https://www.youtube.com/watch?v=_osQMFl4pcw
- Parte 3: https://www.youtube.com/watch?v=zRAQ2fU2WBg
- Parte 4: https://www.youtube.com/watch?v=bxeUQ7rK3OU

Para o desenvolvimento deste projeto eu utilizei os seguintes recursos:

- IDE de desenvolvimento: Microsoft Visual Studio Community 2019
- Linguagem: C#

Uma nova gramática foi criada para ser aceita por este compilador, a sintaxe de tal gramática é bastante semelhante com C e possui declarações parecidas com Action Script 2.0. Diversas palavras chaves da linguagem C foram traduzidas para o português e inseridas como palavras chave para a gramática desta nova linguagem.

A seguir, temos um exemplo simples de um programa escrito nessa linguagem:

```c++
programa TesteUnidades
{
	usando Sorts;
	
	{
		// Teste da unidade padrão System:
		
		var str:char[256];
		CopiaString("abcdefgh", str);
		ConcatenaStrings(str, str, "1234567890");
		escrevaln "\"abcdefgh\"+\"1234567890\"=\"", str, '"';
		
		escrevaln "ComprimentoString(\"", str, "\")=", ComprimentoString(str);
		
		var str2:char[16];
		CopiaString("4567", str2);
		var str2Int:int;
		StringParaInt(str2, str2Int);
		escrevaln "StringParaInt(\"", str2, "\")=", str2Int;
		
		// saída esperada:
		// "abcdefgh"+"1234567890"="abcdefgh1234567890"
		// ComprimentoString("abcdefgh1234567890")=18;
		// StringParaInt("4567")=4567
		
		// Teste da unidade Sorts:
		// teste de entrada: {9, -2, 6, 3)
		
		var a:int[4];

		para (var i:int = 0; i < 4; i++)
		{
			escrevaln "Digite um númeero inteiro para a[", i, "]";
			leia a[i];
		}
		
		QuickSort(a, 0, 3);
		
		escreva "{", a[0];
		para (var i:int = 1; i < 4; i++)
			escreva ", ", a[i];
			
		escrevaln "}";
		
		// saída esperada:
		// {-2, 3, 6, 9)
	}
}
```

A compilação desse código utilizando o compilador simples deste projeto irá gerar um código intermediário que utiliza um conjunto de instruções do tipo bytecode, no qual cada opcode possui apenas um byte. Estas instruções por sua vez são interpretadas por uma máquina virtual quando o programa compilado é executado.

A depuração pode ser feita pelo próprio programa, mas somente sobre o código intermediário. Futuramente será adicionado suporte total para a depuração do código fonte.

Alguns recursos ainda serão inseridos em breve à linguagem:

- Suporte a alocação dinâmica.

- Suporte a arrays dinâmicos.

- Suporte a strings com contagem por referência, da mesma forma como no Delphi.

- Depurador completo.
