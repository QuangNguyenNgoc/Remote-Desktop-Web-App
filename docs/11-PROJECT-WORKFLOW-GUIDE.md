# Hướng dẫn Workflow Dự án (Project Workflow Guide)

> **Tài liệu này hướng dẫn team sử dụng GitHub Projects với automation tự động.**

---

## 📋 Tổng quan Kanban Board

| Column | Mô tả | Trigger tự động |
|--------|-------|-----------------|
| **Backlog** | Các ý tưởng, yêu cầu chưa sẵn sàng | Thủ công |
| **Ready** | Đã rõ ràng, sẵn sàng để làm | Thủ công / Unassign |
| **In Progress** | Đang được thực hiện | ✅ Khi assign issue |
| **In Review** | Đang chờ review PR | ✅ Khi tạo PR |
| **Done** | Hoàn thành | ✅ Khi merge PR |

---

## 🔀 Quy ước Đặt tên Branch

### Format
```
{type}/issue-{number}-{short-description}
```

### Types

| Type | Mục đích | Ví dụ |
|------|----------|-------|
| `feat` | Tính năng mới | `feat/issue-42-add-login-page` |
| `fix` | Sửa lỗi | `fix/issue-15-null-pointer-error` |
| `docs` | Cập nhật tài liệu | `docs/issue-8-update-readme` |
| `refactor` | Tái cấu trúc code | `refactor/issue-23-cleanup-services` |
| `test` | Thêm/sửa tests | `test/issue-31-hub-unit-tests` |
| `chore` | Công việc khác | `chore/issue-5-update-deps` |

### Ví dụ thực tế

```bash
# Issue #42: Thêm trang đăng nhập
git checkout -b feat/issue-42-add-login-page

# Issue #15: Sửa lỗi null pointer
git checkout -b fix/issue-15-null-pointer-error
```

---

## 📝 Quy ước Commit Message

### Format
```
{type}(scope): description

[optional body]

Fixes #issue-number
```

### Ví dụ

```bash
# Commit đơn giản
git commit -m "feat(auth): add login page component"

# Commit với liên kết issue
git commit -m "fix(hub): handle null connection id

Fixes #15"
```

---

## 🚀 Workflow Hàng ngày

### Bước 1: Chọn Issue từ Ready

1. Mở [GitHub Project](https://github.com/users/QuangNguyenNgoc/projects/3)
2. Chọn issue từ column **Ready**
3. Click vào issue → **Assignees** → Assign cho bản thân
4. ✅ **Issue tự động chuyển sang "In Progress"**

### Bước 2: Tạo Branch

```bash
# Đảm bảo đang ở main branch mới nhất
git checkout main
git pull origin main

# Tạo branch theo format
git checkout -b feat/issue-42-add-login-page
```

### Bước 3: Code & Commit

```bash
# Làm việc và commit thường xuyên
git add .
git commit -m "feat(auth): implement login form"

# Push lên remote
git push -u origin feat/issue-42-add-login-page
```

### Bước 4: Tạo Pull Request

1. Truy cập GitHub repository
2. Click **"Compare & pull request"**
3. Điền thông tin PR:

```markdown
## Mô tả
Thêm trang đăng nhập với form validation

## Thay đổi
- Tạo LoginPage component
- Thêm validation cho email/password
- Kết nối với AuthService

## Liên kết
Fixes #42
```

4. ✅ **Issue tự động chuyển sang "In Review"**

### Bước 5: Review & Merge

1. Chờ team member review
2. Sửa theo feedback nếu có
3. Khi được approve → **Merge pull request**
4. ✅ **Issue tự động chuyển sang "Done"**

---

## ⚙️ Setup Ban đầu (Chỉ 1 lần)

> **Chỉ cần 1 người trong team thực hiện setup này.**

### 1. Tạo Personal Access Token

1. Truy cập: [GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)](https://github.com/settings/tokens)
2. Click **"Generate new token (classic)"**
3. Đặt tên: `Project Automation Token`
4. Chọn quyền:
   - ✅ `repo` (Full control of private repositories)
   - ✅ `project` (Full control of projects)
5. Click **"Generate token"**
6. **Copy token ngay** (sẽ không hiển thị lại!)

### 2. Thêm Token vào Repository Secrets

1. Truy cập: Repository → **Settings** → **Secrets and variables** → **Actions**
2. Click **"New repository secret"**
3. Điền:
   - **Name**: `PROJECT_TOKEN`
   - **Value**: Paste token đã copy
4. Click **"Add secret"**

### 3. Kiểm tra Cấu hình Project

Đảm bảo Project có các column với tên **chính xác** sau:
- `Backlog`
- `Ready`
- `In Progress`
- `In Review`
- `Done`

> ⚠️ **Lưu ý**: Tên column phải khớp chính xác (case-sensitive)!

---

## 🔧 Troubleshooting

### Issue không tự động di chuyển

**Nguyên nhân có thể:**

1. **Issue chưa được add vào Project**
   - Mở issue → Click "Projects" ở sidebar → Chọn project

2. **Token hết hạn hoặc thiếu quyền**
   - Kiểm tra Settings → Secrets → `PROJECT_TOKEN`
   - Tạo lại token nếu cần

3. **Tên column không đúng**
   - Kiểm tra tên column trong Project (case-sensitive)

4. **Branch name không đúng format**
   - Phải có `issue-{number}` trong tên branch
   - Ví dụ: `feat/issue-42-login` ✅
   - Sai: `feat/login-42` ❌

### Cách kiểm tra log workflow

1. Truy cập: Repository → **Actions**
2. Chọn workflow run gần nhất
3. Xem logs để debug

---

## 📌 Quick Reference

### Commands thường dùng

```bash
# Tạo branch mới
git checkout -b feat/issue-{number}-{description}

# Push branch
git push -u origin feat/issue-{number}-{description}

# Cập nhật từ main
git checkout main
git pull
git checkout -
git merge main
```

### PR Template

```markdown
## Mô tả
[Mô tả ngắn gọn thay đổi]

## Loại thay đổi
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Checklist
- [ ] Code đã được test
- [ ] Documentation đã được cập nhật

Fixes #[issue-number]
```

---

## 👥 Liên hệ

Nếu có vấn đề với workflow automation, liên hệ:
- Tạo issue mới với label `workflow-bug`
- Hoặc hỏi trực tiếp team lead
