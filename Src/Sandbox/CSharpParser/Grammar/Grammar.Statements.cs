using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronText.Framework;

namespace CSharpParser
{
    public partial interface ICsGrammar
    {
        [Produce]
        CsStatement Statement(CsLabelledStatement statement);

        [Produce]
        CsStatement Statement(CsDeclarationStatement statement);

        [Produce]
        CsStatement Statement(CsEmbeddedStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsBlock block);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsEmptyStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsExpressionStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsSelectionStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsIterationStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsJumpStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsTryStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsCheckedStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsUncheckedStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsLockStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsUsingStatement statement);

        [Produce]
        CsEmbeddedStatement EmbeddedStatement(CsYieldStatement statement);

        [Produce("{", null, "}")]
        CsBlock Block(Opt<CsList<CsStatement>> statements);

        [Produce(";")]
        CsEmptyStatement EmptyStatement();

        [Produce(null, ":", null)]
        CsLabelledStatement LabelledStatement(
                CsIdentifier label,
                CsStatement  statement);

        [Produce(null, ";")]
        CsDeclarationStatement DeclarationStatement(CsLocalVariableDeclaration decl);

        [Produce(null, ";")]
        CsDeclarationStatement DeclarationStatement(CsLocalConstantDeclaration decl);

        [Produce]
        CsLocalVariableDeclaration LocalVariableDeclaration(
                CsLocalVariableType                    type,
                CsCommaList<CsLocalVariableDeclarator> declarators);

        [Produce("var")]
        CsLocalVariableType LocalVariableType();

        [Produce]
        CsLocalVariableType LocalVariableType(CsType type);

        [Produce]
        CsLocalVariableDeclarator LocalVariableDeclarator(
                CsIdentifier identifier);

        [Produce(null, "=", null)]
        CsLocalVariableDeclarator LocalVariableDeclarator(
                CsIdentifier               identifier,
                CsLocalVariableInitializer initializer);

        [Produce]
        CsLocalVariableInitializer LocalVariableInitializer(
                CsExpression expression); 

        [Produce]
        CsLocalVariableInitializer LocalVariableInitializer(
                CsArrayInitializer initializer); 

        [Produce("const", null, null)]
        CsLocalConstantDeclaration LocalConstantDeclaration(
                CsType                            type,
                CsCommaList<CsConstantDeclarator> declarators);

        [Produce(null, "=", null)]
        CsConstantDeclarator ConstantDeclarator(
                CsIdentifier         id,
                CsConstantExpression expression);

        [Produce(null, ";")]
        CsExpressionStatement ExpressionStatement(CsStatementExpression expression);

        [Produce]
        CsStatementExpression StatementExpression(CsInvocationExpression expression);

        [Produce]
        CsStatementExpression StatementExpression(CsObjectCreationExpression expression);

        [Produce]
        CsStatementExpression StatementExpression(CsAssignment assignment);

        [Produce]
        CsStatementExpression StatementExpression(CsPostIncrementExpression expression);

        [Produce]
        CsStatementExpression StatementExpression(CsPostDecrementExpression expression);

        [Produce]
        CsStatementExpression StatementExpression(CsPreIncrementExpression expression);

        [Produce]
        CsStatementExpression StatementExpression(CsPreDecrementExpression expression);

        [Produce]
        CsSelectionStatement SelectionStatement(CsIfStatement statement);

        [Produce]
        CsSelectionStatement SelectionStatement(CsSwitchStatement statement);

        [Produce("if", "(", null, ")", null)]
        CsIfStatement IfStatement(
                CsBooleanExpression condition,
                CsExpression        expression);

        [Produce("if", "(", null, ")", null, "else", null)]
        CsIfStatement IfStatement(
                CsBooleanExpression condition,
                CsExpression        expression1,
                CsExpression        expression2);

        [Produce("switch", "(", null, ")", null)]
        CsSwitchStatement SwitchStatement(
                CsExpression expression,
                CsSwitchBlock block);

        [Produce("{", null, "}")]
        CsSwitchBlock SwitchBlock(Opt<CsList<CsSwitchSection>> sections);

        [Produce]
        CsSwitchSection SwitchSection(
                CsList<CsSwitchLabel> labels,
                CsList<CsStatement> statements);

        [Produce("case", null, ":")]
        CsSwitchLabel SwitchLabel(CsConstantExpression expression);

        [Produce("default", ":")]
        CsSwitchLabel SwitchLabel();

        [Produce]
        CsIterationStatement IterationStatement(CsWhileStatement statement);

        [Produce]
        CsIterationStatement IterationStatement(CsDoStatement statement);

        [Produce]
        CsIterationStatement IterationStatement(CsForStatement statement);

        [Produce]
        CsIterationStatement IterationStatement(CsForeachStatement statement);

