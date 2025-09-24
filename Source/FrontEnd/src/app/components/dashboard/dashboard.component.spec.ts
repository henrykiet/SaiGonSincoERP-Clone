import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DashboardComponent } from './dashboard.component';
import { StatisticsCardComponent } from './statistics-card/statistics-card.component';
import { ChartCardComponent } from './chart-card/chart-card.component';
import { CommonModule } from '@angular/common';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { AuthService } from '../../services/auth.service';

describe('DashboardComponent', () => {
  let component: DashboardComponent;
  let fixture: ComponentFixture<DashboardComponent>;
  let httpMock: HttpTestingController;
  let authService: AuthService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        HttpClientTestingModule,
        RouterTestingModule,
        DashboardComponent,
        StatisticsCardComponent,
        ChartCardComponent
      ],
      providers: [AuthService]
    }).compileComponents();

    fixture = TestBed.createComponent(DashboardComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have today date initialized', () => {
    expect(component.today).toBeTruthy();
    expect(component.today instanceof Date).toBeTruthy();
  });

  it('should have statistics data with correct structure', () => {
    expect(component.statistics).toBeTruthy();
    expect(component.statistics.length).toBe(4);
    
    const firstStat = component.statistics[0];
    expect(firstStat.title).toBe('Tổng doanh thu');
    expect(firstStat.value).toBe('$24,780');
    expect(firstStat.icon).toBe('fas fa-dollar-sign');
    expect(firstStat.trend).toBe(12);
    expect(firstStat.trendText).toBe('so với tháng trước');
    expect(firstStat.trendUp).toBe(true);
  });

  it('should have charts data with correct structure', () => {
    expect(component.charts).toBeTruthy();
    expect(component.charts.length).toBe(2);
    
    const firstChart = component.charts[0];
    expect(firstChart.title).toBe('Doanh thu theo tháng');
    expect(firstChart.type).toBe('line');
    expect(firstChart.data.length).toBe(7);
    expect(firstChart.labels.length).toBe(7);
  });

  it('should render statistics cards', () => {
    const compiled = fixture.nativeElement;
    const statCards = compiled.querySelectorAll('app-statistics-card');
    expect(statCards.length).toBe(4);
  });

  it('should render chart cards', () => {
    const compiled = fixture.nativeElement;
    const chartCards = compiled.querySelectorAll('app-chart-card');
    expect(chartCards.length).toBe(2);
  });

  it('should display current date', () => {
    const compiled = fixture.nativeElement;
    const dateElement = compiled.querySelector('.date-range span');
    expect(dateElement.textContent).toContain(new Date().getFullYear().toString());
  });

  it('should handle API errors gracefully', () => {
    const mockError = new ErrorEvent('Network error', {
      message: 'Failed to fetch data'
    });

    // Mock the API call
    const req = httpMock.expectOne('/api/dashboard');
    req.error(mockError);

    // Verify that the component handles the error
    expect(component.errorMessage).toBe('Failed to load dashboard data');
  });
});

