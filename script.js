// Array to store login attempts
const loginAttempts = [];

// Default credentials
const DEFAULT_USERNAME = 'admin';
const DEFAULT_PASSWORD = 'admin';

// Function to update the clock
function updateClock() {
    const now = new Date();
    const timeString = now.toLocaleTimeString();
    document.getElementById('clock').textContent = timeString;
}

// Update clock every second
setInterval(updateClock, 1000);
updateClock(); // Initial call to avoid delay

// Handle form submission
document.getElementById('loginForm').addEventListener('submit', function(e) {
    e.preventDefault();
    
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    
    // Store login attempt in array
    loginAttempts.push({
        username,
        password,
        timestamp: new Date().toISOString()
    });
    
    // Log all attempts to console
    console.log('All login attempts:', loginAttempts);
    
    // Check credentials
    if (username === DEFAULT_USERNAME && password === DEFAULT_PASSWORD) {
        // Successful login
        alert('Login successful! Redirecting to table page...');
        window.location.href = 'table.html';
    } else {
        // Failed login
        alert('Invalid credentials. Please try again.');
        this.reset();
    }
});

// Toggle form visibility with 'H' key
let formsVisible = true;
document.addEventListener('keydown', function(e) {
    if (e.key.toLowerCase() === 'h') {
        const loginContainer = document.querySelector('.login-container');
        formsVisible = !formsVisible;
        loginContainer.style.display = formsVisible ? 'block' : 'none';
    }
});

// Input focus events
const inputs = document.querySelectorAll('input');
inputs.forEach(input => {
    input.addEventListener('focus', function() {
        this.style.borderColor = '#1a73e8';
        this.style.boxShadow = '0 0 5px rgba(26, 115, 232, 0.3)';
    });
    
    input.addEventListener('blur', function() {
        this.style.borderColor = '#ddd';
        this.style.boxShadow = 'none';
        
        // Simple validation
        if (!this.value) {
            this.style.borderColor = 'red';
        }
    });
}); 