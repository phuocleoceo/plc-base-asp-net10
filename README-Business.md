# JiPLC - Tài Liệu Nghiệp Vụ (Business Documentation)

> **JiPLC** (viết tắt của **Ji**ra + **PLC**) là một hệ thống quản lý dự án (Project Management System) được phát triển trên nền tảng **ASP.NET 10**, mô phỏng và kết hợp các tính năng cốt lõi của **Atlassian Jira** (quản lý issue theo Scrum/Agile) và **Trello** (quản lý task theo Kanban board). Hệ thống hướng tới các đội nhóm và doanh nghiệp vừa & nhỏ (SME) muốn có một công cụ quản lý công việc nội bộ, độc lập, có khả năng tùy biến cao và chi phí thấp thay vì sử dụng dịch vụ SaaS bên ngoài.

---

## Mục Lục

1. [Tổng Quan Sản Phẩm](#1-tổng-quan-sản-phẩm)
2. [Đối Tượng Người Dùng & Vai Trò](#2-đối-tượng-người-dùng--vai-trò)
3. [Mô Hình Phân Quyền Hai Cấp](#3-mô-hình-phân-quyền-hai-cấp)
4. [Các Domain Nghiệp Vụ Chính](#4-các-domain-nghiệp-vụ-chính)
5. [Luồng Nghiệp Vụ Chi Tiết](#5-luồng-nghiệp-vụ-chi-tiết)
6. [Mô Hình Doanh Thu (Monetization)](#6-mô-hình-doanh-thu-monetization)
7. [Quy Tắc Nghiệp Vụ Quan Trọng](#7-quy-tắc-nghiệp-vụ-quan-trọng)
8. [Trạng Thái Vòng Đời Của Các Thực Thể](#8-trạng-thái-vòng-đời-của-các-thực-thể)
9. [Thuật Ngữ Nghiệp Vụ (Glossary)](#9-thuật-ngữ-nghiệp-vụ-glossary)

---

## 1. Tổng Quan Sản Phẩm

### 1.1. Mục Tiêu Sản Phẩm

JiPLC được xây dựng để giải quyết các bài toán quản lý dự án thường gặp tại các đội nhóm phát triển phần mềm và các tổ chức nhỏ:

- **Quản lý công việc theo Scrum/Agile**: Hỗ trợ Sprint, Backlog, Story Point, Issue (User Story, Coding Task, Bug).
- **Quản lý trực quan theo Kanban**: Hỗ trợ Board hiển thị các công việc theo trạng thái tùy biến (To Do / In Progress / Done / ...).
- **Cộng tác trong dự án**: Mời thành viên, phân quyền, bình luận trên issue, tổ chức cuộc họp/sự kiện trong dự án.
- **Quản lý truy cập mịn (fine-grained)**: Phân quyền hai cấp (cấp hệ thống và cấp dự án), mỗi dự án có thể có các vai trò riêng biệt với tập quyền tùy biến.
- **Mô hình freemium có giới hạn**: Cho phép tạo một số dự án miễn phí, vượt giới hạn thì trừ vào credit người dùng (thanh toán qua VNPay).

### 1.2. Phạm Vi Nghiệp Vụ

| Phạm Vi | Mô tả |
|---|---|
| **Quản lý tài khoản** | Đăng ký, xác minh email, đăng nhập, đổi mật khẩu, khôi phục mật khẩu, làm mới token |
| **Quản lý hồ sơ người dùng** | Họ tên, số điện thoại, CMND/CCCD, avatar, địa chỉ, credit (số dư) |
| **Quản lý dự án** | Tạo, sửa, xoá mềm, phân quyền dự án, mời thành viên |
| **Quản lý Sprint** | Tạo Sprint, bắt đầu Sprint, hoàn thành Sprint, di chuyển issue giữa Sprint/Backlog |
| **Quản lý Issue (công việc)** | Tạo, cập nhật, xoá, di chuyển giữa Backlog & Board, bình luận, đính kèm media |
| **Quản lý Board (Kanban)** | Hiển thị issue theo cột trạng thái, kéo thả giữa các cột |
| **Quản lý lịch & sự kiện** | Tạo cuộc họp/sự kiện, thêm người tham dự (Attendees) trong phạm vi dự án |
| **Quản lý phân quyền dự án** | Role và Permission tùy biến cho từng dự án (Admin tạo, Leader gán) |
| **Thanh toán** | Mua thêm credit qua VNPay để tạo dự án vượt giới hạn miễn phí |
| **Quản trị hệ thống** | Admin quản lý users, roles, config settings (giới hạn dự án miễn phí, giá dự án) |
| **Lưu trữ tệp** | Upload file lên AWS S3 (avatar, hình ảnh issue, đính kèm) |

### 1.3. Kiến Trúc Tổng Quan (góc nhìn nghiệp vụ)

```
┌─────────────────────────────────────────────────────────────────┐
│                       Người Dùng Cuối                            │
│   (Web Client - React, deploy tại plc-base-react.vercel.app)     │
└────────────────────────────┬────────────────────────────────────┘
                             │ HTTPS / JWT Bearer
┌────────────────────────────┴────────────────────────────────────┐
│                    JiPLC Backend (ASP.NET 10)                    │
│                                                                  │
│   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌─────────────────┐    │
│   │   Auth   │ │ Project  │ │  Issue   │ │  Sprint/Board   │    │
│   └──────────┘ └──────────┘ └──────────┘ └─────────────────┘    │
│   ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌─────────────────┐    │
│   │Invitation│ │  Event   │ │ Payment  │ │  AccessControl  │    │
│   └──────────┘ └──────────┘ └──────────┘ └─────────────────┘    │
└────┬───────────────────┬───────────────────┬────────────────────┘
     │                   │                   │
┌────┴────┐       ┌──────┴─────┐       ┌─────┴──────┐
│  MySQL  │       │   Redis    │       │   AWS S3   │
│ (Data)  │       │  (Cache)   │       │  (Files)   │
└─────────┘       └────────────┘       └────────────┘
                  │ CAP Message Queue (in-memory)
                  ▼
            ┌──────────────┐         ┌──────────────┐
            │  Mail Worker │────────▶│  SMTP Gmail  │
            └──────────────┘         └──────────────┘
                                     ┌──────────────┐
                                     │   VNPay      │
                                     │ (Payment)    │
                                     └──────────────┘
```

---

## 2. Đối Tượng Người Dùng & Vai Trò

### 2.1. Vai Trò Cấp Hệ Thống (System Role)

Hệ thống có **2 vai trò chính ở cấp toàn hệ thống**, được định nghĩa trong `AppRole`:

| Mã vai trò | Tên hiển thị | Mô tả |
|---|---|---|
| `admin` | Quản trị viên hệ thống | Quản lý toàn bộ hệ thống: users, roles, config settings, project roles & permissions templates |
| `user` | Người dùng thông thường | Đăng ký tài khoản, tham gia/tạo dự án, làm việc trong dự án mình được mời/tạo |

> **Lưu ý**: Vai trò hệ thống được gán cho `UserAccount.RoleId`. Khi đăng ký mới, hệ thống mặc định gán `user`. Admin được seed sẵn (tài khoản đầu tiên `ht10082001@gmail.com`).

### 2.2. Vai Trò Cấp Dự Án (Project Role)

Khác với vai trò hệ thống, mỗi dự án có thể có **nhiều vai trò tùy biến**, ví dụ: Product Owner, Scrum Master, Developer, Tester, Stakeholder... Các vai trò này do **Admin hệ thống** định nghĩa (qua entity `ProjectRoleEntity`), sau đó **Leader dự án** sẽ gán cho từng thành viên (Member-Role mapping).

Vai trò mặc định/đặc biệt trong một dự án:

| Vai trò | Cách xác định | Quyền |
|---|---|---|
| **Creator (Người tạo dự án)** | `Project.CreatorId == user.Id` | Lưu lại để truy vết người sáng lập (audit) |
| **Leader (Trưởng dự án)** | `Project.LeaderId == user.Id` | Toàn quyền trong dự án (bypass mọi kiểm tra quyền) — xem `ProjectService.GetPermissionsInProjectForUser` |
| **Member (Thành viên thường)** | Có record trong `ProjectMember` | Có quyền theo các Project Role được gán qua bảng `MemberRole` |

> Khi tạo dự án, Creator mặc định là Leader. Leader có thể được chuyển giao cho người khác (qua `UpdateProject`).

### 2.3. Trạng Thái Tài Khoản

Một `UserAccount` có 2 cờ trạng thái độc lập:

| Cờ | Ý nghĩa | Khi nào set `true` |
|---|---|---|
| `IsVerified` | Email đã được xác minh | Sau khi user click vào link xác minh trong email |
| `IsActived` | Tài khoản đang hoạt động | Mặc định `true` khi đăng ký; Admin có thể tắt để khoá tài khoản |

Cả hai cờ phải `true` thì user mới có thể đăng nhập (xem `AuthService.Login`).

---

## 3. Mô Hình Phân Quyền Hai Cấp

Đây là một trong những điểm nổi bật về thiết kế nghiệp vụ của JiPLC.

### 3.1. Cấp 1 — Phân Quyền Hệ Thống (Role-Based)

Áp dụng cho các API quản trị chung của hệ thống. Sử dụng attribute `[Authorize(Roles = AppRole.ADMIN)]`.

**Các nhóm chức năng yêu cầu quyền ADMIN:**
- Quản lý danh sách Users (`GET /User`)
- Cập nhật tài khoản User (`PUT /User/Account/{userId}`)
- Quản lý Roles (`GET /api/roles`)
- Quản lý Config Settings (`GET/PUT /api/config-setting`)
- Quản lý Project Roles (`GET/POST/PUT/DELETE /api/project-role`)
- Quản lý Project Permissions của Role (`/api/project-role/{id}/project-permission`)

**Các API thông thường** chỉ yêu cầu `[Authorize]` (bất kỳ user đã đăng nhập).

### 3.2. Cấp 2 — Phân Quyền Dự Án (Permission-Based)

Áp dụng cho các thao tác bên trong một dự án. Mỗi user trong dự án có thể có nhiều `ProjectRole`, mỗi role chứa một tập `ProjectPermission`.

**Các nhóm permission chính** (xem `PermissionPolicy`):

| Module | Các permission tiêu biểu |
|---|---|
| **User** | GetAll, GetOne, Update, GetPersonal, UpdatePersonal, GetAnonymous |
| **Project** | GetAll, GetOne, Create, Update, Delete |
| **ProjectMember** | GetAll, Delete |
| **ProjectStatus** | GetAll, Create, Update, Delete, UpdateForBoard |
| **Sprint** | GetAvailable, GetOne, Create, Update, Delete, Start, Complete |
| **Issue** | GetForBoard, UpdateForBoard, MoveToBacklog, GetForBacklog, UpdateForBacklog, MoveToSprint, GetOne, Create, Update, Delete |
| **Invitation** | GetForProject, GetForUser, Create, Delete, Accept, Decline |
| **Event** | GetAll, GetOne, Create, Update, Delete |
| **Payment** | Create, Submit |
| **ProjectRole** | GetAll, GetSelect, GetOne, Create, Update, Delete |
| **MemberRole** | GetAll, Create, Delete |
| **ConfigSetting** | GetAll, GetOne, Update |

### 3.3. Quy Tắc Đặc Biệt Cho Leader

Khi user là Leader của dự án (`Project.LeaderId == reqUser.Id`), hệ thống **bypass toàn bộ kiểm tra permission cấp dự án** và trả về toàn bộ permission. Điều này đảm bảo Leader luôn có toàn quyền điều hành mà không cần Admin gán thủ công.

### 3.4. Cơ Chế Cache Permissions

Để tối ưu hiệu năng, các permission keys của từng Project Role được cache trong **Redis** bằng cấu trúc `Hash` (Map Cache). Mỗi khi permission của một role bị thay đổi (thêm/xoá), entry tương ứng trong Redis sẽ bị xoá để đảm bảo dữ liệu tươi.

---

## 4. Các Domain Nghiệp Vụ Chính

### 4.1. Identity & Authentication

**Mục đích**: Quản lý đăng ký, đăng nhập, xác minh email, đổi/khôi phục mật khẩu, JWT token, refresh token.

**Các thực thể liên quan**:
- `UserAccountEntity`: email, mật khẩu (BCrypt hash), trạng thái xác minh & kích hoạt, security code (dùng cho confirm email & recover password), refresh token + thời hạn.
- `UserProfileEntity`: tên hiển thị, SĐT, CMND/CCCD, avatar, credit, địa chỉ (liên kết với Ward).
- `RoleEntity` + `PermissionEntity`: vai trò hệ thống (chưa được dùng cho permission-based authorization cấp hệ thống — Role hiện chỉ chứa name/description).

**Token mechanism**:
- **Access Token**: JWT ký bằng RSA (private key tại `Certificate/pri.key`, public key tại `Certificate/pub.crt`), thời hạn 900s (15 phút).
- **Refresh Token**: thời hạn 2.592.000s (30 ngày), lưu vào DB cùng với `RefreshTokenExpiredAt`. Khi user logout (`Revoke-Refresh-Token`), giá trị này được xoá → các thiết bị khác cũng bị logout.

### 4.2. Address (Địa chỉ Hành chính Việt Nam)

**Mục đích**: Cung cấp dữ liệu địa chỉ 3 cấp Việt Nam (Tỉnh/Thành → Quận/Huyện → Phường/Xã) cho người dùng chọn khi đăng ký hoặc cập nhật profile.

**Các API public** (không cần đăng nhập):
- `GET /Provinces` — Danh sách tỉnh/thành
- `GET /Provinces/{provinceId}/Districts` — Danh sách quận/huyện theo tỉnh
- `GET /Districts/{districtId}/Wards` — Danh sách phường/xã theo quận
- `GET /Full-Address/{wardId}` — Lấy địa chỉ đầy đủ từ wardId

Dữ liệu được seed từ file SQL `Common/Data/Migrations/SQL/1.address.sql`.

### 4.3. Project (Dự Án)

**Mục đích**: Đơn vị tổ chức cao nhất của công việc. Mọi Issue, Sprint, Board, Event, Member đều thuộc về một Project.

**Các trường quan trọng**:
- `Name`: Tên dự án (ví dụ: "Dự án CRM 2026")
- `Key`: Mã rút gọn (ví dụ: "CRM") — dùng làm prefix cho Issue (mặc dù trong code hiện chưa thấy gán cụ thể)
- `Image`: Ảnh đại diện
- `CreatorId`: Người tạo (immutable)
- `LeaderId`: Trưởng dự án (có thể thay đổi)
- Hỗ trợ **soft delete** (`DeletedAt`)

**Khi tạo dự án**, hệ thống tự động:
1. Kiểm tra hạn mức miễn phí (xem [Mô hình doanh thu](#6-mô-hình-doanh-thu-monetization))
2. Tạo bản ghi `ProjectMember` cho người tạo
3. Tạo `ProjectStatus` mặc định: `"To Do"` với `Index = 0`

### 4.4. ProjectMember (Thành Viên Dự Án)

**Mục đích**: Bảng nối nhiều-nhiều giữa `UserAccount` và `Project`, kèm thông tin về vai trò trong dự án thông qua `MemberRole`.

**Quy tắc**:
- Một user có thể tham gia nhiều dự án.
- Member bị xoá là **soft delete**.
- Khi user **rời dự án** (`LeaveProject`): chỉ cho phép nếu user không phải Leader.
- Khi **dự án bị xoá**: tất cả member tương ứng cũng bị soft delete.

### 4.5. Invitation (Lời Mời Tham Gia Dự Án)

**Mục đích**: Cơ chế mời thành viên mới vào dự án bằng email.

**Vòng đời lời mời**:
```
[Tạo lời mời (Pending)] ──▶ [Accepted] ─── Thêm vào ProjectMember
                       └─▶ [Declined] ──── Không thêm
```

**Các trường ngày**:
- `AcceptedAt`: thời điểm chấp nhận (null nếu chưa)
- `DeclinedAt`: thời điểm từ chối (null nếu chưa)
- Một lời mời chỉ "còn hiệu lực" khi cả `AcceptedAt` và `DeclinedAt` đều null.

**Ràng buộc**:
- Sender không thể tự mời chính mình (`reqUser.Id != recipient.Id`).
- Recipient phải tồn tại trong hệ thống (truy email).
- Chỉ Sender mới có thể xoá lời mời mình đã gửi.

### 4.6. ProjectStatus (Trạng Thái Cột trên Board)

**Mục đích**: Định nghĩa các cột trên bảng Kanban (Board) của một dự án. Ví dụ: "To Do", "In Progress", "In Review", "Done".

**Đặc điểm**:
- Mỗi `ProjectStatus` có một `Index` (double) — dùng để sắp xếp thứ tự cột. Dùng `double` thay vì `int` để dễ chèn cột mới giữa hai cột hiện có (giá trị giữa, không cần shift toàn bộ).
- Hỗ trợ **soft delete**.

**Quy tắc xoá Status**:
- Mỗi dự án phải có **ít nhất 1 status** — không thể xoá status cuối cùng.
- Khi xoá một status: hệ thống tự động **di chuyển toàn bộ issue ở status đó sang một status khác** (status còn lại đầu tiên/được chỉ định) thay vì để issue "mồ côi".

### 4.7. Sprint (Vòng Lặp Phát Triển)

**Mục đích**: Đơn vị thời gian Agile/Scrum, thường kéo dài 1-4 tuần, chứa các issue cần hoàn thành.

**Các trường ngày**:
- `FromDate`, `ToDate`: Dự kiến (lập kế hoạch)
- `StartedAt`: Thời điểm thực sự bắt đầu (set khi gọi API `Start`)
- `CompletedAt`: Thời điểm hoàn thành (set khi gọi API `Complete`)

**Vòng đời Sprint**:
```
[Created] ──Start──▶ [In Progress] ──Complete──▶ [Completed]
   │
   └─Delete─▶ Issue trong Sprint → Backlog
```

**Quy tắc đặc biệt**:
- **"Available Sprint"** là Sprint duy nhất chưa có `CompletedAt` của một dự án. Khi MoveToSprint, hệ thống đẩy issue vào Sprint này.
- Khi **xoá Sprint**: các issue trong Sprint được chuyển trả về Backlog (giữ `BacklogIndex`, gỡ `SprintId`).
- Khi **hoàn thành Sprint**, user phải chỉ định:
  - `CompletedIssues[]`: Các issue đã hoàn thành → gỡ khỏi Sprint, gỡ khỏi Backlog (coi như done)
  - `UnCompletedIssues[]`: Các issue chưa xong và `MoveType`:
    - `"backlog"`: chuyển trả về Backlog
    - `"next_sprint"`: tự động tạo Sprint mới và chuyển sang đó

### 4.8. Issue (Công Việc)

**Mục đích**: Đơn vị công việc nhỏ nhất có thể giao cho thành viên. Đây là trung tâm của nghiệp vụ JiPLC.

**Phân loại theo `Type`**:
| Type | Mô tả |
|---|---|
| `user_story` | Câu chuyện người dùng (yêu cầu nghiệp vụ) |
| `coding_task` | Công việc lập trình |
| `bug` | Lỗi cần khắc phục |

**Phân loại theo `Priority`**:
| Priority | Mô tả |
|---|---|
| `low` | Thấp |
| `medium` | Trung bình |
| `high` | Cao |
| `critical` | Khẩn cấp/Nghiêm trọng |

**Các vai trò trên Issue**:
- `Reporter` (bắt buộc): Người tạo issue (thường là người phát hiện vấn đề/yêu cầu)
- `Assignee` (tùy chọn): Người được giao xử lý
- `Project`: Dự án chứa issue
- `Sprint` (tùy chọn): Sprint chứa issue. Khi `SprintId == null` → issue ở Backlog.
- `ProjectStatus`: Cột trạng thái hiện tại trên Board

**Hai trường Index điều khiển vị trí**:
- `BacklogIndex` (double, nullable): Vị trí trong Backlog. Khi issue chuyển sang Sprint, set `BacklogIndex = null`.
- `ProjectStatusIndex` (double, nullable): Vị trí trong cột Status. Cho phép kéo thả sắp xếp lại issue trong một cột.

**Các trường khác**:
- `Title`, `Description`: Tiêu đề và mô tả
- `StoryPoint`: Điểm ước lượng theo Scrum (Fibonacci 1, 2, 3, 5, 8, 13...)
- Hỗ trợ **soft delete**

**Quan hệ với Media**: Issue có thể đính kèm nhiều file (ảnh, tài liệu) — lưu qua entity `Media` với `EntityType = "issue"` và `EntityId = issue.Id`.

### 4.9. Issue Comment (Bình Luận)

**Mục đích**: Trao đổi và thảo luận về một issue cụ thể.

**Quy tắc**:
- Chỉ user đã đăng nhập trong dự án mới được comment.
- Chỉ tác giả của comment mới có quyền sửa/xoá comment đó (kiểm tra `userId == reqUser.Id`).
- Có thể đính kèm media với `EntityType = "comment"`.

### 4.10. Board (Bảng Kanban)

**Mục đích**: Hiển thị tất cả issue trong một Sprint dưới dạng các cột theo `ProjectStatus`.

**API**: `GET /api/project/{projectId}/board/{sprintId}/issue`

**Logic hiển thị**:
- Lọc issue có: `projectId`, `sprintId`, `DeletedAt == null`, `BacklogIndex == null` (đã được kéo vào Sprint)
- Hỗ trợ filter theo `Assignees` (multi-select dạng "1,2,3")
- Hỗ trợ search theo `Title` hoặc `StoryPoint`
- Group theo `ProjectStatusId` → mỗi nhóm là một cột
- Trong từng cột, sắp xếp theo `ProjectStatusIndex`

**Thao tác kéo thả trên Board** (`UpdateBoardIssue`):
- Đổi cột: cập nhật `ProjectStatusId`
- Đổi vị trí trong cột: cập nhật `ProjectStatusIndex` (giá trị giữa 2 issue lân cận)
- Đổi assignee: cập nhật `AssigneeId`

### 4.11. Backlog

**Mục đích**: Danh sách công việc chưa được đưa vào Sprint nào, đang chờ lập kế hoạch.

**Định nghĩa Issue thuộc Backlog**:
- `BacklogIndex != null` **VÀ** `SprintId == null`

**Thao tác trên Backlog**:
- Sắp xếp issue trong Backlog theo `BacklogIndex`
- Kéo issue từ Backlog vào Sprint (`MoveToSprint`): set `SprintId = availableSprintId`, giữ `BacklogIndex` (để khi nào move ngược lại còn nhớ vị trí cũ?)
- Kéo issue ngược từ Board về Backlog (`MoveToBacklog`)

### 4.12. Event (Sự Kiện / Cuộc Họp)

**Mục đích**: Lịch họp/sự kiện trong dự án (Daily Standup, Sprint Review, Retro, Planning, v.v.).

**Đặc điểm**:
- Mỗi event có `StartTime`, `EndTime`, `Creator`, `Project`.
- `EventAttendee`: bảng nối nhiều-nhiều với User → ai được mời tham dự.
- **Creator tự động luôn là attendee** — không thể gỡ ra khỏi danh sách attendees.
- Khi lấy danh sách events: chỉ trả về events mà user hiện tại có trong danh sách Attendees (filter theo `reqUser.Id`).
- Có filter theo khoảng thời gian (`Start`, `End`) — phù hợp với view Calendar.

**Cập nhật Attendees**:
- Logic so sánh tập hợp: thêm những user mới, xoá những user không còn trong danh sách (nhưng giữ Creator).

### 4.13. ProjectAccess (Phân Quyền Dự Án)

Gồm 3 thực thể:

#### a. ProjectRole (Vai trò dự án — template do Admin tạo)
- Ví dụ: "Developer", "Tester", "Product Owner"...
- Do Admin hệ thống tạo, dùng chung cho mọi dự án.

#### b. ProjectPermission (Quyền của Role)
- Mỗi `ProjectRole` chứa nhiều `ProjectPermission` (mỗi cái là một string key như `Issue.Create`, `Sprint.Delete`...).
- Admin có thể thêm/xoá permission cho một role.
- Có cache Redis để tối ưu truy vấn permission của role.

#### c. MemberRole (Gán Role cho Member trong dự án)
- Liên kết `ProjectMember` (user trong dự án) với `ProjectRole`.
- Một member có thể có nhiều role (union các permission).
- Do Leader/người có quyền Member.Role.Create gán.

### 4.14. Payment (Thanh Toán)

**Mục đích**: Mua thêm credit để tạo dự án vượt mức miễn phí. Tích hợp **VNPay** (cổng thanh toán Việt Nam).

**Luồng thanh toán** (xem chi tiết tại [Mô hình doanh thu](#6-mô-hình-doanh-thu-monetization)):
1. **Create Payment**: Tạo URL thanh toán VNPay → frontend redirect user sang VNPay.
2. **VNPay callback** → frontend bắt query params → gọi **Submit Payment** với các tham số `vnp_*`.
3. Backend verify response code → cộng credit vào user profile.

**Lưu ý kỹ thuật**:
- VNPay yêu cầu `vnp_Amount = amount × 100` (đơn vị: VND nhân 100).
- `vnp_TxnRef`: tham chiếu giao dịch, sinh từ `DateTime.Ticks` để đảm bảo unique.
- Có cơ chế chống double-submit: nếu payment đã thành công trước đó → throw `"payment_already_handled"`.

### 4.15. ConfigSetting (Cấu hình hệ thống)

**Mục đích**: Cho phép Admin chỉnh các tham số nghiệp vụ mà không cần deploy code.

**Các config quan trọng**:
| Key | Ý nghĩa |
|---|---|
| `free_project` | Số dự án miễn phí tối đa mỗi user được tạo |
| `project_price` | Giá tiền (credit) cho mỗi dự án vượt mức miễn phí |

**Cache**: Mọi config setting được cache trong Redis. Khi update, cache bị xoá theo pattern.

### 4.16. Media (Tệp Đính Kèm)

**Mục đích**: Lưu metadata của các file được upload lên AWS S3 (avatar, hình ảnh trong issue, đính kèm trong comment...).

**Cơ chế upload**:
- **Direct upload**: `POST /api/upload-file` (multipart/form-data) — backend nhận file rồi upload lên S3.
- **Presigned URL**: `POST /api/presigned-upload-url` — backend trả về URL có chữ ký (S3 presigned URL), frontend dùng URL này để upload trực tiếp lên S3 (giảm tải cho backend).

**Polymorphic association**: `MediaEntity.EntityType` cho biết file thuộc loại nào (`issue`, `comment`...), `EntityId` là ID của thực thể đó.

---

## 5. Luồng Nghiệp Vụ Chi Tiết

### 5.1. Luồng Đăng Ký & Xác Minh Tài Khoản

```
┌──────┐                          ┌────────┐                ┌──────┐
│ User │                          │JiPLC BE│                │ Mail │
└──┬───┘                          └────┬───┘                └──┬───┘
   │                                   │                       │
   │ POST /Register                    │                       │
   │ (email, pwd, name, phone, ...)    │                       │
   ├──────────────────────────────────▶│                       │
   │                                   │ Check email/phone/CMND│
   │                                   │ unique?               │
   │                                   │                       │
   │                                   │ Hash password (BCrypt)│
   │                                   │ Sinh securityCode    │
   │                                   │ Insert UserAccount    │
   │                                   │ Insert UserProfile    │
   │                                   │ Publish mail event ──▶│
   │ 200 OK { id, email }              │                       │
   │◀──────────────────────────────────│                       │
   │                                   │                       │ Send confirm
   │                                   │                       │ email với link
   │                                   │◀──────────────────────│ /confirm-email
   │                                   │                       │ ?userId&code
   │                                   │                       │
   │ Click link trong email                                    │
   │ → PUT /Confirm-Email { userId, code }                     │
   ├──────────────────────────────────▶│                       │
   │                                   │ Compare securityCode  │
   │                                   │ Set IsVerified = true │
   │ 200 OK                            │ Clear securityCode    │
   │◀──────────────────────────────────│                       │
```

**Quy tắc check unique khi đăng ký**:
- `Email` phải duy nhất trong `user_account`.
- `PhoneNumber` **HOẶC** `IdentityNumber` phải duy nhất trong `user_profile`.

### 5.2. Luồng Đăng Nhập & Refresh Token

```
[Login] (email, password)
   │
   ├─ Check user exists?           → not_found
   ├─ Check IsVerified?            → account_not_verified
   ├─ Check IsActived?             → account_inactived
   ├─ BCrypt verify password       → invalid_password
   │
   ├─ Tạo access token (15 phút)
   ├─ Tạo refresh token (30 ngày), lưu DB
   └─ Trả { id, email, role, tokens, expiry }

[Refresh Token] (accessToken cũ, refreshToken)
   │
   ├─ Decode access token để lấy userId
   ├─ So sánh refreshToken DB với input
   ├─ Check refreshToken còn hạn?
   ├─ Tạo cặp token mới (giữ nguyên claims)
   └─ Trả { newAccessToken, newRefreshToken, expiry }

[Revoke Refresh Token] (yêu cầu Authorize)
   └─ Set RefreshToken = null, RefreshTokenExpiredAt = null
      → Mọi thiết bị khác cũng bị logout
```

### 5.3. Luồng Quên & Khôi Phục Mật Khẩu

```
[Forgot Password] (identityInformation: email | phone | CMND)
   │
   ├─ Tìm user khớp email/phone/CMND
   ├─ Check IsVerified?
   ├─ Sinh securityCode mới, lưu DB
   └─ Gửi mail recover-password với link
      → /recover-password?userId&code

[Recover Password] (userId, code, newPassword)
   │
   ├─ Tìm user
   ├─ Check IsVerified?
   ├─ So sánh securityCode
   ├─ Hash new password
   ├─ Clear securityCode
   └─ Save
```

### 5.4. Luồng Tạo Dự Án (có Logic Tính Phí)

```
[Create Project] (name, key, image)
   │
   ├─ BEGIN TRANSACTION
   │
   ├─ Lấy config free_project (vd: 3)
   ├─ Đếm số project user đã tạo (CountByCreatorId)
   │
   ├─ IF projectCount >= free_project:
   │    ├─ Lấy config project_price (vd: 100,000)
   │    ├─ Trừ vào UserProfile.CurrentCredit
   │    │   IF không đủ credit → throw "not_enough_credit"
   │    └─ Continue
   │
   ├─ Insert Project (CreatorId = LeaderId = currentUserId)
   ├─ Insert ProjectMember (cho creator)
   ├─ Insert ProjectStatus mặc định ("To Do", Index 0)
   │
   └─ COMMIT TRANSACTION
```

### 5.5. Luồng Mời Thành Viên

```
[Sender] POST /api/project/{projectId}/invitation { recipientEmail }
   │
   ├─ Tìm UserAccount theo email
   │   → recipient_not_found nếu không có
   │
   ├─ Check sender != recipient → invalid_invitation
   │
   └─ Insert Invitation (sender, recipient, project, AcceptedAt=null, DeclinedAt=null)


[Recipient] GET /api/user/personal/invitation → xem danh sách invitation đến mình
   │
   ├─ Chọn 1 invitation, có 2 lựa chọn:
   │
   ├─ Accept: PUT /api/user/personal/invitation/{id}/accept
   │   ├─ Check recipientId == reqUser.Id
   │   ├─ Check chưa accepted/declined
   │   ├─ Set AcceptedAt = NOW
   │   └─ Insert ProjectMember
   │
   └─ Decline: PUT /api/user/personal/invitation/{id}/decline
       ├─ Check ...
       └─ Set DeclinedAt = NOW (không thêm member)
```

### 5.6. Luồng Sprint & Backlog

```
[Initial state]
   Backlog: [ Issue-A, Issue-B, Issue-C ]    (BacklogIndex 1, 2, 3; SprintId = null)
   Sprint: chưa có

[1. Tạo Sprint]
   POST /api/project/{pid}/sprint { title, goal, fromDate, toDate }
   → Sprint mới với StartedAt = null, CompletedAt = null

[2. Move issue từ Backlog → Sprint]
   PUT /api/project/{pid}/backlog/issue/move-to-sprint { issueIds }
   → MoveIssueToSprint: SprintId = availableSprint.Id, giữ BacklogIndex
   (Vẫn coi là "trong sprint" do logic Board filter SprintId == sprintId)

[3. Start Sprint]
   PUT /api/project/{pid}/sprint/{sid}/start
   → StartedAt = NOW

[4. Làm việc trên Board]
   PUT /api/project/{pid}/board/issue/{iid}
   → Cập nhật ProjectStatusId (di chuyển cột), AssigneeId, v.v.

[5. Move issue về Backlog (nếu cần hoãn)]
   PUT /api/project/{pid}/board/issue/move-to-backlog
   → SprintId = null, BacklogIndex được tính lại

[6. Complete Sprint]
   PUT /api/project/{pid}/sprint/{sid}/complete
   { CompletedIssues: [...], UnCompletedIssues: [...], MoveType: "backlog" | "next_sprint" }

   ├─ Set Sprint.CompletedAt = NOW
   ├─ Với CompletedIssues: SprintId = null, BacklogIndex = null (coi như "done & out")
   └─ Với UnCompletedIssues:
       - MoveType = "backlog": Move về Backlog
       - MoveType = "next_sprint": Tạo Sprint mới "next_sprint" và move sang đó
```

### 5.7. Luồng Thanh Toán Mua Credit

```
┌──────┐               ┌────────┐                  ┌──────┐
│ User │               │JiPLC BE│                  │ VNPay│
└──┬───┘               └────┬───┘                  └──┬───┘
   │                        │                         │
   │ POST /Payment {amount} │                         │
   ├───────────────────────▶│                         │
   │                        │ Tạo PaymentEntity       │
   │                        │ (txnRef = NOW.Ticks)    │
   │                        │ Tạo URL VNPay với       │
   │                        │ Hash secure             │
   │ 200 { paymentUrl }     │                         │
   │◀───────────────────────│                         │
   │                                                  │
   │ Redirect đến paymentUrl ────────────────────────▶│
   │ (User thanh toán bằng ATM/QR/...)                │
   │                                                  │
   │◀─────────────── VNPay redirect về ReturnUrl ─────│
   │   (https://plc-base-react.vercel.app/payment/    │
   │    callback?vnp_TxnRef&vnp_Amount&vnp_ResponseCode│
   │    &vnp_TransactionStatus&...)                   │
   │                                                  │
   │ Frontend parse query params                      │
   │ PUT /Payment { vnp_TxnRef, vnp_OrderInfo, ...}   │
   ├───────────────────────▶│                         │
   │                        │ Verify response codes   │
   │                        │ Lookup PaymentEntity    │
   │                        │ theo txnRef             │
   │                        │ Check đã handled?       │
   │                        │ Update payment status   │
   │                        │ UserProfile.CurrentCredit│
   │                        │   += vnp_Amount / 100   │
   │ 200 OK                 │                         │
   │◀───────────────────────│                         │
```

### 5.8. Luồng Phân Quyền Truy Cập Dự Án

```
[Request: GET/POST/PUT/DELETE bất kỳ API trong /api/project/{pid}/...]
   │
   ├─ Middleware Authentication (JWT)
   │  → Lấy reqUser từ token
   │
   ├─ (Trong service handler) Kiểm tra reqUser có trong ProjectMember của projectId?
   │  → BAD_REQUEST "unreachable_project" nếu không
   │
   └─ Check permission (qua attribute hoặc explicit check):
      │
      ├─ IF reqUser.Id == project.LeaderId:
      │    → BYPASS, có toàn quyền
      │
      └─ ELSE:
         ├─ Lấy danh sách ProjectRoleId của member trong project (qua MemberRole)
         ├─ Lấy danh sách permission key của các role (có cache Redis)
         └─ Check action's required permission có nằm trong list không
            → 403 Forbidden nếu không
```

---

## 6. Mô Hình Doanh Thu (Monetization)

JiPLC áp dụng mô hình **Freemium với Credit (số dư)**.

### 6.1. Cơ Chế

| Bước | Nghiệp vụ |
|---|---|
| 1 | User đăng ký miễn phí, có thể tạo **N dự án miễn phí** (N = config `free_project`, vd: 3) |
| 2 | Khi tạo dự án thứ N+1: hệ thống trừ vào `UserProfile.CurrentCredit` một khoản = `project_price` |
| 3 | Nếu không đủ credit: user phải mua thêm qua VNPay |
| 4 | Sau khi VNPay xác nhận thanh toán: credit được cộng vào tài khoản |

### 6.2. Các Tham Số Có Thể Tuỳ Chỉnh (Admin chỉnh runtime)

| Config Key | Loại | Ví dụ giá trị | Ý nghĩa |
|---|---|---|---|
| `free_project` | `double` | 3 | Số dự án miễn phí mỗi user |
| `project_price` | `double` | 100000 | Giá credit cho 1 dự án vượt mức |

> Lưu ý: dù khoản tiền (VND) là số nguyên trong thực tế, code dùng `double` cho linh hoạt (vd: cho phép giá 99.999 VND).

### 6.3. Không Có Cơ Chế Hoàn Tiền Trong Code

- Khi xoá dự án (soft delete) → KHÔNG hoàn lại credit.
- Khi user bị Admin khoá → KHÔNG hoàn credit còn lại.
- Đây là điểm cần lưu ý nếu mở rộng quy mô khách hàng.

---

## 7. Quy Tắc Nghiệp Vụ Quan Trọng

### 7.1. Soft Delete

Các bảng sau hỗ trợ soft delete (cờ `DeletedAt`, không xoá vật lý):

| Entity | Lý do |
|---|---|
| `Project` | Có thể có nhu cầu khôi phục dự án bị xoá nhầm |
| `ProjectMember` | Để track lịch sử ai đã từng tham gia dự án |
| `ProjectStatus` | Để dữ liệu lịch sử của issue vẫn truy vết được |
| `Issue` | Để không mất dữ liệu báo cáo/audit |

Các bảng khác (`Sprint`, `Event`, `Invitation`, `IssueComment`, `EventAttendee`, `Media`, `Payment`) **xoá vật lý**.

### 7.2. Soft Delete Liên Đới (Cascade)

- Khi soft delete `Project`: tất cả `ProjectMember` của project đó cũng bị soft delete.

### 7.3. Ràng Buộc Tính Duy Nhất

| Bảng | Duy nhất theo |
|---|---|
| `user_account.email` | Toàn hệ thống |
| `user_profile.phone_number` | Toàn hệ thống |
| `user_profile.identity_number` | Toàn hệ thống |
| `project_status` | Mỗi dự án phải có ≥ 1 status |

### 7.4. Logic Tính Index Cho Issue

Khi tạo issue mới:
- `BacklogIndex = floor(maxBacklogIndex + step)` — đẩy xuống cuối Backlog
- `ProjectStatusIndex = floor(maxStatusIndex + step)` — đẩy xuống cuối cột status

Dùng `double` để cho phép kéo thả chèn giữa hai issue mà không cần re-index toàn bộ (lấy giá trị trung bình của hai issue lân cận).

### 7.5. Quy Tắc Liên Quan Đến Email

| Hành động | Email gửi đi |
|---|---|
| Đăng ký | Email xác minh tài khoản |
| Forgot password | Email khôi phục mật khẩu |
| Mời thành viên | Hiện **CHƯA** gửi email — chỉ tạo invitation trong DB (frontend hiển thị trong tab Notification của recipient) |

Email được publish lên **CAP Message Queue** (in-memory) và xử lý bởi `WorkerController.SendMail` — đảm bảo API response không bị block bởi việc gửi mail.

### 7.6. Logging & Audit

- Mọi entity kế thừa `BaseEntity` có `CreatedAt`, `UpdatedAt` tự động cập nhật trong `SaveChangesAsync`.
- Có cấu hình `Serilog` để log ra console (đã tắt log cho ASP.NET, EF Core, Diagnostics middleware để giảm noise).

### 7.7. Timezone

Toàn hệ thống dùng timezone **SE Asia Standard Time** (UTC+7, giờ Việt Nam). Cấu hình ở `appsettings.json` → `DateTimeSettings.TimeZone`.

---

## 8. Trạng Thái Vòng Đời Của Các Thực Thể

### 8.1. UserAccount

```
[Register] ──▶ IsVerified=false, IsActived=true  ──ConfirmEmail──▶ IsVerified=true
                                                                          │
                                              Admin disable: IsActived=false
                                                                          │
                                              Admin enable:  IsActived=true
```

### 8.2. Project

```
[Create] ──▶ Active ──Update Leader/Info──▶ Active
                  │
                  └─Soft Delete──▶ DeletedAt != null  (mọi ProjectMember cũng soft deleted)
```

### 8.3. Sprint

```
[Create] ──▶ Pending (StartedAt=null, CompletedAt=null)
              │
              ├─Start──▶ InProgress (StartedAt!=null)
              │            │
              │            └─Complete──▶ Completed (CompletedAt!=null)
              │
              └─Delete──▶ Removed (issues move về Backlog)
```

### 8.4. Issue (theo vị trí lưu trữ)

```
[Create Issue] (mặc định ở Backlog, status đầu tiên của project)
   │
   ├─Move to Sprint──▶  Trong Sprint (SprintId!=null, BacklogIndex giữ nguyên)
   │                       │
   │                       ├─Update Status on Board (đổi ProjectStatusId)
   │                       │
   │                       └─Move back to Backlog──▶ Backlog (SprintId=null)
   │
   └─Soft Delete──▶ Đã xoá (DeletedAt!=null)


[Sprint Complete]
   ├─ Completed Issues: SprintId=null, BacklogIndex=null (rời cả hai, coi như done)
   └─ Uncompleted: hoặc về Backlog hoặc về Sprint tiếp theo
```

### 8.5. Invitation

```
[Sender Create] ──▶ Pending (AcceptedAt=null, DeclinedAt=null)
                       │
                       ├─Recipient Accept──▶ Accepted (AcceptedAt!=null)
                       │                        └─ Insert ProjectMember
                       │
                       ├─Recipient Decline──▶ Declined (DeclinedAt!=null)
                       │
                       └─Sender Delete──▶ Removed (xoá vật lý)
```

### 8.6. Payment

```
[Create Payment] ──▶ Pending (vnp_TransactionStatus null)
                        │
                        ├─VNPay return success──▶ Submit ──▶ Success ("00")
                        │                                     └─ Cộng credit
                        │
                        └─VNPay return fail──▶ Submit ──▶ throw "payment_fail"
                                                          (KHÔNG cộng credit)
```

---

## 9. Thuật Ngữ Nghiệp Vụ (Glossary)

| Thuật ngữ | Giải thích |
|---|---|
| **JiPLC** | Tên sản phẩm. "Ji" = Jira, "PLC" là tên nội bộ của team phát triển. |
| **Issue** | Đơn vị công việc trong dự án. Có 3 loại: User Story, Coding Task, Bug. |
| **Backlog** | Kho lưu trữ các issue chưa được lên Sprint, chờ lập kế hoạch. |
| **Sprint** | Vòng lặp phát triển (1-4 tuần), chứa các issue cần hoàn thành trong khoảng thời gian đó. |
| **Available Sprint** | Sprint chưa hoàn thành của một dự án. Mỗi project tại một thời điểm chỉ có **một** available sprint. |
| **Board** | Bảng Kanban hiển thị các issue trong Sprint theo cột trạng thái (To Do, In Progress, Done...). |
| **Story Point** | Điểm ước lượng độ phức tạp/khối lượng công việc của một issue (thường dùng Fibonacci: 1, 2, 3, 5, 8...). |
| **Reporter** | Người tạo/báo cáo issue. |
| **Assignee** | Người được giao xử lý issue. |
| **ProjectStatus** | Cột trên Board (To Do, In Progress, Done...) — tùy biến theo từng dự án. |
| **Leader** | Trưởng dự án — có toàn quyền trong dự án. |
| **Creator** | Người tạo dự án — không thể thay đổi sau khi tạo. |
| **ProjectRole** | Vai trò dự án (Developer, Tester...) do Admin định nghĩa, gán cho member. |
| **MemberRole** | Bảng nối giữa thành viên dự án và vai trò dự án. |
| **Permission Key** | Chuỗi định danh quyền theo dạng `Module.Action`, vd: `Issue.Create`, `Sprint.Delete`. |
| **Credit** | Số dư của user (tính bằng VND), dùng để thanh toán việc tạo dự án vượt mức miễn phí. |
| **Security Code** | Mã ngẫu nhiên dùng cho xác minh email & khôi phục mật khẩu. |
| **VNPay** | Cổng thanh toán Việt Nam được tích hợp để mua credit. |
| **CAP** | Thư viện EventBus dùng cho gửi mail bất đồng bộ (in-memory queue). |
| **Soft Delete** | Đánh dấu xoá bằng cờ `DeletedAt` thay vì xoá vật lý — vẫn giữ data cho audit. |
| **TxnRef** | Mã giao dịch duy nhất trong giao tiếp với VNPay (sinh từ `DateTime.Ticks`). |

---

## Tài Liệu Liên Quan

- `README.md` — Hướng dẫn build, deploy, dev setup
- `README-Technical.md` — Tài liệu kỹ thuật chi tiết (kiến trúc code, patterns, DI, cấu trúc thư mục...)
- `README-API.md` — Tài liệu API endpoints (OpenAPI/Swagger)

> **Phiên bản tài liệu**: 1.0 — Tài liệu này dựa trên source code phiên bản hiện tại của repo. Khi nghiệp vụ có thay đổi, vui lòng cập nhật lại.
