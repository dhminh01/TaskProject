# Task Management System

This project is a Task Management system built with the following technologies:

- **.NET 8**
- **Microservice Architecture**
- **Microsoft SQL Server**
- **gRPC & Protobuf**
- **CQRS & MediatR**
- **MassTransit & RabbitMQ**
- **Gmail API**
- **React & Typescript**
- **Ant Design**

# Development notes:

## Development Progress

### Refactoring

- Code restructuring and organization
- File and folder structure optimization
- Logical separation of components

### Bug Fixes

- gRPC configuration and implementation
- MassTransit setup and integration
- Service communication improvements

### New Features

- Gateway API implementation
  - Inter-service communication layer
  - Service orchestration
- Task Management
  - Update task functionality
- Frontend Development
  - React implementation
  - Ant Design UI components

## Git Commit Convention

### Format

```
<type>[optional scope]: <description>
[optional body]
[optional footer(s)]
```

### Common Types

- **feat**: New feature
- **fix**: Bug fix
- **docs**: Documentation changes
- **style**: Code formatting (no logic changes)
- **refactor**: Code restructuring (no new features/fixes)
- **perf**: Performance improvements
- **test**: Adding/updating tests
- **build**: Build system changes
- **ci**: CI/CD configuration changes
- **chore**: Maintenance tasks
- **revert**: Reverting previous commits

### Examples

**Basic:**

```
feat: add user login functionality
fix: resolve database connection timeout
docs: update API documentation
style: format code with prettier
refactor: simplify validation logic
test: add unit tests for auth module
chore: update dependencies
```

**With scope:**

```
feat(auth): implement OAuth integration
fix(api): handle null user data
docs(readme): add setup instructions
perf(db): optimize user queries
```

**Breaking changes:**

```
feat!: remove legacy API endpoints
feat(api)!: change response format

BREAKING CHANGE: Legacy endpoints removed.
Migration guide available in docs/
```

### Rules

- Use lowercase for type/scope
- Use imperative mood ("add" not "added")
- Keep description under 50 characters
- No period at end of description
- Reference issues when relevant
- Use ! or BREAKING CHANGE for breaking changes

### Tools

- Commitizen: Interactive commit creation
- Commitlint: Validate commit format
- Husky: Git hooks for validation

### Benefits

- Consistent team standards
- Automated changelog generation
- Easy commit filtering
- Better CI/CD integration
- Cleaner Git history
