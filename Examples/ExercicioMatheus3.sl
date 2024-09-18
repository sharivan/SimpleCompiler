programa ExercicioMatheus3;

{
	var x:int;
	var y: int;
	
	escreva "Digite o primeiro número: ";
	leia x;
	escrevaln;
	
	escreva "Digite o segundo número: ";
	leia y;
	escrevaln;
	
	se (x > y)
	{
		var temp:int;
		temp = x;
		x = y;
		y = temp;
	}
	
	escrevaln "x=" + x;
	escrevaln "y=" + y;
	
	se ((x % 2) == 0)
		escrevaln "x é par";
	senão
		escrevaln "x é ímpar";
	
	se ((y % 2) == 0)
		escrevaln "y é par";
	senão
		escrevaln "y é ímpar";
}