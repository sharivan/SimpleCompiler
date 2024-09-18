programa ExercicioMatheus2;

{
	var vetor:int[40];
	var valor:int;
	
	para (var i:int = 1; i <= 40; i++)
	{
		valor = (i + 1) / 2;
		
		se (i > 1)
			escreva ", ";
		
		vetor[i] = valor;
		escreva valor;
	}
	
	escrevaln;
	
	para (var i:int = 0; i < 40; i++)
	{
		se (i == 0)
			valor = 1;
		senão se (i == 1)
			valor = 1;
		senão
			valor = vetor[i - 1] + vetor[i - 2] ;
		
		vetor[i] = valor;
		
		se (i > 0)
			escreva ", ";
		
		escreva valor;
	}
	
	escrevaln;
	
	para (var i:int = 0; i < 40; i++)
	{
		valor = 2 * i;
		vetor[i] = valor;
		
		se (i > 0)
			escreva ", ";
		
		escreva valor;
	}
}