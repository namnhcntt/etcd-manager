import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { Dialog } from 'primeng/dialog';
import { ExportService } from 'src/app/service/export.service';
import { KeyValueService } from 'src/app/service/key-value.service';

@Component({
    selector: 'app-import-nodes',
    templateUrl: './import-nodes.component.html',
    styleUrls: ['./import-nodes.component.scss']
})
export class ImportNodesComponent implements OnInit {

    @Input() dialog: Dialog;
    @Output() importSuccess = new EventEmitter<any>();

    public buttons: MenuItem[] = [];
    dataSource = [];
    selectedItems = [];
    showValueDetail = false;
    currentSelectedItem: any = {};
    constructor(
        private _exportService: ExportService,
        private _keyValueService: KeyValueService,
        private _confirmService: ConfirmationService,
        private _messageService: MessageService
    ) { }

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
            {
                label: 'Cancel', icon: 'pi pi-times', command: (event) => {
                    this.dialog.close({} as Event);
                },
                styleClass: 'p-ripple p-button-raised p-button-secondary p-button-text'
            },
        ]
    }

    importSelectedNodes(evt) {
        if (this.selectedItems.length == 0) {
            this._messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please select nodes to import' });
            return;
        }
        this._confirmService.confirm({
            message: 'Are you sure to import selected nodes?',
            target: evt.target,
            key: 'confirmPopup',
            icon: 'pi pi-exclamation-triangle',
            closeOnEscape: true,
            accept: () => {
                this.doImport(this.selectedItems);
            }
        });
    }

    importAll(evt) {
        if (this.dataSource.length == 0) {
            this._messageService.add({ severity: 'warn', summary: 'Warning', detail: 'Please select nodes to import' });
            return;
        }
        this._confirmService.confirm({
            message: 'Are you sure to import all nodes?',
            target: evt.target,
            key: 'confirmPopup',
            accept: () => {
                this.doImport(this.dataSource);
            }
        });
    }

    doImport(selectNodes: any[]) {
        this._keyValueService.importNodes(selectNodes).then(rs => {
            if (rs.success) {
                this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Import nodes success' });
                this.importSuccess.emit(true);
                this.dialog.close({} as Event);
            } else {
                this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
            }
        }).catch(err => {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: err.message });
        });
    }

    handleSelectFile(evt) {
        this._exportService.readDataFromFile(evt.currentFiles[0]).then(rs => {
            if (rs) {
                console.log('value', rs);
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
                }
                this.dataSource = arr;
            }
        });
    }

    seeFullValue(selectedItem: any) {
        this.currentSelectedItem = selectedItem;
        this.showValueDetail = true;
    }
}
