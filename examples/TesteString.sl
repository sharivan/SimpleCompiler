programa TesteString;

estrutura E1
{
	i:int;
	s1:texto;
	j:int;
	s2:texto;
	r:real;
}

estrutura E2
{
	i:int;
	s:texto;
	j:int;
}

var s:texto;
var t:E1;

/*
	Teste de entrada:
	
	string
	123
	abc
	
	Saída esperada:
	
	123
	123456
	789123456
	abc123def
	123abc123def
	193
	1233bc123def
	12
	15
	1 texto 2 textoabc123def 3.1
	3 textotextoabc123def 4
	987123-- 789321++ abcxyz a1b2c3
	Digite o valor de s: string
	Digite o valor de s2: 123
	Digite o valor de s3: abc
	string123abc
*/

{
	var s2: texto;
	s2 = "123";
	escrevaln s2;
	
	s = s2 + "456"; // s = "123456"
	escrevaln s;
	
	s = "789" + s; // s = "789123456"
	escrevaln s;
	
	var s3:texto = "abc" + 123 + "def"; // s3 = "abc123def"
	escrevaln s3;
	
	s = s2 + s3; // s = "123abc123def"
	escrevaln s;
	
	s2[1] = '9'; // s2 = "193"
	escrevaln s2;
	
	s[3] = s3[2]; // s = "1233bc123def"
	escrevaln s;
	
	escrevaln s.tamanho; // 12
	
	var l:int = s2.tamanho + s3.tamanho; // l = 15
	escrevaln l;
	
	t.i = 1;
	t.s1 = "texto";
	t.j = 2;
	t.s2 = t.s1 + s3; // t.s2 = "textoabc123def"
	t.r = 3.1;
	
	escrevaln t.i, " ", t.s1, " ", t.j, " ", t.s2, " ", t.r;
	
	var s4:E2;
	s4.i = t.i + t.j; // s4.i = 3
	s4.s = t.s1 + t.s2; // s4.s = "textotextoabc123def"
	s4.j = t.j - t.i + cast<int>(t.r); // s4.j = 4
	
	escrevaln s4.i, " ", s4.s, " ", s4.j;
	
	var a1:E1[3];
	var a2:E2[3];
	var a3:texto[3];
	
	para (var i:int = 0; i < 3; i++)
	{
		a1[i].i = i;
		a1[i].s1 = "987123--";
		a1[i].j = 2 * i;
		a1[i].s2 = "789321++";
		a1[i].r = 3.1 * i;
		
		a2[i].i = a1[i].j;
		a2[i].s = "abcxyz";
		a2[i].j = a1[i].i;
		
		a3[i] = "a1b2c3";
	}
	
	escrevaln a1[0].s1, " ", a1[1].s2, " ", a2[2].s, " ", a3[2];
	
	escreva "Digite o valor de s: ";
	leia s;
	
	escreva "Digite o valor de s2: ";
	leia s2;
	
	escreva "Digite o valor de s3: ";
	leia s3;
	
	s += s2 + s3;
	escrevaln s;
}