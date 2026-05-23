# FE Handoff: Cap nhat response report attempt

Base path: `/report/attempt`

Cac API duoc bo sung field ten, lop, truong:

- `GET /report/attempt/groups`
- `GET /report/attempt/groups/:groupId/students`
- `GET /report/attempt/student`

## 1. `GET /report/attempt/groups`

Moi item tra them:

```json
{
  "id": "5233abe3-1961-4af5-a482-542f1227d844",
  "name": "THNVB - Lop 5A",
  "className": "Lop 5A",
  "schoolId": "a233abe3-1961-4af5-a482-542f1227d844",
  "schoolName": "TH Nguyen Van Bua",
  "shoolName": "TH Nguyen Van Bua",
  "schoolCode": "THNVB",
  "type": "CLASS"
}
```

Luu y:

- `name` van giu format cu de khong gay vo FE hien tai.
- FE co the dung truc tiep `className`, `shoolName`, `schoolName`, `schoolCode` neu can hien thi rieng lop/truong.
- `shoolName` la alias cua `schoolName` theo contract FE hien tai.

## 2. `GET /report/attempt/groups/:groupId/students`

Moi item hoc sinh tra them:

```json
{
  "id": "6233abe3-1961-4af5-a482-542f1227d844",
  "fullName": "Nguyen Van B",
  "studentName": "Nguyen Van B",
  "userName": "student_b",
  "code": "HS001",
  "studentGroupId": "7233abe3-1961-4af5-a482-542f1227d844",
  "studentGroupName": "Lop 5A",
  "classId": "7233abe3-1961-4af5-a482-542f1227d844",
  "className": "Lop 5A",
  "schoolId": "a233abe3-1961-4af5-a482-542f1227d844",
  "schoolName": "TH Nguyen Van Bua",
  "shoolName": "TH Nguyen Van Bua",
  "schoolCode": "THNVB"
}
```

Luu y:

- `classId` la alias cua `studentGroupId`.
- `className` la alias cua `studentGroupName`.
- `studentName` uu tien `fullName`, neu khong co thi dung `userName`.
- `shoolName` la alias cua `schoolName`.

## 3. `GET /report/attempt/student`

Response top-level tra them thong tin lop/truong dang loc:

```json
{
  "groupId": "7233abe3-1961-4af5-a482-542f1227d844",
  "groupName": "Lop 5A",
  "className": "Lop 5A",
  "schoolId": "a233abe3-1961-4af5-a482-542f1227d844",
  "schoolName": "TH Nguyen Van Bua",
  "shoolName": "TH Nguyen Van Bua",
  "schoolCode": "THNVB"
}
```

Field `student` tra cung format voi API danh sach hoc sinh o muc 2.

Moi item trong `attempts[]` tra them:

```json
{
  "studentId": "6233abe3-1961-4af5-a482-542f1227d844",
  "studentName": "Nguyen Van B",
  "studentCode": "HS001",
  "classId": "7233abe3-1961-4af5-a482-542f1227d844",
  "className": "Lop 5A",
  "schoolId": "a233abe3-1961-4af5-a482-542f1227d844",
  "schoolName": "TH Nguyen Van Bua",
  "shoolName": "TH Nguyen Van Bua",
  "schoolCode": "THNVB"
}
```

Luu y:

- Cac field lop/truong co the la `null` neu hoc sinh chua duoc gan lop/truong.
- Khi FE goi `studentId=all`, nen lay ten hoc sinh/lop/truong tu tung item trong `attempts[]`.
- FE can 3 field chinh thi doc `className`, `shoolName`, `studentName`.