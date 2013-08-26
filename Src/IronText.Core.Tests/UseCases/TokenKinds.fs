#light

module TokenKinds

open System

// Goal: 
//       Having token references and scan rule definitions replace token references with token identities
//       with complete token information.
//       Additionally provide error list for all conflicting situaitons.
// 
// Input:
// - TokenRef list
// - ScanRule list
//
// Output:
// - Map<TokenRef, TokenEntry> for token resolution 
// - Errors:
//      - List of conflicts in ScanRule list:
//           1. Conflicting literal rules matching same literal
//           2. *More ??*
//      - List of problems with TokenRef list:
//           1. Referencing token type which has no corresponding rules
//           2. Referencing literal which cannot be used for token identification
//      - List of literals which need implicit ScanRule definition

type ErrorInfo = Error of string

type TType = TType of string

let ttype<'T> = TType typeof<'T>.FullName

type Failable<'T> = {
        Value : 'T;
        Errors : ErrorInfo list;
    }

let getErrors<'T> (f : Failable<'T>) = f.Errors
let getValue<'T> (f : Failable<'T>) = f.Value

let fail message = { Value = (); Errors = [Error message]; }

type FailableBuilder() = 
    member this.Bind<'T,'U>(x : Failable<'T>, f : 'T -> Failable<'U>) =
        // printfn "bind %A ..." x
        match x with
        | { Value = v; Errors = pendingErrors } -> 
            let { Value = r; Errors = e } = f(v)
            { Value = r; Errors = pendingErrors @ e }

    member this.For(items, f) =
        let errors = [for item in items do
                        yield! getErrors (f item)]
        { Value = (); Errors = errors }

    member this.Delay(f) = 
        //printfn "delay %A" f
        f()

    member this.Return(x) = { Value = x; Errors = [] }

    member this.ReturnFrom<'T>(x : Failable<'T>) = x

    member this.Combine(x,y) = 
        printfn "combine"
        let { Errors = xErrors } = x
        let { Value = yValue; Errors = yErrors } = y
        { Value = yValue; Errors = xErrors @ yErrors }

let failable = FailableBuilder()

type ScanPattern = ScanPattern of string

type TokenEntry = {
        Literal    : string option;
        Type       : TType option;
        IsImplicit : bool;
    }

type TokenRef 
    = LiteralRef of string
    | TypeRef of TType

type ScanRule 
    = LiteralRule of string * (TType option)
    | PatternRule of ScanPattern * TType
//    | MultiTokenRule of string * (TType list)

type TokenAcces
    = GrantedType of TType
    | GrantedLiteral of string
    | DeniedLiteral of string

type TokensAccess = {
    GrantedLiterals : string Set;
    DeniedLiterals  : string Set;
    GrantedTypes    : TType Set;
    }

type ScannerConflict
    = ScannerConflict of string

let literalText = function
    | LiteralRule(text, _) -> text
    | PatternRule(patt, _) -> failwith "Non-literal"

let tokenType = function
    | LiteralRule(_, (Some t)) -> t
    | PatternRule(_, t)        -> t
    | _ -> failwith "Untyped scan rule"

let isLiteral = function | LiteralRule _ -> true | _ -> false

let isTypedToken = function
    | LiteralRule(_, (Some t)) -> true
    | PatternRule(patt, t)     -> true
    | _ -> false
    
let collectTokenAccess (rules : ScanRule list) =
    let byLiteral = rules |> Seq.filter  isLiteral
                          |> Seq.groupBy literalText
                          
    let byType = rules |> Seq.filter  isTypedToken
                       |> Seq.groupBy tokenType

    let ambigousLiterals = 
        set [
            for (lit, items) in byLiteral do
                if (Seq.length items) > 1 
                then
                    yield lit
        ]

    let nonIdentifiableLiterals = 
            set [
                for (t, rules) in byType do
                    if (Seq.length rules) > 1 then
                        for rule in rules do
                            match rule with
                            | LiteralRule(lit, _) -> yield lit
                            | _ -> ();
            ]

    let deniedLiterals = Set.union ambigousLiterals nonIdentifiableLiterals
    let allLiterals    = set (Seq.map fst byLiteral)
    let usedTypes      = set (Seq.map fst byType)

    failable {
        for lit in ambigousLiterals do 
            do! fail (sprintf "Conflicting literal rules for literal '%s'" lit)
        return 
            { 
               DeniedLiterals  = deniedLiterals;
               GrantedTypes    = usedTypes;
               GrantedLiterals = Set.difference allLiterals deniedLiterals;
            }
    }

let getTokenResolution (refs : TokenRef list) (rules : ScanRule list) =
    failable {
        do! fail "Starting collect ..."
        return! collectTokenAccess rules
    }

type T1 = class end
type T2 = class end
type T3 = class end

let rules = [ 
        LiteralRule("foo", None);
        PatternRule(ScanPattern "~'x'", ttype<T1>);
        LiteralRule("bar", Some ttype<T2>);
        PatternRule(ScanPattern "'a'..'z'", ttype<T2>);
        LiteralRule("dup", None);
        LiteralRule("dup", Some ttype<T3>);   // duplicated literal rule
        ]

let refs = [
        LiteralRef "foo";  // explicit literal ref
        LiteralRef "bar";  // error because token is not identifiable by "bar"
        LiteralRef "else"; // implicit literal ref
        TypeRef ttype<T1>;  // pattern token ref
        TypeRef ttype<T2>;  // combined token ref
        ]

let mutable i = 0
failable {
    let access = getTokenResolution refs rules
    print access
    for e in 
    printfn "%d : %A" i  c 
    i <- i + 1
}

let {Value = access; Errors = conflicts } = getTokenResolution refs rules

let print x = printfn "%A" x

print access

print ""

for c in conflicts do
    printfn "%d : %A" i  c 
    i <- i + 1

