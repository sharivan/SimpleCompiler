programa ExercicioMatheus;

{
	var vetor:int[5];
	vetor[0] = 69;
	vetor[1] = 171;
	vetor[2] = -3;
	vetor[3] = 3949;
	vetor[4] = 789;
	
	var n:int;
	var posicao:int = -1;
	
	escrevaln "Digite um número, animal velho: ";
	leia n;	
	
	para (var i:int = 0; i < 5 && posicao < 0; i++)
	{
		var entrada:int = vetor[i];
		se (n == entrada)
			posicao = i;
	}
	
	se (posicao >= 0)
		escrevaln "ACHEI PORRA! ESTÁ NA POSIÇÃO " + posicao;
	senão
		escrevaln "NUNCA NEM VI";
}