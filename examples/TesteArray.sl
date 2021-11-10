programa TesteArray
{
	{
		var a:int[10];
		
		escrevaln "Digite um número inteiro para a[0]:";
		leia a[0];

		a[1] = cast<int>(6.38);
		
		var i:int;
		para (i = 2; i < 10; i++)
			a[i] = i;
		
		escrevaln "a[0]=", a[0];
		escrevaln "a[1]=", a[1];
		
		para (i = 2; i < 10; i++)
			escrevaln "a[", i, "]=", a[i];
	}
}