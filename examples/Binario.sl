programa Binario
{
	{
		declare n:int;
		leia n;
		
		declare i:int;
		declare b:bool = falso;
		para (i = 31; i >= 0; i = i - 1)
		{
			se ((n & (1 << i)) != 0)
			{
				b = verdadeiro;
				escreva 1;
			}
			senão se (b)
				escreva 0;
		}
		
		se (!b)
			escreva 0;
	}
}