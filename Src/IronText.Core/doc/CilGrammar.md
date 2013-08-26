%scanner
{
    /* C style alphaNumeric identifier (e.g., Hello_There2) */
    ID      : "(alpha | '_') (alnum | '_')*";

    /* C style quoted string (e.g., "hi\n") */
    /* TODO: Escaping syntax */
    QStr : "quot 
                ~(quot | esc)*
                ( esc any ~(quot | esc)* )*
              quot";

    /* C style singlely quoted string(e.g., 'hi') */
    /* TODO: Escaping syntax */
    SQStr : "['] ~['\\]* ([\\].  ~['\\]*)* [']";

    /* C style 32-bit integer (e.g., 235, 03423, 0x34FFF) */
    /* C style 64-bit integer (e.g., -2353453636235234, 0x34FFFFFFFFFF) */
    Integer    : "digit+"; 

    /* C style floating point number (e.g., -0.2323, 354.3423, 3435.34E-5) */
	Float : "'.' digit+ (('e'|'E') ('+'|'-')? digit+)?";

    DottedName : "'.' alpha (alnum|'$'|'_'|'.')* 
                 | ('_'|'?'|alpha) (alnum|'$'|'_'|'.')*";

    HexByte : "hex hex";

    void    : "blank+";
}

START : decls
;
decls : /* EMPTY */
| decls decl
;
decl : classHead '{' classDecls '}'
| nameSpaceHead '{' decls '}'
| methodHead methodDecls '}'
| fieldDecl
| dataDecl
| vtableDecl
| vtfixupDecl
| extSourceSpec
| fileDecl
| assemblyHead '{' assemblyDecls '}'
| assemblyRefHead '{' assemblyRefDecls '}'
| comtypeHead '{' comtypeDecls '}'
| manifestResHead '{' manifestResDecls '}'
| moduleHead
| secDecl
| customAttrDecl
| '.subsystem' int32
| '.corflags' int32
| '.file' 'alignment' int32
| '.imagebase' int64
| languageDecl
;
compQstring : QStr
| compQstring '+' QStr
;
languageDecl 
    : '.language' SQStr
    | '.language' SQStr ',' SQStr
    | '.language' SQStr ',' SQStr ',' SQStr
    ;
