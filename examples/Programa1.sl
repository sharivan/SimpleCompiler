programa Programa1
{
	declare a:long;

	estrutura Vetor
	{
		x:real;
		y:real;
	}
	
	declare v:Vetor;

	função f(x:real):real
	{
		retorne x * x;
	}
	
	função g(x:int, y:int):int
	{
		retorne x + y;
	}
	
	{
		declare x:int;
		declare y:int;
		declare z:real;
		
		leia x;
		leia y;
		leia z;
		
		declare w:int = g(x, y);
		declare t:real = f(z);
		
		escrevaln "w=", w;
		escrevaln "t=", t;
		
		a = 9L;
		v.x = 8;
		v.y = 9.9E4;
		
		escreva "a=", a, "\nv.x=", v.x, "\nv.y=", v.y;
	}
}