programa TesteUnidades;

usando Sorts;
	
{
	// Teste da unidade padrão System:
		
	var str:texto = "abcdefgh"; // string dinâmica contada por referência
	str = str + 1234567890; // concatenação de string com tipos numéricos
	escrevaln "\"abcdefgh\" + \"1234567890\" = \"", str, '"';
		
	escrevaln "Tamanho do texto \"", str, "\" = ", str.tamanho;
		
	var str2:char[16]; // string estática
	CopiaString("4567", str2);
	var str2Int:int;
	StringParaInt(str2, str2Int);
	escrevaln "StringParaInt(\"", str2, "\")=", str2Int;
		
	// saída esperada:
	// "abcdefgh" + "1234567890" = "abcdefgh1234567890"
	// Tamanho do texto "abcdefgh1234567890" = 18;
	// StringParaInt("4567")=4567
		
	// Teste da unidade Sorts:
	// teste de entrada: {9, -2, 6, 3)
		
	var a:int[4];

	para (var i:int = 0; i < 4; i++)
	{
		escrevaln "Digite um número inteiro para a[", i, "]";
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