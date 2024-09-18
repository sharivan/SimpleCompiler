programa ExercidioDoRevista;

{
	var numero:int;
	var soma:int = 0;
	var quantidade:int = 0;
	
	enquanto (verdade)
	{
		escreva "Digite um número:\n ";
		leia numero;
		
		se (numero == 999)
			quebra;

		soma += numero;
		quantidade++;
	}
	
	escrevaln "Foram lidos " + quantidade + " números";
	escrevaln "A soma total foi " + soma;
}