// Array to store class entries
const classEntries = [];

// Form elements
const classForm = document.getElementById('classForm');
const classNameInput = document.getElementById('className');
const numPeopleInput = document.getElementById('numPeople');
const descriptionInput = document.getElementById('description');
const classTable = document.getElementById('classTable');
const tableBody = classTable.querySelector('tbody');

// Form submit event
classForm.addEventListener('submit', function(e) {
    e.preventDefault();
    
    // Get form values
    const className = classNameInput.value;
    const numPeople = numPeopleInput.value;
    const description = descriptionInput.value;
    
    // Add to array
    classEntries.push({ className, numPeople, description });
    
    // Update table
    addRowToTable(className, numPeople, description);
    
    // Clear form
    classForm.reset();
    
    // Log entries
    console.log('All class entries:', classEntries);
});

// Function to add row to table
function addRowToTable(className, numPeople, description) {
    const row = document.createElement('tr');
    
    row.innerHTML = `
        <td>${className}</td>
        <td>${numPeople}</td>
        <td>${description}</td>
    `;
    
    // Row click event
    row.addEventListener('click', function() {
        console.log('Row clicked:', { className, numPeople, description });
        this.classList.toggle('highlighted');
    });
    
    // Row double-click event
    row.addEventListener('dblclick', function() {
        if (confirm('Do you want to remove this entry?')) {
            const index = Array.from(tableBody.children).indexOf(this);
            classEntries.splice(index, 1);
            this.remove();
            console.log('Entry removed');
        }
    });
    
    // Row hover events
    row.addEventListener('mouseover', function() {
        this.style.backgroundColor = '#e8f0fe';
    });
    
    row.addEventListener('mouseout', function() {
        this.style.backgroundColor = '';
    });
    
    tableBody.appendChild(row);
}

// Table click event (outside rows)
classTable.addEventListener('click', function(e) {
    if (e.target === classTable) {
        console.log('All entries:', classEntries);
    }
});

// Input focus events
const inputs = [classNameInput, numPeopleInput, descriptionInput];
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
    
    // Real-time validation
    input.addEventListener('keyup', function() {
        if (this.value) {
            this.style.borderColor = '#1a73e8';
        } else {
            this.style.borderColor = 'red';
        }
    });
}); 