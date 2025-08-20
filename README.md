# 🧮 Date Math Magic! ✨

A fun, animated web application that turns any date into magical mathematical equations! Perfect for 12-year-olds to explore math in an engaging way.

## Features

- 🎨 **Colorful, animated interface** with floating numbers and sparkle effects
- 📅 **Interactive date picker** - choose any date to solve
- 🔢 **Smart equation solver** - finds mathematical equations using all digits from dates
- 🎯 **Real-time solving** with animated loading states
- 📱 **Mobile-friendly** responsive design
- 🐳 **Dockerized** for easy deployment

## How It Works

1. Pick any date (or use today's date)
2. The app converts the date to dMyy format (day-month-year without leading zeros)
3. Extract all digits from the date
4. Find mathematical equations that use ALL the digits
5. Display the results with fun animations!

**Example**: August 19, 2025 → `19825` → digits `[1, 9, 8, 2, 5]`
- Results: `√(1 + 8) + 2 = 5` and `1 * 9 = 8 / 2 + 5`

## Quick Start with Docker

```bash
# Build the image
docker build -t date-math-magic .

# Run the container
docker run -p 3000:3000 date-math-magic

# Or use docker-compose
docker-compose up
```

Then open http://localhost:3000 in your browser!

## Development Setup

```bash
# Install dependencies
npm install

# Build the C# backend
dotnet build DateMaths/

# Start the server
npm start
```

## Architecture

- **Frontend**: HTML5 + CSS3 + Vanilla JavaScript with animations
- **Backend**: Node.js + Express server
- **Math Engine**: C# console application with advanced equation solver
- **Container**: Multi-stage Docker build with .NET Runtime + Node.js

## API Endpoints

### POST /api/solve
Solve equations for a given date.

**Request:**
```json
{
  "date": "2025-08-19"
}
```

**Response:**
```json
{
  "success": true,
  "dateStr": "19825",
  "digits": [1, 9, 8, 2, 5],
  "equations": [
    "√(1 + 8) + 2 = 5",
    "1 * 9 = 8 / 2 + 5"
  ]
}
```

## Math Engine Features

The C# backend supports:
- Basic arithmetic (+, -, ×, ÷)
- Unary operations (², ³, √, ∛, !, Σ)
- Complex nested patterns
- Square root with parentheses
- Advanced equation patterns

## Educational Value

Perfect for kids to:
- Practice mental math
- Learn about square roots and exponents
- Understand equation structure
- Explore mathematical patterns
- Have fun with numbers!

## License

MIT License - Feel free to use and modify for educational purposes!