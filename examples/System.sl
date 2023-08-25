unidade System;

função externa CopiaMemória(src:*void, dst:*void);

função externa ComprimentoString(str:*char):int;
	
função externa CopiaString(src:*char, dst:*char);
	
função externa ConcatenaStrings(dst:*char, src1:*char, src2:*char);
	
função externa CompareStrings(str1:*char, str2:*char):bool;
	
função externa StringParaInt(src:*char, &dst:int):bool;
	
função externa StringParaLong(src:*char, &dst:long):bool;
	
função externa StringParaFloat(src:*char, &dst:float):bool;
	
função externa StringParaReal(src:*char, &dst:real):bool;

função externa IntParaString(src:int, dst:*char);

função externa LongParaString(src:long, dst:*char);

função externa FloatParaString(src:float, dst:*char);

função externa RealParaString(src:real, dst:*char);

função externa AlocarMemória(len:int):*void;

função externa DesalocarMemória(ptr:*void);
	
// Suporte para strings contadas por referência
	
função externa NovoTexto(str:*char):texto;
	
função externa NovoTexto2(&dst:texto, str:*char);
	
função externa CopiaTexto(src:*char, dst:texto);
	
função externa ComprimentoTexto(str:texto):int;
	
função externa ConcatenaTextos(str1:texto, str2:texto):texto;
	
função externa ConcatenaTextos2(&dst:texto, str1:texto, str2:texto);
	
função externa AtribuiTexto(&dst:texto, src:texto);
	
função externa IncrementaReferenciaTexto(str:texto);
	
função externa DecrementaReferenciaTexto(&str:texto, anule:bool);

função externa DecrementaReferenciaArrayTexto(str:texto[], count:int, anule:bool);