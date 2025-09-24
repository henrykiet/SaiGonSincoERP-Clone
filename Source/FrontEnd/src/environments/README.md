# Sử dụng biến môi trường mặc định trong Angular

Angular cung cấp cơ chế sẵn có để cấu hình các biến môi trường cho từng môi trường như `development`, `production`, `staging`,... thông qua thư mục `environments`.

---

## 📁 Cấu trúc thư mục

```
FrontEnd/
└── src/
    └── environments/
        └── environment.ts         (mặc định: development)
```

---

## 🛠 1. Cấu hình các biến môi trường

### `Source/FrontEnd/src/environments/environment.ts`

```ts
export const environment = {
  production: false,
  apiUrl: "http://localhost:3000",
};
```

Có thể thêm các biến khác như:

```ts
export const environment = {
  googleApiKey: "your-google-api-key",
};
```

---

## 📦 2. Sử dụng trong ứng dụng Angular

Có thể import `environment` ở bất kỳ đâu trong project:

```ts
import { environment } from "src/environments/environment";

this.http.get(`${environment.apiUrl}/users`);
```

---

## ⚙️ 3. Angular tự động thay thế file khi build

Khi build với cấu hình production, Angular sẽ tự động thay `environment.ts` bằng `environment.prod.ts` nếu cấu hình này được định nghĩa trong `angular.json`:

```json
"configurations": {
  "production": {
    "fileReplacements": [
      {
        "replace": "src/environments/environment.ts",
        "with": "src/environments/environment.prod.ts"
      }
    ],
  }
}
```

```bash
ng build --configuration production
```

---