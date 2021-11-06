programa TesteEstrutura
{
	estrutura Estrutura
	{
		x:int;
		y:int;
	}
	
	{
		var e:Estrutura;
		
		leia e.x;
		e.y = cast<int>(6.38);
		
		escreva e.x;
		escreva e.y;
	}
}