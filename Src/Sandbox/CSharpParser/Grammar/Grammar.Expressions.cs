using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce]
        CsLiteral Literal(CsBoolean literal);

        [Produce]
        CsLiteral Literal(CsInteger literal);

        [Produce]
        CsLiteral Literal(CsReal literal);

        [Produce]
        CsLiteral Literal(CsChar literal);

        [Produce]
        CsLiteral Literal(CsString literal);

        [Produce]
        CsLiteral Literal(CsNull literal);

        [Produce]
        CsVariableReference VariableReference(CsExpression expression);

        [Produce]
        CsArgumentList ArgumentList(CsArgument arg);

        [Produce(null, ",", null)]
        CsArgumentList ArgumentList(CsArgumentList list, CsArgument arg);

        [Produce]
        CsArgument Argument(Opt<CsArgumentName> name, CsArgumentValue val);

        [Produce(null, ":")]
        CsArgumentName ArgumentName(CsIdentifier id);

        [Produce]
        CsArgumentValue ArgumentValue(CsExpression expression);

        [Produce("ref")]
        CsArgumentValue ArgumentValueByRef(CsVariableReference variableRef);

        [Produce("out")]
        CsArgumentValue ArgumentValueByOut(CsVariableReference variableRef);

        [Produce]
        CsPrimaryExpression PrimaryExpression(CsPrimaryNoArrayCreationExpression expression);

        [Produce]
        CsPrimaryExpression PrimaryExpression(CsArrayCreationExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsLiteral literal);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsSimpleName name);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsParenthesizedExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsMemberAccess memberAccess);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsInvocationExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsElementAccess access);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsThisAccess thisAccess);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsBaseAccess thisAccess);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsPostIncrementExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsPostDecrementExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsObjectCreationExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsDelegateCreationExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsAnonymousObjectCreationExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsTypeOfExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsCheckedExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsUncheckedExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsDefaultValueExpression expression);

        [Produce]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsAnonymousMethodExpression expression);

        [Produce]
        CsSimpleName SimpleName(CsIdentifier id, Opt<CsTypeArgumentList> typeArgList);

        [Produce("(", null, ")")]
        CsParenthesizedExpression ParenthesizedExpression(CsExpression expression);

        [Produce(null, ".", null, null)]
        CsMemberAccess MemberAccess(
            CsPrimaryExpression     expression,
            CsIdentifier            member,
            Opt<CsTypeArgumentList> argList);

        [Produce(null, ".", null, null)]
        CsMemberAccess MemberAccess(
            CsPredefinedType type,
            CsIdentifier            member,
            Opt<CsTypeArgumentList> argList);

        [Produce(null, ".", null)]
        CsMemberAccess MemberAccess(CsQualifiedAliasMember alias, CsIdentifier id);

        [Produce("bool")]
        [Produce("byte")]
        [Produce("char")]
        [Produce("decimal")]
        [Produce("double")]
        [Produce("float")]
        [Produce("int")]
        [Produce("long")]
        [Produce("object")]
        [Produce("sbyte")]
        [Produce("short")]
        [Produce("string")]
        [Produce("uint")]
        [Produce("ulong")]
        [Produce("ushort")]
        CsPredefinedType PredefinedType();

        [Produce(null, "(", null, ")")]
        CsInvocationExpression InvocationExpression(CsPrimaryExpression expression, Opt<CsArgumentList> args);

        [Produce(null, "[", null, "]")]
        CsElementAccess ElementAccess(CsPrimaryNoArrayCreationExpression expression, CsArgumentList args);

        [Produce("this")]
        CsThisAccess ThisAccess();

        [Produce("base", ".", null)]
        CsBaseAccess BaseAccess(CsIdentifier member);

        [Produce("base", "[", null, "]")]
        CsBaseAccess BaseAccess(CsArgumentList args);

        [Produce(null, "++")]
        CsPostIncrementExpression PostIncrementExpression(CsPrimaryExpression operand);

        [Produce(null, "--")]
        CsPostDecrementExpression PostDecrementExpression(CsPrimaryExpression operand);

        [Produce("new", null, "(", null, ")", null)]
        CsObjectCreationExpression ObjectCreationExpression(
                CsType type,
                Opt<CsArgumentList> args,
                Opt<CsObjectOrCollectionInitializer> initializer);

        [Produce("new", null, null)]
        CsObjectCreationExpression ObjectCreationExpression(
                CsType type,
                CsObjectOrCollectionInitializer initializer);

        [Produce]
        CsObjectOrCollectionInitializer ObjectOrCollectionInitializer(CsObjectInitializer initializer);

        [Produce]
        CsObjectOrCollectionInitializer ObjectOrCollectionInitializer(CsCollectionInitializer initializer);

        [Produce("{", null, "}")]
        CsObjectInitializer ObjectInitializer(Opt<CsCommaList<CsMemberInitializer>> initializer);

        [Produce("{", null, ",", "}")]
        CsObjectInitializer ObjectInitializer(CsCommaList<CsMemberInitializer> initializer);

        [Produce(null, "=", null)]
        CsMemberInitializer MemberInitializer(CsIdentifier id, CsInitializerValue val);

        [Produce]
        CsInitializerValue InitializerValue(CsExpression expression);

        [Produce]
        CsInitializerValue InitializerValue(CsObjectOrCollectionInitializer initializer);

        [Produce("{", null, "}")]
        [Produce("{", null, ",", "}")]
        CsCollectionInitializer CollectionInitializer(CsCommaList<CsElementInitializer> initializers);

        [Produce]
        CsElementInitializer ElementInitializer(CsNonAssignmentExpression expression);

        [Produce("{", null, "}")]
        CsElementInitializer ElementInitializer(CsCommaList<CsExpression> expression);

        [Produce("new", null, "[", null, "]", null, null)]
        CsArrayCreationExpression ArrayCreationExpression(
                CsNonArrayType               type,
                CsCommaList<CsExpression>    expressionList,
                Opt<CsList<CsRankSpecifier>> rankSpecifiers,
                Opt<CsArrayInitializer>      arrayInitializer);

        [Produce("new", null, null)]
        CsArrayCreationExpression ArrayCreationExpression(
                CsNonArrayType      type,
                CsArrayInitializer  arrayInitializer);

        [Produce("new", null, null)]
        CsArrayCreationExpression ArrayCreationExpression(
                CsRankSpecifier     rankSpecifier,
                CsArrayInitializer  arrayInitializer);

        [Produce("new", null, "(", null, ")")]
        CsDelegateCreationExpression DelegateCreationExpression(
                CsDelegateType type,
                CsExpression   expression);

        [Produce("new", null)]
        CsAnonymousObjectCreationExpression AnonymousObjectCreationExpression(
                CsAnonymousObjectInitializer initializer);

        [Produce("{", null, "}")]
        CsAnonymousObjectInitializer AnonymousObjectInitializer(
            Opt<CsCommaList<CsMemberDeclarator>> declarators);

        [Produce("{", null, ",", "}")]
        CsAnonymousObjectInitializer AnonymousObjectInitializer(
            CsCommaList<CsMemberDeclarator> declarators);

        [Produce]
        CsMemberDeclarator MemberDeclarator(CsSimpleName name);

        [Produce]
        CsMemberDeclarator MemberDeclarator(CsMemberAccess access);

        [Produce(null, "=", null)]
        CsMemberDeclarator MemberDeclarator(CsIdentifier access, CsExpression expression);

        [Produce("typeof", "(", null, ")")]
        CsTypeOfExpression TypeOfExpression(CsType type);

        [Produce("typeof", "(", null, ")")]
        CsTypeOfExpression TypeOfExpression(CsUnboundTypeName typeName);

        [Produce("typeof", "(", "void", ")")]
        CsTypeOfExpression TypeOfExpression();

        [Produce]
        CsUnboundTypeName UnboundTypeName(
                CsIdentifier id,
                Opt<CsGenericDimensionSpecifier> specifier);

        [Produce(null, "::", null, null)]
        CsUnboundTypeName UnboundTypeName(
                CsIdentifier id,
                CsIdentifier id2,
                Opt<CsGenericDimensionSpecifier> specifier);

        [Produce(null, ".", null, null)]
        CsUnboundTypeName UnboundTypeName(
                CsUnboundTypeName typeName,
                CsIdentifier      id,
                Opt<CsGenericDimensionSpecifier> specifier);

        [Produce("<", null, ">")]
        CsGenericDimensionSpecifier GenericDimensionSpecifier(CsCommas commas);

        [Produce("checked", "(", null, ")")]
        CsCheckedExpression CheckedExpression(CsExpression expression);

        [Produce("unchecked", "(", null, ")")]
        CsUncheckedExpression UncheckedExpression(CsExpression expression);

        [Produce("default", "(", null, ")")]
        CsDefaultValueExpression DefaultValueExpression(CsType type);

        [Produce]
        CsUnaryExpression UnaryExpression(CsPrimaryExpression operand);

        [Produce("+", null)]
        [Produce("-")]
        [Produce("!")]
        [Produce("~")]
        CsUnaryExpression UnaryExpression(CsUnaryExpression operand);

        [Produce]
        CsUnaryExpression UnaryExpression(CsPreIncrementExpression expression);

        [Produce]
        CsUnaryExpression UnaryExpression(CsPreDecrementExpression expression);

        [Produce]
        CsUnaryExpression UnaryExpression(CsCastExpression expression);

        [Produce("++")]
        CsPreIncrementExpression PreIncrementExpression(CsUnaryExpression operand);

        [Produce("--")]
        CsPreDecrementExpression PreDecrementExpression(CsUnaryExpression operand);

        [Produce("(", null, ")", null)]
        CsCastExpression CastExpression(CsType type, CsUnaryExpression expression);

        [Produce]
        CsMultiplicativeExpression MultiplicativeExpression(CsUnaryExpression expression);

        [Produce(null, "*", null)]
        CsMultiplicativeExpression MultiplicativeExpressionMultiply(
                CsMultiplicativeExpression x,
                CsUnaryExpression y);

        [Produce(null, "/", null)]
        CsMultiplicativeExpression MultiplicativeExpressionDivide(
                CsMultiplicativeExpression x,
                CsUnaryExpression y);

        [Produce(null, "%", null)]
        CsMultiplicativeExpression MultiplicativeExpressionRemainder(
                CsMultiplicativeExpression x,
                CsUnaryExpression y);

        [Produce]
        CsAdditiveExpression AdditiveExpression(
                CsMultiplicativeExpression expression);

        [Produce(null, "+", null)]
        CsAdditiveExpression AdditiveExpressionAdd(
                CsAdditiveExpression       x,
                CsMultiplicativeExpression y);

        [Produce(null, "-", null)]
        CsAdditiveExpression AdditiveExpressionSubstract(
                CsAdditiveExpression       x,
                CsMultiplicativeExpression y);

        [Produce]
        CsShiftExpression ShiftExpression(
                CsAdditiveExpression expression);

        [Produce(null, "<<", null)]
        CsShiftExpression ShiftExpressionLeftShift(
                CsShiftExpression    x,
                CsAdditiveExpression y);
        
        [Produce(null, ">>", null)]
        CsShiftExpression ShiftExpressionLeftRight(
                CsShiftExpression    x,
                CsAdditiveExpression y);

        [Produce]
        CsRelationalExpression RelationalExpression(
                CsShiftExpression shiftExpression);

        [Produce(null, "<", null)]
        CsRelationalExpression RelationalExpressionLess(
                CsRelationalExpression x,
                CsShiftExpression      y);

        [Produce(null, ">", null)]
        CsRelationalExpression RelationalExpressionGreater(
                CsRelationalExpression x,
                CsShiftExpression      y);

        [Produce(null, "<=", null)]
        CsRelationalExpression RelationalExpressionLessOrEqual(
                CsRelationalExpression x,
                CsShiftExpression      y);

        [Produce(null, ">=", null)]
        CsRelationalExpression RelationalExpressionGreaterOrEqual(
                CsRelationalExpression x,
                CsShiftExpression      y);

        [Produce(null, "is", null)]
        CsRelationalExpression RelationalExpressionIs(
                CsRelationalExpression expression,
                CsType                 type);

        [Produce(null, "as", null)]
        CsRelationalExpression RelationalExpressionAs(
                CsRelationalExpression expression,
                CsType                 type);

        [Produce]
        CsEquilityExpression EquilityExpression(
                CsRelationalExpression expression);

        [Produce(null, "==", null)]
        CsEquilityExpression EquilityExpressionEqual(
                CsEquilityExpression   x,
                CsRelationalExpression y);

        [Produce(null, "!=", null)]
        CsEquilityExpression EquilityExpressionNotEqual(
                CsEquilityExpression   x,
                CsRelationalExpression y);

        [Produce]
        CsAndExpression AndExpression(CsEquilityExpression expression);

        [Produce(null, "&", null)]
        CsAndExpression AndExpression(
                CsAndExpression      x,
                CsEquilityExpression y);

        [Produce]
        CsExclusiveOrExpression ExclusiveOrExpression(CsAndExpression expression);

        [Produce(null, "^", null)]
        CsExclusiveOrExpression ExclusiveOrExpression(
                CsExclusiveOrExpression x,
                CsAndExpression         y);

        [Produce]
        CsInclusiveOrExpression InclusiveOrExpression(CsExclusiveOrExpression expression);

        [Produce(null, "|", null)]
        CsInclusiveOrExpression InclusiveOrExpression(
                CsInclusiveOrExpression x,
                CsExclusiveOrExpression y);

        [Produce]
        CsConditionalAndExpression ConditionalEndExpression(CsInclusiveOrExpression expression);

        [Produce(null, "&&", null)]
        CsConditionalAndExpression ConditionalEndExpression(
                CsConditionalAndExpression x,
                CsInclusiveOrExpression    y);

        [Produce]
        CsConditionalOrExpression ConditionalOrExpression(CsConditionalAndExpression expression);

        [Produce(null, "||", null)]
        CsConditionalOrExpression ConditionalOrExpression(
                CsConditionalOrExpression  x,
                CsConditionalAndExpression y);

        [Produce]
        CsNullCoalescingExpression NullCoalescingExpression(CsConditionalOrExpression expression);

        [Produce(null, "??", null)]
        CsNullCoalescingExpression NullCoalescingExpression(
                CsNullCoalescingExpression x,
                CsConditionalOrExpression y);

        [Produce]
        CsConditionalExpression ConditionalExpression(CsNullCoalescingExpression expression);

        [Produce(null, "?", null, ":", null)]
        CsConditionalExpression ConditionalExpression(
                CsNullCoalescingExpression cond, 
                CsExpression               pos,
                CsExpression               neg);

        [Produce(null, "=>", null)]
        CsLambdaExpression LambdaExpression(
                CsAnonymousFunctionSignature signature,
                CsAnonymousFunctionBody      body);

        [Produce("delegate", null, null)]
        CsAnonymousMethodExpression AnonymousMethodExpression(
                Opt<CsExplicitAnonymousFunctionSignature> signature,
                CsBlock                                   block);

        [Produce]
        CsAnonymousFunctionSignature AnonymousFunctionSignature(
                CsExplicitAnonymousFunctionSignature signature);

        [Produce]
        CsAnonymousFunctionSignature AnonymousFunctionSignature(
                CsImplicitAnonymousFunctionSignature signature);

        [Produce("(", null, ")")]
        CsExplicitAnonymousFunctionSignature ExplicitAnonymousFunctionSignature(
                Opt<CsCommaList<CsExplicitAnonymousFunctionParameter>> parmeterList);

        [Produce]
        CsExplicitAnonymousFunctionParameter ExplicitAnonymousFunctionParameter(
                Opt<CsAnonymousFunctionParameterModifier> modifier,
                CsType                                    type,
                CsIdentifier                              identifier); 

        [Produce("ref")]
        [Produce("out")]
        CsAnonymousFunctionParameterModifier AnonymousFunctionParameterModifier();

        [Produce("(", null, ")")]
        CsImplicitAnonymousFunctionSignature ImplicitAnonymousFunctionSignature(
                Opt<CsCommaList<CsImplicitAnonymousFunctionParameter>> parameters);

        [Produce]
        CsImplicitAnonymousFunctionSignature ImplicitAnonymousFunctionSignature(
                CsImplicitAnonymousFunctionParameter parameter);

        [Produce]
        CsImplicitAnonymousFunctionParameter ImplicitAnonymousFunctionParameter(
                CsIdentifier identifier);

        [Produce]
        CsAnonymousFunctionBody AnonymousFunctionBody(CsExpression expression);

        [Produce]
        CsAnonymousFunctionBody AnonymousFunctionBody(CsBlock block);

        [Produce]
        CsQueryExpression QueryExpression(CsFromClause clause, CsQueryBody body);

        [Produce("from", null, null, "in", null)]
        CsFromClause FromClause(
                Opt<CsType>     type,
                CsIdentifier    identifier,
                CsExpression    expression);

        [Produce]
        CsQueryBody QueryBody(
                Opt<CsList<CsQueryBodyClause>>  queryBodyClauses, 
                CsSelectOrGroupClause           selectOrGroup,
                Opt<CsQueryContinuation>        continuation);

        [Produce]
        CsQueryBodyClause QueryBodyClause(CsFromClause clause);

        [Produce]
        CsQueryBodyClause QueryBodyClause(CsLetClause clause);

        [Produce]
        CsQueryBodyClause QueryBodyClause(CsWhereClause clause);

        [Produce]
        CsQueryBodyClause QueryBodyClause(CsJoinClause clause);

        [Produce]
        CsQueryBodyClause QueryBodyClause(CsJoinIntoClause clause);

        [Produce]
        CsQueryBodyClause QueryBodyClause(CsOrderByClause clause);

        [Produce("let", null, "=", null)]
        CsLetClause LetClause(CsIdentifier identifier, CsExpression expression);

        [Produce("where", null)]
        CsWhereClause WhereClause(CsBooleanExpression expression);

        [Produce("join", null, null, "in", null, "on", null, "equals", null)]
        CsJoinClause JoinClause(
                Opt<CsType>  type,
                CsIdentifier identifier,
                CsExpression expression1,
                CsExpression expression2,
                CsExpression expression3);

        [Produce("join", null, null, "in", null, "on", null, "equals", null, "into", null)]
        CsJoinIntoClause JoinIntoClause(
                Opt<CsType>  type,
                CsIdentifier identifier1,
                CsExpression expression1,
                CsExpression expression2,
                CsExpression expression3,
                CsIdentifier identifier2);

        [Produce]
        CsOrderByClause OrderByClause(CsCommaList<CsOrdering> orderings);

        [Produce]
        CsOrdering Ordering(CsExpression expression, Opt<CsOrderingDirection> direction);

        [Produce("ascending")]
        [Produce("descending")]
        CsOrderingDirection OrderingDirection();

        [Produce]
        CsSelectOrGroupClause SelectOrGroupClause(CsSelectClause clause);

        [Produce]
        CsSelectOrGroupClause SelectOrGroupClause(CsGroupClause clause);

        [Produce("select", null)]
        CsSelectClause SelectClause(CsExpression expression);

        [Produce("group", null, "by", null)]
        CsGroupClause GroupClause(
            CsExpression expression1,
            CsExpression expression2);

        [Produce("into", null, null)]
        CsQueryContinuation QueryContinuation(
                CsIdentifier identifier,
                CsQueryBody  queryBody);

        [Produce]
        CsAssignment Assignment(
                CsUnaryExpression    lhs,
                CsAssignmentOperator op,
                CsExpression         rhs);

        [Produce("=")]
        [Produce("+=")]
        [Produce("-=")]
        [Produce("*=")]
        [Produce("/=")]
        [Produce("%=")]
        [Produce("&=")]
        [Produce("|=")]
        [Produce("^=")]
        [Produce("<<=")]
        [Produce(">>=")]
        CsAssignmentOperator AssignmentOperator();

        [Produce]
        CsExpression Expression(CsNonAssignmentExpression expression);

        [Produce]
        CsExpression Expression(CsAssignmentOperator assignment);

        [Produce]
        CsNonAssignmentExpression NonAssignmentExpression(
                CsConditionalExpression expression);

        [Produce]
        CsNonAssignmentExpression NonAssignmentExpression(
                CsLambdaExpression expression);

        [Produce]
        CsNonAssignmentExpression NonAssignmentExpression(
                CsQueryExpression expression);

        [Produce]
        CsBooleanExpression BooleanExpression(CsExpression expression);

        [Produce]
        CsConstantExpression ConstantExpression(CsExpression expression);
    }
}
