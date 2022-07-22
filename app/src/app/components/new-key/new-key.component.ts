import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MenuItem, MessageService } from 'primeng/api';
import { Dialog } from 'primeng/dialog';
import { CodeEditorConstant } from 'src/app/constants/code-editor.constant';
import { AppCtxService } from 'src/app/service/app-ctx.service';
import { KeyValueService } from 'src/app/service/key-value.service';

@Component({
    selector: 'app-new-key',
    templateUrl: './new-key.component.html',
    styleUrls: ['./new-key.component.scss']
})
export class NewKeyComponent implements OnInit, OnDestroy {
    @Input() dialog: Dialog;
    @Input() parentKey: string = '';
    @Output() onSave = new EventEmitter<any>();
    codeEditorConstant = CodeEditorConstant;
    form: FormGroup;
    processing = false;
    codeModel = CodeEditorConstant.DEFAULT_CODE_MODEL;
    rootCtx: any;
    public buttons: MenuItem[] = [];

    constructor(
        private _keyValueService: KeyValueService,
        private _appCtx: AppCtxService,
        private _messageService: MessageService
    ) {
        this.rootCtx = this._appCtx.getRootCtx();

        this.buttons = [
            {
                label: 'Save', icon: 'pi pi-save', command: this.save.bind(this),
                disabled: this.processing,
                styleClass: 'p-ripple p-button-raised p-button-primary p-button-text'
            },
            {
                label: 'Cancel', icon: 'pi pi-times', command: (event) => {
                    this.dialog.close({} as Event);
                },
                styleClass: 'p-ripple p-button-raised p-button-secondary p-button-text'
            }
        ]
    }

    ngOnDestroy(): void {
        this.form.reset();
    }

    ngOnInit() {
        this.form = new FormGroup({
            id: new FormControl(),
            key: new FormControl(this.parentKey, { nonNullable: true, validators: [Validators.required] }),
            value: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
        });
    }

    onCodeChanged(evt) {
        this.form.get('value').markAsDirty();
        this.form.patchValue({
            value: evt
        });
    }

    save() {
        if (this.form.valid) {
            this.processing = true;
            this._keyValueService.save(this.form.value, true).then(rs => {
                if (rs.success) {
                    this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Create new key sucess!' });
                    this.form.reset();
                    this.onSave.emit(true);
                } else {
                    this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
                    this.processing = false;
                }
            }).catch(err => {
                this._messageService.add({ severity: 'error', summary: 'Error', detail: err.message });
                this.processing = false;
            });
        }
    }
}
