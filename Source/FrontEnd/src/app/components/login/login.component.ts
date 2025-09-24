import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NotificationComponent } from '../notification/notification.component';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LayoutService } from '../../services/layout.service';
import { UnitService } from '../../services/unit.service';
import { NotificationService } from '../../services/notification.service';
import { UserLoginDto } from '../../models/user.model';
import { Unit } from '../../models/unit.model';
import { environment } from '../../../environments/environment';
import { TranslateService } from '@ngx-translate/core'

interface LanguageConfig {
  loginTitle: string;
  username: string;
  password: string;
  unit: string;
  loginButton: string;
  loggingIn: string;
  fillAllFields: string;
  incorrectCredentials: string;
  errorOccurred: string;
  selectUnit: string;
  loadingUnits: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, NotificationComponent],
  template: `
    <div class="login-container">
      <!-- Language Selector -->
      <div class="language-selector">
        <button 
          class="lang-btn" 
          [class.active]="currentLanguage === 'vi'"
          (click)="changeLanguage('vi')"
        >
          VI
        </button>
        <button 
          class="lang-btn" 
          [class.active]="currentLanguage === 'en'"
          (click)="changeLanguage('en')"
        >
          EN
        </button>
      </div>

      <div class="login-box">
        <h2>{{ translations.loginTitle }}</h2>
        <form (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="username">{{ translations.username }}</label>
            <input
              type="text"
              id="username"
              name="username"
              [(ngModel)]="username"
              required
            />
          </div>
          <div class="form-group">
            <label for="password">{{ translations.password }}</label>
            <input
              type="password"
              id="password"
              name="password"
              [(ngModel)]="password"
              required
            />
          </div>
          <div class="form-group">
            <label for="unit">{{ translations.unit }}</label>
            <select
              id="unit"
              name="unit"
              [(ngModel)]="selectedUnitCode"
              required
              [disabled]="isLoadingUnits"
            >
              <option value="">{{ isLoadingUnits ? translations.loadingUnits : translations.selectUnit }}</option>
              <option 
                *ngFor="let unit of units" 
                [value]="unit.unitCode"
              >
                {{ currentLanguage === 'vi' ? unit.unitName : unit.unitName2 }}
              </option>
            </select>
          </div>
          <button type="submit" [disabled]="isLoading || isLoadingUnits">
            {{ isLoading ? translations.loggingIn : translations.loginButton }}
          </button>
          <div *ngIf="error" class="error-message">
            {{ error }}
          </div>
        </form>
      </div>
    </div>
    
    <!-- Notification Component -->
    <app-notification></app-notification>
  `,
  styles: [`
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 100vh;
      background-color: #f8f9fa;
      position: relative;
    }

    .language-selector {
      position: absolute;
      top: 20px;
      right: 20px;
      display: flex;
      gap: 5px;
      z-index: 10;
    }

    .lang-btn {
      padding: 8px 12px;
      background-color: transparent;
      border: 1px solid #ddd;
      border-radius: 4px;
      cursor: pointer;
      font-size: 12px;
      font-weight: 500;
      color: #666;
      transition: all 0.2s;

      &:hover {
        background-color: #f0f0f0;
        border-color: #007bff;
      }

      &.active {
        background-color: #007bff;
        color: white;
        border-color: #007bff;
      }
    }

    .login-box {
      background: white;
      padding: 2rem;
      border-radius: 8px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      width: 100%;
      max-width: 400px;
    }

    h2 {
      text-align: center;
      margin-bottom: 2rem;
      color: #333;
    }

    .form-group {
      margin-bottom: 1.5rem;
    }

    label {
      display: block;
      margin-bottom: 0.5rem;
      color: #555;
    }

    input, select {
      width: 100%;
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 4px;
      font-size: 1rem;

      &:focus {
        outline: none;
        border-color: #007bff;
        box-shadow: 0 0 0 2px rgba(0, 123, 255, 0.25);
      }

      &:disabled {
        background-color: #f8f9fa;
        cursor: not-allowed;
      }
    }

    select {
      background-color: white;
      cursor: pointer;
    }

    button {
      width: 100%;
      padding: 0.75rem;
      background-color: #007bff;
      color: white;
      border: none;
      border-radius: 4px;
      font-size: 1rem;
      cursor: pointer;
      transition: background-color 0.2s;

      &:hover {
        background-color: #0056b3;
      }

      &:disabled {
        background-color: #ccc;
        cursor: not-allowed;
      }
    }

    .error-message {
      color: #dc3545;
      margin-top: 1rem;
      text-align: center;
    }
  `]
})
export class LoginComponent implements OnInit, OnDestroy {
  username: string = '';
  password: string = '';
  selectedUnitCode: string = '';
  isLoading: boolean = false;
  isLoadingUnits: boolean = false;
  error: string = '';
  currentLanguage: string = environment.defaultLanguage;
  units: Unit[] = [];

