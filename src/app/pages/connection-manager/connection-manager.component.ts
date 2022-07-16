import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Guid } from 'guid-ts';
import { ConfirmationService } from 'primeng/api';
import { Sidebar } from 'primeng/sidebar';
import { AppEventService } from 'src/app/service/app-event.service';
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

    @Output() closeForm: EventEmitter<void> = new EventEmitter<void>();

    constructor(
        private _connectionService: ConnectionService,
        private _confirmationService: ConfirmationService,
        private _appEventService: AppEventService
    ) {
        this.form = new FormGroup({
            id: new FormControl(),
            name: new FormControl(`connection ${Guid.newGuid().toString()}`, { nonNullable: true, validators: [Validators.required] }),
            server: new FormControl('10.0.68.171:2379', { nonNullable: true, validators: [Validators.required] }),
            username: new FormControl('root', { nonNullable: true, validators: [Validators.required] }),
            password: new FormControl('123456a@', { nonNullable: true, validators: [Validators.minLength(8)], updateOn: 'blur' }),
        });
    }

    ngOnInit() {
        this.loadGrid();
    }

    loadGrid() {
        this.connections = this._connectionService.getDataSource();
    }

    newConnection() {
        this.formState = 'new';
    }

    cancelConnection() {
        this.formState = 'list';
    }

    saveConnection() {
        if (this.form.valid) {
            this.processing = true;

            let result = '';
            if (this.formState == 'new') {
                result = this._connectionService.insert(this.form.value);
            } else {
                result = this._connectionService.update(this.form.value);
            }
            if (result) {
                alert(result);
            } else {
                this._appEventService.reloadDataSource();
                this.formState = 'list';
                this.loadGrid();
            }
            this.processing = false;
        }
    }

    checkConnection() {

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
                this._appEventService.reloadDataSource();
                this.loadGrid();
            },
        });
    }

    onSelectConnection(connection: any) {
        this._appEventService.selectConnection(connection);
        this.closeForm.emit();
    }
}
