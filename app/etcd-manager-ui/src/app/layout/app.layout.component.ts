import { ChangeDetectionStrategy, Component, Renderer2, ViewChild, inject } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { BaseComponent } from '../base.component';
import { AppFooterComponent } from './app.footer.component';
import { AppTopBarComponent } from './app.topbar.component';
import { commonLayoutImport } from './common-layout-import';
import { AppConfigComponent } from './config/app.config.component';
import { LayoutService } from "./service/app.layout.service";

@Component({
  selector: 'app-layout',
  templateUrl: './app.layout.component.html',
  standalone: true,
  imports: [...commonLayoutImport, AppTopBarComponent, AppFooterComponent, AppConfigComponent],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppLayoutComponent extends BaseComponent {

  profileMenuOutsideClickListener: any;

  @ViewChild(AppTopBarComponent) appTopbar!: AppTopBarComponent;

  public layoutService = inject(LayoutService);
  public renderer = inject(Renderer2);
  public router = inject(Router);

  constructor() {
    super();
    this.router.events.pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.hideProfileMenu();
      });
  }

  hideProfileMenu() {
    this.layoutService.state.profileSidebarVisible = false;
    if (this.profileMenuOutsideClickListener) {
      this.profileMenuOutsideClickListener();
      this.profileMenuOutsideClickListener = null;
    }
  }

  blockBodyScroll(): void {
    if (document.body.classList) {
      document.body.classList.add('blocked-scroll');
    }
    else {
      document.body.className += ' blocked-scroll';
    }
  }

  unblockBodyScroll(): void {
    if (document.body.classList) {
      document.body.classList.remove('blocked-scroll');
    }
    else {
      document.body.className = document.body.className.replace(new RegExp('(^|\\b)' +
        'blocked-scroll'.split(' ').join('|') + '(\\b|$)', 'gi'), ' ');
    }
  }

  get containerClass() {
    return {
      'layout-theme-light': this.layoutService.config().colorScheme === 'light',
      'layout-theme-dark': this.layoutService.config().colorScheme === 'dark',
      'layout-overlay': this.layoutService.config().menuMode === 'overlay',
      'layout-static': this.layoutService.config().menuMode === 'static',
      'layout-static-inactive': this.layoutService.state.staticMenuDesktopInactive && this.layoutService.config().menuMode === 'static',
      'layout-overlay-active': this.layoutService.state.overlayMenuActive,
      'layout-mobile-active': this.layoutService.state.staticMenuMobileActive,
      'p-input-filled': this.layoutService.config().inputStyle === 'filled',
      'p-ripple-disabled': !this.layoutService.config().ripple
    }
  }
}
