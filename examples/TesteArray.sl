programa TesteArray
{
	{
		declare a:int[10];
		
		leia a[0];
		a[1] = cast<int>(6.38);
		
		declare i:int;
		para (i = 2; i < 10; i++)
			a[i] = i;
		
		escreva a[0];
		escreva a[1];
		
		para (i = 2; i < 10; i++)
			escreva a[i];
	}
}