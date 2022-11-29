programa TesteString
{
	var s:texto;

	{
		/*
			Saída esperada:
			
			123
			123456
			789123456
			abcdef
			123abcdef
			193
			123cbcdef
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
	}
}