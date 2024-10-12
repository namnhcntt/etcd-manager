import { DatePipe, NgTemplateOutlet } from '@angular/common';
import { Component, OnInit, inject, output, signal } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { Guid } from 'guid-ts';
import { ConfirmationService, MessageService } from 'primeng/api';
import { InputSwitchModule } from 'primeng/inputswitch';
import { InputTextModule } from 'primeng/inputtext';
import { MessagesModule } from 'primeng/messages';
import { TableModule } from 'primeng/table';
import { BaseComponent } from '../../base.component';
import { commonLayoutImport } from '../../layout/common-layout-import';
import { EtcdConnectionService } from '../service/etcd-connection.service';

@Component({
  selector: 'app-connection-manager',
  templateUrl: './connection-manager.component.html',
  styles: [``],
  standalone: true,
  imports: [...commonLayoutImport, MessagesModule, TableModule, DatePipe,
    NgTemplateOutlet, InputTextModule, InputSwitchModule],

})
export class ConnectionManagerComponent extends BaseComponent implements OnInit {

  loading = signal(true);
  processing = signal(false);
  formState: 'list' | 'new' | 'edit' = 'list';
  form: FormGroup;
  msgs = [];

  closeForm = output<any>();

  private readonly _confirmationService = inject(ConfirmationService);
  private readonly _messageService = inject(MessageService);
  private readonly _etcdConnectionService = inject(EtcdConnectionService);

  constructor() {
    super();

    this.form = new FormGroup({
      id: new FormControl(),
      name: new FormControl(`connection ${Guid.newGuid().toString()}`, { nonNullable: true, validators: [Validators.required] }),
      server: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
      enableAuthenticated: new FormControl(true),
      username: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
      password: new FormControl('', { nonNullable: true, updateOn: 'blur' }),
      insecure: new FormControl(true),
      agentDomain: new FormControl(''),
    });


  }

  ngOnInit() {
    this.loadGrid();
  }

  loadGrid() {
    this.loading.update(() => true);
    this._etcdConnectionService.getDataSource().then((data: any) => {
      this.globalStore.setDataSourceConnections(data.connections);
      this.loading.update(() => false);
    });
  }

  handleChangeEnableAuthenticated(evt: any) {
    if (evt.checked) {
      this.form.get('username')!.enable();
      this.form.get('password')!.enable();
    } else {
      this.form.get('username')!.disable();
      this.form.get('password')!.disable();
    }
  }

  checkConnection() {
    this.processing.update(() => true);
    this._etcdConnectionService.testConnection
      (
        this.form.value.server,
        this.form.value.enableAuthenticated,
        this.form.value.insecure,
        this.form.value.username,
        this.form.value.password,
      ).then((data: any) => {
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Connection successful' });
        this.processing.update(() => false);
      }).catch((err: any) => {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
        this.processing.update(() => false);
      })
  }

  async saveConnection() {
    if (this.form.valid) {
      this.processing.update(() => true);
      try {
        if (this.formState == 'new') {
          await this._etcdConnectionService.insert(this.form.value);
        } else {
          await this._etcdConnectionService.update(this.form.value.id, this.form.value);
        }
        this.formState = 'list';
        this.loadGrid();
      } catch (e: any) {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: e.error });
      }
      this.processing.update(() => false);
    }
  }

  cancelConnection(evt: any) {
    this._confirmationService.confirm({
      target: evt.target,
      message: 'Are you sure you want to cancel this connection?',
      accept: () => {
        this.form.reset();
        this.formState = 'list';
      },
    });
  }

  newConnection() {
    this.formState = 'new';
  }

  onSelectConnection(selectedItem: any) {
    this.globalStore.selectedEtcdConnection(selectedItem);
    this.closeForm.emit(true);
  }

  editConnection(item: any) {
    this.formState = 'edit';
    this.form.patchValue(item);
  }

  deleteConnection(evt: any, item: any) {
    this._confirmationService.confirm({
      target: evt.target,
      message: 'Are you sure you want to delete this connection?',
      accept: async () => {
        await this._etcdConnectionService.delete(item.id).then(() => {
          this.loadGrid();
        }).catch(err => {
          this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
        });
      },
    });
  }
}