customAttrDecl : '.custom' customType
| '.custom' customType '=' compQstring
| customHead bytes ')'
| '.custom' '(' ownerType ')' customType
| '.custom' '(' ownerType ')' customType '='
compQstring
| customHeadWithOwner bytes ')'
;
moduleHead : '.module'
| '.module' name1
| '.module' 'extern' name1
;
vtfixupDecl : '.vtfixup' '[' int32 ']' vtfixupAttr 'at' id
;
vtfixupAttr : /* EMPTY */
| vtfixupAttr 'int32'
| vtfixupAttr 'int64'
| vtfixupAttr 'fromunmanaged'
| vtfixupAttr 'callmostderived'
;
vtableDecl : vtableHead bytes ')'
;
vtableHead : '.vtable' '=' '('
;
nameSpaceHead : '.namespace' name1
;
classHead : '.class' classAttr id extendsClause implClause
;
classAttr : /* EMPTY */
| classAttr 'public'
| classAttr 'private'
| classAttr 'value'
| classAttr 'enum'
| classAttr 'interface'
| classAttr 'sealed'
| classAttr 'abstract'
| classAttr 'auto'
| classAttr 'sequential'
| classAttr 'explicit'
| classAttr 'ansi'
| classAttr 'unicode'
| classAttr 'autochar'
| classAttr 'import'
| classAttr 'serializable'
| classAttr 'nested' 'public'
| classAttr 'nested' 'private'
| classAttr 'nested' 'family'
| classAttr 'nested' 'assembly'
| classAttr 'nested' 'famandassem'
| classAttr 'nested' 'famorassem'
| classAttr 'beforefieldinit'
| classAttr 'specialname'
| classAttr 'rtspecialname'
;
extendsClause : /* EMPTY */
| 'extends' className
;
implClause : /* EMPTY */
| 'implements' classNames
;
classNames : classNames ',' className
| className
;
classDecls : /* EMPTY */
| classDecls classDecl
;
classDecl : methodHead methodDecls '}'
| classHead '{' classDecls '}'
| eventHead '{' eventDecls '}'
| propHead '{' propDecls '}'
| fieldDecl
| dataDecl
| secDecl
| extSourceSpec
| customAttrDecl
| '.size' int32
| '.pack' int32
| exportHead '{' comtypeDecls '}'
| '.override' typeSpec '::' methodName 'with'
callConv type typeSpec '::' methodName '(' sigArgs0 ')'
| languageDecl
;
fieldDecl : '.field' repeatOpt fieldAttr type id atOpt
initOpt
;
atOpt : /* EMPTY */
| 'at' id
;
initOpt : /* EMPTY */
| '=' fieldInit
;
repeatOpt : /* EMPTY */
| '[' int32 ']'
;
customHead : '.custom' customType '=' '('
;
customHeadWithOwner : '.custom' '(' ownerType ')' customType '='
'('
;
memberRef : methodSpec callConv type typeSpec
'::' methodName '(' sigArgs0 ')'
| methodSpec callConv type methodName '('
sigArgs0 ')'
| 'field' type typeSpec '::' id
| 'field' type id
;
customType : callConv type typeSpec '::' '.ctor' '('
sigArgs0 ')'
| callConv type '.ctor' '(' sigArgs0 ')'
;
ownerType : typeSpec
| memberRef
;
eventHead : '.event' eventAttr typeSpec id
| '.event' eventAttr id
;
eventAttr : /* EMPTY */
| eventAttr 'rtspecialname' /**/
| eventAttr 'specialname'
;
eventDecls : /* EMPTY */
| eventDecls eventDecl
;
eventDecl : '.addon' callConv type typeSpec '::'
methodName '(' sigArgs0 ')'
| '.addon' callConv type methodName '('
sigArgs0 ')'
| '.removeon' callConv type typeSpec '::'
methodName '(' sigArgs0 ')'
| '.removeon' callConv type methodName '('
sigArgs0 ')'
| '.fire' callConv type typeSpec '::'
methodName '(' sigArgs0 ')'
| '.fire' callConv type methodName '('
sigArgs0 ')'
| '.other' callConv type typeSpec '::'
methodName '(' sigArgs0 ')'
| '.other' callConv type methodName '('
sigArgs0 ')'
| extSourceSpec
| customAttrDecl
| languageDecl
;
propHead : '.property' propAttr callConv type id '('
sigArgs0 ')' initOpt
;
propAttr : /* EMPTY */
| propAttr 'rtspecialname' /**/
| propAttr 'specialname'
;
propDecls : /* EMPTY */
| propDecls propDecl
;
propDecl : '.set' callConv type typeSpec '::'
methodName '(' sigArgs0 ')'
| '.set' callConv type methodName '('
sigArgs0 ')'
| '.get' callConv type typeSpec '::'
methodName '(' sigArgs0 ')'
| '.get' callConv type methodName '('
sigArgs0 ')'
| '.other' callConv type typeSpec '::'
methodName '(' sigArgs0 ')'
| '.other' callConv type methodName '('
sigArgs0 ')'
| customAttrDecl
| extSourceSpec
| languageDecl
;
methodHeadPart1 : '.method'
;
methodHead 
: methodHeadPart1 methAttr callConv paramAttr type methodName '(' sigArgs0 ')' implAttr '{'
| methodHeadPart1 methAttr callConv paramAttr type 'marshal' '(' nativeType ')' methodName '(' sigArgs0 ')' implAttr '{'
;
methAttr : /* EMPTY */
| methAttr 'static'
| methAttr 'public'
| methAttr 'private'
| methAttr 'family'
| methAttr 'final'
| methAttr 'specialname'
| methAttr 'virtual'
| methAttr 'abstract'
| methAttr 'assembly'
| methAttr 'famandassem'
| methAttr 'famorassem'
| methAttr 'privatescope'
| methAttr 'hidebysig'
| methAttr 'newslot'
| methAttr 'rtspecialname' /**/
| methAttr 'unmanagedexp'
| methAttr 'reqsecobj'
| methAttr 'pinvokeimpl' '(' compQstring 'as' compQstring pinvAttr ')'
| methAttr 'pinvokeimpl' '(' compQstring pinvAttr ')'
| methAttr 'pinvokeimpl' '(' pinvAttr ')'
;

