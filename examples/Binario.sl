programa Binario;

{
	var n:int;

	escrevaln "Digite um número inteiro:";
	leia n;

	escreva "Seu valor em binário é ";
		
	var b:bool = falso;
	para (var i:int = 31; i >= 0; i = i - 1)
	{
		se ((n & (1 << i)) != 0)
		{
			b = verdade;
			escreva 1;
		}
		senão se (b)
			escreva 0;
	}
		
	se (!b)
		escreva 0;

	escrevaln;
}