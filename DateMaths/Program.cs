namespace DateMaths
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Date Mathematics Equation Solver ===\n");
            
            // Check for special modes
            if (args.Length > 0 && args[0] == "--find-no-solution")
            {
                FindNextDayWithNoSolution();
                return;
            }
            
            if (args.Length > 0 && args[0] == "--help")
            {
                ShowHelp();
                return;
            }
            
            // Get today's date in dMyy format (without leading zeros for day and month)
            DateTime today = DateTime.Now;
            string dateStr = args.Length > 0 && args[0] != "--find-no-solution" ? args[0] : FormatDateWithoutLeadingZeros(today);
            Console.WriteLine($"Date (dMyy): {dateStr}");
            
            // Extract individual digits
            List<int> digits = ExtractDigits(dateStr);
            Console.WriteLine($"Available digits: [{string.Join(", ", digits)}]\n");
            
            // Find valid equations
            Console.WriteLine("Searching for valid equations...\n");
            var equations = FindValidEquations(digits);
            
            if (equations.Any())
            {
                Console.WriteLine($"Found {equations.Count} valid equation(s):");
                foreach (var eq in equations.Take(10)) // Show first 10
                {
                    Console.WriteLine($"  {eq}");
                }
                
                if (equations.Count > 10)
                {
                    Console.WriteLine($"  ... and {equations.Count - 10} more!");
                }
            }
            else
            {
                Console.WriteLine("No valid equations found with the available digits.");
            }
            
