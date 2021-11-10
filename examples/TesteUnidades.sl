programa TesteUnidades
{
	usando Sorts;
	
	{
		// Teste da unidade padrão System:
		
		var str:char[256];
		CopiaString("abcdefgh", &str);
		ConcatenaStrings(&str, &str, "1234567890");
		escrevaln "\"abcdefgh\"+\"1234567890\"=\"", &str, '"';
		
		escrevaln "ComprimentoString(\"", &str, "\")=", ComprimentoString(&str);
		
		var str2:char[16];
		CopiaString("4567", &str2);
		escrevaln "StringParaInt(\"", &str2, "\")=", StringParaInt(&str2);
		
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