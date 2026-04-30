// src/app/app.component.spec.ts
//
// Tests for AppComponent — the thin root router-outlet wrapper.
//
// AppComponent is intentionally minimal (just a router-outlet). These tests verify:
// 1. The component creates successfully.
// 2. It renders a router-outlet element.
// 3. It has OnPush change detection.
//
// Note: RouterTestingModule is used to prevent router initialisation errors
// in the test environment when RouterOutlet is present.

import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { ChangeDetectionStrategy } from '@angular/core';
import { AppComponent } from './app.component';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent, RouterTestingModule],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  it('should render a router-outlet element', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    // AppComponent's only template content is <router-outlet />
    expect(compiled.querySelector('router-outlet')).toBeTruthy();
  });

  it('should use OnPush change detection', () => {
    const metadata = TestBed.createComponent(AppComponent).componentRef.changeDetectorRef;
    // Verify the component was defined with OnPush
    const componentDef = AppComponent as unknown as { ɵcmp: { onPush: boolean } };
    expect(componentDef.ɵcmp.onPush).toBe(true);
  });
});
