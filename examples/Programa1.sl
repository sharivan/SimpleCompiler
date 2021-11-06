programa Programa1
{
	var a:long;

	estrutura Vetor
	{
		x:real;
		y:real;
	}
	
	var v:Vetor;

	função f(x:real):real
	{
		retorne x * x;
	}
	
	função g(x:int, y:int):int
	{
		retorne x + y;
	}
	
	{
		var x:int;
		var y:int;
		var z:real;
		
		leia x;
		leia y;
		leia z;
		
		var w:int = g(x, y);
		var t:real = f(z);
		
		escrevaln "w=", w;
		escrevaln "t=", t;
		
		a = 9L;
		v.x = 8;
		v.y = 9.9E4;
		
		escreva "a=", a, "\nv.x=", v.x, "\nv.y=", v.y;
	}
}