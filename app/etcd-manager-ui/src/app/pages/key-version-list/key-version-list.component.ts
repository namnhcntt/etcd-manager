import { ChangeDetectionStrategy, Component, inject, input, model, output, signal } from '@angular/core';
import { PrimeIcons } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { Dialog, DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { BaseComponent } from '../../base.component';
import { commonLayoutImport } from '../../layout/common-layout-import';
import { DiffTextComponent } from '../diff-text/diff-text.component';
import { KeyValueService } from '../service/key-value.service';

@Component({
  selector: 'app-key-version-list',
  templateUrl: './key-version-list.component.html',
  styles: [``],
  standalone: true,
  imports: [...commonLayoutImport, DialogModule, DiffTextComponent, TableModule, ButtonModule, InputTextModule, TooltipModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KeyVersionListComponent extends BaseComponent {

  dialog = input.required<Dialog>();
  key = input.required<string>();
  onUseValue = output<any>();

  showDiffDialog = model(false);
  loading = true;
  versions = [];
  createRevision = signal(0);
  currentVersion = signal(0);
  currentVersionItem: any;
  diffItem: any;
  public primengIcons = PrimeIcons;
  private _keyValueService = inject(KeyValueService);

  ngOnInit() {
    this._keyValueService.getRevision(this.globalStore.connections.selectedEtcdConnection.id(), this.key()).then(rs => {
      const arr = rs.filter((x: any) => x.eventType == 0);
      if (arr.length > 0) {
        this.createRevision.update(() => arr[0].createRevision);
        this.currentVersionItem = arr[arr.length - 1];
        this.currentVersion.update(() => arr[arr.length - 1].version);
      }
      for (const item of arr) {
        const temp = item.value.split(' ');
        if (temp.length > 20) {
          item.shortValue = temp.slice(0, 20).join(' ') + '...';
        } else {
          item.shortValue = item.value;
        }
      }
      this.versions = arr;
      this.loading = false;
    });
  }

  compareVersion(item: any) {
    this.diffItem = item;
    this.showDiffDialog.update(() => true);
  }

  selectThisValue(item: any) {
    this.onUseValue.emit(item.value);
  }

  closeDiffDialog() {
    this.showDiffDialog.update(() => false)
  }
}
