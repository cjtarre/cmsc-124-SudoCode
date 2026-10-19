# `SudoCode`

## Creators
- Gabrielle Sumergido ([`freshlybakedsnep`](https://github.com/freshlybakedsnep))
- Ma. Christie Jude Tarre ([`cjtarre`](https://github.com/cjtarre))

## Overview
`SudoCode` is a simplified dynamically-typed language that helps programmers visualize algorithm behavior without being constrained by rigid syntactic overhead. It features a natural-language layout and a loosely structured design that mirrors classic academic pseudocode conventions. Writing in `SudoCode` should feel like drafting a flowchart or a textbook algorithm directly into an executable text file, allowing developers to focus entirely on core computational logic and sequence design rather than bracket tracking and type safety. 

## Host language and build
- Host language: C# (.NET 10.0)
- Version metadata: `/SudoCode.csproj` (Target Framework: `net10.0`)
- Build: `./build.sh`
- A fresh clone requires the host machine to have the .NET 10.0 SDK (or later) installed. Running `./build.sh` triggers `dotnet build --configuration Release`.

## Running it
| Command | What it does |
|---|---|
| `./run <file>` | [Executes a program. Available from Lab 4.] |
| `./run --tokenize <file>` | Prints the token stream to standard output. |
| `./run --parse <file>` | Prints the parsed tree. |
| `./run --eval <file>` | [Evaluates each expression and prints its value.] |
| `./run` | [Starts the REPL.] |

Exit codes: 
- `0`: When a program runs or tokenizes completely without any lexical, syntactic, or execution failures. 
- `65`: When a static error is encountered (such as a lexical flaw or invalid character during tokenization).
- `70`: When a runtime exception is thrown during evaluation.

## File extension
`.sudo` - a stylized abbreviation of the word '*pseudo*'.

## Lexical structure
### Keywords
All keywords in this language are defined and written in uppercase.
| Keyword | Purpose |
|---|---|
| `INITIALIZE` | Sets up a variable or data structure with an initial value using `<-`. |
| `SET` | Modifies the value of an existing variable using `<-`. |
| `INPUT` | Receives raw input streams from the tracking host environment context. |
| `OUTPUT` / `PRINT` | Displays stringified evaluation representations to the standard stdout terminal stream. |
| `IF` / `THEN` / `ELSE` | Controls conditional multi-branch logical flow paths. |
| `FOR` / `EACH` / `IN` / `TO` / `STEP` | Handles counter and range-based sequence loops. |
| `WHILE` / `DO` | Spawns traditional conditional execution iteration tracks. |
| `REPEAT` / `UNTIL` | Spawns standard bottom-driven boundary loop structures. |
| `CASE` / `OF` / `DEFAULT` | Manages structural multi-option expression selections. |
| `FUNCTION` / `PROCEDURE` / `CALL` / `RETURN` | Formulates explicit modular routine structures and scope executions. |
| `END` | Explicitly seals structural layout blocks (e.g., `END IF`, `END WHILE`). |

### Master Token Registry
```
LEFT_PAREN  RIGHT_PAREN
LEFT_BRACKET  RIGHT_BRACKET
PLUS  MINUS  STAR  SLASH  MOD EXP
ASSIGN  EQUAL  NOT_EQUAL  LESSER  LESSER_EQUAL  GREATER  GREATER_EQUAL
IDENTIFIER  STRING  NUMBER
INITIALIZE  SET  INPUT  OUTPUT  PRINT
IF  THEN  ELSE  WHILE  DO  FOR  EACH  IN  TO  STEP  REPEAT  UNTIL
CASE  OF  DEFAULT  FUNCTION  PROCEDURE  CALL  RETURN  END
AND  OR  NOT  XOR  TRUE  FALSE  NIL
DOT  COLON  COMMA
EOF
```

### Operators
| Operator | Category | Operands | Associativity | Precedence |
|---|---|---|---|---|
| `(`, `)` | grouping | binary | right | 9 (*tightest*) |
| `^`, `**` | arithmetic | binary | right | 8 |
| `-` (unary) | arithmetic | unary | right | 7 |
| `*`, `/`, `%`, `MOD` | arithmetic | binary | left | 6 |
| `+`, `-` | arithmetic | binary | left | 5 |
| `=`, `>`, `>=`, `<`, `<=`, `!=` | comparison | binary | left | 4 |
| `NOT`, `!` | logical | unary | right | 3 |
| `AND`, `&&` | logical | binary | left | 2 |
| `OR`, `\|\|`, `XOR` | logical | binary | left | 1 |
| `<-` | assignment | binary | right | 0 (*loosest*) |

### Literals
| Kind | Syntax | Produces |
|---|---|---|
| `IDENTIFIER` | *see [`Identifiers`](#identifiers)* | A bound reference name to a stored variable context |
| `NUMBER` | Digits with optional single dot fractional notation (e.g., `42`, `3.14`) | IEEE 754 double-precision floating-point runtime values |
| `STRING` | Characters wrapped inside matching double quotation marks (e.g., `"hello"`) | UTF-16 character string runtime values |
| `BOOLEAN` | Case-sensitive Boolean keywords: `TRUE`, `FALSE` | Logic bit values (`true` / `false`) |
| `NIL` | The explicit empty value keyword `NIL` | Null pointer baseline references |

### Identifiers
- Start characters: Letters `a`-`z`, `A`-`Z`, or an underscore `_`
- Continue characters: Letters `a`-`z`, `A`-`Z`, numbers `0`-`9` or an underscore `_`
- Case-sensitive: **Yes**.
- Compound instructions like `END IF` or `ELSE IF` are scanned as independent tokens (`END` followed by `IF`) and handled at the grammar phase. 
- Variable names cannot perfectly match any singular keyword identifier list.
- String Boundaries: String literals must begin and end with explicit double quotes (`"`). Direct use of single quotes (`'`) are not recognized and will throw a lexical error.
- Multi-line Strings: String literals cannot span multiple lines. Hitting a newline character `\n` before finding the closing double quote will immediately terminate scanning and trigger an `Unterminated String` static error (Exit Code `65`).
- Escape Characters: `SudoCode` strings do not process backslash escape sequences (like `\n` or `\t`) within text literals. A backslash is treated as a literal text character.

### Comments
- Line comments: `>>`
- Block comments: Not supported
- Nesting: Not supported
- Harness note: `comment_prefix` in `tests/lab*/manifest.json` is set to "`>>`".

## Whitespace and termination
- Whitespace significant: **No**. Whitespace acts as a delimiter to distinguish individual words and symbols but does not dictate program structural grouping or indent validation.
- Statement terminator: **None**. During tokenization, newlines are treated as whitespace and are tracked through token line numbers. During expression parsing, each physical line may contain at most one top-level expression, so a newline acts as the boundary between top-level expressions.
- Block delimiters: **Marked explicitly** by corresponding keyword bounds <br>(e.g., `IF` ... `THEN` ... `END IF` or `WHILE` ... `DO` ... `END WHILE`).
- Grouping delimiters: Standard parentheses `(` and `)` are used to force expression precedence and enclose function/procedure parameters.
- Token separators: 
  - Commas `,` are used to separate parameters in function definitions.
  - Colons `:` mark branch blocks inside conditional `CASE` statements
  - Dots `.` are used as an explicit member access operator for dot notation structures.

## Token output format
```
Token(type=INITIALIZE, lexeme=INITIALIZE, literal=null, line=1)
Token(type=IDENTIFIER, lexeme=counter, literal=null, line=1)
Token(type=ASSIGN, lexeme=<-, literal=null, line=1)
Token(type=NUMBER, lexeme=10, literal=10.0, line=1)
Token(type=EOF, lexeme=, literal=null, line=1)
```
- `type`: The internal enum categorization of the matched string token.
- `lexeme`: The literal exact substring sequence sliced directly from the `.sudo` raw file.
- `literal`: The interpreted object primitive representation (unboxed strings, parsed double precision values, or `null`).
- `line`: The tracker value pointing to the line number the token was detected on.

## Grammar
```
expression      → assignment
assignment      → logicalOr ( "<-" assignment )?
logicalOr       → logicalAnd ( ( "OR" | "||" | "XOR" ) logicalAnd )*
logicalAnd      → logicalNot ( ( "AND" | "&&" ) logicalNot )*
logicalNot      → ( "NOT" | "!" ) logicalNot | comparison
comparison      → term ( ( ">" | ">=" | "<" | "<=" | "=" | "!=" ) term )*
term            → factor ( ( "-" | "+" ) factor )*
factor          → unary ( ( "*" | "/" | "%" | "MOD" ) unary )*
unary           → "-" unary | power
power           → primary ( ( "^" | "**" ) power )?
primary         → NUMBER | STRING | "TRUE" | "FALSE" | "NIL" | 
                  IDENTIFIER | "(" expression ")"
```
Notes on deviations from the textbook grammar:
- `assignment` is right-associative and checked structurally rather than by looking ahead for `IDENTIFIER "<-"`: the parser parses the left side as a full `logicalOr` expression first, then verifies at parse time that the result is a valid assignment target (an `Expr.Variable`). An invalid target (e.g. `1 + 1 <- 5`) is a parse error, not a scan error.
- `^` and `**` (exponentiation) are right-associative, matching mathematical convention (`2^3^2` parses as `2^(3^2)`, not `(2^3)^2`).
- Unary `-` sits between `factor` and `power`, binding tighter than `*`/`/` but looser than `^`. This means `-2^2` parses as `-(2^2) = -4`, matching the reading Python and standard mathematical notation use, rather than `(-2)^2 = 4`.
- `NOT` sits above `comparison` (looser), not down near `factor`/`term`, so that `NOT a = b` parses as `NOT (a = b)` rather than `(NOT a) = b`.
- This grammar covers expressions only. Keywords governing control flow, declarations, and I/O (`IF`, `FOR`, `WHILE`, `INITIALIZE`, `SET`, `PRINT`, etc.) are not part of any expression production and are reserved for a later statement grammar; a bare keyword like `IF` cannot begin an `expression` and is rejected as a syntax error under this lab's scope.

## Parse output format
`--parse` prints one line per top-level expression parsed from the file, in prefix parenthesized form: the operator (or a fixed keyword) comes first, followed by its operands, all wrapped in parentheses.
```
Token(type=NUMBER, ...) + Token(type=NUMBER, ...) → (+ 1.0 1.0)

X <- 17 → (assign X 17.0)

(1 + 2) * 3 → (* (group (+ 1.0 2.0)) 3.0)

NOT 3 → (NOT 3.0)
```
- Binary expressions print as `(OPERATOR LEFT RIGHT)`, using the operator's lexeme (e.g. `+`, `-`, `=`, `AND`).
- Unary expressions print as `(OPERATOR OPERAND)`.
- Grouped expressions print as `(group INNER)` — the parentheses themselves are not part of the tree, so the printer reinserts a `group` marker to make the grouping visible in output.
- Assignments print as `(assign NAME VALUE)`.
- Variables print as their identifier name (bare, no wrapping).
- Numbers print using the same convention as token output: 
  - an integral double prints with one decimal place (`5` → `5.0`); 
  - a fractional double prints its natural decimal form (`5.25` → `5.25`).
- Booleans print as lowercase `true` / `false`.
- `NIL` prints as `nil`.
- File-splitting rule: Each physical line contains at most one top-level expression. A newline terminates the current top-level expression, and `--parse` prints one AST line for each valid expression. Multiple expressions on the same physical line are rejected as a syntax error.
- Rejections: a file containing a syntax error prints nothing to stdout, reports `[Line N] Parsing Error: ...` diagnostics to stderr, and exits with code `65`. The parser attempts to recover after each error by synchronizing to the next line so multiple errors in one file can be reported in a single run.
  
## Semantics

### Values and types
`SudoCode` supports four core runtime data primitive types managed dynamically in the host architecture:
- **Numbers**: Represented inside the host engine environment as native C# `double` precision variables for dynamic typing compatibility.
- **Strings**: Stored and tracked as native .NET `string` objects.
- **Booleans**: Represented via native C# `bool` flags (`true` and `false`).
- **Nil**: Represents an empty or uninitialized state, mapped straight to a native C# `null` reference value.

### Value printing
- Numbers: Prints without trailing decimals if it is an integer representation (e.g., `5`), or with explicit fractional scales if a true floating-point value is maintained (e.g., `5.25`).
- Strings: Extracted and printed directly to standard output without surrounding quotation marks.
- Booleans: Prints explicitly as the lowercase textual literals `true` and `false`.
- Nil: Prints explicitly as the lowercase textual literal `nil`.

### Truthiness
`SudoCode` follows a strict truthiness evaluation rule:
- Only the Boolean flag value `FALSE` and the empty object state `NIL` are interpreted as falsy conditions.
- Every other initialized value type, object, number, or string (including an empty string) is resolved as **true**.

### Operator semantics
- Arithmetic: Requires both matching operands to be `NUMBER` types. Any alternative configuration throws a runtime error.
- `+`: Adds two numbers. If either operand is a string, it performs string concatenation (e.g., `"Value: " + 5` produces `"Value: 5"`).
- Mixed types: Triggers an operational type-mismatch error unless evaluated by string concatenation setups.
- Comparison: Allowed exclusively between numerical items.
- Equality across types: Comparing different types (e.g., matching a string to a number) resolves directly as `false` safely without triggering a system crash.
- Logical operators: `NOT`, `AND`, `OR`, and `XOR` evaluate operands according to SudoCode truthiness and produce Boolean results.
- Division or modulo by zero: Causes a runtime evaluation error with exit code `70`.

### Scope and bindings
- Lab 3 evaluator behavior: The `<-` operator creates or updates a variable binding in the current evaluation environment. Referencing an undefined variable during evaluation causes a runtime error with exit code `70`.
- Redeclaration in the same scope: Attempting to `INITIALIZE` a variable that already exists in the same scope causes a static analysis error exiting under code `65`.
- Uninitialized variable holds: Cannot occur. Variables must explicitly pass through an `INITIALIZE` binding sequence, unless it is a counter variable such as in `FOR`.
- Shadowing: Inner local block levels fully shadow broader identifier declarations safely.
- Undefined name: Referencing unmapped variable names causes a static validation crash exiting with code `65`.

### Control flow and functions
- Logical operators return: The specific logical evaluated `bool` answer (`TRUE` or `FALSE`).
- Dangling else binds to: The nearest nested, unresolved, open `IF` statement block.
- Closure capture of a loop variable: Shared reference capture behavior across execution paths.
- Function with no return statement produces: A standard default state tracking value of `NIL`.
- Arity mismatch: Passing the wrong number of arguments to a function throws a static signature mismatch error exiting under code `65`. 

## Native functions
*To be defined in a later lab activity.*

## Errors and diagnostics
Message format:

```text
[Line 3] Lexical Error: Unexpected character '@' found in scan block.
[Line 5] Runtime Error: Division by zero
```

| Failure | Exit code |
|---|---|
| lexical error | `65` |
| syntax error | `65` |
| runtime error | `70` |


## Testing conventions
| Folder | Activity | Mode | Flag |
|---|---|---|---|
| tests/lab1 | Scanner | sidecar | `--tokenize` |
| tests/lab2 | Parser | sidecar | `--parse` |
| tests/lab3 | Evaluator | inline | `--eval` |
| tests/lab4 | Context | inline | none |
| tests/lab5 | Functions | inline | none |

### Lab 1 Tests
| Test | Coverage |
|---|---|
| `00_keywords` | All keyword tokens in token registry are read by the scanner.  |
| `01_identifiers` | Scanner can read variations with variable names. |
| `02_operators` | All operator tokens in token registry are recognized for each symbol and operator. |
| `03_strings` | Scanner properly reads strings (treating backslashes as literal text). |
| `04_numbers` | All numbers are stored with the correct floating value, with ints defaulted to use 1 decimal place. |
| `05_comments` | Comments ignore all character following its lexeme.  |
| `06_whitespace` | Validates that spaces, tabs, and newlines do not generate tokens. Newlines only update line numbers. |
| `07_boundaries` | Reads the correct token based on different character combinations and boundaries (like `<-` vs `<`). |
| `08_empty` | Shows only `EOF` as the only token. |
| `09_sample` | Reads a sample code and returns the correct tokens |
| `10_escape_sequences` | Verifies that backslashes in strings are treated as literal text. |
| `err_invalid_char` | Should return nothing, with exit code `65` after detecting an invalid character.  |
| `err_string` | Should return nothing, with exit code `65` after an unclosed string instance |
| `err_multiline_string` | Should return nothing, with exit code `65` when a string spans multiple lines. |

### Lab 2 Tests
| Test | Coverage |
|---|---|
| `00_literals` | Covers integer and fractional numbers, strings (including empty strings), Boolean values, and `NIL`. |
| `01_precedence` | Verifies precedence across arithmetic, exponentiation, comparison, and logical levels, including alternate operator forms. |
| `02_associativity` | Verifies left associativity for subtraction, division, `%`, and `MOD`, and right associativity for exponentiation and assignment. |
| `03_grouping` | Verifies grouping expressions, including nested/redundant parentheses and grouping that changes expression structure. |
| `04_unary` | Covers unary minus, `NOT`/`!`, chained unary operators, and unary/exponentiation precedence. |
| `05_line_boundaries` | Verifies that each physical line is parsed as a separate top-level expression. |
| `06_logical` | Covers comparison, `NOT`, `AND`, `OR`, and `XOR` expressions and their precedence. |
| `07_assignment` | Covers variables, simple assignment, assignment precedence, and chained right-associative assignment. |
| `08_mixed` | Verifies a mixed expression spanning multiple precedence levels. |
| `09_empty` | Verifies that an empty source file produces no AST output and exits successfully. |
| `10_comment_only` | Verifies that a comment-only source file produces no AST output and exits successfully. |
| `err_invalid_char` | Rejects input containing a lexical error during parsing. |
| `err_invalid_assignment` | Rejects an assignment whose left-hand side is not a variable. |
| `err_invalid_expression` | Rejects a token that cannot begin an expression. |
| `err_missing_operand` | Rejects a binary operator with a missing right-hand operand. |
| `err_multiple_expressions` | Rejects multiple top-level expressions on the same physical line. |
| `err_unclosed_group` | Rejects a parenthesized expression missing its closing `)`. |
| `err_multiline_group` | Rejects a grouped expression whose closing parenthesis appears on a different physical line. |

### Lab 3 Tests
| Test | Coverage |
|---|---|
| `00_literals` | Evaluates numbers, strings, Boolean values, and `NIL`. |
| `01_arithmetic` | Evaluates arithmetic operators and arithmetic precedence. |
| `02_strings` | Evaluates string concatenation, including concatenation with other values. |
| `03_comparison` | Evaluates numerical comparison operators. |
| `04_equality` | Evaluates equality and inequality across same and different value types. |
| `05_logical` | Evaluates `NOT`, `AND`, `OR`, and `XOR` using SudoCode truthiness rules. |
| `06_grouping_mixed` | Evaluates grouping and expressions combining multiple operator types and precedence levels. |
| `07_assignment` | Evaluates variable lookup, assignment, reassignment, and chained assignment. |
| `err_arithmetic_string` | Reports a runtime error for invalid arithmetic operand types. |
| `err_comparison_types` | Reports a runtime error for invalid comparison operand types. |
| `err_division_zero` | Reports a runtime error for division by zero and stops further evaluation. |
| `err_negate_nonnumber` | Reports a runtime error when unary negation is applied to a non-number. |
| `err_static` | Reports a syntax error before evaluation for malformed input. |
| `err_undefined_variable` | Reports a runtime error when an undefined variable is referenced. |

Run locally with:
```bash
curl -sSL https://raw.githubusercontent.com/WhiteLicorice/cmsc-124-harness/v1.1/run_tests.py -o run_tests.py
./build.sh
python3 run_tests.py tests/lab0
python3 run_tests.py tests/lab1
python3 run_tests.py tests/lab2
python3 run_tests.py tests/lab3
```

## Sample code
```text
INITIALIZE counter <- 1
FOR i IN 1 TO 5 DO
    IF i MOD 2 = 0 THEN
        PRINT i
        SET counter <- counter + 1
    END IF
END FOR
```

Output:
```text
2
4
```

## Design rationale
`SudoCode` was intentionally designed to capture the structural clarity of classic academic pseudocode while eliminating C-style syntax clutter. We explicitly decoupled our keywords to follow simple, clean, individual token blocks (such as processing `END` and `IF` as separate tokens rather than a single compound lexeme), allowing our upcoming parser rules to safely establish scope logic. We chose to separate declaration (`INITIALIZE`) from mutation (`SET`) to enforce absolute clarity when reading state transformations. 

Our most significant mid-design pivot was removing the dual assignment meaning of the `TO` keyword. Originally conceptualized for assignments (e.g., `SET x TO 5`), this created a parsing ambiguity with traditional `FOR i IN 1 TO 10` iteration limits. We resolved this conflict by adopting the universal pseudocode assignment arrow (`<-`) and reserving `TO` strictly for loop boundaries. We also opted out of using curly braces, leaning entirely into matching text boundaries (like `IF` paired with `END IF`) to keep program code clean, flowing, and readable. It was also decided to allow for alternative symbols such as `\|\|` and `&&` for shorthand input for more flexible code with the conceptualized language. 

## Known limitations
- Standard multi-line block commenting structures are completely unsupported; documentation annotations are restricted strictly to single-line `>>` prefixes.
- Compact mutating expressions such as `++`, `--`, or `+=` do not exist, requiring explicit manual assignments (`SET x <- x + 1`).
- Escape code literals (such as `\n` or `\t`) inside text strings are currently treated as uninterpreted character sets rather than active formatting commands.

## Changelog
| Activity | What changed in the language |
|---|---|
| Lab 1 | Lexical specifications defined; scanner implementation and lexical rules for tokens, operators, literals, comments, and whitespace established. |
| Lab 2 | Expression grammar defined; parser, AST representation and printer, operator precedence and associativity, assignment and logical expressions, and syntax error handling established. |
| Lab 3 | Expression evaluation implemented; runtime values, truthiness, arithmetic, string concatenation, comparisons, equality, logical operations, variable assignment, and runtime error handling established. |
