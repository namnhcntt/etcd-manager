import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import * as allFonts from '@fortawesome/free-solid-svg-icons';
import { MenuItem, MessageService } from 'primeng/api';
import { Dialog } from 'primeng/dialog';
import { KeyValueService } from 'src/app/service/key-value.service';

@Component({
    selector: 'app-key-version-list',
    templateUrl: './key-version-list.component.html',
    styleUrls: ['./key-version-list.component.scss']
})
export class KeyVersionListComponent implements OnInit {

    @Input() dialog: Dialog;
    @Input() key: string;
    @Output() onUseValue = new EventEmitter<any>();

    showDiffDialog = false;
    loading = true;
    versions = [];
    createRevision = 0;
    currentVersion = 0;
    currentVersionItem: any;
    diffItem: any;
    fonts = allFonts;
    public buttons: MenuItem[] = [];

    constructor(
        private _keyValueService: KeyValueService,
        private _messageService: MessageService
    ) {
    }

    ngOnInit() {
        this._keyValueService.getRevisionOfKey(this.key).then(rs => {
            if (rs.success) {
                const arr = rs.data.filter(x => x.eventType == 0);
                if (arr.length > 0) {
                    this.createRevision = arr[0].createRevision;
                    this.currentVersionItem = arr[arr.length - 1];
                    this.currentVersion = arr[arr.length - 1].version;
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
            } else {
                this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
            }
            this.loading = false;
        });

        this.buttons = [
            {
                label: 'Cancel', icon: 'pi pi-times', command: (event) => {
                    this.dialog.close({} as Event);
                },
                styleClass: 'p-ripple p-button-raised p-button-secondary p-button-text'
            }
        ]
    }

    compareVersion(item) {
        this.diffItem = item;
        this.showDiffDialog = true;
    }

    selectThisValue(item) {
        this.onUseValue.emit(item.value);
        this.dialog.close(item);
    }
}