pinvAttr : /* EMPTY */
| pinvAttr 'nomangle'
| pinvAttr 'ansi'
| pinvAttr 'unicode'
| pinvAttr 'autochar'
| pinvAttr 'lasterr'
| pinvAttr 'winapi'
| pinvAttr 'cdecl'
| pinvAttr 'stdcall'
| pinvAttr 'thiscall'
| pinvAttr 'fastcall'
;
methodName : '.ctor'
| '.cctor'
| name1
;
paramAttr : /* EMPTY */
| paramAttr '[' 'in' ']'
| paramAttr '[' 'out' ']'
| paramAttr '[' 'opt' ']'
| paramAttr '[' int32 ']'
;
fieldAttr : /* EMPTY */
| fieldAttr 'static'
| fieldAttr 'public'
| fieldAttr 'private'
| fieldAttr 'family'
| fieldAttr 'initonly'
| fieldAttr 'rtspecialname' /**/
| fieldAttr 'specialname'
/* commented out because PInvoke for fields is not supported by EE
| fieldAttr 'pinvokeimpl' '(' compQstring
'as' compQstring pinvAttr ')'
| fieldAttr 'pinvokeimpl' '(' compQstring
pinvAttr ')'
| fieldAttr 'pinvokeimpl' '(' pinvAttr ')'
*/
| fieldAttr 'marshal' '(' nativeType ')'
| fieldAttr 'assembly'
| fieldAttr 'famandassem'
| fieldAttr 'famorassem'
| fieldAttr 'privatescope'
| fieldAttr 'literal'
| fieldAttr 'notserialized'
;
implAttr : /* EMPTY */
| implAttr 'native'
| implAttr 'cil'
| implAttr 'optil'
| implAttr 'managed'
| implAttr 'unmanaged'
| implAttr 'forwardref'
| implAttr 'preservesig'
| implAttr 'runtime'
| implAttr 'internalcall'
| implAttr 'synchronized'
| implAttr 'noinlining'
;
localsHead : '.locals'
;
methodDecl : '.emitbyte' int32
| sehBlock
| '.maxstack' int32
| localsHead '(' sigArgs0 ')'
| localsHead 'init' '(' sigArgs0 ')'
| '.entrypoint'
| '.zeroinit'
| dataDecl
| instr
| id ':'
| secDecl
| extSourceSpec
| languageDecl
| customAttrDecl
| '.export' '[' int32 ']'
| '.export' '[' int32 ']'
'as' id
| '.vtentry' int32 ':' int32
| '.override' typeSpec '::' methodName
| scopeBlock
| '.param' '[' int32 ']' initOpt
;
scopeBlock : scopeOpen methodDecls '}'
;
scopeOpen : '{'
;
sehBlock : tryBlock sehClauses
;
sehClauses : sehClause sehClauses
| sehClause
;
tryBlock : tryHead scopeBlock
| tryHead id 'to' id
| tryHead int32 'to' int32
;
tryHead : '.try'
;
sehClause : catchClause handlerBlock
| filterClause handlerBlock
| finallyClause handlerBlock
| faultClause handlerBlock
;
filterClause : filterHead scopeBlock
| filterHead id
| filterHead int32
;
filterHead : 'filter'
;
catchClause : 'catch' className
;
finallyClause : 'finally'
;
faultClause : 'fault'
;
handlerBlock : scopeBlock
| 'handler' id 'to' id
| 'handler' int32 'to' int32
;
methodDecls : /* EMPTY */
| methodDecls methodDecl
;
dataDecl : ddHead ddBody
;
ddHead : '.data' tls id '='
| '.data' tls
;
tls : /* EMPTY */
| 'tls'
;
ddBody : '{' ddItemList '}'
| ddItem
;
ddItemList : ddItem ',' ddItemList
| ddItem
;
ddItemCount : /* EMPTY */
| '[' int32 ']'
;
ddItem : 'char' '*' '(' compQstring ')'
| '&' '(' id ')'
| bytearrayhead bytes ')'
| 'float32' '(' float64 ')' ddItemCount
| 'float64' '(' float64 ')' ddItemCount
| 'int64' '(' int64 ')' ddItemCount
| 'int32' '(' int32 ')' ddItemCount
| 'int16' '(' int32 ')' ddItemCount
| 'int8' '(' int32 ')' ddItemCount
| 'float32' ddItemCount
| 'float64' ddItemCount
| 'int64' ddItemCount
| 'int32' ddItemCount
| 'int16' ddItemCount
| 'int8' ddItemCount
;
fieldInit : 'float32' '(' float64 ')'
| 'float64' '(' float64 ')'
| 'float32' '(' int64 ')'
| 'float64' '(' int64 ')'
| 'int64' '(' int64 ')'
| 'int32' '(' int64 ')'
| 'int16' '(' int64 ')'
| 'char' '(' int64 ')'
| 'int8' '(' int64 ')'
| 'bool' '(' truefalse ')'
| compQstring
| bytearrayhead bytes ')'
| 'nullref'
;
bytearrayhead : 'bytearray' '('
;
bytes : /* EMPTY */
| hexbytes
;
hexbytes : HexByte
| hexbytes HexByte
;
methodSpec : 'method'
;
instr 
: instr_none
| instr_var
| instr_i
| instr_i8
| instr_r
| instr_brtarget
| instr_method
| instr_field
| instr_type
| instr_string
| instr_sig
/* TODO: ANTLR version has no such instructions
| instr_rva id
| instr_rva int32
*/
| instr_tok
| instr_switch
/* TODO: Unclear what is it
| instr_phi int16s
*/
;

