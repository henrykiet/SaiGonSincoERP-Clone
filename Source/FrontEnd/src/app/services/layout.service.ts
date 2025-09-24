import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LayoutService {
  private showLayoutSubject = new BehaviorSubject<boolean>(true);
  showLayout$ = this.showLayoutSubject.asObservable();

  constructor() { }

  hideLayout() {
    this.showLayoutSubject.next(false);
  }

  showLayout() {
    this.showLayoutSubject.next(true);
  }
} 