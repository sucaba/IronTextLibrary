using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Parse]
        CsVariableReference VariableReference();

        [Parse]
        CsArgumentList ArgumentList(CsArgument arg);

        [Parse(null, ",", null)]
        CsArgumentList ArgumentList(CsArgumentList list, CsArgument arg);

        [Parse]
        CsArgument Argument(CsArgumentValue val);

        [Parse(null, ":", null)]
        CsArgument Argument(CsIdentifier name, CsArgumentValue val);

        [Parse]
        CsArgumentValue ArgumentValue(CsExpression expression);

        [Parse("ref")]
        CsArgumentValue ArgumentValueByRef(CsVariableReference variableRef);

        [Parse("out")]
        CsArgumentValue ArgumentValueByOut(CsVariableReference variableRef);

        [Parse]
        CsPrimaryExpression PrimaryExpression(CsPrimaryNoArrayCreationExpression expression);

        [Parse]
        CsPrimaryExpression PrimaryExpression(CsArrayCreationExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsLiteral literal);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsSimpleName name);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsParenthesizedExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsMemberAccess memberAccess);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsInvocationExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsElementAccess access);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsThisAccess thisAccess);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsBaseAccess thisAccess);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsPostIncrementExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsPostDecrementExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsObjectCreationExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsDelegateCreationExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsAnonymousObjectCreationExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsTypeOfExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsCheckedExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsUncheckedExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsDefaultValueExpression expression);

        [Parse]
        CsPrimaryNoArrayCreationExpression PrimaryNoArrayCreationExpression(CsAnonymousMethodExpression expression);

        [Parse]
        CsSimpleName SimpleName(CsIdentifier id, Opt<CsTypeArgumentList> typeArgList);

        [Parse("(", null, ")")]
        CsParenthesizedExpression ParenthesizedExpression(CsExpression expression);

        [Parse(null, ".", null)]
        CsMemberAccess MemberAccess(CsPrimaryExpression expression, Opt<CsTypeArgumentList> argList);

        [Parse(null, ".", null)]
        CsMemberAccess MemberAccess(CsPredefinedType type, Opt<CsTypeArgumentList> argList);

        [Parse(null, ".", null)]
        CsMemberAccess MemberAccess(CsQualifiedAliasMember alias, Opt<CsTypeArgumentList> argList);

        [Parse("bool")]
        [Parse("byte")]
        [Parse("char")]
        [Parse("decimal")]
        [Parse("double")]
        [Parse("float")]
        [Parse("int")]
        [Parse("long")]
        [Parse("object")]
        [Parse("sbyte")]
        [Parse("short")]
        [Parse("string")]
        [Parse("uint")]
        [Parse("ulong")]
        [Parse("ushort")]
        CsPredefinedType PredefinedType();

        [Parse(null, "(", null, ")")]
        CsInvocationExpression InvocationExpression(CsPrimaryExpression expression, Opt<CsArgumentList> args);

        [Parse(null, "[", null, "]")]
        CsElementAccess ElementAccess(CsPrimaryNoArrayCreationExpression expression, CsArgumentList args);

        [Parse("this")]
        CsThisAccess ThisAccess();

        [Parse("base", ".", null)]
        CsBaseAccess BaseAccess(CsIdentifier member);

        [Parse("base", "[", null, "]")]
        CsBaseAccess BaseAccess(CsArgumentList args);

        [Parse(null, "++")]
        CsPostIncrementExpression PostIncrementExpression(CsPrimaryExpression operand);

        [Parse(null, "--")]
        CsPostDecrementExpression PostDecrementExpression(CsPrimaryExpression operand);

        [Parse("new", null, "(", null, ")", null)]
        CsObjectCreationExpression ObjectCreationExpression(
                CsType type,
                Opt<CsArgumentList> args,
                Opt<CsObjectOrCollectionInitializer> initializer);

        [Parse("new", null, null)]
        CsObjectCreationExpression ObjectCreationExpression(
                CsType type,
                CsObjectOrCollectionInitializer initializer);

        [Parse]
        CsObjectOrCollectionInitializer ObjectOrCollectionInitializer(CsObjectInitializer initializer);

        [Parse]
        CsObjectOrCollectionInitializer ObjectOrCollectionInitializer(CsCollectionInitializer initializer);

        [Parse("{", null, "}")]
        CsObjectInitializer ObjectInitializer(Opt<CsCommaList<CsMemberInitializer>> initializer);

        [Parse("{", null, ",", "}")]
        CsObjectInitializer ObjectInitializer(CsCommaList<CsMemberInitializer> initializer);

        [Parse(null, "=", null)]
        CsMemberInitializer MemberInitializer(CsIdentifier id, CsInitializerValue val);

        [Parse]
        CsInitializerValue InitializerValue(CsExpression expression);

        [Parse]
        CsInitializerValue InitializerValue(CsObjectOrCollectionInitializer initializer);

        [Parse("{", null, "}")]
        [Parse("{", null, ",", "}")]
        CsCollectionInitializer CollectionInitializer(CsCommaList<CsElementInitializer> initializers);

        [Parse]
        CsElementInitializer ElementInitializer(CsNonAssignmentExpression expression);

        [Parse("{", null, "}")]
        CsElementInitializer ElementInitializer(CsCommaList<CsExpression> expression);

        [Parse("new", null, "[", null, "]", null, null)]
        CsArrayCreationExpression ArrayCreationExpression(
                CsNonArrayType               type,
                CsCommaList<CsExpression>    expressionList,
                Opt<CsList<CsRankSpecifier>> rankSpecifiers,
                Opt<CsArrayInitializer>      arrayInitializer);

        [Parse("new", null, null)]
        CsArrayCreationExpression ArrayCreationExpression(
                CsNonArrayType      type,
                CsArrayInitializer  arrayInitializer);

        [Parse("new", null, null)]
        CsArrayCreationExpression ArrayCreationExpression(
                CsRankSpecifier     rankSpecifier,
                CsArrayInitializer  arrayInitializer);

        [Parse("new", null, "(", null, ")")]
        CsDelegateCreationExpression DelegateCreationExpression(
                CsDelegateType type,
                CsExpression   expression);

        [Parse("new", null)]
        CsAnonymousObjectCreationExpression AnonymousObjectCreationExpression(
                CsAnonymousObjectInitializer initializer);

        [Parse("{", null, "}")]
        CsAnonymousObjectInitializer AnonymousObjectInitializer(
            Opt<CsCommaList<CsMemberDeclarator>> declarators);

        [Parse("{", null, ",", "}")]
        CsAnonymousObjectInitializer AnonymousObjectInitializer(
            CsCommaList<CsMemberDeclarator> declarators);

        [Parse]
        CsMemberDeclarator MemberDeclarator(CsSimpleName name);

        [Parse]
        CsMemberDeclarator MemberDeclarator(CsMemberAccess access);

        [Parse(null, "=", null)]
        CsMemberDeclarator MemberDeclarator(CsIdentifier access, CsExpression expression);

        [Parse("typeof", "(", null, ")")]
        CsTypeOfExpression TypeOfExpression(CsType type);

        [Parse("typeof", "(", null, ")")]
        CsTypeOfExpression TypeOfExpression(CsUnboundTypeName typeName);

        [Parse("typeof", "(", "void", ")")]
        CsTypeOfExpression TypeOfExpression();

        [Parse]
        CsUnboundTypeName UnboundTypeName(
                CsIdentifier id,
                Opt<CsGenericDimensionSpecifier> specifier);

        [Parse(null, "::", null, null)]
        CsUnboundTypeName UnboundTypeName(
                CsIdentifier id,
                CsIdentifier id2,
                Opt<CsGenericDimensionSpecifier> specifier);

        [Parse(null, ".", null, null)]
        CsUnboundTypeName UnboundTypeName(
                CsUnboundTypeName typeName,
                CsIdentifier      id,
                Opt<CsGenericDimensionSpecifier> specifier);

        [Parse("<", null, ">")]
        CsGenericDimensionSpecifier GenericDimensionSpecifier(CsCommas commas);

        [Parse("checked", "(", null, ")")]
        CsCheckedExpression CheckedExpression(CsExpression expression);

        [Parse("unchecked", "(", null, ")")]
        CsUncheckedExpression UncheckedExpression(CsExpression expression);

        [Parse("default", "(", null, ")")]
        CsDefaultValueExpression DefaultValueExpression(CsType type);

        [Parse]
        CsUnaryExpression UnaryExpression(CsPrimaryExpression operand);

        [Parse("+", null)]
        [Parse("-")]
        [Parse("!")]
        [Parse("~")]
        CsUnaryExpression UnaryExpression(CsUnaryExpression operand);

        [Parse]
        CsUnaryExpression UnaryExpression(CsPreIncrementExpression expression);

        [Parse]
        CsUnaryExpression UnaryExpression(CsPreDecrementExpression expression);

        [Parse]
        CsUnaryExpression UnaryExpression(CsCastExpression expression);

        [Parse("++")]
        CsPreIncrementExpression PreIncrementExpression(CsUnaryExpression operand);

        [Parse("--")]
        CsPreDecrementExpression PreDecrementExpression(CsUnaryExpression operand);

        [Parse("(", null, ")", null)]
        CsCastExpression CastExpression(CsType type, CsUnaryExpression expression);

        [Parse]
        CsMultiplicativeExpression MultiplicativeExpression(CsUnaryExpression expression);

        [Parse(null, "*", null)]
        CsMultiplicativeExpression MultiplicativeExpressionMultiply(
                CsMultiplicativeExpression x,
                CsUnaryExpression y);

        [Parse(null, "/", null)]
        CsMultiplicativeExpression MultiplicativeExpressionDivide(
                CsMultiplicativeExpression x,
                CsUnaryExpression y);

        [Parse(null, "%", null)]
        CsMultiplicativeExpression MultiplicativeExpressionRemainder(
                CsMultiplicativeExpression x,
                CsUnaryExpression y);

        [Parse]
        CsAdditiveExpression AdditiveExpression(
                CsMultiplicativeExpression expression);

        [Parse(null, "+", null)]
        CsAdditiveExpression AdditiveExpressionAdd(
                CsAdditiveExpression       x,
                CsMultiplicativeExpression y);

        [Parse(null, "-", null)]
        CsAdditiveExpression AdditiveExpressionSubstract(
                CsAdditiveExpression       x,
                CsMultiplicativeExpression y);

        [Parse]
        CsShiftExpression ShiftExpression(
                CsAdditiveExpression expression);

        [Parse(null, "<<", null)]
        CsShiftExpression ShiftExpressionLeftShift(
                CsShiftExpression    x,
                CsAdditiveExpression y);
        
        [Parse(null, ">>", null)]
        CsShiftExpression ShiftExpressionLeftRight(
                CsShiftExpression    x,
                CsAdditiveExpression y);

        [Parse]
        CsRelationalExpression RelationalExpression(
                CsShiftExpression shiftExpression);

        [Parse(null, "<", null)]
        CsRelationalExpression RelationalExpressionLess(
                CsRelationalExpression x,
                CsShiftExpression      y);

        [Parse(null, ">", null)]
        CsRelationalExpression RelationalExpressionGreater(
                CsRelationalExpression x,
                CsShiftExpression      y);

        [Parse(null, "<=", null)]
        CsRelationalExpression RelationalExpressionLessOrEqual(
                CsRelationalExpression x,
                CsShiftExpression      y);

        [Parse(null, ">=", null)]
        CsRelationalExpression RelationalExpressionGreaterOrEqual(
                CsRelationalExpression x,
                CsShiftExpression      y);

        [Parse(null, "is", null)]
        CsRelationalExpression RelationalExpressionIs(
                CsRelationalExpression expression,
                CsType                 type);

        [Parse(null, "as", null)]
        CsRelationalExpression RelationalExpressionAs(
                CsRelationalExpression expression,
                CsType                 type);

        [Parse]
        CsEquilityExpression EquilityExpression(
                CsRelationalExpression expression);

        [Parse(null, "==", null)]
        CsEquilityExpression EquilityExpressionEqual(
                CsEquilityExpression   x,
                CsRelationalExpression y);

        [Parse(null, "!=", null)]
        CsEquilityExpression EquilityExpressionNotEqual(
                CsEquilityExpression   x,
                CsRelationalExpression y);

        [Parse]
        CsAndExpression AndExpression(CsEquilityExpression expression);

        [Parse(null, "&", null)]
        CsAndExpression AndExpression(
                CsAndExpression      x,
                CsEquilityExpression y);

        [Parse]
        CsExclusiveOrExpression ExclusiveOrExpression(CsAndExpression expression);

        [Parse(null, "^", null)]
        CsExclusiveOrExpression ExclusiveOrExpression(
                CsExclusiveOrExpression x,
                CsAndExpression         y);

        [Parse]
        CsInclusiveOrExpression InclusiveOrExpression(CsExclusiveOrExpression expression);

        [Parse(null, "|", null)]
        CsInclusiveOrExpression InclusiveOrExpression(
                CsInclusiveOrExpression x,
                CsExclusiveOrExpression y);

        [Parse]
        CsConditionalAndExpression ConditionalEndExpression(CsInclusiveOrExpression expression);

        [Parse(null, "&&", null)]
        CsConditionalAndExpression ConditionalEndExpression(
                CsConditionalAndExpression x,
                CsInclusiveOrExpression    y);

        [Parse]
        CsConditionalOrExpression ConditionalOrExpression(CsConditionalAndExpression expression);

        [Parse(null, "||", null)]
        CsConditionalOrExpression ConditionalOrExpression(
                CsConditionalOrExpression  x,
                CsConditionalAndExpression y);

        [Parse]
        CsNullCoalescingExpression NullCoalescingExpression(CsConditionalOrExpression expression);

        [Parse(null, "??", null)]
        CsNullCoalescingExpression NullCoalescingExpression(
                CsNullCoalescingExpression x,
                CsConditionalOrExpression y);

        [Parse]
        CsConditionalExpression ConditionalExpression(CsNullCoalescingExpression expression);

        [Parse(null, "?", null, ":", null)]
        CsConditionalExpression ConditionalExpression(
                CsNullCoalescingExpression cond, 
                CsExpression               pos,
                CsExpression               neg);

        [Parse(null, "=>", null)]
        CsLambdaExpression LambdaExpression(
                CsAnonymousFunctionSignature signature,
                CsAnonymousFunctionBody      body);

        [Parse("delegate", null, null)]
        CsAnonymousMethodExpression AnonymousMethodExpression(
                Opt<CsExplicitAnonymousFunctionSignature> signature,
                CsBlock                                   block);

        [Parse]
        CsAnonymousFunctionSignature AnonymousFunctionSignature(
                CsExplicitAnonymousFunctionSignature signature);

        [Parse]
        CsAnonymousFunctionSignature AnonymousFunctionSignature(
                CsImplicitAnonymousFunctionSignature signature);

        [Parse("(", null, ")")]
        CsExplicitAnonymousFunctionSignature ExplicitAnonymousFunctionSignature(
                Opt<CsCommaList<CsExplicitAnonymousFunctionParameter>> parmeterList);

        [Parse]
        CsExplicitAnonymousFunctionParameter ExplicitAnonymousFunctionParameter(
                Opt<CsAnonymousFunctionParameterModifier> modifier,
                CsType                                    type,
                CsIdentifier                              identifier); 

        [Parse("ref")]
        [Parse("out")]
        CsAnonymousFunctionParameterModifier AnonymousFunctionParameterModifier();

        [Parse("(", null, ")")]
        CsImplicitAnonymousFunctionSignature ImplicitAnonymousFunctionSignature(
                Opt<CsCommaList<CsImplicitAnonymousFunctionParameter>> parameters);

        [Parse]
        CsImplicitAnonymousFunctionSignature ImplicitAnonymousFunctionSignature(
                CsImplicitAnonymousFunctionParameter parameter);

        [Parse]
        CsImplicitAnonymousFunctionParameter ImplicitAnonymousFunctionParameter(
                CsIdentifier identifier);

        [Parse]
        CsAnonymousFunctionBody AnonymousFunctionBody(CsExpression expression);

        [Parse]
        CsAnonymousFunctionBody AnonymousFunctionBody(CsBlock block);

        [Parse]
        CsQueryExpression QueryExpression(CsFromClause clause, CsQueryBody body);

        [Parse("from", null, null, "in", null)]
        CsFromClause FromClause(
                Opt<CsType>     type,
                CsIdentifier    identifier,
                CsExpression    expression);

        [Parse]
        CsQueryBody QueryBody(
                Opt<CsList<CsQueryBodyClause>>  queryBodyClauses, 
                CsSelectOrGroupClause           selectOrGroup,
                Opt<CsQueryContinuation>        continuation);

        [Parse]
        CsQueryBodyClause QueryBodyClause(CsFromClause clause);

        [Parse]
        CsQueryBodyClause QueryBodyClause(CsLetClause clause);

        [Parse]
        CsQueryBodyClause QueryBodyClause(CsWhereClause clause);

        [Parse]
        CsQueryBodyClause QueryBodyClause(CsJoinClause clause);

        [Parse]
        CsQueryBodyClause QueryBodyClause(CsJoinIntoClause clause);

        [Parse]
        CsQueryBodyClause QueryBodyClause(CsOrderByClause clause);

        [Parse("let", null, "=", null)]
        CsLetClause LetClause(CsIdentifier identifier, CsExpression expression);

        [Parse("where", null)]
        CsWhereClause WhereClause(CsBooleanExpression expression);

        [Parse("join", null, null, "in", null, "on", null, "equals", null)]
        CsJoinClause JoinClause(
                Opt<CsType>  type,
                CsIdentifier identifier,
                CsExpression expression1,
                CsExpression expression2);

        [Parse("join", null, null, "in", null, "on", null, "equals", null, "into", null)]
        CsJoinIntoClause JoinIntoClause(
                Opt<CsType>  type,
                CsIdentifier identifier1,
                CsExpression expression1,
                CsExpression expression2,
                CsIdentifier identifier2);

        [Parse]
        CsOrderByClause OrderByClause(CsCommaList<CsOrdering> orderings);

        [Parse]
        CsOrdering Ordering(CsExpression expression, Opt<CsOrderingDirection> direction);

        [Parse("ascending")]
        [Parse("descending")]
        CsOrderingDirection OrderingDirection();

        [Parse]
        CsSelectOrGroupClause SelectOrGroupClause(CsSelectClause clause);

        [Parse]
        CsSelectOrGroupClause SelectOrGroupClause(CsGroupClause clause);

        [Parse("select", null)]
        CsSelectClause SelectClause(CsExpression expression);

        [Parse("group", null, "by", null)]
        CsGroupClause GroupClause();

        [Parse("into", null, null)]
        CsQueryContinuation QueryContinuation(
                CsIdentifier identifier,
                CsQueryBody  queryBody);

        [Parse]
        CsAssignment Assignment(
                CsUnaryExpression    lhs,
                CsAssignmentOperator op,
                CsExpression         rhs);

        [Parse("=")]
        [Parse("+=")]
        [Parse("-=")]
        [Parse("*=")]
        [Parse("/=")]
        [Parse("%=")]
        [Parse("&=")]
        [Parse("|=")]
        [Parse("^=")]
        [Parse("<<=")]
        [Parse(">>=")]
        CsAssignmentOperator AssignmentOperator();

        [Parse]
        CsExpression Expression(CsNonAssignmentExpression expression);

        [Parse]
        CsExpression Expression(CsAssignmentOperator assignment);

        [Parse]
        CsNonAssignmentExpression NonAssignmentExpression(
                CsConditionalExpression expression);

        [Parse]
        CsNonAssignmentExpression NonAssignmentExpression(
                CsLambdaExpression expression);

        [Parse]
        CsNonAssignmentExpression NonAssignmentExpression(
                CsQueryExpression expression);

        [Parse]
        CsBooleanExpression BooleanExpression(CsExpression expression);

        [Parse]
        CsConstantExpression ConstantExpression(CsExpression expression);
    }
}
