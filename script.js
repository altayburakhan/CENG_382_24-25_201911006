// Array to store login attempts
const loginAttempts = [];

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
    
    // Clear form
    this.reset();
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