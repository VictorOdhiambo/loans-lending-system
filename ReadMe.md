# Loan Management System

## Overview

A Loan Management System build with ASP.NET Core MVC intended to handle loan operations.

## Features

### 1. User Management
- **Register, authenticate and managers users within the system focusing on two major roles:**
  - **System Users**: System administrators in charge of viewing reports, managing customers (borrowers), handling loan operations like loan disbursement and loan repayment.
  - **Customers (borrowers)**: Loan applicants and existing borrowers who can:
    - Apply for loans online 
    - Track application status 
    - View loan details and balances 
    - Make payments and view payment history 
    - Update personal information 
    - Receive notifications about their accounts
- Basic user profile management and role-based access control

### 2. Loan Application and Loan Products
- **Submit and track loan applications with:**
  - **Loan Products**: The system should have different loan types being offered that have different terms (Interest rates, Minimum amount borrowed, Maximum amount borrowed, Charges and Penalties).

### 3. Customer Accounts
- **Customer Accounts**: Customer accounts represent individual ledgers for each borrower and the system ledgers as well. These accounts are meant to track financial transactions related to the customer—such as loan disbursements (debits), repayments (credits), interest, fees, and penalties—providing an accurate and auditable record.

### 4. Loan Repayment
- **Monitor repayments, calculate outstanding balances, and generate payment schedules:**
  - Automated payment schedule generation
  - Track individual payments and remaining balances
  - Payment recording and history
  - Outstanding balance calculations

### 5. Notifications
- **Automated reminders for repayments and status updates:**
  - **Email Integration:** Automated email notifications
  - **SMS Integration:** Text message alerts and reminders
  - Application status updates
  - Payment due reminders
  - Overdue payment notifications

### 6. Reporting
- **Generate reports on loans, repayments and user activity:**
  - Active loans summary
  - Payment history reports
  - Customer account statements
  - Basic dashboard with key metrics

### 7. Audit Logs
- **Track all system activities for compliance and troubleshooting:**
  - User login/logout tracking
  - Application status changes
  - Payment recordings
  - System configuration changes

## Coding Rules & Workflow

To maintain code quality and collaboration, follow these rules:

### 1. Always Pull Latest Code from Main

```bash
git checkout main
git pull origin main
```

### 2. Create a Working Branch for Each Issue

```bash
git checkout -b feature/issue-<issue-number>-<short-description>
```

### 3. Work on Your Feature or Bugfix

Make your changes and commit them:

```bash
git add .
git commit -m "Describe your changes"
```

### 4. Push Your Branch

```bash
git push origin feature/issue-<issue-number>-<short-description>
```

### 5. Create a Pull Request

- Go to the repository on GitHub.
- Create a Pull Request from your branch to `main`.
- Request a code review.

---

**Note:**  
Always pull from the `main` branch before starting new work to ensure you have the latest codebase.
