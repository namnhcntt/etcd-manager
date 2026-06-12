import { APP_BASE_HREF } from '@angular/common';
import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { LoginComponent } from './login.component';
import { AuthService } from '../service/auth.service';

describe('LoginComponent', () => {
  let fixture: ComponentFixture<LoginComponent>;
  let component: LoginComponent;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', [
      'hasValidAccessToken',
      'login',
      'saveToken'
    ]);
    authServiceSpy.hasValidAccessToken.and.returnValue(false);

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        { provide: AuthService, useValue: authServiceSpy },
        // The component assigns window.location.href = baseHref on success.
        // Use the current document URL plus a fragment so the assignment is a
        // pure hash change and the Karma page never reloads/navigates.
        { provide: APP_BASE_HREF, useValue: window.location.href.split('#')[0] + '#login-redirect' }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  function renderedMessageTexts(): string[] {
    const host: HTMLElement = fixture.nativeElement;
    return Array.from(host.querySelectorAll('p-message'))
      .map(el => (el.textContent ?? '').trim());
  }

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('shows a validation message when username is empty', async () => {
    component.userName.set('');
    component.password.set('secret');

    component.signIn();
    await fixture.whenStable();

    expect(component.loginMessages()).toEqual([
      jasmine.objectContaining({ severity: 'error', detail: 'Please enter userName' })
    ]);
    expect(renderedMessageTexts().some(t => t.includes('Please enter userName'))).toBeTrue();
    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });

  it('shows a validation message when password is empty', async () => {
    component.userName.set('admin');
    component.password.set('');

    component.signIn();
    await fixture.whenStable();

    expect(component.loginMessages()).toEqual([
      jasmine.objectContaining({ severity: 'error', detail: 'Please enter password' })
    ]);
    expect(renderedMessageTexts().some(t => t.includes('Please enter password'))).toBeTrue();
    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });

  it('renders the server error message in the DOM when login fails (zoneless regression guard)', async () => {
    component.userName.set('admin');
    component.password.set('wrong');
    authServiceSpy.login.and.returnValue(Promise.reject({ error: { error: 'Bad credentials' } }));

    component.signIn();
    // let the rejected promise's catch handler run, then let zoneless CD render
    await new Promise<void>(resolve => setTimeout(resolve));
    await fixture.whenStable();

    expect(component.loginMessages()).toEqual([
      jasmine.objectContaining({ severity: 'error', detail: 'Bad credentials' })
    ]);
    expect(renderedMessageTexts().some(t => t.includes('Bad credentials'))).toBeTrue();
  });

  it('saves the access token on successful login', async () => {
    component.userName.set('admin');
    component.password.set('correct');
    authServiceSpy.login.and.returnValue(Promise.resolve({ token: 'jwt-token' }));

    component.signIn();
    await new Promise<void>(resolve => setTimeout(resolve));
    await fixture.whenStable();

    expect(authServiceSpy.login).toHaveBeenCalledWith('admin', 'correct');
    // refresh token is delivered via HttpOnly cookie, so saveToken takes only the access token
    expect(authServiceSpy.saveToken).toHaveBeenCalledWith('jwt-token');
    expect(component.loginMessages()).toEqual([]);
    // the redirect was performed (hash-only navigation, see APP_BASE_HREF stub above)
    expect(window.location.hash).toBe('#login-redirect');
  });
});