//            Console.WriteLine("\nPress any key to exit...");
//
// Console.ReadKey();
        }
        
        static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run                        - Solve for today's date");
            Console.WriteLine("  dotnet run -- [date]              - Solve for specific date (format: dMyy)");
            Console.WriteLine("  dotnet run -- --find-no-solution  - Find next day with no valid equation");
            Console.WriteLine("  dotnet run -- --help              - Show this help");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  dotnet run -- 1125                - Solve for 1st January 2025");
            Console.WriteLine("  dotnet run -- 31123               - Solve for 3rd November 2023");
        }
        
        static void FindNextDayWithNoSolution()
        {
            Console.WriteLine("=== Finding Next Day With No Valid Equation ===\n");
            
            DateTime currentDate = DateTime.Now;
            int daysChecked = 0;
            const int maxDaysToCheck = 1000; // Check up to a year ahead
            
            Console.WriteLine($"Starting search from: {currentDate:dd/MM/yy}");
            Console.WriteLine("Checking each day for valid equations...\n");
            
            while (daysChecked < maxDaysToCheck)
            {
                currentDate = currentDate.AddDays(1);
                daysChecked++;
                
                string dateStr = FormatDateWithoutLeadingZeros(currentDate);
                List<int> digits = ExtractDigits(dateStr);
                
                // Skip dates with fewer than 3 digits (unlikely to have solutions)
                if (digits.Count < 3)
                {
                    Console.WriteLine($"Day {daysChecked}: {currentDate:dd/MM/yy} -> {dateStr} (skipped - too few digits)");
                    continue;
                }
                
                var equations = FindValidEquations(digits);
                
                Console.WriteLine($"Day {daysChecked}: {currentDate:dd/MM/yy} -> {dateStr}");
                
                if (equations.Count == 0)
                {
                    Console.WriteLine($"🎯 FOUND ONE! {currentDate:dd/MM/yy} has NO valid equations!");
                    Console.WriteLine($"Date format: {dateStr}");
                    Console.WriteLine($"Digits: [{string.Join(", ", digits)}]");
                    Console.WriteLine("---------------------------------------------");
                    return;
                } 
                else
                {
                    Console.WriteLine($"    Example: {equations.First()}");
                }
            }
            
            Console.WriteLine($"\n⚠️  No date without solutions found in the next {maxDaysToCheck} days.");
        }
        
        public static string FormatDateWithoutLeadingZeros(DateTime date)
        {
            // Get day without leading zero (d)
            int day = date.Day;
            
            // Get month without leading zero (M) 
            int month = date.Month;
            
            // Get year as yy
            string year = date.ToString("yy");
            
            // Combine: day + month + year (removing any leading zeros from day and month)
            return $"{day}{month}{year}";
        }
        
        public static List<int> ExtractDigits(string dateStr)
        {
            return dateStr.Where(char.IsDigit)
                         .Select(c => int.Parse(c.ToString()))
                         .ToList();
        }
        
        public static List<string> FindValidEquations(List<int> digits)
        {
            var validEquations = new HashSet<string>();
            
            // Early exit for very small digit sets
            if (digits.Count < 2) return validEquations.ToList();
            
            // Performance optimization: For 6+ digits, be more aggressive about early termination
            int targetEquationCount = digits.Count >= 6 ? 5 : 10;
            
            // FIRST: Try simple basic equations (handles cases like "1 + 1 = 2") - These are fast
            FindBasicEquations(digits, validEquations);
            
            // Early exit if we found enough basic equations
            if (validEquations.Count >= targetEquationCount) 
                return validEquations.OrderBy(eq => eq.Length).ToList();
            
            // SECOND: Try parenthesized square root operations (specific patterns)
            FindSquareRootWithParentheses(digits, validEquations);
            
            // Early exit if we found enough equations
            if (validEquations.Count >= targetEquationCount) 
                return validEquations.OrderBy(eq => eq.Length).ToList();
            
            // THIRD: Try advanced equations only if we don't have many results yet
            // This is the expensive operation, so we limit it more aggressively for large digit sets
            if (validEquations.Count == 0)
            {
                int maxResults = digits.Count >= 6 ? 10 : 20;
                FindAdvancedEquationsOptimized(digits, validEquations, maxResults);
            }
            
            return validEquations.OrderBy(eq => eq.Length).ToList();
        }
        
        static void FindBasicEquations(List<int> digits, HashSet<string> validEquations)
        {
            var operators = new[] { "+", "-", "*", "/" };
            
            // Generate all ways to split digits into groups for basic equations
            // Try patterns like: a op b = c, a op b op c = d, etc.
            // Limit to reasonable number counts for performance
            
            // More aggressive limits for large digit sets
            int maxNumCount = digits.Count >= 6 ? 3 : Math.Min(5, digits.Count);
            int earlyExitThreshold = digits.Count >= 6 ? 10 : 50;
            
            for (int numCount = 2; numCount <= maxNumCount; numCount++)
            {
                GenerateBasicEquationsRecursive(digits, 0, new List<int>(), new List<string>(), 
                    numCount, operators, validEquations);
                
                // Early exit if we found enough equations
                if (validEquations.Count > earlyExitThreshold) break;
            }
        }
        
        static void GenerateBasicEquationsRecursive(List<int> digits, int index, List<int> numbers, 
            List<string> operators, int targetCount, string[] opList, HashSet<string> validEquations)
        {
            // Early exit if we have enough equations
            if (validEquations.Count > (digits.Count >= 6 ? 10 : 50)) return;
            
            if (numbers.Count == targetCount)
            {
                // CRITICAL FIX: Only create equations if we've used ALL digits
                if (index >= digits.Count)
                {
                    // Try different positions for equals sign
                    for (int equalsPos = 1; equalsPos < numbers.Count; equalsPos++)
                    {
                        TryBasicEquation(numbers, operators, equalsPos, validEquations);
                        // Early exit if we have enough equations
                        if (validEquations.Count > (digits.Count >= 6 ? 10 : 50)) return;
                    }
                }
                return;
            }
            
            if (index >= digits.Count) return;
            
            // Try different length numbers starting from current position
            for (int endIndex = index; endIndex < digits.Count; endIndex++)
            {
                // Create number from digits[index] to digits[endIndex]
                int number = 0;
                for (int i = index; i <= endIndex; i++)
                {
                    number = number * 10 + digits[i];
                }
                
                numbers.Add(number);
                
                if (numbers.Count < targetCount)
                {
                    // Need to add an operator
                    foreach (var op in opList)
                    {
                        operators.Add(op);
                        GenerateBasicEquationsRecursive(digits, endIndex + 1, numbers, operators, 
                            targetCount, opList, validEquations);
                        operators.RemoveAt(operators.Count - 1);
                    }
                }
                else
                {
                    // Continue only if we might use all digits
                    GenerateBasicEquationsRecursive(digits, endIndex + 1, numbers, operators, 
                        targetCount, opList, validEquations);
                }
                
                numbers.RemoveAt(numbers.Count - 1);
            }
        }
        
        static void TryBasicEquation(List<int> numbers, List<string> operators, int equalsPos, HashSet<string> validEquations)
        {
            try
            {
                // Evaluate left side
                double leftValue = numbers[0];
                for (int i = 0; i < equalsPos - 1; i++)
                {
                    string op = operators[i];
                    int nextNum = numbers[i + 1];
                    
                    leftValue = op switch
                    {
                        "+" => leftValue + nextNum,
                        "-" => leftValue - nextNum,
                        "*" => leftValue * nextNum,
                        "/" when nextNum != 0 => leftValue / nextNum,
                        _ => double.NaN
                    };
                    
                    if (double.IsNaN(leftValue)) return;
                }
                
                // Evaluate right side
                double rightValue = numbers[equalsPos];
                for (int i = equalsPos; i < numbers.Count - 1; i++)
                {
                    if (i - equalsPos + equalsPos - 1 >= operators.Count) break;
                    
                    string op = operators[i - equalsPos + equalsPos - 1];
                    int nextNum = numbers[i + 1];
                    
                    rightValue = op switch
                    {
                        "+" => rightValue + nextNum,
                        "-" => rightValue - nextNum,
                        "*" => rightValue * nextNum,
                        "/" when nextNum != 0 => rightValue / nextNum,
                        _ => double.NaN
                    };
                    
                    if (double.IsNaN(rightValue)) return;
                }
                
                // Check if equation is valid
                if (Math.Abs(leftValue - rightValue) < 0.0001)
                {
                    // Build equation string
                    var leftParts = new List<string>();
                    var rightParts = new List<string>();
                    
                    // Left side
                    leftParts.Add(numbers[0].ToString());
                    for (int i = 0; i < equalsPos - 1; i++)
                    {
                        leftParts.Add(operators[i]);
                        leftParts.Add(numbers[i + 1].ToString());
                    }
                    
                    // Right side
                    rightParts.Add(numbers[equalsPos].ToString());
                    for (int i = equalsPos; i < numbers.Count - 1; i++)
                    {
                        if (i - equalsPos + equalsPos - 1 >= operators.Count) break;
                        rightParts.Add(operators[i - equalsPos + equalsPos - 1]);
                        rightParts.Add(numbers[i + 1].ToString());
                    }
                    
                    string equation = $"{string.Join(" ", leftParts)} = {string.Join(" ", rightParts)}";
                    validEquations.Add(equation);
                }
            }
            catch
            {
                // Skip invalid operations
            }
        }
        
        static void FindAdvancedEquationsOptimized(List<int> digits, HashSet<string> validEquations, int maxResults)
        {
            var basicOperators = new[] { "+", "-", "*", "/" };
            var unaryOperators = new[] { "²", "³", "√", "∛", "!" };
            
            // Generate expressions more selectively with early termination
            GenerateExpressionsOptimized(digits, 0, new List<ExpressionElement>(), validEquations, 
                basicOperators, unaryOperators, maxResults, 0);
            
            // ADDITION: Try patterns with repeated unary operations like √2 * √2
            if (validEquations.Count < maxResults)
            {
                FindRepeatedUnaryPatterns(digits, validEquations, maxResults);
            }
            
            // ADDITION: Try nested square root patterns like √(√9 + 2)
            if (validEquations.Count < maxResults)
            {
                FindNestedSquareRootPatterns(digits, validEquations, maxResults);
            }
            
            // ADDITION: Try complex nested patterns like √((8 - √9) ^ 2) = 5
            if (validEquations.Count < maxResults)
            {
                FindComplexNestedPatterns(digits, validEquations, maxResults);
            }
            
            // ADDITION: Try mixed unary patterns like 2 + 6 - √9 = √25
            if (validEquations.Count < maxResults)
            {
                FindMixedUnaryPatterns(digits, validEquations, maxResults);
            }
        }
        
        static void GenerateExpressionsOptimized(List<int> digits, int index, List<ExpressionElement> current, 
            HashSet<string> validEquations, string[] basicOperators, string[] unaryOperators, 
            int maxResults, int depth)
        {
            // Early termination conditions
            if (validEquations.Count >= maxResults) return;
            if (depth > 6) return; // Limit recursion depth
            if (index >= digits.Count)
            {
                // CRITICAL FIX: Only try equations when we've consumed all digits
                if (current.Count >= 3) // Need at least 3 elements for a meaningful equation
                {
                    TryEqualsAtAllPositions(current, validEquations);
                }
                return;
            }
            
            // Try creating numbers of different lengths starting from current index
            for (int endIndex = index; endIndex < Math.Min(index + 2, digits.Count); endIndex++) // Limit number length
            {
                // Early termination check
                if (validEquations.Count >= maxResults) return;
                
                // Create number from digits[index] to digits[endIndex]
                int number = 0;
                for (int i = index; i <= endIndex; i++)
                {
                    number = number * 10 + digits[i];
                }
                
                var element = new ExpressionElement { Type = "number", Value = number, Text = number.ToString() };
                
                // Try without unary operator
                current.Add(element);
                TryAddingOperatorsAndContinueOptimized(digits, endIndex + 1, current, validEquations, 
                    basicOperators, unaryOperators, maxResults, depth + 1);
                current.RemoveAt(current.Count - 1);
                
                // Try with unary operators (only for single digits and small numbers)
                if (index == endIndex && number <= 9 && validEquations.Count < maxResults)
                {
                    foreach (var unaryOp in unaryOperators)
                    {
                        if (IsValidUnaryOperation(number, unaryOp))
                        {
                            var unaryElement = new ExpressionElement 
                            { 
                                Type = "unary", 
                                Value = CalculateUnary(number, unaryOp), 
                                Text = FormatUnary(number, unaryOp) 
                            };
                            
                            current.Add(unaryElement);
                            TryAddingOperatorsAndContinueOptimized(digits, endIndex + 1, current, validEquations, 
                                basicOperators, unaryOperators, maxResults, depth + 1);
                            current.RemoveAt(current.Count - 1);
                        }
                        
                        if (validEquations.Count >= maxResults) break;
                    }
                }
            }
        }
        
        static void TryAddingOperatorsAndContinueOptimized(List<int> digits, int nextIndex, List<ExpressionElement> current,
            HashSet<string> validEquations, string[] basicOperators, string[] unaryOperators, 
            int maxResults, int depth)
        {
            if (validEquations.Count >= maxResults) return;
            
            if (nextIndex >= digits.Count)
            {  
                // CRITICAL FIX: Only try equations when we've consumed all digits
                if (current.Count >= 3)
                {
                    TryEqualsAtAllPositions(current, validEquations);
                }
                return;
            }
            
            // Try basic operators
            foreach (var op in basicOperators)
            {
                if (validEquations.Count >= maxResults) break;
                
                current.Add(new ExpressionElement { Type = "operator", Text = op });
                GenerateExpressionsOptimized(digits, nextIndex, current, validEquations, 
                    basicOperators, unaryOperators, maxResults, depth + 1);
                current.RemoveAt(current.Count - 1);
            }
            
            // Try power operator (^) - but limit this expensive operation
            if (nextIndex < digits.Count && depth < 4)
            {
                // Try using next 1 digit as exponent only
                int exponent = digits[nextIndex];
                if (exponent >= 1 && exponent <= 5) // Very limited exponent range
                {
                    current.Add(new ExpressionElement { Type = "operator", Text = "^" });
                    current.Add(new ExpressionElement { Type = "number", Value = exponent, Text = exponent.ToString() });
                    
                    TryAddingOperatorsAndContinueOptimized(digits, nextIndex + 1, current, validEquations, 
                        basicOperators, unaryOperators, maxResults, depth + 1);
                    
                    current.RemoveAt(current.Count - 1); // Remove exponent
                    current.RemoveAt(current.Count - 1); // Remove operator
                }
            }
        }
        
        public static List<List<ExpressionElement>> GenerateAllExpressionsAdvanced(List<int> digits)
        {
            var results = new List<List<ExpressionElement>>();
            var basicOperators = new[] { "+", "-", "*", "/" };
            var unaryOperators = new[] { "²", "³", "√", "∛", "!" };
            
            // Generate all possible ways to partition digits and assign operators
            GenerateExpressionsRecursive(digits, 0, new List<ExpressionElement>(), results, basicOperators, unaryOperators);
            
            return results;
        }
        
        static void GenerateExpressionsRecursive(List<int> digits, int index, List<ExpressionElement> current, 
            List<List<ExpressionElement>> results, string[] basicOperators, string[] unaryOperators)
        {
            if (index >= digits.Count)
            {
                if (current.Count > 0)
                    results.Add(new List<ExpressionElement>(current));
                return;
            }
            
            // Try creating numbers of different lengths starting from current index
            for (int endIndex = index; endIndex < digits.Count; endIndex++)
            {
                // Create number from digits[index] to digits[endIndex]
                int number = 0;
                for (int i = index; i <= endIndex; i++)
                {
                    number = number * 10 + digits[i];
                }
                
                var element = new ExpressionElement { Type = "number", Value = number, Text = number.ToString() };
                
                // Try without unary operator
                current.Add(element);
                TryAddingOperatorsAndContinue(digits, endIndex + 1, current, results, basicOperators, unaryOperators);
                current.RemoveAt(current.Count - 1);
                
                // Try with unary operators (only for single digits for readability)
                if (index == endIndex && number <= 12) // Reasonable limit for factorial, etc.
                {
                    foreach (var unaryOp in unaryOperators)
                    {
                        if (IsValidUnaryOperation(number, unaryOp))
                        {
                            var unaryElement = new ExpressionElement 
                            { 
                                Type = "unary", 
                                Value = CalculateUnary(number, unaryOp), 
                                Text = FormatUnary(number, unaryOp) 
                            };
                            
                            current.Add(unaryElement);
                            TryAddingOperatorsAndContinue(digits, endIndex + 1, current, results, basicOperators, unaryOperators);
                            current.RemoveAt(current.Count - 1);
                        }
                    }
                }
            }
        }
        
        static void TryAddingOperatorsAndContinue(List<int> digits, int nextIndex, List<ExpressionElement> current,
            List<List<ExpressionElement>> results, string[] basicOperators, string[] unaryOperators)
        {
            if (nextIndex >= digits.Count)
            {
                results.Add(new List<ExpressionElement>(current));
                return;
            }
            
            // Try basic operators
            foreach (var op in basicOperators)
            {
                current.Add(new ExpressionElement { Type = "operator", Text = op });
                GenerateExpressionsRecursive(digits, nextIndex, current, results, basicOperators, unaryOperators);
                current.RemoveAt(current.Count - 1);
            }
            
            // Try power operator (^) - exponent must come from remaining digits
            if (nextIndex < digits.Count)
            {
                // Try using next 1-2 digits as exponent
                for (int expLength = 1; expLength <= Math.Min(2, digits.Count - nextIndex); expLength++)
                {
                    int exponent = 0;
                    for (int i = 0; i < expLength; i++)
                    {
                        exponent = exponent * 10 + digits[nextIndex + i];
                    }
                    
                    if (exponent >= 0 && exponent <= 10) // Reasonable exponent range
                    {
                        current.Add(new ExpressionElement { Type = "operator", Text = "^" });
                        current.Add(new ExpressionElement { Type = "number", Value = exponent, Text = exponent.ToString() });
                        
                        TryAddingOperatorsAndContinue(digits, nextIndex + expLength, current, results, basicOperators, unaryOperators);
                        
                        current.RemoveAt(current.Count - 1); // Remove exponent
                        current.RemoveAt(current.Count - 1); // Remove operator
                    }
                }
            }
        }
        
        static bool IsValidUnaryOperation(int number, string op)
        {
            return op switch
            {
                "!" => number >= 0 && number <= 10, // Factorial only for small numbers
                "²" => number <= 100, // Square for reasonable numbers
                "³" => number <= 20,  // Cube for reasonable numbers  
                "√" => number >= 0,   // Square root for non-negative
                "∛" => true,          // Cube root works for any number
                _ => false
            };
        }
        
        public static double CalculateUnary(int number, string op)
        {
            return op switch
            {
                "!" => Factorial(number),
                "²" => number * number,
                "³" => number * number * number,
                "√" => Math.Sqrt(number),
                "∛" => Math.Pow(number, 1.0/3.0),
                _ => number
            };
        }
        
        static string FormatUnary(int number, string op)
        {
            return op switch
            {
                "!" => $"{number}!",
                "²" => $"{number}²",
                "³" => $"{number}³", 
                "√" => $"√{number}",
                "∛" => $"∛{number}",
                _ => number.ToString()
            };
        }
        
        public static double Factorial(int n)
        {
            if (n < 0) return double.NaN;
            if (n <= 1) return 1;
            double result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }
        
        static void FindRepeatedUnaryPatterns(List<int> digits, HashSet<string> validEquations, int maxResults)
        {
            // This method specifically looks for patterns like √2 * √2 + 8 = 2 * 5
            // where the same digit value appears multiple times and can be used with unary operators
            
            if (validEquations.Count >= maxResults) return;
            
            var unaryOperators = new[] { "√", "²", "³", "∛", "!" };
            var basicOperators = new[] { "+", "-", "*", "/" };
            
            // Look for patterns where we can use the same digit value multiple times
            // For digits [2,2,8,2,5], try patterns like: √2 * √2 + 8 = 2 * 5
            
            for (int i = 0; i < digits.Count - 4; i++) // Need at least 5 digits for this pattern
            {
                if (validEquations.Count >= maxResults) break;
                
                // Try pattern: unary(digit[i]) op unary(digit[i+1]) op digit[i+2] = digit[i+3] op digit[i+4]
                // This specifically handles √2 * √2 + 8 = 2 * 5
                
                int firstDigit = digits[i];
                int secondDigit = digits[i + 1];
                int thirdDigit = digits[i + 2];
                int fourthDigit = digits[i + 3];
                int fifthDigit = digits[i + 4];
                
                // Try various unary operators on first two digits
                foreach (var unaryOp1 in unaryOperators)
                {
                    if (!IsValidUnaryOperation(firstDigit, unaryOp1)) continue;
                    
                    foreach (var unaryOp2 in unaryOperators)
                    {
                        if (!IsValidUnaryOperation(secondDigit, unaryOp2)) continue;
                        
                        // Calculate unary values
                        double unaryValue1 = CalculateUnary(firstDigit, unaryOp1);
                        double unaryValue2 = CalculateUnary(secondDigit, unaryOp2);
                        
                        // Try different operators between unary operations
                        foreach (var op1 in basicOperators)
                        {
                            foreach (var op2 in basicOperators)
                            {
                                foreach (var op3 in basicOperators)
                                {
                                    // Calculate left side: unary(first) op1 unary(second) op2 third
                                    double leftPart1 = op1 switch
                                    {
                                        "+" => unaryValue1 + unaryValue2,
                                        "-" => unaryValue1 - unaryValue2,
                                        "*" => unaryValue1 * unaryValue2,
                                        "/" when unaryValue2 != 0 => unaryValue1 / unaryValue2,
                                        _ => double.NaN
                                    };
                                    
                                    if (double.IsNaN(leftPart1)) continue;
                                    
                                    double leftSide = op2 switch
                                    {
                                        "+" => leftPart1 + thirdDigit,
                                        "-" => leftPart1 - thirdDigit,
                                        "*" => leftPart1 * thirdDigit,
                                        "/" when thirdDigit != 0 => leftPart1 / thirdDigit,
                                        _ => double.NaN
                                    };
                                    
                                    if (double.IsNaN(leftSide)) continue;
                                    
                                    // Calculate right side: fourth op3 fifth
                                    double rightSide = op3 switch
                                    {
                                        "+" => fourthDigit + fifthDigit,
                                        "-" => fourthDigit - fifthDigit,
                                        "*" => fourthDigit * fifthDigit,
                                        "/" when fifthDigit != 0 => (double)fourthDigit / fifthDigit,
                                        _ => double.NaN
                                    };
                                    
                                    if (double.IsNaN(rightSide)) continue;
                                    
                                    // Check if equation is valid
                                    if (Math.Abs(leftSide - rightSide) < 0.0001)
                                    {
                                        string equation = $"{FormatUnary(firstDigit, unaryOp1)} {op1} {FormatUnary(secondDigit, unaryOp2)} {op2} {thirdDigit} = {fourthDigit} {op3} {fifthDigit}";
                                        validEquations.Add(equation);
                                        
                                        if (validEquations.Count >= maxResults) return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            // Also try simpler patterns where only some digits have unary operators
            for (int i = 0; i < digits.Count - 3; i++) // Need at least 4 digits
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];
                int secondDigit = digits[i + 1];
                int thirdDigit = digits[i + 2];
                int fourthDigit = digits[i + 3];
                
                // Pattern 1: unary(digit[i]) op digit[i+1] op digit[i+2] = digit[i+3]
                foreach (var unaryOp in unaryOperators)
                {
                    if (!IsValidUnaryOperation(firstDigit, unaryOp)) continue;
                    
                    double unaryValue = CalculateUnary(firstDigit, unaryOp);
                    
                    foreach (var op1 in basicOperators)
                    {
                        foreach (var op2 in basicOperators)
                        {
                            // Left side: unary(first) op1 second op2 third
                            double leftPart = op1 switch
                            {
                                "+" => unaryValue + secondDigit,
                                "-" => unaryValue - secondDigit,
                                "*" => unaryValue * secondDigit,
                                "/" when secondDigit != 0 => unaryValue / secondDigit,
                                _ => double.NaN
                            };
                            
                            if (double.IsNaN(leftPart)) continue;
                            
                            double leftSide = op2 switch
                            {
                                "+" => leftPart + thirdDigit,
                                "-" => leftPart - thirdDigit,
                                "*" => leftPart * thirdDigit,
                                "/" when thirdDigit != 0 => leftPart / thirdDigit,
                                _ => double.NaN
                            };
                            
                            if (double.IsNaN(leftSide)) continue;
                            
                            // Check if equals fourth digit
                            if (Math.Abs(leftSide - fourthDigit) < 0.0001)
                            {
                                string equation = $"{FormatUnary(firstDigit, unaryOp)} {op1} {secondDigit} {op2} {thirdDigit} = {fourthDigit}";
                                validEquations.Add(equation);
                                
                                if (validEquations.Count >= maxResults) return;
                            }
                        }
                    }
                }
                
                // Pattern 2: digit[i] op unary(digit[i+1]) = digit[i+2] op digit[i+3]
                // This handles cases like "7 + √9 = 2 * 5"
                foreach (var unaryOp in unaryOperators)
                {
                    if (!IsValidUnaryOperation(secondDigit, unaryOp)) continue;
                    
                    double unaryValue = CalculateUnary(secondDigit, unaryOp);
                    
                    foreach (var op1 in basicOperators)
                    {
                        foreach (var op2 in basicOperators)
                        {
                            // Left side: first op1 unary(second)
                            double leftSide = op1 switch
                            {
                                "+" => firstDigit + unaryValue,     // 7 + √9 = 7 + 3 = 10
                                "-" => firstDigit - unaryValue,
                                "*" => firstDigit * unaryValue,
                                "/" when unaryValue != 0 => firstDigit / unaryValue,
                                _ => double.NaN
                            };
                            
                            if (double.IsNaN(leftSide)) continue;
                            
                            // Right side: third op2 fourth
                            double rightSide = op2 switch
                            {
                                "+" => thirdDigit + fourthDigit,    // 2 * 5 = 10
                                "-" => thirdDigit - fourthDigit,
                                "*" => thirdDigit * fourthDigit,
                                "/" when fourthDigit != 0 => (double)thirdDigit / fourthDigit,
                                _ => double.NaN
                            };
                            
                            if (double.IsNaN(rightSide)) continue;
                            
                            // Check if equation is valid
                            if (Math.Abs(leftSide - rightSide) < 0.0001)
                            {
                                string equation = $"{firstDigit} {op1} {FormatUnary(secondDigit, unaryOp)} = {thirdDigit} {op2} {fourthDigit}";
                                validEquations.Add(equation);
                                
                                if (validEquations.Count >= maxResults) return;
                            }
                        }
                    }
                }
            }
        }
        
        static void FindNestedSquareRootPatterns(List<int> digits, HashSet<string> validEquations, int maxResults)
        {
            // This method specifically looks for patterns like √5 * √(√9 + 2) = 5
            // where we have nested square roots
            
            if (validEquations.Count >= maxResults || digits.Count < 4) return;
            
            var basicOperators = new[] { "+", "-", "*", "/" };
            
            // Look for pattern: √a * √(√b + c) = d
            // For digits [5,9,9,2,5], this would be: √5 * √(√9 + 2) = 5
            
            for (int i = 0; i < digits.Count - 4; i++) // Need at least 5 digits
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];      // 5
                int secondDigit = digits[i + 1]; // 9  
                int thirdDigit = digits[i + 2];  // 9 (but we'll use it as the inner √)
                int fourthDigit = digits[i + 3]; // 2
                int fifthDigit = digits[i + 4];  // 5
                
                // Try √firstDigit * √(√secondDigit + fourthDigit) = fifthDigit
                if (IsValidUnaryOperation(firstDigit, "√") && 
                    IsValidUnaryOperation(secondDigit, "√"))
                {
                    double sqrtFirst = CalculateUnary(firstDigit, "√");         // √5
                    double sqrtSecond = CalculateUnary(secondDigit, "√");       // √9 = 3
                    
                    // Try different operations inside the nested parentheses
                    foreach (var innerOp in basicOperators)
                    {
                        double innerResult = innerOp switch
                        {
                            "+" => sqrtSecond + fourthDigit,    // √9 + 2 = 3 + 2 = 5
                            "-" => sqrtSecond - fourthDigit,    // √9 - 2 = 3 - 2 = 1  
                            "*" => sqrtSecond * fourthDigit,    // √9 * 2 = 3 * 2 = 6
                            "/" when fourthDigit != 0 => sqrtSecond / fourthDigit, // √9 / 2 = 1.5
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(innerResult) || innerResult < 0) continue;
                        
                        // Calculate √(innerResult)
                        double sqrtInner = Math.Sqrt(innerResult);              // √5
                        if (double.IsNaN(sqrtInner)) continue;
                        
                        // Calculate left side: √firstDigit * √(innerResult)
                        double leftSide = sqrtFirst * sqrtInner;                // √5 * √5 = 5
                        
                        // Check if it equals the fifth digit
                        if (Math.Abs(leftSide - fifthDigit) < 0.0001)
                        {
                            // Format the equation
                            string equation = $"√{firstDigit} * √(√{secondDigit} {innerOp} {fourthDigit}) = {fifthDigit}";
                            validEquations.Add(equation);
                            
                            if (validEquations.Count >= maxResults) return;
                        }
                    }
                }
                
                // Also try the reverse pattern: √(√a + b) * √c = d
                if (IsValidUnaryOperation(firstDigit, "√") && 
                    IsValidUnaryOperation(thirdDigit, "√"))
                {
                    double sqrtFirst = CalculateUnary(firstDigit, "√");         // √5 = 2.236
                    double sqrtThird = CalculateUnary(thirdDigit, "√");         // √9 = 3
                    
                    foreach (var innerOp in basicOperators)
                    {
                        double innerResult = innerOp switch
                        {
                            "+" => sqrtFirst + secondDigit,    // √5 + 9
                            "-" => sqrtFirst - secondDigit,    // √5 - 9 (negative)
                            "*" => sqrtFirst * secondDigit,    // √5 * 9
                            "/" when secondDigit != 0 => sqrtFirst / secondDigit, // √5 / 9
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(innerResult) || innerResult < 0) continue;
                        
                        double sqrtInner = Math.Sqrt(innerResult);
                        if (double.IsNaN(sqrtInner)) continue;
                        
                        double leftSide = sqrtInner * sqrtThird;
                        
                        if (Math.Abs(leftSide - fifthDigit) < 0.0001)
                        {
                            string equation = $"√(√{firstDigit} {innerOp} {secondDigit}) * √{thirdDigit} = {fifthDigit}";
                            validEquations.Add(equation);
                            
                            if (validEquations.Count >= maxResults) return;
                        }
                    }
                }
            }
            
            // Also try simpler nested patterns with 4 digits
            for (int i = 0; i < digits.Count - 3; i++)
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];   // 5
                int secondDigit = digits[i + 1]; // 9
                int thirdDigit = digits[i + 2];  // 2
                int fourthDigit = digits[i + 3]; // 5
                
                // Try √firstDigit * √(√secondDigit + thirdDigit) = fourthDigit
                // This handles √5 * √(√9 + 2) = 5
                if (IsValidUnaryOperation(firstDigit, "√") && 
                    IsValidUnaryOperation(secondDigit, "√"))
                {
                    double sqrtFirst = CalculateUnary(firstDigit, "√");    // √5
                    double sqrtSecond = CalculateUnary(secondDigit, "√");  // √9 = 3
                    
                    foreach (var op in basicOperators)
                    {
                        double innerResult = op switch
                        {
                            "+" => sqrtSecond + thirdDigit,    // √9 + 2 = 3 + 2 = 5
                            "-" => sqrtSecond - thirdDigit,    // √9 - 2 = 3 - 2 = 1
                            "*" => sqrtSecond * thirdDigit,    // √9 * 2 = 3 * 2 = 6
                            "/" when thirdDigit != 0 => sqrtSecond / thirdDigit, // √9 / 2 = 1.5
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(innerResult) || innerResult < 0) continue;
                        
                        double sqrtInner = Math.Sqrt(innerResult); // √5
                        if (double.IsNaN(sqrtInner)) continue;
                        
                        double leftSide = sqrtFirst * sqrtInner;   // √5 * √5 = 5
                        
                        if (Math.Abs(leftSide - fourthDigit) < 0.0001)
                        {
                            string equation = $"√{firstDigit} * √(√{secondDigit} {op} {thirdDigit}) = {fourthDigit}";
                            validEquations.Add(equation);
                            
                            if (validEquations.Count >= maxResults) return;
                        }
                    }
                }
                
                // Try √(√a + b) = c
                if (IsValidUnaryOperation(firstDigit, "√"))
                {
                    double sqrtFirst = CalculateUnary(firstDigit, "√");
                    
                    foreach (var op in basicOperators)
                    {
                        double innerResult = op switch
                        {
                            "+" => sqrtFirst + secondDigit,
                            "-" => sqrtFirst - secondDigit,
                            "*" => sqrtFirst * secondDigit,
                            "/" when secondDigit != 0 => sqrtFirst / secondDigit,
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(innerResult) || innerResult < 0) continue;
                        
                        double sqrtInner = Math.Sqrt(innerResult);
                        if (double.IsNaN(sqrtInner)) continue;
                        
                        if (Math.Abs(sqrtInner - fourthDigit) < 0.0001)
                        {
                            string equation = $"√(√{firstDigit} {op} {secondDigit}) = {fourthDigit}";
                            validEquations.Add(equation);
                            
                            if (validEquations.Count >= maxResults) return;
                        }
                    }
                }
            }
        }
        
        static void FindComplexNestedPatterns(List<int> digits, HashSet<string> validEquations, int maxResults)
        {
            // This method handles very complex nested patterns like √((8 - √9) ^ 2) = 5
            // Pattern: √((a - √b) ^ c) = d
            
            if (validEquations.Count >= maxResults || digits.Count < 4) return;
            
            var basicOperators = new[] { "+", "-", "*", "/" };
            
            // NEW: Handle pattern √(√a ^ √a - b) = c for digits [9,9,2,5]
            // This specifically handles √(√9 ^ √9 - 2) = 5
            for (int i = 0; i < digits.Count - 3; i++)
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];   // 9
                int secondDigit = digits[i + 1]; // 9
                int thirdDigit = digits[i + 2];  // 2
                int fourthDigit = digits[i + 3]; // 5
                
                // Try pattern: √(√firstDigit ^ √secondDigit - thirdDigit) = fourthDigit
                if (IsValidUnaryOperation(firstDigit, "√") && 
                    IsValidUnaryOperation(secondDigit, "√") &&
                    firstDigit == secondDigit) // Both digits must be the same for √a ^ √a pattern
                {
                    double sqrtFirst = CalculateUnary(firstDigit, "√");   // √9 = 3
                    double sqrtSecond = CalculateUnary(secondDigit, "√"); // √9 = 3
                    
                    // Calculate √first ^ √second (3 ^ 3 = 27)
                    double powerResult = Math.Pow(sqrtFirst, sqrtSecond);
                    if (double.IsNaN(powerResult)) continue;
                    
                    // Calculate powerResult - thirdDigit (27 - 2 = 25)
                    double subtractResult = powerResult - thirdDigit;
                    if (subtractResult < 0) continue;
                    
                    // Calculate √(subtractResult) (√25 = 5)
                    double finalResult = Math.Sqrt(subtractResult);
                    if (double.IsNaN(finalResult)) continue;
                    
                    // Check if it equals the fourth digit
                    if (Math.Abs(finalResult - fourthDigit) < 0.0001)
                    {
                        string equation = $"√(√{firstDigit} ^ √{secondDigit} - {thirdDigit}) = {fourthDigit}";
                        validEquations.Add(equation);
                        
                        if (validEquations.Count >= maxResults) return;
                    }
                }
                
                // Also try with addition: √(√a ^ √a + b) = c
                if (IsValidUnaryOperation(firstDigit, "√") && 
                    IsValidUnaryOperation(secondDigit, "√") &&
                    firstDigit == secondDigit)
                {
                    double sqrtFirst = CalculateUnary(firstDigit, "√");
                    double sqrtSecond = CalculateUnary(secondDigit, "√");
                    
                    double powerResult = Math.Pow(sqrtFirst, sqrtSecond);
                    if (double.IsNaN(powerResult)) continue;
                    
                    double addResult = powerResult + thirdDigit;
                    if (addResult < 0) continue;
                    
                    double finalResult = Math.Sqrt(addResult);
                    if (double.IsNaN(finalResult)) continue;
                    
                    if (Math.Abs(finalResult - fourthDigit) < 0.0001)
                    {
                        string equation = $"√(√{firstDigit} ^ √{secondDigit} + {thirdDigit}) = {fourthDigit}";
                        validEquations.Add(equation);
                        
                        if (validEquations.Count >= maxResults) return;
                    }
                }
            }
            
            // ORIGINAL: For digits [8,9,2,5], try pattern: √((8 - √9) ^ 2) = 5
            for (int i = 0; i < digits.Count - 3; i++)
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];   // 8
                int secondDigit = digits[i + 1]; // 9  
                int thirdDigit = digits[i + 2];  // 2 (exponent)
                int fourthDigit = digits[i + 3]; // 5
                
                // Try pattern: √((firstDigit op √secondDigit) ^ thirdDigit) = fourthDigit
                if (IsValidUnaryOperation(secondDigit, "√"))
                {
                    double sqrtSecond = CalculateUnary(secondDigit, "√");  // √9 = 3
                    
                    foreach (var innerOp in basicOperators)
                    {
                        // Calculate (firstDigit op √secondDigit)
                        double innerResult = innerOp switch
                        {
                            "+" => firstDigit + sqrtSecond,    // 8 + 3 = 11
                            "-" => firstDigit - sqrtSecond,    // 8 - 3 = 5
                            "*" => firstDigit * sqrtSecond,    // 8 * 3 = 24
                            "/" when sqrtSecond != 0 => firstDigit / sqrtSecond, // 8 / 3 = 2.67
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(innerResult)) continue;
                        
                        // Calculate (innerResult ^ thirdDigit)
                        if (thirdDigit >= 0 && thirdDigit <= 5) // Reasonable exponent range
                        {
                            double powResult = Math.Pow(innerResult, thirdDigit); // 5^2 = 25
                            if (double.IsNaN(powResult) || powResult < 0) continue;
                            
                            // Calculate √(powResult)
                            double sqrtResult = Math.Sqrt(powResult); // √25 = 5
                            if (double.IsNaN(sqrtResult)) continue;
                            
                            // Check if it equals the fourth digit
                            if (Math.Abs(sqrtResult - fourthDigit) < 0.0001)
                            {
                                string equation = $"√(({firstDigit} {innerOp} √{secondDigit}) ^ {thirdDigit}) = {fourthDigit}";
                                validEquations.Add(equation);
                                
                                if (validEquations.Count >= maxResults) return;
                            }
                        }
                    }
                }
                
                // Also try the reverse pattern where the square root is in different positions
                // Pattern: √((√firstDigit op secondDigit) ^ thirdDigit) = fourthDigit
                if (IsValidUnaryOperation(firstDigit, "√"))
                {
                    double sqrtFirst = CalculateUnary(firstDigit, "√");
                    
                    foreach (var innerOp in basicOperators)
                    {
                        double innerResult = innerOp switch
                        {
                            "+" => sqrtFirst + secondDigit,
                            "-" => sqrtFirst - secondDigit,
                            "*" => sqrtFirst * secondDigit,
                            "/" when secondDigit != 0 => sqrtFirst / secondDigit,
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(innerResult)) continue;
                        
                        if (thirdDigit >= 0 && thirdDigit <= 5)
                        {
                            double powResult = Math.Pow(innerResult, thirdDigit);
                            if (double.IsNaN(powResult) || powResult < 0) continue;
                            
                            double sqrtResult = Math.Sqrt(powResult);
                            if (double.IsNaN(sqrtResult)) continue;
                            
                            if (Math.Abs(sqrtResult - fourthDigit) < 0.0001)
                            {
                                string equation = $"√((√{firstDigit} {innerOp} {secondDigit}) ^ {thirdDigit}) = {fourthDigit}";
                                validEquations.Add(equation);
                                
                                if (validEquations.Count >= maxResults) return;
                            }
                        }
                    }
                }
            }
        }
        
        static void FindMixedUnaryPatterns(List<int> digits, HashSet<string> validEquations, int maxResults)
        {
            // This method handles patterns like 2 + 6 - √9 = √25
            // where we have mixed arithmetic and unary operations
            
            if (validEquations.Count >= maxResults || digits.Count < 4) return;
            
            var basicOperators = new[] { "+", "-", "*", "/" };
            var unaryOperators = new[] { "√", "²", "³", "∛", "!" };
            
            // For digits [2,6,9,2,5], try pattern: 2 + 6 - √9 = √25
            // Pattern: a op1 b op2 √c = √de
            for (int i = 0; i < digits.Count - 4; i++)
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];     // 2
                int secondDigit = digits[i + 1]; // 6
                int thirdDigit = digits[i + 2];  // 9
                int fourthDigit = digits[i + 3]; // 2
                int fifthDigit = digits[i + 4];  // 5
                
                // Try pattern: firstDigit op1 secondDigit op2 √thirdDigit = √(fourthDigit + fifthDigit)
                if (IsValidUnaryOperation(thirdDigit, "√"))
                {
                    double sqrtThird = CalculateUnary(thirdDigit, "√"); // √9 = 3
                    
                    // Create multi-digit number from remaining digits for right side
                    int rightSideNumber = fourthDigit * 10 + fifthDigit; // 25
                    if (IsValidUnaryOperation(rightSideNumber, "√"))
                    {
                        double sqrtRight = CalculateUnary(rightSideNumber, "√"); // √25 = 5
                        
                        foreach (var op1 in basicOperators)
                        {
                            foreach (var op2 in basicOperators)
                            {
                                // Calculate left side: firstDigit op1 secondDigit op2 √thirdDigit
                                double leftPart = op1 switch
                                {
                                    "+" => firstDigit + secondDigit,     // 2 + 6 = 8
                                    "-" => firstDigit - secondDigit,     // 2 - 6 = -4
                                    "*" => firstDigit * secondDigit,     // 2 * 6 = 12
                                    "/" when secondDigit != 0 => (double)firstDigit / secondDigit, // 2 / 6 = 0.33
                                    _ => double.NaN
                                };
                                
                                if (double.IsNaN(leftPart)) continue;
                                
                                double leftSide = op2 switch
                                {
                                    "+" => leftPart + sqrtThird,        // 8 + 3 = 11
                                    "-" => leftPart - sqrtThird,        // 8 - 3 = 5
                                    "*" => leftPart * sqrtThird,        // 8 * 3 = 24
                                    "/" when sqrtThird != 0 => leftPart / sqrtThird, // 8 / 3 = 2.67
                                    _ => double.NaN
                                };
                                
                                if (double.IsNaN(leftSide)) continue;
                                
                                // Check if equation is valid
                                if (Math.Abs(leftSide - sqrtRight) < 0.0001)
                                {
                                    string equation = $"{firstDigit} {op1} {secondDigit} {op2} √{thirdDigit} = √{rightSideNumber}";
                                    validEquations.Add(equation);
                                    
                                    if (validEquations.Count >= maxResults) return;
                                }
                            }
                        }
                    }
                }
            }
            
            // Also try pattern with 4 digits: a op1 b op2 √c = d
            for (int i = 0; i < digits.Count - 3; i++)
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];     // 2
                int secondDigit = digits[i + 1]; // 6
                int thirdDigit = digits[i + 2];  // 9
                int fourthDigit = digits[i + 3]; // 5
                
                if (IsValidUnaryOperation(thirdDigit, "√"))
                {
                    double sqrtThird = CalculateUnary(thirdDigit, "√"); // √9 = 3
                    
                    foreach (var op1 in basicOperators)
                    {
                        foreach (var op2 in basicOperators)
                        {
                            // Calculate left side: firstDigit op1 secondDigit op2 √thirdDigit
                            double leftPart = op1 switch
                            {
                                "+" => firstDigit + secondDigit,
                                "-" => firstDigit - secondDigit,
                                "*" => firstDigit * secondDigit,
                                "/" when secondDigit != 0 => (double)firstDigit / secondDigit,
                                _ => double.NaN
                            };
                            
                            if (double.IsNaN(leftPart)) continue;
                            
                            double leftSide = op2 switch
                            {
                                "+" => leftPart + sqrtThird,
                                "-" => leftPart - sqrtThird,
                                "*" => leftPart * sqrtThird,
                                "/" when sqrtThird != 0 => leftPart / sqrtThird,
                                _ => double.NaN
                            };
                            
                            if (double.IsNaN(leftSide)) continue;
                            
                            // Check if equation is valid
                            if (Math.Abs(leftSide - fourthDigit) < 0.0001)
                            {
                                string equation = $"{firstDigit} {op1} {secondDigit} {op2} √{thirdDigit} = {fourthDigit}";
                                validEquations.Add(equation);
                                
                                if (validEquations.Count >= maxResults) return;
                            }
                        }
                    }
                }
            }
            
            // Try reverse pattern: √a = b op1 c op2 d
            for (int i = 0; i < digits.Count - 3; i++)
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];     // Could be part of multi-digit under sqrt
                int secondDigit = digits[i + 1]; // Could be part of multi-digit under sqrt or standalone
                int thirdDigit = digits[i + 2];  
                int fourthDigit = digits[i + 3];
                
                // Try √(firstDigit + secondDigit) = thirdDigit op fourthDigit
                int leftSideNumber = firstDigit * 10 + secondDigit; // Form multi-digit number
                if (IsValidUnaryOperation(leftSideNumber, "√"))
                {
                    double sqrtLeft = CalculateUnary(leftSideNumber, "√");
                    
                    foreach (var op in basicOperators)
                    {
                        double rightSide = op switch
                        {
                            "+" => thirdDigit + fourthDigit,
                            "-" => thirdDigit - fourthDigit,
                            "*" => thirdDigit * fourthDigit,
                            "/" when fourthDigit != 0 => (double)thirdDigit / fourthDigit,
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(rightSide)) continue;
                        
                        if (Math.Abs(sqrtLeft - rightSide) < 0.0001)
                        {
                            string equation = $"√{leftSideNumber} = {thirdDigit} {op} {fourthDigit}";
                            validEquations.Add(equation);
                            
                            if (validEquations.Count >= maxResults) return;
                        }
                    }
                }
            }
            
            // NEW: Try parentheses with factorial patterns like (1 * 1 + 2)! = 6
            for (int i = 0; i < digits.Count - 3; i++)
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];     // 1
                int secondDigit = digits[i + 1]; // 1
                int thirdDigit = digits[i + 2];  // 2
                int fourthDigit = digits[i + 3]; // 6
                
                // Try pattern: (firstDigit op1 secondDigit op2 thirdDigit)! = fourthDigit
                foreach (var op1 in basicOperators)
                {
                    foreach (var op2 in basicOperators)
                    {
                        // Calculate inside parentheses: firstDigit op1 secondDigit op2 thirdDigit
                        double firstOperation = op1 switch
                        {
                            "+" => firstDigit + secondDigit,     // 1 + 1 = 2
                            "-" => firstDigit - secondDigit,     // 1 - 1 = 0
                            "*" => firstDigit * secondDigit,     // 1 * 1 = 1
                            "/" when secondDigit != 0 => (double)firstDigit / secondDigit, // 1 / 1 = 1
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(firstOperation)) continue;
                        
                        double parenthesesResult = op2 switch
                        {
                            "+" => firstOperation + thirdDigit,  // 1 + 2 = 3
                            "-" => firstOperation - thirdDigit,  // 1 - 2 = -1
                            "*" => firstOperation * thirdDigit,  // 1 * 2 = 2
                            "/" when thirdDigit != 0 => firstOperation / thirdDigit, // 1 / 2 = 0.5
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(parenthesesResult)) continue;
                        
                        // Check if the result is a valid input for factorial
                        int factorialInput = (int)Math.Round(parenthesesResult);
                        if (Math.Abs(parenthesesResult - factorialInput) < 0.0001 && 
                            IsValidUnaryOperation(factorialInput, "!"))
                        {
                            double factorialResult = CalculateUnary(factorialInput, "!"); // 3! = 6
                            
                            // Check if it equals the fourth digit
                            if (Math.Abs(factorialResult - fourthDigit) < 0.0001)
                            {
                                string equation = $"({firstDigit} {op1} {secondDigit} {op2} {thirdDigit})! = {fourthDigit}";
                                validEquations.Add(equation);
                                
                                if (validEquations.Count >= maxResults) return;
                            }
                        }
                    }
                }
                
                // NEW: Try pattern with negative first digit: (-firstDigit op1 secondDigit op2 thirdDigit)! = fourthDigit
                // This handles cases like (-5 + 6 + 2)! = 6
                foreach (var op1 in basicOperators)
                {
                    foreach (var op2 in basicOperators)
                    {
                        // Calculate inside parentheses: -firstDigit op1 secondDigit op2 thirdDigit
                        double firstOperation = op1 switch
                        {
                            "+" => -firstDigit + secondDigit,     // -5 + 6 = 1
                            "-" => -firstDigit - secondDigit,     // -5 - 6 = -11
                            "*" => -firstDigit * secondDigit,     // -5 * 6 = -30
                            "/" when secondDigit != 0 => (double)(-firstDigit) / secondDigit, // -5 / 6 = -0.83
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(firstOperation)) continue;
                        
                        double parenthesesResult = op2 switch
                        {
                            "+" => firstOperation + thirdDigit,  // 1 + 2 = 3
                            "-" => firstOperation - thirdDigit,  // 1 - 2 = -1
                            "*" => firstOperation * thirdDigit,  // 1 * 2 = 2
                            "/" when thirdDigit != 0 => firstOperation / thirdDigit, // 1 / 2 = 0.5
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(parenthesesResult)) continue;
                        
                        // Check if the result is a valid input for factorial
                        int factorialInput = (int)Math.Round(parenthesesResult);
                        if (Math.Abs(parenthesesResult - factorialInput) < 0.0001 && 
                            IsValidUnaryOperation(factorialInput, "!"))
                        {
                            double factorialResult = CalculateUnary(factorialInput, "!"); // 3! = 6
                            
                            // Check if it equals the fourth digit
                            if (Math.Abs(factorialResult - fourthDigit) < 0.0001)
                            {
                                string equation = $"(-{firstDigit} {op1} {secondDigit} {op2} {thirdDigit})! = {fourthDigit}";
                                validEquations.Add(equation);
                                
                                if (validEquations.Count >= maxResults) return;
                            }
                        }
                    }
                }
            }
            
            // NEW: Try negative parentheses arithmetic patterns like (-4 + 7) * 2 = 6
            for (int i = 0; i < digits.Count - 3; i++)
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];     // 4
                int secondDigit = digits[i + 1]; // 7
                int thirdDigit = digits[i + 2];  // 2
                int fourthDigit = digits[i + 3]; // 6
                
                // Try pattern: (-firstDigit op1 secondDigit) op2 thirdDigit = fourthDigit
                foreach (var op1 in basicOperators)
                {
                    foreach (var op2 in basicOperators)
                    {
                        // Calculate inside parentheses: -firstDigit op1 secondDigit
                        double parenthesesResult = op1 switch
                        {
                            "+" => -firstDigit + secondDigit,     // -4 + 7 = 3
                            "-" => -firstDigit - secondDigit,     // -4 - 7 = -11
                            "*" => -firstDigit * secondDigit,     // -4 * 7 = -28
                            "/" when secondDigit != 0 => (double)(-firstDigit) / secondDigit, // -4 / 7 = -0.57
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(parenthesesResult)) continue;
                        
                        // Calculate full equation: parenthesesResult op2 thirdDigit
                        double leftSide = op2 switch
                        {
                            "+" => parenthesesResult + thirdDigit,  // 3 + 2 = 5
                            "-" => parenthesesResult - thirdDigit,  // 3 - 2 = 1
                            "*" => parenthesesResult * thirdDigit,  // 3 * 2 = 6
                            "/" when thirdDigit != 0 => parenthesesResult / thirdDigit, // 3 / 2 = 1.5
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(leftSide)) continue;
                        
                        // Check if it equals the fourth digit
                        if (Math.Abs(leftSide - fourthDigit) < 0.0001)
                        {
                            string equation = $"(-{firstDigit} {op1} {secondDigit}) {op2} {thirdDigit} = {fourthDigit}";
                            validEquations.Add(equation);
                            
                            if (validEquations.Count >= maxResults) return;
                        }
                    }
                }
            }
            
            // NEW: Try arithmetic + factorial patterns like 1 + (1 + 2)! = 7
            for (int i = 0; i < digits.Count - 3; i++)
            {
                if (validEquations.Count >= maxResults) break;
                
                int firstDigit = digits[i];     // 1
                int secondDigit = digits[i + 1]; // 1
                int thirdDigit = digits[i + 2];  // 2
                int fourthDigit = digits[i + 3]; // 7
                
                // Try pattern: firstDigit op1 (secondDigit op2 thirdDigit)! = fourthDigit
                foreach (var op1 in basicOperators)
                {
                    foreach (var op2 in basicOperators)
                    {
                        // Calculate inside parentheses: secondDigit op2 thirdDigit
                        double parenthesesResult = op2 switch
                        {
                            "+" => secondDigit + thirdDigit,     // 1 + 2 = 3
                            "-" => secondDigit - thirdDigit,     // 1 - 2 = -1
                            "*" => secondDigit * thirdDigit,     // 1 * 2 = 2
                            "/" when thirdDigit != 0 => (double)secondDigit / thirdDigit, // 1 / 2 = 0.5
                            _ => double.NaN
                        };
                        
                        if (double.IsNaN(parenthesesResult)) continue;
                        
                        // Check if the result is a valid input for factorial
                        int factorialInput = (int)Math.Round(parenthesesResult);
                        if (Math.Abs(parenthesesResult - factorialInput) < 0.0001 && 
                            IsValidUnaryOperation(factorialInput, "!"))
                        {
                            double factorialResult = CalculateUnary(factorialInput, "!"); // 3! = 6
                            
                            // Calculate full equation: firstDigit op1 factorialResult
                            double leftSide = op1 switch
                            {
                                "+" => firstDigit + factorialResult,  // 1 + 6 = 7
                                "-" => firstDigit - factorialResult,  // 1 - 6 = -5
                                "*" => firstDigit * factorialResult,  // 1 * 6 = 6
                                "/" when factorialResult != 0 => firstDigit / factorialResult, // 1 / 6 = 0.17
                                _ => double.NaN
                            };
                            
                            if (double.IsNaN(leftSide)) continue;
                            
                            // Check if it equals the fourth digit
                            if (Math.Abs(leftSide - fourthDigit) < 0.0001)
                            {
                                string equation = $"{firstDigit} {op1} ({secondDigit} {op2} {thirdDigit})! = {fourthDigit}";
                                validEquations.Add(equation);
                                
                                if (validEquations.Count >= maxResults) return;
                            }
                        }
                    }
                }
            }
        }
        
        static void FindSquareRootWithParentheses(List<int> digits, HashSet<string> validEquations)
        {
            // Early exit if we already have many equations or for very large digit sets
            if (validEquations.Count > 20 || digits.Count > 6) return;
            
            // Try patterns like "√(a + b) + c = d" or "√(a * b) - c = d"
            var basicOps = new[] { "+", "-", "*", "/" };
            
            // For digits [2,7,2,5], try: √(2+7) + 2 = 5, √(2*7) - 2 = 5, etc.
            // Limit the search space for performance - more aggressive for large digit sets
            int maxFirstNumEnd = digits.Count >= 6 ? 1 : Math.Min(2, digits.Count - 2);
            for (int firstNumEnd = 0; firstNumEnd < maxFirstNumEnd; firstNumEnd++)
            {
                for (int secondNumStart = firstNumEnd + 1; secondNumStart < digits.Count - 1; secondNumStart++)
                {
                    for (int secondNumEnd = secondNumStart; secondNumEnd < Math.Min(secondNumStart + 1, digits.Count - 1); secondNumEnd++)
                    {
                        // First number in parentheses
                        int firstNum = 0;
                        for (int i = 0; i <= firstNumEnd; i++)
                        {
                            firstNum = firstNum * 10 + digits[i];
                        }
                        
                        // Second number in parentheses  
                        int secondNum = 0;
                        for (int i = secondNumStart; i <= secondNumEnd; i++)
                        {
                            secondNum = secondNum * 10 + digits[i];
                        }
                        
                        // Remaining digits for rest of equation
                        var remainingDigits = new List<int>();
                        for (int i = secondNumEnd + 1; i < digits.Count; i++)
                        {
                            remainingDigits.Add(digits[i]);
                        }
                        
                        if (remainingDigits.Count == 0) continue;
                        
                        // Try different operators inside parentheses
                        foreach (var innerOp in basicOps)
                        {
                            // Calculate value inside parentheses
                            double innerValue = innerOp switch
                            {
                                "+" => firstNum + secondNum,
                                "-" => firstNum - secondNum,
                                "*" => firstNum * secondNum,
                                "/" when secondNum != 0 => (double)firstNum / secondNum,
                                _ => double.NaN
                            };
                            
                            if (double.IsNaN(innerValue) || innerValue < 0) continue;
                            
                            // Calculate square root
                            double sqrtValue = Math.Sqrt(innerValue);
                            if (double.IsNaN(sqrtValue)) continue;
                            
                            // Try different ways to use remaining digits
                            TrySquareRootEquations(sqrtValue, firstNum, secondNum, 
                                innerOp, remainingDigits, validEquations);
                        }
                    }
                }
            }
        }
        
        static void TrySquareRootEquations(double sqrtValue, int firstNum, int secondNum, 
            string innerOp, List<int> remainingDigits, HashSet<string> validEquations)
        {
            var basicOps = new[] { "+", "-", "*", "/" };
            
            // Generate all possible numbers from remaining digits
            for (int split = 1; split <= remainingDigits.Count; split++)
            {
                // First remaining number
                int firstRemainingNum = 0;
                for (int i = 0; i < split; i++)
                {
                    firstRemainingNum = firstRemainingNum * 10 + remainingDigits[i];
                }
                
                // Second remaining number (if exists)
                int? secondRemainingNum = null;
                if (split < remainingDigits.Count)
                {
                    int num = 0;
                    for (int i = split; i < remainingDigits.Count; i++)
                    {
                        num = num * 10 + remainingDigits[i];
                    }
                    secondRemainingNum = num;
                }
                
                // Try different patterns
                foreach (var outerOp in basicOps)
                {
                    // Pattern: √(a op b) outerOp c = d
                    if (secondRemainingNum.HasValue)
                    {
                        double leftSide = outerOp switch
                        {
                            "+" => sqrtValue + firstRemainingNum,
                            "-" => sqrtValue - firstRemainingNum,
                            "*" => sqrtValue * firstRemainingNum,
                            "/" when firstRemainingNum != 0 => sqrtValue / firstRemainingNum,
                            _ => double.NaN
                        };
                        
                        if (!double.IsNaN(leftSide) && Math.Abs(leftSide - secondRemainingNum.Value) < 0.0001)
                        {
                            // Found valid equation
                            string equation = $"√({firstNum} {innerOp} {secondNum}) {outerOp} {firstRemainingNum} = {secondRemainingNum}";
                            validEquations.Add(equation);
                        }
                    }
                    
                    // Pattern: √(a op b) = c outerOp d (if we have enough remaining digits)
                    if (secondRemainingNum.HasValue)
                    {
                        double rightSide = outerOp switch
                        {
                            "+" => firstRemainingNum + secondRemainingNum.Value,
                            "-" => firstRemainingNum - secondRemainingNum.Value,
                            "*" => firstRemainingNum * secondRemainingNum.Value,
                            "/" when secondRemainingNum != 0 => (double)firstRemainingNum / secondRemainingNum.Value,
                            _ => double.NaN
                        };
                        
                        if (!double.IsNaN(rightSide) && Math.Abs(sqrtValue - rightSide) < 0.0001)
                        {
                            string equation = $"√({firstNum} {innerOp} {secondNum}) = {firstRemainingNum} {outerOp} {secondRemainingNum}";
                            validEquations.Add(equation);
                        }
                    }
                    
                    // Simple pattern: √(a op b) = c
                    if (!secondRemainingNum.HasValue && Math.Abs(sqrtValue - firstRemainingNum) < 0.0001)
                    {
                        string equation = $"√({firstNum} {innerOp} {secondNum}) = {firstRemainingNum}";
                        validEquations.Add(equation);
                    }
                }
            }
        }
        
        public static void TryEqualsAtAllPositions(List<ExpressionElement> expression, HashSet<string> validEquations)
        {
            for (int equalsPos = 1; equalsPos < expression.Count; equalsPos++)
            {
                var leftSide = expression.Take(equalsPos).ToList();
                var rightSide = expression.Skip(equalsPos).ToList();
                
                // Skip if right side is empty or starts with an operator
                if (rightSide.Count == 0 || rightSide[0].Type == "operator") continue;
                
                double leftValue = EvaluateExpression(leftSide);
                double rightValue = EvaluateExpression(rightSide);
                
                if (!double.IsNaN(leftValue) && !double.IsNaN(rightValue) && 
                    Math.Abs(leftValue - rightValue) < 0.0001)
                {
                    string leftText = string.Join(" ", leftSide.Select(e => e.Text));
                    string rightText = string.Join(" ", rightSide.Select(e => e.Text));
                    validEquations.Add($"{leftText} = {rightText}");
                }
            }
        }
        
        public static double EvaluateExpression(List<ExpressionElement> elements)
        {
            if (elements.Count == 0) return double.NaN;
            if (elements.Count == 1) return elements[0].Value;
            
            try
            {
                // Handle operator precedence: ^ first, then *, /, then +, -
                var values = new List<double>();
                var operators = new List<string>();
                
                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Type == "number" || elements[i].Type == "unary")
                    {
                        values.Add(elements[i].Value);
                    }
                    else if (elements[i].Type == "operator")
                    {
                        operators.Add(elements[i].Text);
                    }
                }
                
                // Handle power operations first
                for (int i = operators.Count - 1; i >= 0; i--)
                {
                    if (operators[i] == "^")
                    {
                        double result = Math.Pow(values[i], values[i + 1]);
                        values[i] = result;
                        values.RemoveAt(i + 1);
                        operators.RemoveAt(i);
                    }
                }
                
                // Handle multiplication and division
                for (int i = 0; i < operators.Count; i++)
                {
                    if (operators[i] == "*" || operators[i] == "/")
                    {
                        double result = operators[i] == "*" ? 
                            values[i] * values[i + 1] : 
                            values[i] / values[i + 1];
                        values[i] = result;
                        values.RemoveAt(i + 1);
                        operators.RemoveAt(i);
                        i--;
                    }
                }
                
                // Handle addition and subtraction
                for (int i = 0; i < operators.Count; i++)
                {
                    if (operators[i] == "+" || operators[i] == "-")
                    {
                        double result = operators[i] == "+" ? 
                            values[i] + values[i + 1] : 
                            values[i] - values[i + 1];
                        values[i] = result;
                        values.RemoveAt(i + 1);
                        operators.RemoveAt(i);
                        i--;
                    }
                }
                
                return values.Count == 1 ? values[0] : double.NaN;
            }
            catch
            {
                return double.NaN;
            }
        }
        
        public class ExpressionElement
        {
            public string Type { get; set; } = ""; // "number", "operator", "unary"
            public double Value { get; set; }
            public string Text { get; set; } = "";
        }
    }
}

