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
programa Programa1
{
	var a:long;

	estrutura Vetor
	{
		x:real;
		y:real;
	}
	
	var v:Vetor;

	função f(x:real):real
	{
		retorne x * x;
	}
	
	função g(x:int, y:int):int
	{
		retorne x + y;
	}
	
	{
		var x:int;
		var y:int;
		var z:real;
		
		leia x;
		leia y;
		leia z;
		
		var w:int = g(x, y);
		var t:real = f(z);
		
		escrevaln "w=", w;
		escrevaln "t=", t;
		
		a = 9L;
		v.x = 8;
		v.y = 9.9E4;
		
		escreva "a=", a, "\nv.x=", v.x, "\nv.y=", v.y;
	}
}
```

A compilação desse código utilizando o compilador simples deste projeto irá gerar um código intermediário que utiliza um conjunto de instruções do tipo bytecode, no qual cada opcode possui apenas um byte. Estas instruções por sua vez são interpretadas por uma máquina virtual quando o programa compilado é executado.

Alguns recursos ainda serão inseridos em breve à linguagem:

- Suporte a alocação dinâmica.

- Suporte a arrays dinâmicos.

- Suporte a strings com contagem por referência, da mesma forma como no Delphi.

- Suporte a bibliotecas, incluindo uma biblioteca padrão contendo todas as funções essenciais.
