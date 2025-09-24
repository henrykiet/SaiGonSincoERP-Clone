# 🔑 Primary Key Update Feature - Hướng dẫn sử dụng

## 📋 Tổng quan

Tính năng này tự động detect khi user thay đổi primary key và sử dụng action phù hợp khi save dữ liệu:

- **Insert**: Tạo mới record → `action: "insert"`
- **Update**: Cập nhật record không thay đổi primary key → `action: "update"`  
- **Update Primary Key**: Cập nhật record có thay đổi primary key → `action: "update_primary_key"`

## 🔧 Cách hoạt động

### 1. **Khi load form edit**
```typescript
// Tự động lưu original primary key values
originalPrimaryKeyValues = {
  "customerCode": "CUST-001",
  "branchCode": "HN"
}
```

### 2. **Khi user thay đổi dữ liệu**
```typescript
// Method hasPrimaryKeyChanged() sẽ so sánh:
Original: { customerCode: "CUST-001", branchCode: "HN" }
Current:  { customerCode: "CUST-NEW-001", branchCode: "HN" }
// → Primary key changed: true
```

### 3. **Khi save**
```typescript
// Payload sẽ có action tương ứng:
{
  "action": "update_primary_key",  // Thay vì "update"
  "formId": "customer",
  "primaryKey": ["customerCode", "branchCode"],
  "data": {
    "customerCode": "CUST-NEW-001",  // New value
    "branchCode": "HN",
    "customerName": "Updated Customer"
  }
}
```

## 📝 Console Logs

Khi debug, bạn sẽ thấy các log sau:

```
🆕 Using insert action for new record
✅ Using standard update action (no primary key changes)
🔄 Using update_primary_key action due to primary key changes
📋 Original PK values: { customerCode: "CUST-001" }
📋 Current PK values: { customerCode: "CUST-NEW-001" }
```

## ⚠️ User Experience

### Confirmation Dialog
Khi user thay đổi primary key, sẽ hiện dialog xác nhận:

```
"Bạn đang thay đổi khóa chính của bản ghi. 
Điều này có thể ảnh hưởng đến dữ liệu liên quan. 
Bạn có chắc chắn muốn tiếp tục?"
```

### Primary Key Change Detection
```typescript
// Primary key field thay đổi từ:
"Primary key field 'customerCode' changed from 'CUST-001' to 'CUST-NEW-001'"
```

## 🧪 Testing Scenarios

### Test Case 1: Normal Update
1. Mở form edit record có customerCode = "CUST-001"
2. Thay đổi customerName = "New Name" 
3. Save → Action = "update" ✅

### Test Case 2: Primary Key Update  
1. Mở form edit record có customerCode = "CUST-001"
2. Thay đổi customerCode = "CUST-NEW-001"
3. Save → Hiện confirm dialog → Action = "update_primary_key" ✅

### Test Case 3: New Record
1. Mở form thêm mới
2. Nhập dữ liệu
3. Save → Action = "insert" ✅

### Test Case 4: Multi Primary Key
1. Form có primaryKey = ["customerCode", "branchCode"]
2. Thay đổi chỉ 1 trong 2 field → Action = "update_primary_key" ✅
3. Không thay đổi field nào → Action = "update" ✅

## 🔍 Debugging

### Check originalPrimaryKeyValues
```typescript
console.log('Original PK:', this.originalPrimaryKeyValues)
// Output: { customerCode: "CUST-001", branchCode: "HN" }
```

### Check current values
```typescript
const current = this.mergeFormData()
console.log('Current PK:', this.metadata?.primaryKey?.map(pk => current[pk]))
// Output: ["CUST-NEW-001", "HN"]
```

### Test hasPrimaryKeyChanged()
```typescript
console.log('PK Changed:', this.hasPrimaryKeyChanged())
// Output: true/false
```

## ✅ Integration Status

- ✅ **DynamicPopupComponent**: Implemented
- ✅ **Backend DynamicRepository**: Supports update_primary_key action
- ✅ **Job Module**: Uses DynamicPopupComponent (auto inherited)
- ✅ **All dynamic forms**: Auto support via DynamicPopupComponent

## 🚀 Rollout

Tính năng này tương thích ngược và không ảnh hưởng đến code cũ:
- Forms cũ vẫn hoạt động bình thường
- Chỉ khi thay đổi primary key mới sử dụng action mới
- Backend hỗ trợ cả action cũ và mới 