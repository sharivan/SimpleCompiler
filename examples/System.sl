unidade System
{
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
}