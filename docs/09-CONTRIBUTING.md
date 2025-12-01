# Contributing Guidelines - Remote Control Desktop

## Quy Tắc Đóng Góp Code

### Code Style

#### C# Conventions
- **Naming**: PascalCase cho classes/methods, camelCase cho variables
- **Indentation**: 4 spaces (no tabs)
- **Braces**: K&R style (opening brace  on same line)
```csharp
public class AgentManager
{
    public void RegisterAgent(AgentInfo agent)
    {
        // Implementation
    }
}
```

#### Blazor Component Conventions
- **File naming**: PascalCase, match component name (`DeviceCard.razor`)
- **Parameter naming**: PascalCase with `[Parameter]` attribute
- **Event callbacks**: Prefix with `On` (`OnDeviceClick`)
```razor
@code {
    [Parameter] public AgentInfo AgentInfo { get; set; } = default!;
    [Parameter] public EventCallback<string> OnDeviceClick { get; set; }
}
```

#### Tailwind CSS Guidelines
- **Use utility classes** thay vì custom CSS
- **Responsive design**: Mobile-first với `md:`, `lg:`, `xl:` prefixes
- **CSS Variables** cho theming:
```razor
<div style="background-color: var(--card-bg);">
```

---

## Git Workflow

### Branching Strategy

```
main (production-ready)
 ├── develop (integration branch)
 │    ├── feature/screenshot-service
 │    ├── feature/signalr-hub
 │    ├── bugfix/connection-timeout
 │    └── hotfix/critical-bug
```

### Creating Feature Branch

```bash
# Checkout develop
git checkout develop
git pull origin develop

# Create feature branch
git checkout -b feature/my-feature

# Work on feature
git add .
git commit -m "feat(scope): description"

# Push to remote
git push origin feature/my-feature
```

### Commit Message Format

```
<type>(<scope>): <subject>

[optional body]

[optional footer]
```

**Types**:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Code formatting (no logic change)
- `refactor`: Code restructuring
- `test`: Adding/updating tests
- `chore`: Build/config changes

**Examples**:
```bash
git commit -m "feat(agent): add screenshot capture service"
git commit -m "fix(hub): resolve agent disconnection issue"
git commit -m "docs(setup): add macOS setup instructions"
git commit -m "refactor(ui): extract DeviceCard component"
```

---

## Pull Request Process

### 1. Create PR

**Title**: `feat: Add screenshot capture feature`

**Description Template**:
```markdown
## Changes Made
- Added ScreenshotService in Agent
- Created RemoteScreen.razor component
- Implemented CaptureScreen command handling

## Testing
- [ ] Built successfully (`dotnet build`)
- [ ] Manually tested screenshot capture
- [ ] Screenshot displays correctly in UI

## Screenshots
[Add screenshots if UI changes]

## Related Issues
Closes #123
```

### 2. Code Review Checklist

**Reviewers Should Check**:
- [ ] Code follows style guidelines
- [ ] No compiler warnings
- [ ] Documentation updated (if needed)
- [ ] Tests added/updated (if applicable)
- [ ] No security vulnerabilities
- [ ] Performance considerations addressed

### 3. Addressing Review Comments

```bash
# Make changes based on feedback
git add .
git commit -m "refactor: address review comments"
git push origin feature/my-feature
```

### 4. Merge

**After Approval**:
```bash
# Squash and merge to keep history clean
# Or rebase and merge for linear history
```

**Delete Branch After Merge**:
```bash
git branch -d feature/my-feature
git push origin --delete feature/my-feature
```

---

## Testing Requirements

### Unit Tests (When Implemented)

```csharp
// RemoteControl.Web.Tests/Services/AgentManagerServiceTests.cs
public class AgentManagerServiceTests
{
    [Fact]
    public void RegisterAgent_ShouldAddToAgentList()
    {
        var service = new AgentManagerService();
        var agent = new AgentInfo { AgentId = "test123" };
        
        service.RegisterAgent(agent);
        
        Assert.Contains(agent, service.GetAllAgents());
    }
}
```