instr_tok
    : 'ldtoken' ownerType /* ownerType ::= memberRef | typeSpec */
    ;

instr_r 
    : 'ldc.r4' float64
    | 'ldc.r8' float64
    | 'ldc.r4' int64
    | 'ldc.r8' int64
    | 'ldc.r4' '(' bytes ')'
    | 'ldc.r8' '(' bytes ')'
	;

instr_brtarget 
    : 'beq'  int32
    | 'beq'  id
    | 'beq.s'  int32
    | 'beq.s'  id
    | 'bge'  int32
    | 'bge'  id
    | 'bge.s' int32
    | 'bge.s' id
    | 'bge.un'  int32
    | 'bge.un'  id
    | 'bge.un.s'  int32
    | 'bge.un.s'  id
    | 'bgt'  int32
    | 'bgt'  id
    | 'bgt.s'  int32
    | 'bgt.s'  id
    | 'bgt.un'  int32
    | 'bgt.un'  id
    | 'bgt.un.s' int32
    | 'bgt.un.s' id
    | '.vtable'  int32
    | '.vtable'  id
    | 'ble.s'  int32
    | 'ble.s'  id
    | 'ble.un'  int32
    | 'ble.un'  id
    | 'ble.un.s'  int32
    | 'ble.un.s'  id
    | 'blt'  int32
    | 'blt'  id
    | 'blt.s' int32
    | 'blt.s' id
    | 'blt.un'  int32
    | 'blt.un'  id
    | 'blt.un.s'  int32
    | 'blt.un.s'  id
    | 'bne.un'  int32
    | 'bne.un'  id
    | 'bne.un.s'  int32
    | 'bne.un.s'  id
    | 'br'  int32
    | 'br'  id
    | 'br.s' int32
    | 'br.s' id
    | 'brfalse'  int32
    | 'brfalse'  id
    | 'brfalse.s'  int32
    | 'brfalse.s'  id
    | 'brtrue'  int32
    | 'brtrue'  id
    | 'brtrue.s'  int32
    | 'brtrue.s'  id
    | 'leave'  int32
    | 'leave'  id
    | 'leave.s' int32
    | 'leave.s' id
	;

