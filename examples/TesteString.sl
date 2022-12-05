programa TesteString
{
	var s:texto;

	{
		/*
			Saída esperada (excluindo o último escrevaln):
			
			123
			123456
			789123456
			abcdef
			123abcdef
			193
			123cbcdef
			9
			9
		*/
	
		var s2: texto;
		s2 = "123";
		escrevaln s2;
		
		s = s2 + "456";
		escrevaln s;
		
		s = "789" + s;
		escrevaln s;
		
		var s3:texto = "abc" + "def";
		escrevaln s3;
		
		s = s2 + s3;
		escrevaln s;
		
		s2[1] = '9';
		escrevaln s2;
		
		s[3] = s3[2];
		escrevaln s;

		escrevaln s.tamanho;

		var l:int = s2.tamanho + s3.tamanho;
		escrevaln l;

		escreva "Digite o valor de s: ";
		leia s;

		escreva "Digite o valor de s2: ";
		leia s2;

		escreva "Digite o valor de s3: ";
		leia s3;

		s += s2 + s3;
		escrevaln s;
	}
}