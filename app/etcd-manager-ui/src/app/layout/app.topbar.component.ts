import { NgClass } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, effect, inject, model, untracked } from '@angular/core';
import { patchState } from '@ngrx/signals';
import { MenuItem, PrimeIcons } from 'primeng/api';
import { AvatarModule } from 'primeng/avatar';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { MenuModule } from 'primeng/menu';
import { BaseComponent } from '../base.component';
import { AuthService } from '../pages/service/auth.service';
import { LocalCacheService } from '../pages/service/local-cache.service';
import { commonLayoutImport } from './common-layout-import';
import { LayoutService } from "./service/app.layout.service";

@Component({
  selector: 'app-topbar',
  templateUrl: './app.topbar.component.html',
  standalone: true,
  styles: [`
  .user-topbar {
        height: 3rem;
        line-height: 3rem;
        margin-left:10px;
        display: inline-flex;
        justify-content: right;
        align-items: right;
    }
    .user-topbar:hover {
        cursor: pointer;

    }

    .user-topbar span {
        margin-left:10px;
    }

    .layout-topbar-menu-left .layout-topbar-button {
        margin-left:20px;

    }`],
  imports: [...commonLayoutImport, NgClass, MenuModule, DropdownModule, AvatarModule, DialogModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppTopBarComponent extends BaseComponent implements OnInit {
  public layoutService = inject(LayoutService);
  public authService = inject(AuthService);

  private _localCacheService = inject(LocalCacheService);

  selectedConnection = model<any>(-1);
  selfEvent = false;

  showChangeMyPassword = false;
  userMenuItems: MenuItem[] = [
    {
      label: 'Change Password', icon: PrimeIcons.LOCK,
      command: this.changePassword.bind(this)
    },
    {
      label: 'Logout', icon: PrimeIcons.SIGN_OUT,
      command: this.logout.bind(this)
    },
  ];
  hasManageUserPermission = true;

  constructor() {
    super();

    effect(() => {
      const selectedItem = this.globalStore.connections.selectedEtcdConnection();
      if (selectedItem && selectedItem.id && selectedItem.id > 0) {
        untracked(() => {
          this.selectedConnection.update(() => selectedItem.id);
          // save cache
          this._localCacheService.set('selectedConnection', selectedItem);
        });
      }
    });

    effect(() => {
      const selectedConnectionChanged = this.selectedConnection();
      const selectedItem = this.globalStore.connections.dataSource().find(x => x.id == selectedConnectionChanged);
      untracked(() => {
        this.globalStore.selectedEtcdConnection(selectedItem);
      });
    });
  }

  ngOnInit(): void {
    // restore selected item from cache
    const selectedConnection = this._localCacheService.get('selectedConnection');
    if (selectedConnection) {
      this.selectedConnection.update(() => selectedConnection.id);
    }
  }

  changePassword() {
    this.showChangeMyPassword = true;
  }

  logout() {
    this.hasManageUserPermission = false;
    this.authService.logout();
  }
}
