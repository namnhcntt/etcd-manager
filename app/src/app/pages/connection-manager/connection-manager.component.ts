import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Guid } from 'guid-ts';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ResponseModel } from 'src/app/models/response.model';
import { AppCtxService } from 'src/app/service/app-ctx.service';
import { ComCtxService } from 'src/app/service/com-ctx.service';
import { ConnectionService } from 'src/app/service/connection.service';

@Component({
    selector: 'app-connection-manager',
    templateUrl: './connection-manager.component.html',
    styleUrls: ['./connection-manager.component.scss'],
})
export class ConnectionManagerComponent implements OnInit {
    loading = false;
    processing = false;
    connections = [];
    formState: 'list' | 'new' | 'edit' = 'list';
    form: FormGroup;
    msgs = [];
    rootCtx: ComCtxService;

    @Output() closeForm: EventEmitter<void> = new EventEmitter<void>();

    constructor(
        private _connectionService: ConnectionService,
        private _confirmationService: ConfirmationService,
        private _messageService: MessageService,
        private _appCtxService: AppCtxService
    ) {
        this.rootCtx = this._appCtxService.getRootCtx();
        this.form = new FormGroup({
            id: new FormControl(),
            name: new FormControl(`connection ${Guid.newGuid().toString()}`, { nonNullable: true, validators: [Validators.required] }),
            server: new FormControl('14.225.3.52:2379', { nonNullable: true, validators: [Validators.required] }),
            enableAuthenticated: new FormControl(true),
            username: new FormControl('root', { nonNullable: true, validators: [Validators.required] }),
            password: new FormControl('giaothong@123', { nonNullable: true, validators: [Validators.minLength(8)], updateOn: 'blur' }),
            insecure: new FormControl(true)
        });
    }

    ngOnInit() {
        this.loadGrid();
    }

    async loadGrid() {
        this.connections = (await this._connectionService.getDataSource()).data;
    }

    newConnection() {
        this.formState = 'new';
    }

    cancelConnection() {
        this.formState = 'list';
    }

    async saveConnection() {
        if (this.form.valid) {
            this.processing = true;

            let result: ResponseModel;
            if (this.formState == 'new') {
                result = await this._connectionService.insert(this.form.value);
            } else {
                result = await this._connectionService.update(this.form.value);
            }
            if (!result.success) {
                this._messageService.add({ severity: 'error', summary: 'Error', detail: result.error });
            } else {
                this.rootCtx.dispatchReplayEvent('RELOAD_DATASOURCE');
                this.formState = 'list';
                this.loadGrid();
            }
            this.processing = false;
        }
    }

    checkConnection() {
        this._connectionService.checkConnection(this.form.value).then(rs => {
            console.log('result', rs);
            this.msgs = [];
            if (rs.success) {
                this.msgs.push({
                    severity: 'success',
                    summary: 'Success',
                    detail: rs.data
                });
            } else {
                this.msgs.push({
                    severity: 'error',
                    summary: 'Error',
                    detail: rs.message
                });
            }
        });
    }

    editConnection(item: any) {
        this.formState = 'edit';
        this.form.patchValue(item);
    }

    deleteConnection(event: Event, item: any) {
        this._confirmationService.confirm({
            target: event.target,
            message: 'Are you sure you want to delete this connection?',
            accept: () => {
                this._connectionService.deleteByName(item.name);
                this.rootCtx.dispatchReplayEvent('RELOAD_DATASOURCE');
                this.loadGrid();
            },
        });
    }

    onSelectConnection(connection: any) {
        this.rootCtx.dispatchReplayEvent('SELECT_CONNECTION', connection);
        this.closeForm.emit();
    }

    handleChangeEnableAuthenticated(evt) {
        if (evt.checked) {
            this.form.get('username').enable();
            this.form.get('password').enable();
        } else {
            this.form.get('username').disable();
            this.form.get('password').disable();
        }
    }
}