### Integration Tests (When Implemented)

```csharp
public class SignalRHubTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task RegisterAgent_ShouldBroadcastToClients()
    {
        var connection = await CreateHubConnection();
        // Test SignalR flow
    }
}
```

### Manual Testing Checklist

**Before Submitting PR**:
- [ ] App builds without errors
- [ ] App runs without crashes
- [ ] Feature works as expected
- [ ] UI displays correctly (responsive design)
- [ ] No console errors in browser
- [ ] SignalR connection stable (when applicable)

---

## Documentation Requirements

**When Adding New Feature**, Update:
- [ ] `docs/06-FEATURES-IMPLEMENTATION.md` - Add feature details
- [ ] `docs/04-DATA-MODELS.md` - If new models added
- [ ] `docs/05-SIGNALR-PROTOCOL.md` - If new SignalR events
- [ ] `docs/03-CODEBASE-MAP.md` - If new files/folders

**XML Documentation**:
```csharp
/// <summary>
/// Captures a screenshot of the primary screen
/// </summary>
/// <returns>Screenshot data as base64 string</returns>
public ScreenshotResult CaptureScreen()
{
    // ...
}
```

---

## Project Structure Guidelines

### New Files Naming

**Services**: `[Feature]Service.cs` (e.g., `ScreenshotService.cs`)

**Components**: PascalCase matching class name (e.g., `DeviceCard.razor`)

**Hubs**: `[Feature]Hub.cs` (e.g., `RemoteControlHub.cs`)

### Folder Organization

```
RemoteControl.Web/
├── Components/
│   ├── Layout/      # Layout components only
│   ├── Pages/       # Routable pages (@page directive)
│   └── Shared/      # Reusable components (no @page)
├── Services/
│   └── [Feature]Service.cs
└── Hubs/
    └── RemoteControlHub.cs
```

---

## Security Checklist

**Before Committing**:
- [ ] No hardcoded passwords/API keys
- [ ] No sensitive data in logs
- [ ] Input validation for user inputs
- [ ] Proper error handling (don't expose stack traces)

**Use Environment Variables**:
```csharp
// ❌ Bad
var apiKey = "sk-1234567890";

// ✅ Good
var apiKey = Environment.GetEnvironmentVariable("API_KEY");
```

---

## Performance Best Practices

### Blazor Performance

**Virtualization** for large lists:
```razor
<Virtualize Items="@LargeProcessList" Context="process">
    <div>@process.ProcessName</div>
</Virtualize>
```

**Avoid Frequent StateHasChanged**:
```csharp
// ❌ Bad - StateHasChanged() in tight loop
foreach (var item in items)
{
    ProcessItem(item);
    StateHasChanged();  // Expensive!
}

// ✅ Good
foreach (var item in items)
{
    ProcessItem(item);
}
StateHasChanged();  // Once at the end
```

### SignalR Performance

**Batch Updates**:
```csharp
// Instead of sending 100 individual SystemInfo updates
// Send one batch update with all metrics
```

---

## Questions & Help

### Getting Help

- **Documentation**: Read `docs/` folder first
- **GitHub Issues**: Search existing issues before creating new
- **Discussions**: Use GitHub Discussions for questions
- **Code of Conduct**: Be respectful and constructive

### Reporting Bugs

**Issue Template**:
```markdown
## Bug Description
[Clear description of the bug]

## Steps to Reproduce
1. Go to '...'
2. Click on '....'
3. See error

## Expected Behavior
[What should happen]

## Actual Behavior
[What actually happens]

## Screenshots
[If applicable]

## Environment
- OS: Windows 11
- .NET Version: 10.0.0
- Browser: Chrome 120
```

---

## Acknowledgments

**Contributors**:
- [List of contributors]

**Special Thanks**:
- AI Assistants for documentation help
- Open-source libraries used

---

**Cập Nhật**: 02/12/2024  
**Phiên Bản**: 1.0
