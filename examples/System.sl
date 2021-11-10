unidade System
{
	função externa CopiaMemória(src:*void, dst:*void);

	função externa ComprimentoString(str:*char):int;
	
	função externa CopiaString(src:*char, dst:*char);
	
	função externa ConcatenaStrings(dst:*char, src1:*char, src2:*char);
	
	função externa CompareStrings(str1:*char, str2:*char): bool;
	
	função externa StringParaInt(str:*char): int;
	
	função externa StringParaLong(str:*char): long;
	
	função externa StringParaFloat(str:*char): float;
	
	função externa StringParaReal(str:*char): real;
}