function togglePasswordVisibility(fieldId, btn) {
    var input = document.getElementById(fieldId);
    if (input.type === 'password') {
        input.type = 'text';
        btn.textContent = 'Hide';
    } else {
        input.type = 'password';
        btn.textContent = 'Show';
    }
}

document.addEventListener('DOMContentLoaded', function() {
    var form = document.querySelector('form');
    if (!form) return;
    form.addEventListener('submit', function(e) {
        // Date of Birth validation
        var dobInput = document.getElementById('DateOfBirth');
        var dobError = document.getElementById('dob-error');
        var dobValue = dobInput.value;
        if (dobValue) {
            var dob = new Date(dobValue);
            var today = new Date('2025-07-17'); // fixed as per requirement
            var age = today.getFullYear() - dob.getFullYear();
            var m = today.getMonth() - dob.getMonth();
            if (m < 0 || (m === 0 && today.getDate() < dob.getDate())) {
                age--;
            }
            if (age < 18) {
                dobError.style.display = 'inline';
                dobError.textContent = 'You must be at least 18 years old.';
                dobInput.focus();
                e.preventDefault();
                return false;
            } else {
                dobError.style.display = 'none';
            }
        }
        // Phone number validation
        var phoneInput = document.getElementById('PhoneNumber');
        if (!/^\d{10}$/.test(phoneInput.value)) {
            phoneInput.setCustomValidity('Phone number must be exactly 10 digits.');
            phoneInput.reportValidity();
            e.preventDefault();
            return false;
        } else {
            phoneInput.setCustomValidity('');
        }
        // National ID validation
        var idInput = document.getElementById('NationalId');
        if (!/^\d{8}$/.test(idInput.value)) {
            idInput.setCustomValidity('ID number must be exactly 8 digits.');
            idInput.reportValidity();
            e.preventDefault();
            return false;
        } else {
            idInput.setCustomValidity('');
        }
        // Email validation (HTML5 covers this, but for extra safety)
        var emailInput = document.getElementById('Email');
        var emailValue = emailInput.value;
        var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailPattern.test(emailValue)) {
            emailInput.setCustomValidity('Please enter a valid email address.');
            emailInput.reportValidity();
            e.preventDefault();
            return false;
        } else {
            emailInput.setCustomValidity('');
        }
        // Password match validation
        var passInput = document.getElementById('Password');
        var confirmInput = document.getElementById('ConfirmPassword');
        var passError = document.getElementById('password-match-error');
        if (passInput.value !== confirmInput.value) {
            passError.style.display = 'inline';
            passError.textContent = 'Passwords do not match.';
            confirmInput.focus();
            e.preventDefault();
            return false;
        } else {
            passError.style.display = 'none';
        }
    });
});