instr_method 
    : 'call'      callConv type typeSpec '::' methodName '(' sigArgs0 ')'
    | 'call'      callConv type               methodName '(' sigArgs0 ')'
    | 'callvirt'  callConv type typeSpec '::' methodName '(' sigArgs0 ')'
    | 'callvirt'  callConv type               methodName '(' sigArgs0 ')'
    | 'jmp'  callConv type typeSpec '::' methodName '(' sigArgs0 ')'
    | 'jmp'  callConv type methodName '(' sigArgs0 ')'
    | 'ldftn'  callConv type typeSpec '::' methodName '(' sigArgs0 ')'
    | 'ldftn'  callConv type methodName '(' sigArgs0 ')'
    | 'ldvirtftn'  callConv type typeSpec '::' methodName '(' sigArgs0 ')'
    | 'ldvirtftn'  callConv type methodName '(' sigArgs0 ')'
    | 'newobj' callConv type typeSpec '::' methodName '(' sigArgs0 ')'
    | 'newobj' callConv type methodName '(' sigArgs0 ')'
	;

instr_field 
    : 'ldfld'   type typeSpec '::' id
    | 'ldfld'   type id
    | 'ldflda'  type typeSpec '::' id
    | 'ldflda'  type id
    | 'ldsfld'  type typeSpec '::' id
    | 'ldsfld'  type id
    | 'ldsflda' type typeSpec '::' id
    | 'ldsflda' type id
    | 'stfld'   type typeSpec '::' id
    | 'stfld'   type id
    | 'stsfld'  type typeSpec '::' id
    | 'stsfld'  type id
	;

instr_type
    : 'box'        typeSpec
    | 'castclass'  typeSpec
    | 'cpobj'      typeSpec
    | 'initobj'    typeSpec
    | 'isinst'     typeSpec
    | 'ldelema'    typeSpec
    | 'ldobj'      typeSpec
    | 'mkrefany'   typeSpec
    | 'newarr'     typeSpec
    | 'refanyval'  typeSpec
    | 'sizeof'     typeSpec
    | 'stobj'      typeSpec
    | 'unbox'      typeSpec
	;

instr_string 
    : 'ldstr' compQstring
    | 'ldstr' bytearrayhead bytes ')'
    ;

instr_sig 
    : 'calli' callConv type '(' sigArgs0 ')'
    ;

instr_tok 
    : 'ldtoken' 
    ;

instr_switch 
    : 'switch'  '(' labels ')'
    ;

instr_i 
    : 'ldc.i4' int32
	| 'ldc.i4.s' int32
	| 'unaligned.' int32
	;

instr_i8 
    : 'ldc.i8' int64
	;

instr_var
    : 'ldarg' int32
    | 'ldarg' id
	| 'ldarg.s' int32
	| 'ldarg.s' id
	| 'ldarga' int32
	| 'ldarga' id
	| 'ldarga.s' int32
	| 'ldarga.s' id
	| 'ldloc' int32
	| 'ldloc' id
	| 'ldloc.s' int32
	| 'ldloc.s' id
	| 'ldloca' int32
	| 'ldloca' id
	| 'ldloca.s' int32
	| 'ldloca.s' id
	| 'starg' int32
	| 'starg' id
	| 'starg.s' int32
	| 'starg.s' id
	| 'stloc' int32
	| 'stloc' id
	| 'stloc.s' int32
	| 'stloc.s' id
	;

