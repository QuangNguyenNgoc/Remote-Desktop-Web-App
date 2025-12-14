# Issue #27: [DevOps] GitHub Actions CI/CD

### 🎯 Mục Tiêu

Tự động build, test và deploy với GitHub Actions

### ✅ Checklist

**CI - Continuous Integration:**
- [ ] Tạo `.github/workflows/ci.yml`
- [ ] Trigger: push, pull_request
- [ ] Job: restore → build → test
- [ ] Add test coverage badge
- [ ] Cache NuGet packages

**CD - Continuous Deployment (optional):**
- [ ] Tạo `.github/workflows/deploy.yml`
- [ ] Build Docker image
- [ ] Push to Docker Hub / GitHub Container Registry
- [ ] Deploy to VPS/Cloud (SSH hoặc Azure)

**Status Badges:**
- [ ] Add build status badge to README
- [ ] Add test coverage badge

### 🔗 Dependencies

- ⏳ #26: Docker (cho CD)
- Có thể làm CI trước mà không cần Docker

### 📝 Sample Workflow

```yaml
# .github/workflows/ci.yml
name: CI

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore -c Release
    
    - name: Test
      run: dotnet test --no-build -c Release --verbosity normal
```
