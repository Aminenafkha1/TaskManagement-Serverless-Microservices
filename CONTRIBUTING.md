# Contributing to Task Management System

First off, thank you for considering contributing to the Task Management System! 🎉

The following is a set of guidelines for contributing to this project. These are mostly guidelines, not rules. Use your best judgment, and feel free to propose changes to this document in a pull request.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Guidelines](#development-guidelines)
- [Pull Request Process](#pull-request-process)
- [Style Guidelines](#style-guidelines)

## Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code.

## Getting Started

### Prerequisites

- .NET 8 SDK
- Azure CLI
- Visual Studio 2022 or VS Code
- Git

### Development Setup

1. Fork the repository
2. Clone your fork: `git clone https://github.com/yourusername/task-management.git`
3. Create a branch: `git checkout -b feature/your-feature-name`
4. Follow the setup instructions in the README.md

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the issue list as you might find out that you don't need to create one. When you are creating a bug report, please include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples to demonstrate the steps**
- **Describe the behavior you observed and what behavior you expected**
- **Include screenshots if applicable**
- **Include environment details** (OS, .NET version, browser, etc.)

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion, please include:

- **Use a clear and descriptive title**
- **Provide a step-by-step description of the suggested enhancement**
- **Provide specific examples to demonstrate the enhancement**
- **Describe the current behavior and explain the behavior you expected**
- **Explain why this enhancement would be useful**

### Your First Code Contribution

Unsure where to begin contributing? You can start by looking through these issue labels:

- `good-first-issue` - Issues that should only require a few lines of code
- `help-wanted` - Issues that should be a bit more involved

### Pull Requests

- Fill in the required template
- Do not include issue numbers in the PR title
- Include screenshots and animated GIFs in your pull request whenever possible
- Follow the C# and JavaScript style guides
- Include thoughtfully-worded, well-structured tests
- Document new code based on the Documentation Style Guide
- End all files with a newline

## Development Guidelines

### Architecture Principles

- Follow SOLID principles
- Use dependency injection
- Implement proper error handling
- Write unit tests for new functionality
- Follow Domain-Driven Design (DDD) patterns

### Code Quality

- Write self-documenting code
- Use meaningful variable and method names
- Keep methods small and focused
- Avoid deep nesting
- Handle exceptions appropriately

### Testing Requirements

- Unit tests for business logic
- Integration tests for API endpoints
- End-to-end tests for critical user flows
- Maintain minimum 80% code coverage

## Pull Request Process

1. **Create a feature branch** from `main`
2. **Make your changes** following the style guidelines
3. **Add or update tests** as needed
4. **Update documentation** if required
5. **Ensure all tests pass** locally
6. **Submit a pull request** with a clear description

### PR Template

```markdown
## Description
Brief description of what this PR does.

## Type of Change
- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds functionality)
- [ ] Breaking change (fix or feature that causes existing functionality to not work as expected)
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed

## Screenshots (if applicable)

## Checklist
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review of my own code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
```

## Style Guidelines

### C# Code Style

- Use PascalCase for public members
- Use camelCase for private fields
- Use meaningful names for variables and methods
- Follow Microsoft's C# Coding Conventions
- Use `var` when the type is obvious
- Prefer explicit types when clarity is important

### File Organization

```csharp
// 1. Using statements
using System;
using Microsoft.Extensions.DependencyInjection;

// 2. Namespace
namespace TaskManagement.Services
{
    // 3. Class definition
    public class TaskService : ITaskService
    {
        // 4. Private fields
        private readonly ICosmosDbService _cosmosDbService;
        
        // 5. Constructor
        public TaskService(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }
        
        // 6. Public methods
        public async Task<Task> CreateTaskAsync(CreateTaskDto dto)
        {
            // Implementation
        }
        
        // 7. Private methods
        private void ValidateTask(CreateTaskDto dto)
        {
            // Implementation
        }
    }
}
```

### Commit Message Format

Use the conventional commit format:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation only changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Build process or auxiliary tool changes

Examples:
```
feat(auth): add JWT token refresh functionality
fix(tasks): resolve null reference exception in task creation
docs(readme): update installation instructions
```

### Branch Naming

- `feature/description` - New features
- `bugfix/description` - Bug fixes
- `hotfix/description` - Critical fixes
- `docs/description` - Documentation updates

## Code Review Guidelines

### For Authors

- Keep PRs small and focused
- Provide clear descriptions
- Respond to feedback promptly
- Update PRs based on review comments

### For Reviewers

- Be constructive and respectful
- Focus on the code, not the person
- Suggest improvements
- Approve when ready

## Getting Help

- Check the [README.md](README.md) for setup instructions
- Browse existing [Issues](https://github.com/yourusername/task-management/issues)
- Join our [Discussions](https://github.com/yourusername/task-management/discussions)
- Contact the maintainers

## Recognition

Contributors will be recognized in:
- README.md contributors section
- Release notes for significant contributions
- GitHub repository insights

Thank you for contributing! 🚀
