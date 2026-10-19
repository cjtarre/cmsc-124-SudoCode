public enum TokenType {
    // --- Keywords ---

    // Variable Operations
    INITIALIZE, SET,

    // Input/Output
    INPUT, OUTPUT, PRINT,

    // Conditionals
    IF, THEN, ELSE,             

    // Loops & Iteration
    FOR, EACH, IN, TO, STEP, WHILE, DO, REPEAT, UNTIL,
    
    // Pattern Matching
    CASE, OF, DEFAULT,

    // Subroutines & Scope
    FUNCTION, PROCEDURE, CALL, RETURN, 

    // Block Termination
    END,

    // Boolean & Null Literals
    TRUE, FALSE, NIL,

    // --- Operators ---
    // Arithmetic
    STAR, SLASH, MOD,           
    PLUS, MINUS,
    EXP,

    // Comparison
    EQUAL, GREATER, LESSER,
    GREATER_EQUAL, LESSER_EQUAL, NOT_EQUAL,

    // Logical
    NOT, AND, OR, XOR,

    // Assignment
    ASSIGN, 

    // --- Punctuation & Separators ---
    LEFT_PAREN, RIGHT_PAREN,
    LEFT_BRACKET, RIGHT_BRACKET,
    COMMA, COLON, DOT,

    // --- Dynamic Literals & Identifiers ---
    IDENTIFIER, NUMBER, STRING,

    // --- Special Control Tokens ---
    EOF
}