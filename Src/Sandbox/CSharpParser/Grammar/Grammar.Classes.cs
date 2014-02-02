using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce(null, null, null, "class")]
        CsClassDeclaration ClassDeclaration(
                Opt<CsAttributes>            attributes,
                Opt<CsList<CsClassModifier>>   modifiers,
                Opt<CsPartial>               partial,
                CsIdentifier                 identifier,
                Opt<CsTypeParameterList>     typeParameters,
                Opt<CsClassBase>             classBase,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParamConstraints,
                CsClassBody                  body);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        [Produce("abstract")]
        [Produce("sealed")]
        [Produce("static")]
        CsClassModifier ClassModifier();

        [Produce("<", null, ">")]
        CsTypeParameterList TypeParameterList(CsCommaList<CsTypeParameterEntry> param);

        [Produce]
        CsTypeParameterEntry TypeParameters(Opt<CsAttributes> attributes, CsTypeParameter param);

        [Produce(":", null)]
        CsClassBase ClassBase(CsClassType type);

        [Produce(":", null)]
        CsClassBase ClassBase(CsCommaList<CsInterfaceType> type);

        [Produce(":", null, ",", null)]
        CsClassBase ClassBase(CsClassBase baseType, CsCommaList<CsInterfaceType> type);

        [Produce("where", null, ":", null)]
        CsTypeParameterConstraintClause TypeParameterConstraintClause(
                CsTypeParameter            typeParameter,
                CsTypeParameterConstraints constraints);

        [Produce]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsPrimaryConstraint constraint);

        [Produce]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsCommaList<CsSecondaryConstraint> constraints);

        [Produce]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsConstructorConstraint constraint);

        [Produce(null, ",", null)]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsPrimaryConstraint    constraint,
                CsCommaList<CsSecondaryConstraint> secondaryConstraints);
        
        [Produce(null, ",", null)]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsPrimaryConstraint     constraint,
                CsConstructorConstraint constructorConstraint);

        [Produce(null, ",", null)]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsCommaList<CsSecondaryConstraint>  constraints,
                CsConstructorConstraint constructorConstraint);

        [Produce(null, ",", null, ",", null)]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsPrimaryConstraint     constraint,
                CsCommaList<CsSecondaryConstraint>  constraints,
                CsConstructorConstraint constructorConstraint);

        [Produce]
        CsPrimaryConstraint PrimaryConstraint(CsClassType type);

        [Produce("class")]
        [Produce("struct")]
        CsPrimaryConstraint PrimaryConstraint();

        [Produce]
        CsSecondaryConstraint SecondaryConstraint(CsInterfaceType type);

        [Produce]
        CsSecondaryConstraint SecondaryConstraint(CsTypeParameter type);

        [Produce("new", "(", ")")]
        CsConstructorConstraint ConstructorConstraint();

        [Produce("{", null, "}")]
        CsClassBody ClassBody(Opt<CsList<CsClassMemberDeclaration>> declarations);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsConstantDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsFieldDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsMethodDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsPropertyDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsEventDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsIndexerDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsOperatorDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsConstructorDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsDestructorDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsStaticConstructorDeclaration decl);

        [Produce]
        CsClassMemberDeclaration ClassMemberDeclaration(CsTypeDeclaration decl);

        [Produce(null, null, "const", null, null, ";")]
        CsConstantDeclaration ConstantDeclaration(
                Opt<CsAttributes>                  attributes,
                Opt<CsList<CsConstantModifier>>      modifiers,
                CsType                             type,
                CsCommaList<CsConstantDeclarator>  declarators);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        CsConstantModifier ConstantModifier();

        [Produce(null, null, null, null, ";")]
        CsFieldDeclaration FieldDeclaration(
                Opt<CsAttributes>                 attributes,
                Opt<CsList<CsFieldModifiers>>       modifiers,
                CsType                            type,
                CsCommaList<CsVariableDeclarator> declarators);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        [Produce("static")]
        [Produce("readonly")]
        [Produce("volatile")]
        CsFieldModifiers FieldModifiers();

        [Produce]
        CsVariableDeclarator VariableDeclarator(
                CsIdentifier          id);

        [Produce(null, "=", null)]
        CsVariableDeclarator VariableDeclarator(
                CsIdentifier          id,
                CsVariableInitializer initializer);

        [Produce]
        CsVariableInitializer VariableInitializer(CsExpression expression);

        [Produce]
        CsVariableInitializer VariableInitializer(CsArrayInitializer initializer);

        [Produce]
        CsMethodDeclaration MethodDeclaration(
                CsMethodHeader header,
                CsMethodBody body);

        [Produce(null, null, null, null, null, null, "(", null, ")", null)]
        CsMethodHeader MethodHeader(
                Opt<CsAttributes>      attributes,
                Opt<CsList<CsMethodModifier>> modifiers,
                Opt<CsPartial>                partial,
                CsReturnType                  returnType,
                CsMemberName                  memberName,
                Opt<CsTypeParameterList>      typeParameters,
                Opt<CsFormalParameterList>    formalParams,
                Opt<CsList<CsTypeParameterConstraintClause>> typeParamConstraints);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        [Produce("static")]
        [Produce("virtual")]
        [Produce("sealed")]
        [Produce("override")]
        [Produce("abstract")]
        [Produce("extern")]
        CsMethodModifier MethodModifier();

        [Produce("void")]
        CsReturnType ReturnType();

        [Produce]
        CsReturnType ReturnType(CsType type);

        [Produce]
        CsMethodBody MethodBody(CsBlock block);

        [Produce]
        CsFormalParameterList FormalParameterList(
                CsCommaList<CsFixedParameter> fixedParameters);

        [Produce(null, ",", null)]
        CsFormalParameterList FormalParameterList(
                CsCommaList<CsFixedParameter> fixedParameters,
                CsParameterArray              parameterArray);

        [Produce]
        CsFormalParameterList FormalParameterList(
                CsParameterArray              parameterArray);

        [Produce]
        CsFixedParameter FixedParameter(
                Opt<CsAttributes> attributes,
                Opt<CsParameterModifier> modfier,
                CsType                   type,
                CsIdentifier             id,
                Opt<CsDefaultArgument>   defaultArgument);

        [Produce]
        CsDefaultArgument DefaultArgument(CsExpression expression);

        [Produce("ref")]
        [Produce("out")]
        [Produce("this")]
        CsParameterModifier ParameterModifier();

        [Produce(null, "params", null, null)]
        CsParameterArray ParameterArray(
                Opt<CsAttributes>  attributes,
                CsArrayType               arrayType,
                CsIdentifier              identifier);

        [Produce(null, null, null, null, "{", null, "}")]
        CsPropertyDeclaration PropertyDeclaration(
                Opt<CsAttributes>        attributes,
                Opt<CsList<CsPropertyModifier>> modifiers,
                CsType                          type,
                CsMemberName                    name,
                CsAccessorDecrations            accessors);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        [Produce("static")]
        [Produce("virtual")]
        [Produce("sealed")]
        [Produce("override")]
        [Produce("abstract")]
        [Produce("extern")]
        CsPropertyModifier PropertyModifier();

        [Produce]
        CsMemberName MemberName(CsIdentifier id);

        [Produce(null, ".", null)]
        CsMemberName MemberName(
                CsInterfaceType interfacetype,
                CsIdentifier id);

        [Produce]
        CsAccessorDecrations AccessorDecrations(
                CsGetAccessorDeclaration getter,
                Opt<CsSetAccessorDeclaration> setter);

        [Produce]
        CsAccessorDecrations AccessorDecrations(
                CsSetAccessorDeclaration setter,
                Opt<CsGetAccessorDeclaration> getter);

        [Produce(null, null, "get", null)]
        CsGetAccessorDeclaration GetAccessorDeclaration(
                Opt<CsAttributes>    attributes,
                Opt<CsAccessorModifier>     modifier,
                CsAccessorBody              body);

        [Produce(null, null, "set", null)]
        CsSetAccessorDeclaration SetAccessorDeclaration(
                Opt<CsAttributes>    attributes,
                Opt<CsAccessorModifier>     modifier,
                CsAccessorBody              body);

        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        [Produce("protected", "internal")]
        [Produce("internal", "protected")]
        CsAccessorModifier AccessorModifier();

        [Produce(";")]
        CsAccessorBody AccessorBody();

        [Produce]
        CsAccessorBody AccessorBody(CsBlock block);

        [Produce(null, null, "event", null, null, ";")]
        CsEventDeclaration EventDeclaration(
                Opt<CsAttributes>          attributes,
                Opt<CsList<CsEventModifier>>      modifiers,
                CsType                            type,
                CsCommaList<CsVariableDeclarator> variableDeclarators);

        [Produce(null, null, "event", null, null, "{", null, "}")]
        CsEventDeclaration EventDeclaration(
                Opt<CsAttributes>          attributes,
                Opt<CsList<CsEventModifier>>      modifiers,
                CsType                            type,
                CsMemberName                      memberName,
                CsEventAccessorDeclarations       accessorDeclarations);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        [Produce("static")]
        [Produce("virtual")]
        [Produce("sealed")]
        [Produce("override")]
        [Produce("abstract")]
        [Produce("extern")]
        CsEventModifier EventModifier();

        [Produce]
        CsEventAccessorDeclarations EventAccessorDeclarations(
                CsAddAccessorDeclaration    adder,
                CsRemoveAccessorDeclaration remover);

        [Produce(null, "add", null)]
        CsAddAccessorDeclaration AddAccessorDeclaration(
                Opt<CsAttributes>    attributes,
                CsBlock                     block);

        [Produce(null, "remove", null)]
        CsRemoveAccessorDeclaration RemoveAccessorDeclaration(
                Opt<CsAttributes>    attributes,
                CsBlock                     block);

        [Produce(null, null, null, "{", null, "}")]
        CsIndexerDeclaration IndexerDeclaration(
                Opt<CsAttributes>       attributes,
                Opt<CsList<CsIndexerModifier>> modifiers,
                CsIndexerDeclarator            declarator,
                CsAccessorDecrations           accessorDeclarations);

        [Produce("new")]
        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        [Produce("static")]
        [Produce("virtual")]
        [Produce("sealed")]
        [Produce("override")]
        [Produce("abstract")]
        [Produce("extern")]
        CsIndexerModifier IndexerModifier();

        [Produce(null, "this", "[", null, "]")]
        CsIndexerDeclarator IndexerDeclarator(
                CsType                type,
                CsFormalParameterList paramList);

        [Produce(null, null, ".", "this", "[", null, "]")]
        CsIndexerDeclarator IndexerDeclarator(
                CsType                type,
                CsInterfaceType       interfaceType,
                CsFormalParameterList paramList);

        [Produce]
        CsOperatorDeclaration OperatorDeclaration(
                Opt<CsAttributes>    attributes,
                CsList<CsOperatorModifier>  modifiers,
                CsOperatorDeclarator        declarator,
                CsOperatorBody              operatorBody);

        [Produce("public")]
        [Produce("static")]
        [Produce("extern")]
        CsOperatorModifier OperatorModifier();

        [Produce]
        CsOperatorDeclarator OperatorDeclarator(CsUnaryOperatorDeclarator decl);

        [Produce]
        CsOperatorDeclarator OperatorDeclarator(CsBinaryOperatorDeclarator decl);

        [Produce]
        CsOperatorDeclarator OperatorDeclarator(CsConversionOperatorDeclarator decl);

        [Produce(null, "operator", null, "(", null, null, ")")]
        CsUnaryOperatorDeclarator UnaryOperatorDeclarator(
                CsType                      type,
                CsOverloadableUnaryOperator op,
                CsType                      paramType,
                CsIdentifier                paramId);

        [Produce("+")]
        [Produce("-")]
        [Produce("!")]
        [Produce("~")]
        [Produce("++")]
        [Produce("--")]
        [Produce("true")]
        [Produce("false")]
        CsOverloadableUnaryOperator OverloadableUnaryOperator();

        [Produce(null, "operator", null, "(", null, null, ",", null, null, ")")]
        CsBinaryOperatorDeclarator BinaryOperatorDeclarator(
                CsType                       type,
                CsOverloadableBinaryOperator op,
                CsType                       paramType1,
                CsIdentifier                 paramId1,
                CsType                       paramType2,
                CsIdentifier                 paramId2);

        [Produce("+")]
        [Produce("-")]
        [Produce("*")]
        [Produce("/")]
        [Produce("%")]
        [Produce("&")]
        [Produce("||")]
        [Produce("^")]
        [Produce("<<")]
        [Produce(">>")]
        [Produce("==")]
        [Produce("!=")]
        [Produce(">")]
        [Produce("<")]
        [Produce(">=")]
        [Produce("<=")]
        CsOverloadableBinaryOperator OverloadableBinaryOperator();

        [Produce("implicit", "operator", null, "(", null, null, ")")]
        [Produce("explicit", "operator", null, "(", null, null, ")")]
        CsConversionOperatorDeclarator ConversionOperatorDeclarator(
                CsType       type,
                CsType       paramType,
                CsIdentifier paramId);

        [Produce]
        CsOperatorBody OperatorBody(CsBlock block);

        [Produce(";")]
        CsOperatorBody OperatorBody();

        [Produce]
        CsConstructorDeclaration ConstructorDeclaration(
                Opt<CsAttributes>    attributes,
                Opt<CsList<CsConstructorModifier>> modifiers,
                CsConstructorDeclarator     declarator,
                CsConstructorBody           body);

        [Produce("public")]
        [Produce("protected")]
        [Produce("internal")]
        [Produce("private")]
        [Produce("extern")]
        CsConstructorModifier ConstructorModifier();

        [Produce(null, "(", null, ")", null)]
        CsConstructorDeclarator ConstructorDeclarator(
                CsIdentifier                  id,
                Opt<CsFormalParameterList>    formalParams,
                Opt<CsConstructorInitializer> initializer);
        
        [Produce(":", "base", "(", null, ")")]
        [Produce(":", "this", "(", null, ")")]
        CsConstructorInitializer ConstructorInitializer(
            Opt<CsArgumentList> args);

        [Produce]
        CsConstructorBody ConstructorBody(CsBlock block);

        [Produce(";")]
        CsConstructorBody ConstructorBody();

        [Produce(null, null, null, "(", ")", null)]
        CsStaticConstructorDeclaration StaticConstructorDeclaration(
            Opt<CsAttributes>               attributes,
            CsStaticConstructorModifiers    modifiers,
            CsIdentifier                    id,
            CsStaticConstructorBody         body);

        [Produce("extern")]
        [Produce("static")]
        [Produce("extern", "static")]
        [Produce("static", "extern")]
        CsStaticConstructorModifiers StaticConstructorModifiers();

        [Produce]
        CsStaticConstructorBody StaticConstructorBody(CsBlock block);

        [Produce(";")]
        CsStaticConstructorBody StaticConstructorBody();

        [Produce(null, null, "~", null, "(", ")", null)]
        CsDestructorDeclaration DestructorDeclaration(
                Opt<CsAttributes>    attributes,
                Opt<CsExtern>        externModifier,
                CsIdentifier         id,
                CsDestructorBody     body);

        [Produce]
        CsDestructorBody DestructorBody(CsBlock block);

        [Produce(";")]
        CsDestructorBody DestructorBody();
    }
}
