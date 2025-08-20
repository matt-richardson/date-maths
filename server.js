const express = require('express');
const { spawn } = require('child_process');
const path = require('path');
const app = express();
const port = 3000;

app.use(express.json());
app.use(express.static('.'));

// Endpoint to solve math equations for a given date
app.post('/api/solve', (req, res) => {
    const { date } = req.body;
    
    if (!date) {
        return res.status(400).json({ error: 'Date is required' });
    }

    // Convert date to dMyy format
    const dateObj = new Date(date);
    const day = dateObj.getDate();
    const month = dateObj.getMonth() + 1;
    const year = dateObj.getFullYear() % 100;
    const dateStr = `${day}${month}${year}`;
    
    console.log(`Converting date: ${date} → Day: ${day}, Month: ${month}, Year: ${year} → dMyy: ${dateStr}`);

    // Run the compiled C# application using dotnet
    const dotnetProcess = spawn('dotnet', ['/app/DateMaths/date-maths.dll', dateStr], {
        stdio: ['pipe', 'pipe', 'pipe']
    });

    let output = '';
    let errorOutput = '';

    dotnetProcess.stdout.on('data', (data) => {
        output += data.toString();
    });

    dotnetProcess.stderr.on('data', (data) => {
        errorOutput += data.toString();
    });

    dotnetProcess.on('close', (code) => {
        if (code !== 0) {
            console.error('C# process error:', errorOutput);
            return res.status(500).json({ error: 'Failed to solve equations' });
        }

        // Parse the output
        const lines = output.split('\n').filter(line => line.trim());
        let equations = [];
        let dateDisplay = '';
        let digitsDisplay = '';
        
        let foundEquations = false;
        
        for (const line of lines) {
            if (line.includes('Date (dMyy):')) {
                dateDisplay = line.replace('Date (dMyy):', '').trim();
            } else if (line.includes('Available digits:')) {
                digitsDisplay = line.replace('Available digits:', '').trim();
            } else if (line.includes('Found') && line.includes('valid equation')) {
                foundEquations = true;
            } else if (foundEquations && line.startsWith('  ') && !line.includes('and') && !line.includes('more')) {
                equations.push(line.trim());
            } else if (line.includes('No valid equations found')) {
                equations = [];
                break;
            }
        }

        res.json({
            success: true,
            dateStr: dateDisplay,
            digits: digitsDisplay.replace(/[\[\]]/g, '').split(', ').map(d => parseInt(d.trim())),
            equations: equations
        });
    });

    dotnetProcess.on('error', (error) => {
        console.error('Failed to start C# process:', error);
        res.status(500).json({ error: 'Failed to start solver process' });
    });
});

// Serve the main page
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'index.html'));
});

app.listen(port, () => {
    console.log(`🧮 Date Math Magic server running at http://localhost:${port}`);
    console.log('🎉 Ready to solve some equations!');
});