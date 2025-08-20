name: Pull Request
description: Submit a pull request to improve the project
title: "[PR]: "
labels: ["pull-request"]
body:
  - type: markdown
    attributes:
      value: |
        Thank you for contributing to the Task Management System! 🎉
        
        Please fill out this template to help us review your pull request.

  - type: dropdown
    id: pr-type
    attributes:
      label: Type of Change
      description: What type of change does this PR introduce?
      multiple: true
      options:
        - Bug fix (non-breaking change that fixes an issue)
        - New feature (non-breaking change that adds functionality)
        - Breaking change (fix or feature that causes existing functionality to not work as expected)
        - Performance improvement
        - Code refactoring (no functional changes)
        - Documentation update
        - Test improvements
        - CI/CD improvements
        - Security enhancement
    validations:
      required: true

  - type: textarea
    id: description
    attributes:
      label: Description
      description: Please describe what this PR does and why
      placeholder: |
        - What does this PR do?
        - Why is this change needed?
        - How does it work?
    validations:
      required: true

  - type: textarea
    id: related-issues
    attributes:
      label: Related Issues
      description: Link any related issues
      placeholder: |
        Fixes #(issue number)
        Closes #(issue number)
        Related to #(issue number)

  - type: checkboxes
    id: testing
    attributes:
      label: Testing
      description: How has this been tested?
      options:
        - label: Unit tests added/updated
        - label: Integration tests added/updated
        - label: End-to-end tests added/updated
        - label: Manual testing completed
        - label: Performance testing completed
        - label: Security testing completed

  - type: textarea
    id: test-description
    attributes:
      label: Test Description
      description: Describe the tests you ran to verify your changes
      placeholder: |
        - Test A: Description and result
        - Test B: Description and result
        - Manual testing steps and results

  - type: checkboxes
    id: code-quality
    attributes:
      label: Code Quality
      description: Please confirm the following
      options:
        - label: My code follows the style guidelines of this project
        - label: I have performed a self-review of my own code
        - label: I have commented my code, particularly in hard-to-understand areas
        - label: I have made corresponding changes to the documentation
        - label: My changes generate no new warnings
        - label: I have added tests that prove my fix is effective or that my feature works
        - label: New and existing unit tests pass locally with my changes
        - label: Any dependent changes have been merged and published

  - type: textarea
    id: breaking-changes
    attributes:
      label: Breaking Changes
      description: If this PR contains breaking changes, please describe them
      placeholder: |
        - Change 1: Description and migration path
        - Change 2: Description and migration path

  - type: textarea
    id: screenshots
    attributes:
      label: Screenshots/GIFs
      description: If applicable, add screenshots or GIFs to demonstrate the changes
      placeholder: Drag and drop images/GIFs here

  - type: textarea
    id: performance-impact
    attributes:
      label: Performance Impact
      description: Describe any performance implications of your changes
      placeholder: |
        - Database queries: faster/slower/same
        - API response times: faster/slower/same
        - Frontend rendering: faster/slower/same
        - Memory usage: lower/higher/same

  - type: textarea
    id: deployment-notes
    attributes:
      label: Deployment Notes
      description: Any special deployment considerations?
      placeholder: |
        - Database migrations required
        - Configuration changes needed
        - New environment variables
        - Third-party service updates

  - type: checkboxes
    id: security-considerations
    attributes:
      label: Security Considerations
      description: Security checklist (if applicable)
      options:
        - label: No sensitive data exposed in logs
        - label: Input validation implemented
        - label: Authentication/authorization properly handled
        - label: No hardcoded secrets or credentials
        - label: CORS settings reviewed
        - label: SQL injection prevention considered

  - type: textarea
    id: additional-context
    attributes:
      label: Additional Context
      description: Add any other context about the pull request here
      placeholder: Any additional information that reviewers should know