instr_none 
    : '.addon' 
    | 'add.ovf' 
    | 'add.ovf.un' 
    | 'and'

    | 'arglist' 
    | 'break' 
    | 'ceq' 
    | 'cgt'

    | 'cgt.un' 
    | 'ckfinite' 
    | 'clt' 
    | 'clt.un'

    | 'conv.i' 
    | 'conv.i1' 
    | 'conv.i2' 
    | 'conv.i4'

    | 'conv.i8' 
    | 'conv.ovf.i' 
    | 'conv.ovf.i.un' 
    | 'conv.ovf.i1'

    | 'conv.ovf.i1.un' 
    | 'conv.ovf.i2' 
    | 'conv.ovf.i2.un' 
    | 'conv.ovf.i4'

    | 'conv.ovf.i4.un' 
    | 'conv.ovf.i8' 
    | 'conv.ovf.i8.un' 
    | 'conv.ovf.u'

    | 'conv.ovf.u.un' 
    | 'conv.ovf.u1' 
    | 'conv.ovf.u1.un' 
    | 'conv.ovf.u2'

    | 'conv.ovf.u2.un' 
    | 'conv.ovf.u4' 
    | 'conv.ovf.u4.un' 
    | 'conv.ovf.u8'

    | 'conv.ovf.u8.un' 
    | 'conv.r.un' 
    | 'conv.r4' 
    | 'conv.r8'

    | 'conv.u' 
    | 'conv.u1' 
    | 'conv.u2' 
    | 'conv.u4'

    | 'conv.u8' 
    | 'cpblk' 
    | 'div' 
    | 'div.un'

    | 'dup' 
    | 'endfault' 
    | 'endfilter' 
    | 'endfinally'

    | 'initblk' 
    | 'ldarg.0' 
    | 'ldarg.1'

    | 'ldarg.2' 
    | 'ldarg.3' 
    | 'ldc.i4.0' 
    | 'ldc.i4.1'

    | 'ldc.i4.2' 
    | 'ldc.i4.3' 
    | 'ldc.i4.4' 
    | 'ldc.i4.5'

    | 'ldc.i4.6' 
    | 'ldc.i4.7' 
    | 'ldc.i4.8' 
    | 'ldc.i4.M1'

    | 'ldelem.i' 
    | 'ldelem.i1' 
    | 'ldelem.i2' 
    | 'ldelem.i4'

    | 'ldelem.i8' 
    | 'ldelem.r4' 
    | 'ldelem.r8' 
    | 'ldelem.ref'

    | 'ldelem.u1' 
    | 'ldelem.u2' 
    | 'ldelem.u4' 
    | 'ldind.i'

    | 'ldind.i1' 
    | 'ldind.i2' 
    | 'ldind.i4' 
    | 'ldind.i8'

    | 'ldind.r4' 
    | 'ldind.r8' 
    | 'ldind.ref' 
    | 'ldind.u1'

    | 'ldind.u2' 
    | 'ldind.u4' 
    | 'ldlen' 
    | 'ldloc.0'

    | 'ldloc.1' 
    | 'ldloc.2' 
    | 'ldloc.3' 
    | 'ldnull'

    | 'localloc' 
    | 'mul' 
    | 'mul.ovf' 
    | 'mul.ovf.un'

    | 'neg' 
    | 'nop' 
    | 'not' 
    | '.cctor'

    | 'pop' 
    | 'refanytype' 
    | '.removeon' 
    | 'rem.un'

    | 'ret' 
    | 'rethrow' 
    | 'shl' 
    | 'shr'

    | 'shr.un' 
    | 'stelem.i' 
    | 'stelem.i1' 
    | 'stelem.i2'

    | 'stelem.i4' 
    | 'stelem.i8' 
    | 'stelem.r4' 
    | 'stelem.r8'
    | 'stelem.ref' 
    | 'stind.i' 
    | 'stind.i1' 
    | 'stind.i2'

    | 'stind.i4' 
    | 'stind.i8' 
    | 'stind.r4' 
    | 'stind.r8'

    | 'stind.ref' 
    | 'stloc.0' 
    | 'stloc.1' 
    | 'stloc.2'

    | 'stloc.3' 
    | '.subsystem' 
    | 'sub.ovf' 
    | 'sub.ovf.un'

    | 'tail.' 
    | 'rethrow' 
    | 'volatile.' 
    | 'xor'
    ;

