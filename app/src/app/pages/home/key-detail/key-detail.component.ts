import { Component, OnInit, ViewChild } from '@angular/core';
import { CodeEditorComponent } from '@ngstack/code-editor';
import { MessageService } from 'primeng/api';
import { FileUpload } from 'primeng/fileupload';
import { Inplace } from 'primeng/inplace';
import { CodeEditorConstant } from 'src/app/constants/code-editor.constant';
import { AppCtxService } from 'src/app/service/app-ctx.service';
import { ComCtxService } from 'src/app/service/com-ctx.service';
import { ExportService } from 'src/app/service/export.service';
import { KeyValueService } from 'src/app/service/key-value.service';

@Component({
    selector: 'app-key-detail',
    templateUrl: './key-detail.component.html',
    styleUrls: ['./key-detail.component.scss'],
    providers: [ComCtxService]
})
export class KeyDetailComponent implements OnInit {
    showKeyVersion = false;
    loaded = false;
    rootCtx: ComCtxService;
    connection: any;
    keyDetail: any;
    showCodeEditor = true;
    codeEditorConstant = CodeEditorConstant;
    codeModel = CodeEditorConstant.DEFAULT_CODE_MODEL;
    inplaceRenameValue: string;
    editing = false;
    //  TODO: Error on change language but syntax highlight not changed
    languages = [
        { value: 'yaml' },
        { value: 'json' },
        { value: 'xml' },
        { value: 'javascript' },
        { value: 'typescript' },
        { value: 'plaintext' },
    ];

    @ViewChild('codeEditor') codeEditor: CodeEditorComponent;
    @ViewChild('fileControl') fileControl: FileUpload;

    constructor(
        private _appCtxService: AppCtxService,
        public _keyValueService: KeyValueService,
        private _messageService: MessageService,
        private _exportService: ExportService
    ) {
        this.rootCtx = this._appCtxService.getRootCtx();
        this.rootCtx.replaySubscribe('SELECT_CONNECTION', rs => {
            this.connection = rs;
        });
    }

    ngOnInit() {
        this.rootCtx.subscribe('changeSelectedKey', async (value) => {
            this.loaded = true;
            this.getByKey(value);
        });
    }

    async getByKey(value: any) {
        const keyDetail = await this._keyValueService.getByKey(value.key);
        if (keyDetail.success) {
            this.keyDetail = keyDetail.data;
            this.codeModel.value = keyDetail.data.value;
            this.codeModel = { ...this.codeModel };
        } else {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: keyDetail.message });
        }
    }

    save() {
        this._keyValueService.save(this.rootCtx.data.connection, this.keyDetail).then(rs => {
            if (rs.success) {
                this._messageService.add({ severity: 'success', summary: 'Success', detail: 'save success!' });
            } else {
                this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
            }
        }).catch(err => {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: err.message });
        });
    }

    refresh() {
        this.getByKey(this.keyDetail);
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'refresh success!' });
    }

    renameKey(renameKeyInplace: Inplace) {
        this.editing = true;
        this.inplaceRenameValue = this.keyDetail.key;
        renameKeyInplace.activate();
    }

    saveRenameKey(renameKeyInplace: Inplace) {
        console.log('save rename key', this.inplaceRenameValue);
        this._keyValueService.renameKey(this.keyDetail.key, this.inplaceRenameValue).then(rs => {
            if (rs) {
                this.keyDetail.key = this.inplaceRenameValue;
                this.editing = false;
                renameKeyInplace.deactivate();
                this.rootCtx.dispatchEvent('keyRenamed', { newKey: this.inplaceRenameValue, oldKey: this.keyDetail.key });
                this._messageService.add({ severity: 'success', summary: 'Success', detail: 'rename success!' });
            } else {
                this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
            }
        });
    }

    cancelRenameKey(renameKeyInplace: Inplace) {
        this.editing = false;
        renameKeyInplace.deactivate();
    }

    deleteKey() {
        this._keyValueService.onDelete(this.keyDetail.key).then(rs => {
            if (rs) {
                this.loaded = false;
            }
        });
    }

    onCodeChanged(evt) {
        this.keyDetail.value = evt;
    }

    changeLanguage(evt) {
        this.codeModel.language = evt.value;
        this.codeModel.uri = `*.${evt.value}`;
        this.codeModel = { ...this.codeModel };
        this.showCodeEditor = false;
        setTimeout(() => {
            this.showCodeEditor = true;
        }, 1000);
    }

    useValueFromOldVersion(value: string) {
        this.showCodeEditor = false;
        this.keyDetail.value = value;
        this.codeModel.value = value;
        this.codeModel = { ...this.codeModel };
        setTimeout(() => {
            this.showCodeEditor = true;
        }, 1000);
    }

    viewAllVersion() {
        this.showKeyVersion = true;
    }

    export() {
        this._exportService.exportJsonNode(this.keyDetail);
    }

    import() {
        if (this.fileControl.basicFileInput) {
            this.fileControl.basicFileInput.nativeElement.click();
        }
    }

    handleSelectFile(evt) {
        this._exportService.readDataFromFile(evt.currentFiles[0]).then(rs => {
            if (rs) {
                this.keyDetail.value = rs.value;
                this.codeModel.value = this.keyDetail.value;
                this.codeModel = { ...this.codeModel };
            }
        });
    }
}
