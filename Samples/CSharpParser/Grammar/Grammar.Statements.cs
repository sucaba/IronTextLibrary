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
        CsStatement Statement(CsLabelledStatement statement);

        [Parse]
        CsStatement Statement(CsDeclarationStatement statement);

        [Parse]
        CsStatement Statement(CsEmbeddedStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsBlock block);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsEmptyStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsExpressionStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsSelectionStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsIterationStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsJumpStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsTryStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsCheckedStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsUncheckedStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsLockStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsUsingStatement statement);

        [Parse]
        CsEmbeddedStatement EmbeddedStatement(CsYieldStatement statement);

        [Parse("{", null, "}")]
        CsBlock Block(CsList<CsStatement> statements);

        [Parse(";")]
        CsEmptyStatement EmptyStatement();

        [Parse(null, ":", null)]
        CsLabelledStatement LabelledStatement(
                CsIdentifier label,
                CsStatement  statement);

        [Parse(null, ";")]
        CsDeclarationStatement DeclarationStatement(CsLocalVariableDeclaration decl);

        [Parse(null, ";")]
        CsDeclarationStatement DeclarationStatement(CsLocalConstantDeclaration decl);

        [Parse]
        CsLocalVariableDeclaration LocalVariableDeclaration(
                CsLocalVariableType                    type,
                CsCommaList<CsLocalVariableDeclarator> declarators);

        [Parse]
        CsLocalVariableDeclarator LocalVariableDeclarator(
                CsIdentifier identifier);

        [Parse(null, "=", null)]
        CsLocalVariableDeclarator LocalVariableDeclarator(
                CsIdentifier               identifier,
                CsLocalVariableInitializer initializer);

        [Parse]
        CsLocalVariableInitializer LocalVariableInitializer(
                CsExpression expression); 

        [Parse]
        CsLocalVariableInitializer LocalVariableInitializer(
                CsArrayInitializer initializer); 

        [Parse("const", null, null)]
        CsLocalConstantDeclaration LocalConstantDeclaration(
                CsType                            type,
                CsCommaList<CsConstantDeclarator> declarators);

        [Parse(null, "=", null)]
        CsConstantDeclarator ConstantDeclarator(
                CsIdentifier         id,
                CsConstantExpression expression);

        [Parse(null, ";")]
        CsExpressionStatement ExpressionStatement(CsStatementExpression expression);

        [Parse]
        CsStatementExpression StatementExpression(CsInvocationExpression expression);

        [Parse]
        CsStatementExpression StatementExpression(CsObjectCreationExpression expression);

        [Parse]
        CsStatementExpression StatementExpression(CsAssignment assignment);

        [Parse]
        CsStatementExpression StatementExpression(CsPostIncrementExpression expression);

        [Parse]
        CsStatementExpression StatementExpression(CsPostDecrementExpression expression);

        [Parse]
        CsStatementExpression StatementExpression(CsPreIncrementExpression expression);

        [Parse]
        CsStatementExpression StatementExpression(CsPreDecrementExpression expression);

        [Parse]
        CsSelectionStatement SelectionStatement(CsIfStatement statement);

        [Parse]
        CsSelectionStatement SelectionStatement(CsSwitchStatement statement);

        [Parse("if", "(", null, ")", null)]
        CsIfStatement IfStatement(
                CsBooleanExpression condition,
                CsExpression        expression);

        [Parse("if", "(", null, ")", null, "else", null)]
        CsIfStatement IfStatement(
                CsBooleanExpression condition,
                CsExpression        expression1,
                CsExpression        expression2);

        [Parse("switch", "(", null, ")", null)]
        CsSwitchStatement SwitchStatement(
                CsExpression expression,
                CsSwitchBlock block);

        [Parse("{", null, "}")]
        CsSwitchBlock SwitchBlock(Opt<CsList<CsSwitchSection>> sections);

        [Parse]
        CsSwitchSection SwitchSection(
                CsList<CsSwitchLabel> labels,
                CsList<CsStatement> statements);

        [Parse("case", null, ":")]
        CsSwitchLabel SwitchLabel(CsConstantExpression expression);

        [Parse("default", ":")]
        CsSwitchLabel SwitchLabel();

        [Parse]
        CsIterationStatement IterationStatement(CsWhileStatement statement);

        [Parse]
        CsIterationStatement IterationStatement(CsDoStatement statement);

        [Parse]
        CsIterationStatement IterationStatement(CsForStatement statement);

        [Parse]
        CsIterationStatement IterationStatement(CsForeachStatement statement);

        [Parse("while", "(", null, ")", null)]
        CsWhileStatement WhileStatement(
                CsBooleanExpression condition,
                CsEmbeddedStatement statement);

        [Parse("do", null, "while", "(", null, ")", ";")]
        CsDoStatement DoStatement(
                CsEmbeddedStatement statement,
                CsBooleanExpression condition);

        [Parse("for", "(", null, ";", null, ";", null, ")", null)]
        CsForStatement ForStatement(
                Opt<CsForInitializer> initializer,
                Opt<CsForCondition>   condition,
                Opt<CsForIterator>    iterator,
                CsEmbeddedStatement   statement);

        [Parse]
        CsForInitializer ForInitializer(CsLocalVariableDeclaration declaration);

        [Parse]
        CsForInitializer ForInitializer(CsCommaList<CsStatementExpression> expressions);


        [Parse]
        CsForCondition ForCondition(CsBooleanExpression expression);

        [Parse]
        CsForIterator ForIterator(CsCommaList<CsStatementExpression> expressions);

        [Parse("foreach", "(", null, null, "in", null, ")", null)]
        CsForeachStatement ForeachStatement(
                CsLocalVariableType type,
                CsIdentifier        identifier,
                CsExpression        expression,
                CsEmbeddedStatement statement);
        
        [Parse]
        CsJumpStatement JumpStatement(CsBreakStatement statement);

        [Parse]
        CsJumpStatement JumpStatement(CsContinueStatement statement);

        [Parse]
        CsJumpStatement JumpStatement(CsGotoStatement statement);

        [Parse]
        CsJumpStatement JumpStatement(CsReturnStatement statement);

        [Parse]
        CsJumpStatement JumpStatement(CsThrowStatement statement);

        [Parse("break", ";")]
        CsBreakStatement BreakStatement();

        [Parse("continue", ";")]
        CsContinueStatement ContinueStatement();

        [Parse("goto", null, ";")]
        CsGotoStatement GotoStatement(CsIdentifier label);
        
        [Parse("goto", "case", null, ";")]
        CsGotoStatement GotoStatement(CsConstantExpression expression);

        [Parse("goto", "default", ";")]
        CsGotoStatement GotoStatement();

        [Parse("return", null, ";")]
        CsReturnStatement ReturnStatement(Opt<CsExpression> expression);

        [Parse("throw", null, ";")]
        CsThrowStatement ThrowStatement(Opt<CsExpression> expression);

        [Parse("try", null, null)]
        CsTryStatement TryStatement(
                CsBlock        block,
                CsCatchClauses catchClauses);

        [Parse("try", null, null)]
        CsTryStatement TryStatement(
                CsBlock         block,
                CsFinallyClause finallyClause);

        [Parse("try", null, null, null)]
        CsTryStatement TryStatement(
                CsBlock         block,
                CsCatchClauses  catchClauses,
                CsFinallyClause finallyClause);

        [Parse]
        CsCatchClauses CatchClauses(
                CsList<CsSpecificCatchClause>     specific,
                Opt<CsList<CsGeneralCatchClause>> general);

        [Parse]
        CsCatchClauses CatchClauses(
                Opt<CsList<CsSpecificCatchClause>> specific,
                CsList<CsGeneralCatchClause>       general);

        [Parse("catch", "(", null, null, ")", null)]
        CsSpecificCatchClause SpecificCatchClause(
                CsClassType       type,
                Opt<CsIdentifier> identifier,
                CsBlock           block);

        [Parse("catch", null)]
        CsGeneralCatchClause GeneralCatchClause(
                CsBlock            block);

        [Parse("finally", null)]
        CsFinallyClause FinallyClause(
                CsBlock            block);

        [Parse("checked", null)]
        CsCheckedStatement CheckedStatement(
                CsBlock            block);

        [Parse("unchecked", null)]
        CsUncheckedStatement UncheckedStatement(
                CsBlock            block);

        [Parse("lock", "(", null, ")", null)]
        CsLockStatement LockStatement(
                CsExpression        expression,
                CsEmbeddedStatement statement);

        [Parse("using", "(", null, ")", null)]
        CsUsingStatement UsingStatement(
                CsResourceAcquisition acquisition,
                CsEmbeddedStatement   statement);

        [Parse]
        CsResourceAcquisition ResourceAcquisition(
                CsLocalVariableDeclaration decl);

        [Parse]
        CsResourceAcquisition ResourceAcquisition(
                CsExpression               expression);

        [Parse("yield", "return", null, ";")]
        CsYieldStatement YieldStatement(CsExpression expression);

        [Parse("yield", "break", ";")]
        CsYieldStatement YieldStatement();

        [Parse]
        CsCommas Commas();

        [Parse(null, ",")]
        CsCommas Commas(CsCommas commas);
    }
}
