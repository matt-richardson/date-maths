# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DateMaths is a C# console application that solves mathematical equations using digits from dates. It converts dates into dMyy format (day-month-year without leading zeros) and finds valid mathematical equations using all the available digits.

## Core Architecture

The application is built around a sophisticated equation-solving engine with multiple algorithmic layers:

### Main Algorithm Flow (`FindValidEquations`)
1. **Basic Equations** - Simple arithmetic patterns (e.g., "1 + 1 = 2")
2. **Square Root with Parentheses** - Patterns like "√(2 + 7) + 2 = 5"  
3. **Advanced Equations** - Complex nested patterns including:
   - Repeated unary patterns: "√2 * √2 + 8 = 2 * 5"
   - Nested square roots: "√5 * √(√9 + 2) = 5"
   - Complex nested patterns: "√(√9 ^ √9 - 2) = 5"

### Expression System
- `ExpressionElement` class represents components: numbers, operators, or unary operations
- `EvaluateExpression` handles operator precedence: ^ (power), then *, /, then +, -
- Supports unary operators: ², ³, √, ∛, ! (factorial)

### Performance Optimization
- Early termination based on digit count (6+ digits = more aggressive limits)
- Layered approach: tries simpler patterns first before expensive operations
- Configurable result limits to prevent excessive computation

## Common Commands

### Build and Run
```bash
dotnet build
dotnet run                          # Solve for today's date
dotnet run -- 1125                 # Solve for specific date (1st Jan 2025)
dotnet run -- --help               # Show usage help
dotnet run -- --find-no-solution   # Find next day with no equations
```

### Testing
```bash
dotnet test                                    # Run all tests
dotnet test --verbosity normal                # Run with detailed output
dotnet test --filter "TestMethodName"         # Run specific test
dotnet test --filter "digits: [9, 9, 2, 5]"  # Run specific theory data
```

### Git Workflow - Auto-save Changes
**IMPORTANT**: After every code change, automatically commit changes to git using these commands:

```bash
# 1. Stage all changes
git add .

# 2. Commit with descriptive message
git commit -m "Update algorithm: [brief description of change]

🤖 Generated with [Claude Code](https://claude.ai/code)

Co-Authored-By: Claude <noreply@anthropic.com>"

# 3. Check status to verify commit succeeded
git status
```

**Auto-commit Pattern**: Always follow this sequence after making any code modifications:
1. Make the code change (edit files, add features, fix bugs)
2. Test the change (run `dotnet test` to ensure functionality)
3. Immediately commit to git using the above commands
4. Never leave uncommitted changes - preserve all work automatically

### Development Testing Patterns
```bash
# Test specific digit combinations
dotnet run -- 9925    # Test [9,9,2,5] digits
dotnet run -- 2725    # Test [2,7,2,5] digits
```

## Key Implementation Details

### Date Format (dMyy)
- Removes leading zeros from day and month
- Example: 01/01/25 → "1125", 03/11/23 → "31123"
- Implemented in `FormatDateWithoutLeadingZeros`

### Critical Algorithm Methods
- `FindBasicEquations` - Handles simple arithmetic combinations
- `FindSquareRootWithParentheses` - Specialized for √(a op b) patterns  
- `FindAdvancedEquationsOptimized` - Entry point for complex patterns
- `FindComplexNestedPatterns` - Handles patterns like √(√a ^ √a - b) = c
- `TryEqualsAtAllPositions` - Tests equation validity at different equals positions

### Test Structure
- Comprehensive unit tests in `DateMathsTests.cs`
- Theory-driven tests for multiple digit combinations
- Manual component testing for complex equations
- Debug output helpers using `ITestOutputHelper`

## Important Constraints

### Algorithm Behavior
- Must use ALL digits from input in the final equation
- Equations are ordered by length (shorter first)
- Early termination prevents infinite computation on difficult cases
- Unary operations limited to reasonable ranges (factorial ≤ 10, square ≤ 100)

### Test Expectations
- All equation results must contain exactly one equals s ign
- Mathematical validation with tolerance (< 0.0001 for floating point)
- Complex nested patterns require specific algorithm enhancements

### Performance Considerations
- 6+ digit sets use aggressive early termination
- Recursive depth limited to prevent stack overflow
- Expression generation bounded by reasonable mathematical limits
