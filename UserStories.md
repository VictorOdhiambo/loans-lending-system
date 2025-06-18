## User Stories Brief

A user story is a brief, informal description of a software feature told from the perspective of the user who desires that feature. It outlines what the user wants to achieve and why, typically using a simple template: "As a [type of user], I want [some goal] so that [some reason]". User stories are a core component of agile software development, promoting collaboration and shared understanding of requirements

---

Quick links: [Summary](#summary) | [Description](#description) | [Template](#template) | [Example](#example) | [Resources](#resources)

---

### Summary

A user story should typically have a summary structured this way:

1. **As a** [user concerned by the story]
1. **I want** [goal of the story]
1. **so that** [reason for the story]

The “so that” part is optional if more details are provided in the description.

This can then become “As a user managing my properties, I want notifications when adding or removing images.”

You can read about some reasons for this structure in this [nicely put article][1].

### Description

We’re using the following template to create user stories.

Since we have mentioned the type of user, the user story can refer to it with “I”.
This is useful for **consistency** and to **avoid repetition** in the Acceptance criteria.
It’s also good to practice a little **empathy**.

For example:

```markdown
1. I click on the “submit” button.
2. A modal window appears if I don’t have enough credits.
3. The modal window contains the following:
```

### Template

```markdown
[
The user story should have a reason to exist: what do I need as the user described in the summary?
This part details any detail that could not be passed by the summary.
]

### Acceptance Criteria

1. [If I do A B should happen.]
2. [If C happens the outcome should be.]

### Comments
Use this to add any relevant comments
```

## Loan Management System User Stories/Use Cases

### 1. User Management

- [ ] As a **system admin/user** I want to be able register other system users to the system.
- [ ] As a **customer** I want to be able to register as a borrower to the system.
As a **registered user (system admin/customer)** I want to tbe able to login into the system using my email and password.

#### Acceptance Criteria

1. If I try to register with an existing email address the system should prevent this.
2. If I try to use a password with less than 8 characters the system should prevent this.
3. Users password should not be stored in plain text at the database level.

#### Comments

```markdown
//Type here for any relevant comments.
```

### 2. Loan Application and Loan Products User Stories/Use Cases

- [ ] As a customer I want to apply for a loan for a particular loan product.
- [ ] As a customer I want to choose a loan product for the type of loan I want.
- [ ] As a customer I want to track my loan application being able to know if it is pending, approved, disbursed, cleared or rejected.
- [ ] As the system admin I should see all details of customers who have applied for loans within the system.
- [ ] As the system admin I should be able to approve or rejected loan applications submitted by customers.

#### Acceptance Criteria

1. If a loan does is not tied to a loan product I should be able to apply for it.

#### Comments

```markdown
//Type here for any relevant comments.
```

### 3. Customer Accounts User Stories/Use Cases

- [ ] As a customer I want to view my current account balance and transaction history so that I can track my loan status and payment history.
- [ ] As a system admin I want to automatically view all financial transactions (disbursements, payments, interest, fees) to from customer accounts so that I can maintain accurate financial records for each borrower.

#### Acceptance Criteria

1. Loan disbursements are recorded as debits.
2. Customer payments are recorded as credits.
3. Interest and fees are automatically calculated and posted.
4. All transactions show date, amount, type, and running balance.
5. System ledger is updated simultaneously.
6. Display current outstanding balance.
7. Show breakdown of principal, interest, and fees.

#### Comments

```markdown
//Type here for any relevant comments.
```

### 4. Loan Repayment User Stories/Use Cases


#### Acceptance Criteria


#### Comments

```markdown
//Type here for any relevant comments.
```

### 5. Notifications User Stories/Use Cases

- [ ] As a customer I want to receive a notification when I apply for a loan.
- [ ] As a customer I want to receive a notification when the system admin approves or rejects my loan application.
- [ ] As a customer I want to receive notifications to remind me to pay off my existing loans.

#### Acceptance Criteria

1. The email notifications should be automatically be sent out immediately.
2. The system should have email templates from which it sends out emails depending on the use case or type of notification.

#### Comments

```markdown
//Type here for any relevant comments.
```