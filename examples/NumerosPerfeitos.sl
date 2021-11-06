programa NumerosPerfeitos
{
	função É_Divisor(a:int, b:int):bool
	{
		retorne b % a == 0;
	}

	função É_Perfeito(n:int):bool
	{
		var soma:int = 0;
		var i:int;
		para (i = 1; i < n; i = i + 1)
		{
			se (É_Divisor(i, n))
				soma += i;
		}
		
		retorne soma == n;
	}

	{
		var n:int;
		leia n;
		se (É_Perfeito(n))
			escreva 1;
		senão
			escreva 0;
	}
}