        [Produce("while", "(", null, ")", null)]
        CsWhileStatement WhileStatement(
                CsBooleanExpression condition,
                CsEmbeddedStatement statement);

        [Produce("do", null, "while", "(", null, ")", ";")]
        CsDoStatement DoStatement(
                CsEmbeddedStatement statement,
                CsBooleanExpression condition);

        [Produce("for", "(", null, ";", null, ";", null, ")", null)]
        CsForStatement ForStatement(
                Opt<CsForInitializer> initializer,
                Opt<CsForCondition>   condition,
                Opt<CsForIterator>    iterator,
                CsEmbeddedStatement   statement);

        [Produce]
        CsForInitializer ForInitializer(CsLocalVariableDeclaration declaration);

        [Produce]
        CsForInitializer ForInitializer(CsCommaList<CsStatementExpression> expressions);


        [Produce]
        CsForCondition ForCondition(CsBooleanExpression expression);

        [Produce]
        CsForIterator ForIterator(CsCommaList<CsStatementExpression> expressions);

        [Produce("foreach", "(", null, null, "in", null, ")", null)]
        CsForeachStatement ForeachStatement(
                CsLocalVariableType type,
                CsIdentifier        identifier,
                CsExpression        expression,
                CsEmbeddedStatement statement);
        
        [Produce]
        CsJumpStatement JumpStatement(CsBreakStatement statement);

        [Produce]
        CsJumpStatement JumpStatement(CsContinueStatement statement);

        [Produce]
        CsJumpStatement JumpStatement(CsGotoStatement statement);

        [Produce]
        CsJumpStatement JumpStatement(CsReturnStatement statement);

        [Produce]
        CsJumpStatement JumpStatement(CsThrowStatement statement);

        [Produce("break", ";")]
        CsBreakStatement BreakStatement();

        [Produce("continue", ";")]
        CsContinueStatement ContinueStatement();

        [Produce("goto", null, ";")]
        CsGotoStatement GotoStatement(CsIdentifier label);
        
        [Produce("goto", "case", null, ";")]
        CsGotoStatement GotoStatement(CsConstantExpression expression);

        [Produce("goto", "default", ";")]
        CsGotoStatement GotoStatement();

        [Produce("return", null, ";")]
        CsReturnStatement ReturnStatement(Opt<CsExpression> expression);

        [Produce("throw", null, ";")]
        CsThrowStatement ThrowStatement(Opt<CsExpression> expression);

        [Produce("try", null, null)]
        CsTryStatement TryStatement(
                CsBlock        block,
                CsCatchClauses catchClauses);

        [Produce("try", null, null)]
        CsTryStatement TryStatement(
                CsBlock         block,
                CsFinallyClause finallyClause);

        [Produce("try", null, null, null)]
        CsTryStatement TryStatement(
                CsBlock         block,
                CsCatchClauses  catchClauses,
                CsFinallyClause finallyClause);

        [Produce]
        CsCatchClauses CatchClauses(
                CsList<CsSpecificCatchClause>     specific,
                Opt<CsList<CsGeneralCatchClause>> general);

        [Produce]
        CsCatchClauses CatchClauses(
                Opt<CsList<CsSpecificCatchClause>> specific,
                CsList<CsGeneralCatchClause>       general);

        [Produce("catch", "(", null, null, ")", null)]
        CsSpecificCatchClause SpecificCatchClause(
                CsClassType       type,
                Opt<CsIdentifier> identifier,
                CsBlock           block);

        [Produce("catch", null)]
        CsGeneralCatchClause GeneralCatchClause(
                CsBlock            block);

        [Produce("finally", null)]
        CsFinallyClause FinallyClause(
                CsBlock            block);

        [Produce("checked", null)]
        CsCheckedStatement CheckedStatement(
                CsBlock            block);

        [Produce("unchecked", null)]
        CsUncheckedStatement UncheckedStatement(
                CsBlock            block);

        [Produce("lock", "(", null, ")", null)]
        CsLockStatement LockStatement(
                CsExpression        expression,
                CsEmbeddedStatement statement);

        [Produce("using", "(", null, ")", null)]
        CsUsingStatement UsingStatement(
                CsResourceAcquisition acquisition,
                CsEmbeddedStatement   statement);

        [Produce]
        CsResourceAcquisition ResourceAcquisition(
                CsLocalVariableDeclaration decl);

        [Produce]
        CsResourceAcquisition ResourceAcquisition(
                CsExpression               expression);

        [Produce("yield", "return", null, ";")]
        CsYieldStatement YieldStatement(CsExpression expression);

        [Produce("yield", "break", ";")]
        CsYieldStatement YieldStatement();

        [Produce]
        CsCommas Commas();

        [Produce(null, ",")]
        CsCommas Commas(CsCommas commas);
    }
}
