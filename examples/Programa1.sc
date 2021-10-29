programa Programa1
{
	estrutura Vetor
	{
		x:real;
		y:real;
	}

	função f(x:real):real
	{
		retorne x * x;
	}
	
	função g(x:int, y:int):int
	{
		retorne x + y;
	}
	
	{
		declare x:int = 1;
		declare y:int = 2;
		declare z:real = 3.3;
		
		declare w:int = g(x, y);
		declare t:real = f(z);
		
		escreva w;
		escreva t;
	}
}