  private languageTexts: { [key: string]: LanguageConfig } = {
    vi: {
      loginTitle: 'Đăng nhập',
      username: 'Tên đăng nhập',
      password: 'Mật khẩu',
      unit: 'Đơn vị cơ sở',
      loginButton: 'Đăng nhập',
      loggingIn: 'Đang đăng nhập...',
      fillAllFields: 'Vui lòng nhập đầy đủ thông tin đăng nhập',
      incorrectCredentials: 'Tên đăng nhập hoặc mật khẩu không chính xác.',
      errorOccurred: 'Có lỗi xảy ra. Vui lòng thử lại sau.',
      selectUnit: 'Chọn đơn vị',
      loadingUnits: 'Đang tải danh sách đơn vị...'
    },
    en: {
      loginTitle: 'Login',
      username: 'Username',
      password: 'Password',
      unit: 'Unit',
      loginButton: 'Login',
      loggingIn: 'Logging in...',
      fillAllFields: 'Please fill in all login information',
      incorrectCredentials: 'Username or password is incorrect.',
      errorOccurred: 'An error occurred. Please try again later.',
      selectUnit: 'Select unit',
      loadingUnits: 'Loading units...'
    }
  };

  constructor(
    private authService: AuthService,
    private router: Router,
    private layoutService: LayoutService,
    private unitService: UnitService,
    private notificationService: NotificationService,
    public translate: TranslateService,
  ) {
    this.translate.setDefaultLang(localStorage.getItem("language") ?? "vi")
  }

  ngOnInit() {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }
    this.layoutService.hideLayout();

    // Load saved language from localStorage or use environment default
    const savedLanguage = localStorage.getItem('language');
    if (savedLanguage && environment.supportedLanguages.includes(savedLanguage)) {
      this.currentLanguage = savedLanguage;
      environment.currentLanguage = savedLanguage; // Update environment variable
    } else {
      this.currentLanguage = environment.currentLanguage;
    }

    // Load units
    this.loadUnits();
  }

  ngOnDestroy() {
    this.layoutService.showLayout();
  }

  get translations(): LanguageConfig {
    return this.languageTexts[this.currentLanguage];
  }

  changeLanguage(language: string) {
    if (environment.supportedLanguages.includes(language)) {
      this.currentLanguage = language;
      environment.currentLanguage = language; // Update environment variable
      localStorage.setItem('language', language);
      this.translate.setDefaultLang(language)
      this.error = ''; // Clear error message when language changes
    }
  }

  loadUnits() {
    this.isLoadingUnits = true;
    this.unitService.getUnits().subscribe({
      next: (response) => {
        if (response.success) {
          this.units = response.data;
        } else {
          console.error('Failed to load units:', response.message);
        }
        this.isLoadingUnits = false;
      },
      error: (err) => {
        console.error('Error loading units:', err);
        this.isLoadingUnits = false;
      }
    });
  }

  async onSubmit() {
    if (!this.username || !this.password || !this.selectedUnitCode) {
      this.error = this.translations.fillAllFields;
      return;
    }

    this.isLoading = true;
    this.error = '';

    try {
      const credentials: UserLoginDto = {
        userName: this.username,
        password: this.password,
        unit: this.selectedUnitCode
      };

      this.authService.login(credentials).subscribe({
        next: (response) => {
          if (response.success) {
            if (response.data) {
              localStorage.setItem('userId', response.data.userId.toString());
              localStorage.setItem('userName', response.data.userName);
              localStorage.setItem('token', response.data.token);
              if (response.data.sessionId) {
                localStorage.setItem('sessionId', response.data.sessionId);
              }
            }

            // Hiển thị cảnh báo nếu có session khác đang hoạt động
            if (response.hasExistingSession) {
              this.notificationService.warning(
                'Cảnh báo: Tài khoản này đã được đăng nhập ở trình duyệt khác và đã được đăng xuất.',
                8000
              );
            }

            this.router.navigate(['/dashboard']);
          } else {
            this.error = response.message || this.translations.incorrectCredentials;
            this.isLoading = false;
          }
        },
        error: (err) => {
          this.error = this.translations.incorrectCredentials;
          this.isLoading = false;
        }
      });
    } catch (err) {
      this.error = this.translations.errorOccurred;
      this.isLoading = false;
    }
  }
}