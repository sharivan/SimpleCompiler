unidade System;

função esterna CopiaMemória(src:*void, dst:*void);

função esterna ComprimentoString(str:*char):int;
	
função esterna CopiaString(src:*char, dst:*char);
	
função esterna ConcatenaStrings(dst:*char, src1:*char, src2:*char);
	
função esterna CompareStrings(str1:*char, str2:*char):bool;
	
função esterna StringParaInt(src:*char, &dst:int):bool;
	
função esterna StringParaLong(src:*char, &dst:long):bool;
	
função esterna StringParaFloat(src:*char, &dst:float):bool;
	
função esterna StringParaReal(src:*char, &dst:real):bool;

função esterna IntParaString(src:int, dst:*char);

função esterna LongParaString(src:long, dst:*char);

função esterna FloatParaString(src:float, dst:*char);

função esterna RealParaString(src:real, dst:*char);

função esterna AlocarMemória(len:int):*void;

função esterna DesalocarMemória(ptr:*void);
	
// Suporte para strings contadas por referência
	
função esterna NovoTexto(str:*char):texto;
	
função esterna NovoTexto2(&dst:texto, str:*char);
	
função esterna CopiaTexto(src:*char, dst:texto);
	
função esterna ComprimentoTexto(str:texto):int;
	
função esterna ConcatenaTextos(str1:texto, str2:texto):texto;
	
função esterna ConcatenaTextos2(&dst:texto, str1:texto, str2:texto);
	
função esterna AtribuiTexto(&dst:texto, src:texto);
	
função esterna IncrementaReferenciaTexto(str:texto);
	
função esterna DecrementaReferenciaTexto(&str:texto, anule:bool);

função esterna DecrementaReferenciaArrayTexto(str:texto[], count:int, anule:bool);