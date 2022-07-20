import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { MenuItem, MessageService } from 'primeng/api';
import { Dialog } from 'primeng/dialog';
import { KeyValueService } from 'src/app/service/key-value.service';
import { CommonUtils } from 'src/app/utils/common.utils';
import * as allFonts from '@fortawesome/free-solid-svg-icons';

@Component({
    selector: 'app-key-version-list',
    templateUrl: './key-version-list.component.html',
    styleUrls: ['./key-version-list.component.scss']
})
export class KeyVersionListComponent implements OnInit {

    @Input() dialog: Dialog;
    @Input() key: string;
    @Output() onClose = new EventEmitter<any>();

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
                this.versions = CommonUtils.sortArray(arr, 'modRevision', false);
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
}
