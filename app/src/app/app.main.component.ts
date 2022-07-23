import { animate, state, style, transition, trigger } from '@angular/animations';
import { AfterViewInit, Component, OnDestroy, OnInit, Renderer2 } from '@angular/core';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { AppConfig } from './api/appconfig';
import { AppComponent } from './app.component';
import { AppCtxService } from './service/app-ctx.service';
import { ConfigService } from './service/app.config.service';
import { AuthenService } from './service/authen.service';
import { ComCtxService } from './service/com-ctx.service';

@Component({
    selector: 'app-main',
    templateUrl: './app.main.component.html',
    animations: [
        trigger('submenu', [
            state('hidden', style({
                height: '0px'
            })),
            state('visible', style({
                height: '*'
            })),
            transition('visible => hidden', animate('400ms cubic-bezier(0.86, 0, 0.07, 1)')),
            transition('hidden => visible', animate('400ms cubic-bezier(0.86, 0, 0.07, 1)'))
        ])
    ],
    providers: [ComCtxService]
})
export class AppMainComponent implements AfterViewInit, OnDestroy, OnInit {

    public menuInactiveDesktop: boolean;

    public menuActiveMobile: boolean;

    public overlayMenuActive: boolean;

    public staticMenuInactive: boolean = false;

    public profileActive: boolean;

    public topMenuActive: boolean;

    public topMenuLeaving: boolean;

    public theme: string;

    documentClickListener: () => void;

    menuClick: boolean;

    topMenuButtonClick: boolean;

    configActive: boolean;

    configClick: boolean;

    config: AppConfig;

    subscription: Subscription;
    dipslaySidebar = {
        '/connection-manager': false,
        '/user-manager': false
    }

    pageLoaded = false;

    constructor(
        public renderer: Renderer2,
        public app: AppComponent,
        public configService: ConfigService,
        private _appCtxService: AppCtxService,
        private _comCtxService: ComCtxService,
        private _authenService: AuthenService,
        private _router: Router
    ) {
        this._appCtxService.createRootCtx(this._comCtxService);
    }

    ngOnInit() {
        if (this._authenService.hasValidAccessToken()) {
            this.pageLoaded = true;
        } else {
            this._router.navigate(['/login']);
        }
        this.config = this.configService.config;
        this.subscription = this.configService.configUpdate$.subscribe(config => this.config = config);
    }

    ngAfterViewInit() {
        // hides the overlay menu and top menu if outside is clicked
        this.documentClickListener = this.renderer.listen('body', 'click', (event) => {
            if (!this.isDesktop()) {
                if (!this.menuClick) {
                    this.menuActiveMobile = false;
                }

                if (!this.topMenuButtonClick) {
                    this.hideTopMenu();
                }
            }
            else {
                if (!this.menuClick && this.isOverlay()) {
                    this.menuInactiveDesktop = true;
                }
                if (!this.menuClick) {
                    this.overlayMenuActive = false;
                }
            }

            if (this.configActive && !this.configClick) {
                this.configActive = false;
            }

            this.configClick = false;
            this.menuClick = false;
            this.topMenuButtonClick = false;
        });
    }

    toggleMenu(event: Event) {
        this.menuClick = true;

        if (this.isDesktop()) {
            if (this.app.menuMode === 'overlay') {
                if (this.menuActiveMobile === true) {
                    this.overlayMenuActive = true;
                }

                this.overlayMenuActive = !this.overlayMenuActive;
                this.menuActiveMobile = false;
            }
            else if (this.app.menuMode === 'static') {
                this.staticMenuInactive = !this.staticMenuInactive;
            }
        }
        else {
            this.menuActiveMobile = !this.menuActiveMobile;
            this.topMenuActive = false;
        }

        event.preventDefault();
    }

    toggleProfile(event: Event) {
        this.profileActive = !this.profileActive;
        event.preventDefault();
    }

    toggleTopMenu(event: Event) {
        this.topMenuButtonClick = true;
        this.menuActiveMobile = false;

        if (this.topMenuActive) {
            this.hideTopMenu();
        } else {
            this.topMenuActive = true;
        }

        event.preventDefault();
    }

    hideTopMenu() {
        this.topMenuLeaving = true;
        setTimeout(() => {
            this.topMenuActive = false;
            this.topMenuLeaving = false;
        }, 1);
    }

    onMenuClick() {
        this.menuClick = true;
    }

    onConfigClick(event: any) {
        this.configClick = true;
    }

    isStatic() {
        return this.app.menuMode === 'static';
    }

    isOverlay() {
        return this.app.menuMode === 'overlay';
    }

    isDesktop() {
        return window.innerWidth > 992;
    }

    isMobile() {
        return window.innerWidth < 1024;
    }

    onSearchClick() {
        this.topMenuButtonClick = true;
    }

    ngOnDestroy() {
        if (this.documentClickListener) {
            this.documentClickListener();
        }


        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    onClickPageTopbar(url) {
        this.hiddenAllSidebar();
        this.dipslaySidebar[url] = true;
    }

    hiddenAllSidebar() {
        Object.keys(this.dipslaySidebar).forEach(key => this.dipslaySidebar[key] = false);
    }
}
