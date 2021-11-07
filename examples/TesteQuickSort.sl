programa TesteQuickSort
{
	função Troca(&a:int, &b:int)
	{
		var temp:int = a;
		a = b;
		b = temp;
	}
	
	função Particiona(arr:int[], inicio:int, fim:int):int
	{
		var pivo:int = inicio; // selecione o elemento pivô
		var i:int = inicio;
		var j:int = fim;
		enquanto (i < j)
		{
			// increment i até que seja obtido um número maior que o elemento pivô
			enquanto (arr[i] <= arr[pivo] && i <= fim)
				i++;

			// decremente j até que seja obtido um número menor que o elemento pivô
			enquanto (arr[j] > arr[pivo] && j >= inicio)
				j--;

			// se i < j, troque os elementos nas posições i e j
			se (i < j)
				Troca(arr[i], arr[j]);
		}
		
		// quando i >= j, significa que a j-ésima posição é a posição correta do elemento pivô
		// portanto, troque o elemento pivô com o elemento da j-ésima posição
		Troca(arr[j], arr[pivo]);
		
		retorne j;
	}

	função QuickSort(arr:int[], inicio:int, fim:int)
	{
		se (inicio < fim)
		{
			var p:int = Particiona(arr, inicio, fim);
			QuickSort(arr, inicio, p - 1);
			QuickSort(arr, p + 1, fim);
		}
	}
	
	{
		// teste de entrada: {9, -2, 6, 3)
		
		var a:int[4];

		var i:int;
		para (i = 0; i < 4; i++)
			leia a[i];
		
		QuickSort(a, 0, 3);
		
		escreva "{", a[0];
		para (i = 1; i < 4; i++)
			escreva ", ", a[i];
			
		escrevaln "}";
		
		// saída esperada: {-2, 3, 6, 9)
	}
}