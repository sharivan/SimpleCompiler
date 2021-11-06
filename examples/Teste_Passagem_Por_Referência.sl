programa Teste_Passagem_Por_Referência
{
	função f(x:int, &y:int, z:real, &t:real) // f(4, 9, 2, 5)
	{
		y = y + x + 1; // y = 14
		t = y + z * z; // t = 18
	}
	
	função g(x:int, &y:int):int // g(4, 9)
	{
		var z:real = x - 2; // z = 2;
		var t:real = z + 3; // t = 5;
		f(x, y, z, t); // f(4, 9, 2, 5)
		// y = 14
		// t = 18
		retorne cast<int>(x + y + z + t); // cast<int>(4 + 14 + 2 + 18) = 38
	}
	
	{
		var x:int = 4;
		var y:int = 9;
		
		var z:int = g(x, y);
		// y = 14
		
		escrevaln "x=", x;
		escrevaln "y=", y;
		escrevaln "z=", z;
		
		/* Saída esperada:
		* x=4
		* y=14
		* z=38
		*/
	}
}