import { Component, inject, input, model, output, signal } from '@angular/core';
import { ConfirmationService, MenuItem, MessageService, PrimeIcons } from 'primeng/api';
import { BadgeModule } from 'primeng/badge';
import { Dialog, DialogModule } from 'primeng/dialog';
import { FileUploadModule } from 'primeng/fileupload';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { BaseComponent } from '../../base.component';
import { commonLayoutImport } from '../../layout/common-layout-import';
import { DiffTextComponent } from '../diff-text/diff-text.component';
import { ExportService } from '../service/export.service';
import { KeyValueService } from '../service/key-value.service';
import { ValueDetailComponent } from '../value-detail/value-detail.component';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
@Component({
  selector: 'app-import-nodes',
  templateUrl: './import-nodes.component.html',
  styles: [``],
  standalone: true,
  imports: [...commonLayoutImport, DialogModule, FileUploadModule, TableModule, InputTextModule, ValueDetailComponent, DiffTextComponent, BadgeModule,
    TooltipModule
  ]
})
export class ImportNodesComponent extends BaseComponent {
  dialog = input.required<Dialog>();
  importSuccess = output<any>();
  public primeIcons = PrimeIcons;
  public buttons: MenuItem[] = [];
  dataSource = signal([] as any[]);
  selectedItems = model([] as any[]);
  showDiffText = model(false);
  leftDiffItem = signal({} as any);
  rightDiffItem = signal({} as any);
  currentSelectedItem = signal({} as any);

  private _exportService = inject(ExportService);
  private _keyValueService = inject(KeyValueService);
  private _confirmService = inject(ConfirmationService);
  private _messageService = inject(MessageService);

  ngOnInit() {
    this.buttons = [
      {
        label: 'Import selected', icon: 'pi pi-check', command: (event) => {
          this.importSelectedNodes(event);
        },
        styleClass: 'p-ripple p-button-raised p-button-primary p-button-text'
      },
      {
        label: 'Import all', icon: 'pi pi-check', command: (event) => {
          this.importAll(event);
        },
        styleClass: 'p-ripple p-button-raised p-button-help p-button-text'
      },
    ]
  }

  importSelectedNodes(evt: any) {
    if (this.selectedItems().length == 0) {
      this._messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please select nodes to import' });
      return;
    }
    this._confirmService.confirm({
      message: 'Are you sure to import selected nodes?',
      target: evt.target,
      icon: PrimeIcons.EXCLAMATION_TRIANGLE,
      closeOnEscape: true,
      accept: () => {
        this.doImport(this.selectedItems());
      }
    });
  }

  importAll(evt: any) {
    if (this.dataSource().length == 0) {
      this._messageService.add({ severity: 'warn', summary: 'Warning', detail: 'No nodes to import' });
      return;
    }
    this._confirmService.confirm({
      message: 'Are you sure to import all nodes?',
      target: evt.target,
      icon: PrimeIcons.EXCLAMATION_TRIANGLE,
      closeOnEscape: true,
      accept: () => {
        this.doImport(this.dataSource());
      }
    });
  }

  doImport(selectNodes: any[]) {
    this._keyValueService.importNodes(this.globalStore.connections.selectedEtcdConnection.id(), selectNodes).then((rs: any) => {
      this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Import nodes success' });
      this.importSuccess.emit(true);
    }).catch((err: any) => {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
    });
  }

  handleSelectFile(evt: any) {
    this._exportService.readDataFromFile(evt.currentFiles[0]).then(rs => {
      if (rs) {
        let arr: any[];
        if (rs.value) {
          // import single node
          arr = [rs.value];
        } else {
          // import nodes
          arr = rs;
        }

        for (const item of arr) {
          const temp = item.value.split(' ');
          if (temp.length > 20) {
            item.shortValue = temp.slice(0, 20).join(' ') + '...';
          } else {
            item.shortValue = item.value;
          }

          // check exist
          item.exist = this.globalStore.keyValues.dataSource().find((x: any) => x.key == item.key);
        }
        this.dataSource.update(() => arr);
      }
    });
  }

  async seeFullValue(selectedItem: any) {
    let leftItem = { key: selectedItem.key, value: '' };
    if (selectedItem.exist) {
      const leftKey = selectedItem.exist.key;
      leftItem = await this._keyValueService.getByKey(this.globalStore.connections.selectedEtcdConnection.id(), leftKey);
    }
    this.leftDiffItem.update(() => leftItem);
    this.rightDiffItem.update(() => selectedItem);
    this.showDiffText.update(() => true);
  }

  onValueDetailButtonCommand(button: MenuItem, event: any) {
    if (button.command) {
      button.command(event);
    }
  }

  hideDialog() {
    this.showDiffText.update(() => false);
  }
}
