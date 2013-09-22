using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Parse(null, null, null, "class")]
        CsClassDeclaration ClassDeclaration(
                Opt<CsAttributes>            attributes,
                CsOptList<CsClassModifier>   modifiers,
                Opt<CsPartial>               partial,
                CsIdentifier                 identifier,
                Opt<CsTypeParameterList>     typeParameters,
                Opt<CsClassBase>             classBase,
                CsOptList<CsTypeParameterConstraintClause> typeParamConstraints,
                CsClassBody                  body);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        [Parse("abstract")]
        [Parse("sealed")]
        [Parse("static")]
        CsClassModifier ClassModifier();

        [Parse("<", null, ">")]
        CsTypeParameterList TypeParameterList(CsCommaList<CsTypeParameterEntry> param);

        [Parse]
        CsTypeParameterEntry TypeParameters(Opt<CsAttributes> attributes, CsTypeParameter param);

        [Parse(":", null)]
        CsClassBase ClassBase(CsClassType type);

        [Parse(":", null)]
        CsClassBase ClassBase(CsCommaList<CsInterfaceType> type);

        [Parse(":", null, ",", null)]
        CsClassBase ClassBase(CsClassBase baseType, CsCommaList<CsInterfaceType> type);

        [Parse("where", null, ":", null)]
        CsTypeParameterConstraintClause TypeParameterConstraintClause(
                CsTypeParameter            typeParameter,
                CsTypeParameterConstraints constraints);

        [Parse]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsPrimaryConstraint constraint);

        [Parse]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsCommaList<CsSecondaryConstraint> constraints);

        [Parse]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsConstructorConstraint constraint);

        [Parse(null, ",", null)]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsPrimaryConstraint    constraint,
                CsCommaList<CsSecondaryConstraint> secondaryConstraints);
        
        [Parse(null, ",", null)]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsPrimaryConstraint     constraint,
                CsConstructorConstraint constructorConstraint);

        [Parse(null, ",", null)]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsCommaList<CsSecondaryConstraint>  constraints,
                CsConstructorConstraint constructorConstraint);

        [Parse(null, ",", null, ",", null)]
        CsTypeParameterConstraints TypeParameterConstraints(
                CsPrimaryConstraint     constraint,
                CsCommaList<CsSecondaryConstraint>  constraints,
                CsConstructorConstraint constructorConstraint);

        [Parse]
        CsPrimaryConstraint PrimaryConstraint(CsClassType type);

        [Parse("class")]
        [Parse("struct")]
        CsPrimaryConstraint PrimaryConstraint();

        [Parse]
        CsSecondaryConstraint SecondaryConstraint(CsInterfaceType type);

        [Parse]
        CsSecondaryConstraint SecondaryConstraint(CsTypeParameter type);

        [Parse("new", "(", ")")]
        CsConstructorConstraint ConstructorConstraint();

        [Parse("{", null, "}")]
        CsClassBody ClassBody(CsOptList<CsClassMemberDeclaration> declarations);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsConstantDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsFieldDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsMethodDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsPropertyDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsEventDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsIndexerDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsOperatorDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsConstructorDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsDestructorDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsStaticConstructorDeclaration decl);

        [Parse]
        CsClassMemberDeclaration ClassMemberDeclaration(CsTypeDeclaration decl);

        [Parse(null, null, "const", null, null, ";")]
        CsConstantDeclaration ConstantDeclaration(
                Opt<CsAttributes>                  attributes,
                CsOptList<CsConstantModifier>      modifiers,
                CsType                             type,
                CsCommaList<CsConstantDeclarator>  declarators);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        CsConstantModifier ConstantModifier();

        [Parse(null, null, null, null, ";")]
        CsFieldDeclaration FieldDeclaration(
                Opt<CsAttributes>                 attributes,
                CsOptList<CsFieldModifiers>       modifiers,
                CsType                            type,
                CsCommaList<CsVariableDeclarator> declarators);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        [Parse("static")]
        [Parse("readonly")]
        [Parse("volatile")]
        CsFieldModifiers FieldModifiers();

        [Parse]
        CsVariableDeclarator VariableDeclarator(
                CsIdentifier          id);

        [Parse(null, "=", null)]
        CsVariableDeclarator VariableDeclarator(
                CsIdentifier          id,
                CsVariableInitializer initializer);

        [Parse]
        CsVariableInitializer VariableInitializer(CsExpression expression);

        [Parse]
        CsVariableInitializer VariableInitializer(CsArrayInitializer initializer);

        [Parse]
        CsMethodDeclaration MethodDeclaration(
                CsMethodHeader header,
                CsMethodBody body);

        [Parse(null, null, null, null, null, null, "(", null, ")", null)]
        CsMethodHeader MethodHeader(
                Opt<CsAttributes>      attributes,
                CsOptList<CsMethodModifier> modifiers,
                Opt<CsPartial>                partial,
                CsReturnType                  returnType,
                CsMemberName                  memberName,
                Opt<CsTypeParameterList>      typeParameters,
                Opt<CsFormalParameterList>    formalParams,
                CsOptList<CsTypeParameterConstraintClause> typeParamConstraints);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        [Parse("static")]
        [Parse("virtual")]
        [Parse("sealed")]
        [Parse("override")]
        [Parse("abstract")]
        [Parse("extern")]
        CsMethodModifier MethodModifier();

        [Parse("void")]
        CsReturnType ReturnType();

        [Parse]
        CsReturnType ReturnType(CsType type);

        [Parse]
        CsMethodBody MethodBody(CsBlock block);

        [Parse]
        CsFormalParameterList FormalParameterList(
                CsCommaList<CsFixedParameter> fixedParameters);

        [Parse(null, ",", null)]
        CsFormalParameterList FormalParameterList(
                CsCommaList<CsFixedParameter> fixedParameters,
                CsParameterArray              parameterArray);

        [Parse]
        CsFormalParameterList FormalParameterList(
                CsParameterArray              parameterArray);

        [Parse]
        CsFixedParameter FixedParameter(
                Opt<CsAttributes> attributes,
                Opt<CsParameterModifier> modfier,
                CsType                   type,
                CsIdentifier             id,
                Opt<CsDefaultArgument>   defaultArgument);

        [Parse]
        CsDefaultArgument DefaultArgument(CsExpression expression);

        [Parse("ref")]
        [Parse("out")]
        [Parse("this")]
        CsParameterModifier ParameterModifier();

        [Parse(null, "params", null, null)]
        CsParameterArray ParameterArray(
                Opt<CsAttributes>  attributes,
                CsArrayType               arrayType,
                CsIdentifier              identifier);

        [Parse(null, null, null, null, "{", null, "}")]
        CsPropertyDeclaration PropertyDeclaration(
                Opt<CsAttributes>        attributes,
                CsOptList<CsPropertyModifier> modifiers,
                CsType                          type,
                CsMemberName                    name,
                CsAccessorDecrations            accessors);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        [Parse("static")]
        [Parse("virtual")]
        [Parse("sealed")]
        [Parse("override")]
        [Parse("abstract")]
        [Parse("extern")]
        CsPropertyModifier PropertyModifier();

        [Parse]
        CsMemberName MemberName(CsIdentifier id);

        [Parse(null, ".", null)]
        CsMemberName MemberName(
                CsInterfaceType interfacetype,
                CsIdentifier id);

        [Parse]
        CsAccessorDecrations AccessorDecrations(
                CsGetAccessorDeclaration getter,
                Opt<CsSetAccessorDeclaration> setter);

        [Parse]
        CsAccessorDecrations AccessorDecrations(
                CsSetAccessorDeclaration setter,
                Opt<CsGetAccessorDeclaration> getter);

        [Parse(null, null, "get", null)]
        CsGetAccessorDeclaration GetAccessorDeclaration(
                Opt<CsAttributes>    attributes,
                Opt<CsAccessorModifier>     modifier,
                CsAccessorBody              body);

        [Parse(null, null, "set", null)]
        CsSetAccessorDeclaration SetAccessorDeclaration(
                Opt<CsAttributes>    attributes,
                Opt<CsAccessorModifier>     modifier,
                CsAccessorBody              body);

        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        [Parse("protected", "internal")]
        [Parse("internal", "protected")]
        CsAccessorModifier AccessorModifier();

        [Parse(";")]
        CsAccessorBody AccessorBody();

        [Parse]
        CsAccessorBody AccessorBody(CsBlock block);

        [Parse(null, null, "event", null, null, ";")]
        CsEventDeclaration EventDeclaration(
                Opt<CsAttributes>          attributes,
                CsOptList<CsEventModifier>      modifiers,
                CsType                            type,
                CsCommaList<CsVariableDeclarator> variableDeclarators);

        [Parse(null, null, "event", null, null, "{", null, "}")]
        CsEventDeclaration EventDeclaration(
                Opt<CsAttributes>          attributes,
                CsOptList<CsEventModifier>      modifiers,
                CsType                            type,
                CsMemberName                      memberName,
                CsEventAccessorDeclarations       accessorDeclarations);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        [Parse("static")]
        [Parse("virtual")]
        [Parse("sealed")]
        [Parse("override")]
        [Parse("abstract")]
        [Parse("extern")]
        CsEventModifier EventModifier();

        [Parse]
        CsEventAccessorDeclarations EventAccessorDeclarations(
                CsAddAccessorDeclaration    adder,
                CsRemoveAccessorDeclaration remover);

        [Parse(null, "add", null)]
        CsAddAccessorDeclaration AddAccessorDeclaration(
                Opt<CsAttributes>    attributes,
                CsBlock                     block);

        [Parse(null, "remove", null)]
        CsRemoveAccessorDeclaration RemoveAccessorDeclaration(
                Opt<CsAttributes>    attributes,
                CsBlock                     block);

        [Parse(null, null, null, "{", null, "}")]
        CsIndexerDeclaration IndexerDeclaration(
                Opt<CsAttributes>       attributes,
                CsOptList<CsIndexerModifier> modifiers,
                CsIndexerDeclarator            declarator,
                CsAccessorDecrations           accessorDeclarations);

        [Parse("new")]
        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        [Parse("static")]
        [Parse("virtual")]
        [Parse("sealed")]
        [Parse("override")]
        [Parse("abstract")]
        [Parse("extern")]
        CsIndexerModifier IndexerModifier();

        [Parse(null, "this", "[", null, "]")]
        CsIndexerDeclarator IndexerDeclarator(
                CsType                type,
                CsFormalParameterList paramList);

        [Parse(null, null, ".", "this", "[", null, "]")]
        CsIndexerDeclarator IndexerDeclarator(
                CsType                type,
                CsInterfaceType       interfaceType,
                CsFormalParameterList paramList);

        [Parse]
        CsOperatorDeclaration OperatorDeclaration(
                Opt<CsAttributes>    attributes,
                CsList<CsOperatorModifier>  modifiers,
                CsOperatorDeclarator        declarator,
                CsOperatorBody              operatorBody);

        [Parse("public")]
        [Parse("static")]
        [Parse("extern")]
        CsOperatorModifier OperatorModifier();

        [Parse]
        CsOperatorDeclarator OperatorDeclarator(CsUnaryOperatorDeclarator decl);

        [Parse]
        CsOperatorDeclarator OperatorDeclarator(CsBinaryOperatorDeclarator decl);

        [Parse]
        CsOperatorDeclarator OperatorDeclarator(CsConversionOperatorDeclarator decl);

        [Parse(null, "operator", null, "(", null, null, ")")]
        CsUnaryOperatorDeclarator UnaryOperatorDeclarator(
                CsType                      type,
                CsOverloadableUnaryOperator op,
                CsType                      paramType,
                CsIdentifier                paramId);

        [Parse("+")]
        [Parse("-")]
        [Parse("!")]
        [Parse("~")]
        [Parse("++")]
        [Parse("--")]
        [Parse("true")]
        [Parse("false")]
        CsOverloadableUnaryOperator OverloadableUnaryOperator();

        [Parse(null, "operator", null, "(", null, null, ",", null, null, ")")]
        CsBinaryOperatorDeclarator BinaryOperatorDeclarator(
                CsType                       type,
                CsOverloadableBinaryOperator op,
                CsType                       paramType1,
                CsIdentifier                 paramId1,
                CsType                       paramType2,
                CsIdentifier                 paramId2);

        [Parse("+")]
        [Parse("-")]
        [Parse("*")]
        [Parse("/")]
        [Parse("%")]
        [Parse("&")]
        [Parse("||")]
        [Parse("^")]
        [Parse("<<")]
        [Parse(">>")]
        [Parse("==")]
        [Parse("!=")]
        [Parse(">")]
        [Parse("<")]
        [Parse(">=")]
        [Parse("<=")]
        CsOverloadableBinaryOperator OverloadableBinaryOperator();

        [Parse("implicit", "operator", null, "(", null, null, ")")]
        [Parse("explicit", "operator", null, "(", null, null, ")")]
        CsConversionOperatorDeclarator ConversionOperatorDeclarator(
                CsType       type,
                CsType       paramType,
                CsIdentifier paramId);

        [Parse]
        CsOperatorBody OperatorBody(CsBlock block);

        [Parse(";")]
        CsOperatorBody OperatorBody();

        [Parse]
        CsConstructorDeclaration ConstructorDeclaration(
                Opt<CsAttributes>    attributes,
                CsOptList<CsConstructorModifier> modifiers,
                CsConstructorDeclarator     declarator,
                CsConstructorBody           body);

        [Parse("public")]
        [Parse("protected")]
        [Parse("internal")]
        [Parse("private")]
        [Parse("extern")]
        CsConstructorModifier ConstructorModifier();

        [Parse(null, "(", null, ")", null)]
        CsConstructorDeclarator ConstructorDeclarator(
                CsIdentifier                  id,
                Opt<CsFormalParameterList>    formalParams,
                Opt<CsConstructorInitializer> initializer);
        
        [Parse(":", "base", "(", null, ")")]
        [Parse(":", "this", "(", null, ")")]
        CsConstructorInitializer ConstructorInitializer(
            Opt<CsArgumentList> args);

        [Parse]
        CsConstructorBody ConstructorBody(CsBlock block);

        [Parse(";")]
        CsConstructorBody ConstructorBody();

        [Parse(null, null, null, "(", ")", null)]
        CsStaticConstructorDeclaration StaticConstructorDeclaration(
            Opt<CsAttributes>               attributes,
            CsStaticConstructorModifiers    modifiers,
            CsIdentifier                    id,
            CsStaticConstructorBody         body);

        [Parse("extern")]
        [Parse("static")]
        [Parse("extern", "static")]
        [Parse("static", "extern")]
        CsStaticConstructorModifiers StaticConstructorModifiers();

        [Parse]
        CsStaticConstructorBody StaticConstructorBody(CsBlock block);

        [Parse(";")]
        CsStaticConstructorBody StaticConstructorBody();

        [Parse(null, null, "~", null, "(", ")", null)]
        CsDestructorDeclaration DestructorDeclaration(
                Opt<CsAttributes>    attributes,
                Opt<CsExtern>        externModifier,
                CsIdentifier         id,
                CsDestructorBody     body);

        [Parse]
        CsDestructorBody DestructorBody(CsBlock block);

        [Parse(";")]
        CsDestructorBody DestructorBody();
    }
}
