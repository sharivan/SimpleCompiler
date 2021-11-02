programa NumerosPerfeitos
{
	função É_Divisor(a:int, b:int):bool
	{
		retorne b % a == 0;
	}

	função É_Perfeito(n:int):bool
	{
		declare soma:int = 0;
		declare i:int;
		para (i = 1; i < n; i = i + 1)
		{
			se (É_Divisor(i, n))
				soma += i;
		}
		
		retorne soma == n;
	}

	{
		declare n:int;
		leia n;
		se (É_Perfeito(n))
			escreva 1;
		senão
			escreva 0;
	}
}