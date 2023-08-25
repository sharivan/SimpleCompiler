programa TesteEstrutura;

estrutura Estrutura
{
	x:int;
	y:int;
}
	
{
	var e:Estrutura;
		
	escrevaln "Digite um número inteiro para e.x:";
	leia e.x;

	e.y = cast<int>(6.38);
		
	escrevaln "e.x=", e.x;
	escrevaln "e.y=", e.y;
}