sigArgs0 : /* EMPTY */
| sigArgs1
;
sigArgs1 : sigArg
| sigArgs1 ',' sigArg
;
sigArg : '...'
| paramAttr type
| paramAttr type id
| paramAttr type 'marshal' '(' nativeType ')'
| paramAttr type 'marshal' '(' nativeType ')' id
;
name1 
: id
| DottedName
| name1 '.' name1
;
className 
: '[' name1 ']' slashedName
| '[' '.module' name1 ']' slashedName
| slashedName
;
slashedName : name1
| slashedName '/' name1
;
typeSpec : className
| '[' name1 ']'
| '[' '.module' name1 ']'
| type
;
callConv : 'instance' callConv
| 'explicit' callConv
| callKind
;
callKind : /* EMPTY */
| 'default'
| 'vararg'
| 'unmanaged' 'cdecl'
| 'unmanaged' 'stdcall'
| 'unmanaged' 'thiscall'
| 'unmanaged' 'fastcall'
;
nativeType : /* EMPTY */
| 'custom' '(' compQstring ',' compQstring ',' compQstring ',' compQstring ')'
| 'custom' '(' compQstring ',' compQstring ')'
| 'fixed' 'sysstring' '[' int32 ']'
| 'fixed' 'array' '[' int32 ']'
| 'variant'
| 'currency'
| 'syschar'
| 'void'
| 'bool'
| 'int8'
| 'int16'
| 'int32'
| 'int64'
| 'float32'
| 'float64'
| 'error'
| 'unsigned' 'int8'
| 'unsigned' 'int16'
| 'unsigned' 'int32'
| 'unsigned' 'int64'
| nativeType '*'
| nativeType '[' ']'
| nativeType '[' int32 ']'
| nativeType '[' int32 '+' int32 ']'
| nativeType '[' '+' int32 ']'
| 'decimal'
| 'date'
| 'bstr'
| 'lpstr'
| 'lpwstr'
| 'lptstr'
| 'objectref'
| 'iunknown'
| 'idispatch'
| 'struct'
| 'interface'
| 'safearray' variantType
| 'safearray' variantType ',' compQstring
| 'int'
| 'unsigned' 'int'
| 'nested' 'struct'
| 'byvalstr'
| 'ansi' 'bstr'
| 'tbstr'
| 'variant' 'bool'
| methodSpec
| 'as' 'any'
| 'lpstruct'
;
variantType : /* EMPTY */
| 'null'
| 'variant'
| 'currency'
| 'void'
| 'bool'
| 'int8'
| 'int16'
| 'int32'
| 'int64'
| 'float32'
| 'float64'
| 'unsigned' 'int8'
| 'unsigned' 'int16'
| 'unsigned' 'int32'
| 'unsigned' 'int64'
| '*'
| variantType '[' ']'
| variantType 'vector'
| variantType '&'
| 'decimal'
| 'date'
| 'bstr'
| 'lpstr'
| 'lpwstr'
| 'iunknown'
| 'idispatch'
| 'safearray'
| 'int'
| 'unsigned' 'int'
| 'error'
| 'hresult'
| 'carray'
| 'userdefined'
| 'record'
| 'filetime'
| 'blob'
| 'stream'
| 'storage'
| 'streamed_object'
| 'stored_object'
| 'blob_object'
| 'cf'
| 'clsid'
;
type : 'class' className
| 'object'
| 'string'
| 'value' 'class' className
| 'valuetype' className
| type '[' ']'
| type '[' bounds1 ']'
/* uncomment when and if this
type is supported by the Runtime
| type 'value' '[' int32 ']'
*/
| type '&'
| type '*'
| type 'pinned'
| type 'modreq' '(' className ')'
| type 'modopt' '(' className ')'
| '!' int32
| methodSpec callConv type '*' '(' sigArgs0 ')'
| 'typedref'
| 'char'
| 'void'
| 'bool'
| 'int8'
| 'int16'
| 'int32'
| 'int64'
| 'float32'
| 'float64'
| 'unsigned' 'int8'
| 'unsigned' 'int16'
| 'unsigned' 'int32'
| 'unsigned' 'int64'
| 'native' 'int'
| 'native' 'unsigned' 'int'
| 'native' 'float'
;
bounds1 : bound
| bounds1 ',' bound
;
bound : /* EMPTY */
| '...'
| int32
| int32 '...' int32
| int32 '...'
;
labels : /* empty */
| id ',' labels
| int32 ',' labels
| id
| int32
;
id : ID
| SQStr
;
int16s : /* EMPTY */
| int16s int32
;
int32 : Integer
;
int64 : Integer
;
float64 : Float
| 'float32' '(' int32 ')'
| 'float64' '(' int64 ')'
;
secDecl : '.permission' secAction typeSpec '('
nameValPairs ')'
| '.permission' secAction typeSpec
| psetHead bytes ')'
;
psetHead : '.permissionset' secAction '=' '('
;
nameValPairs : nameValPair
| nameValPair ',' nameValPairs
;
nameValPair : compQstring '=' caValue
;
truefalse : 'true'
| 'false'
;
caValue : truefalse
| int32
| 'int32' '(' int32 ')'
| compQstring
| className '(' 'int8' ':' int32 ')'
| className '(' 'int16' ':' int32 ')'
| className '(' 'int32' ':' int32 ')'
| className '(' int32 ')'
;
secAction : 'request'
| 'demand'
| 'assert'
| 'deny'
| 'permitonly'
| 'linkcheck'
| 'inheritcheck'
| 'reqmin'
| 'reqopt'
| 'reqrefuse'
| 'prejitgrant'
| 'prejitdeny'
| 'noncasdemand'
| 'noncaslinkdemand'
| 'noncasinheritance'
;
extSourceSpec : '.line' int32 SQStr
| '.line' int32
| '.line' int32 ':' int32 SQStr
| '.line' int32 ':' int32
;
fileDecl : '.file' fileAttr name1 fileEntry hashHead
bytes ')' fileEntry
| '.file' fileAttr name1 fileEntry
;
fileAttr : /* EMPTY */
| fileAttr 'nometadata'
;
fileEntry : /* EMPTY */
| '.entrypoint'
;
hashHead : '.hash' '=' '('
;
assemblyHead : '.assembly' asmAttr name1
;
asmAttr : /* EMPTY */
| asmAttr 'noappdomain'
| asmAttr 'noprocess'
| asmAttr 'nomachine'
;
assemblyDecls : /* EMPTY */
| assemblyDecls assemblyDecl
;
assemblyDecl : '.hash' 'algorithm' int32
| secDecl
| asmOrRefDecl
;
asmOrRefDecl : publicKeyHead bytes ')'
| '.ver' int32 ':' int32 ':' int32 ':' int32
| '.locale' compQstring
| localeHead bytes ')'
| customAttrDecl
;
publicKeyHead : '.publickey' '=' '('
;
publicKeyTokenHead : '.publickeytoken' '=' '('
;
localeHead : '.locale' '=' '('
;
assemblyRefHead : '.assembly' 'extern' name1
| '.assembly' 'extern' name1 'as' name1
;
assemblyRefDecls : /* EMPTY */
| assemblyRefDecls assemblyRefDecl
;
assemblyRefDecl : hashHead bytes ')'
| asmOrRefDecl
| publicKeyTokenHead bytes ')'
;
comtypeHead : '.class' 'extern' comtAttr name1
;
exportHead : '.export' comtAttr name1
;
comtAttr : /* EMPTY */
| comtAttr 'private'
| comtAttr 'public'
| comtAttr 'nested' 'public'
| comtAttr 'nested' 'private'
| comtAttr 'nested' 'family'
| comtAttr 'nested' 'assembly'
| comtAttr 'nested' 'famandassem'
| comtAttr 'nested' 'famorassem'
;
comtypeDecls : /* EMPTY */
| comtypeDecls comtypeDecl
;
comtypeDecl : '.file' name1
| '.class' 'extern' name1
| '.class' int32
| customAttrDecl
;
manifestResHead : '.mresource' manresAttr name1
;
manresAttr : /* EMPTY */
| manresAttr 'public'
| manresAttr 'private'
;
manifestResDecls : /* EMPTY */
| manifestResDecls manifestResDecl
;
manifestResDecl : '.file' name1 'at' int32
| '.assembly' 'extern' name1
| customAttrDecl
;

