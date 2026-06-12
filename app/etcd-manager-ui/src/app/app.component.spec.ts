import { provideZonelessChangeDetection } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { AppComponent } from './app.component';
import { AuthService } from './pages/service/auth.service';
import { EtcdConnectionService } from './pages/service/etcd-connection.service';
import { globalStore } from './global.store';

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;
  let component: AppComponent;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let etcdConnectionServiceSpy: jasmine.SpyObj<EtcdConnectionService>;

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', [
      'hasValidAccessToken',
      'hasSessionHint',
      'refreshAccessToken'
    ]);
    // valid token → ngOnInit skips silent refresh and the /login redirect
    authServiceSpy.hasValidAccessToken.and.returnValue(true);
    authServiceSpy.hasSessionHint.and.returnValue(false);

    etcdConnectionServiceSpy = jasmine.createSpyObj<EtcdConnectionService>(
      'EtcdConnectionService', ['getDataSource']);
    etcdConnectionServiceSpy.getDataSource.and.returnValue(Promise.resolve({ connections: [] } as any));

    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        MessageService,
        ConfirmationService,
        { provide: AuthService, useValue: authServiceSpy },
        { provide: EtcdConnectionService, useValue: etcdConnectionServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('should create and compile the template (p-drawer present)', () => {
    expect(component).toBeTruthy();
    const host: HTMLElement = fixture.nativeElement;
    expect(host.querySelector('p-drawer')).withContext('p-drawer renders').not.toBeNull();
  });

  it('opens the drawer when the global store requests the connection manager sidebar', async () => {
    expect(component.displaySidebar).toBeFalse();

    const store = TestBed.inject(globalStore);
    store.clickPage('connectionManager');
    await fixture.whenStable();

    expect(component.displaySidebar).toBeTrue();
    const host: HTMLElement = fixture.nativeElement;
    expect(host.querySelector('app-connection-manager'))
      .withContext('connection manager rendered inside drawer').not.toBeNull();

    store.closeSidebar();
    await fixture.whenStable();
    expect(component.displaySidebar).toBeFalse();
  });
});
