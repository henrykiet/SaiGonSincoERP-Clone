import { TestBed } from '@angular/core/testing';
import { HttpClientModule } from '@angular/common/http';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should have login method', () => {
    expect(service.login).toBeTruthy();
  });

  it('should have logout method', () => {
    expect(service.logout).toBeTruthy();
  });

  it('should have isLoggedIn method', () => {
    expect(service.isLoggedIn).toBeTruthy();
  });
});
