import { ChangeDetectionStrategy, Component, ViewChild, effect, inject, signal, untracked } from '@angular/core';
import { patchState, signalState } from '@ngrx/signals';
import { CodeEditorComponent, CodeEditorModule } from '@ngstack/code-editor';
import { ConfirmationService, MessageService } from 'primeng/api';
import { AutoFocusModule } from 'primeng/autofocus';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { FileUpload, FileUploadModule } from 'primeng/fileupload';
import { Inplace, InplaceModule } from 'primeng/inplace';
import { InputTextModule } from 'primeng/inputtext';
import { ToolbarModule } from 'primeng/toolbar';
import { TooltipModule } from 'primeng/tooltip';
import { BaseComponent } from '../../../base.component';
import { commonLayoutImport } from '../../../layout/common-layout-import';
import { CodeEditorConstant } from '../../constants/code-editor.constant';
import { KeyValueService } from '../../service/key-value.service';

@Component({
  selector: 'app-key-detail',
  templateUrl: './key-detail.component.html',
  styles: [`
  .key-detail-value {
    width: 100%;
    height: 100%;
}

.more-info {
    display: block;
    margin-left: 20px;
    font-size: 0.9em;
}

.dropdown-language {
    margin-left: 20px;
}

`],
  standalone: true,
  imports: [...commonLayoutImport, ToolbarModule, DropdownModule, DialogModule,
    FileUploadModule, CodeEditorModule, InplaceModule, TooltipModule,
    InputTextModule, AutoFocusModule],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KeyDetailComponent extends BaseComponent {
  showKeyVersion = false;
  loaded = signal(false);
  editorLoaded = signal(false);
  isNewState = signal(false);
  connection: any;
  keyDetail = signalState({ key: null as string | null, value: null as string | null });
  showCodeEditor = true;
  codeEditorConstant = CodeEditorConstant;
  codeModel = CodeEditorConstant.DEFAULT_CODE_MODEL;
  inplaceRenameValue?: string | null;
  editing = signal(false);
  //  TODO: Error on change language but syntax highlight not changed
  languages = [
    { value: 'yaml' },
    { value: 'json' },
    { value: 'xml' },
    { value: 'javascript' },
    { value: 'typescript' },
    { value: 'plaintext' },
  ];

  @ViewChild('codeEditor') codeEditor!: CodeEditorComponent;
  @ViewChild('fileControl') fileControl!: FileUpload;
  @ViewChild('renameKeyInplace') renameKeyInplace!: Inplace;

  public _keyValueService = inject(KeyValueService);
  private _messageService = inject(MessageService);
  _confirmationService = inject(ConfirmationService);

  constructor() {
    super();

    // on update key
    this.handleKeySelected();
    // add new key
    this.handleOnRequestFormNewKey();
    // rename key success
    this.handleOnRenameKeySucceed();
    // delete key success
    this.handleOnDeleteKeySucceed();
  }

  private handleOnDeleteKeySucceed() {
    effect(() => {
      if (this.globalStore.keyValues.deleteSuccessAt()) {
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Deleted' });
        untracked(() => {
          this.loaded.update(() => false);
        });
      }
    });
  }

  private handleOnRenameKeySucceed() {
    effect(() => {
      if (this.globalStore.keyValues.renameKeySuccessAt()) {
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Renamed' });
        untracked(() => {
          patchState(this.keyDetail, { key: this.inplaceRenameValue! });
          this.renameKeyInplace.deactivate();
          this.editing.update(() => false);
        });
      }
    });
  }

  private handleOnRequestFormNewKey() {
    effect(() => {
      if (this.globalStore.keyValues.isNewState()) {
        untracked(() => {
          // init data
          patchState(this.keyDetail, { key: 'key1', value: null });
          // content
          this.codeModel.value = '';
          this.codeModel = { ...this.codeModel };
          this.isNewState.update(() => true);
          this.loaded.update(() => true);
          this.editorLoaded.update(() => true);
        });
        // title
        this.renameKey(this.renameKeyInplace!);
      }
    });
  }

  private handleKeySelected() {
    effect(() => {
      const selectedKeyChanged = this.globalStore.keyValues.selectedKey();
      if (selectedKeyChanged) {
        untracked(() => {
          this.isNewState.update(() => false);
          this.getByKey(selectedKeyChanged);
        });
      }
    });
  }

  async getByKey(key: string) {
    let keyDetail: any = { key: key, value: '' };
    try {
      keyDetail = await this._keyValueService.getByKey(this.globalStore.connections.selectedEtcdConnection.id(), key);
    } catch (err: any) {
      console.warn(err);
    }

    patchState(this.keyDetail, keyDetail);
    this.codeModel.value = keyDetail.value;
    this.codeModel = { ...this.codeModel };
    this.loaded.update(() => true);
    this.editorLoaded.update(() => true);
  }

  saveRenameKey() {
    this._keyValueService.renameKey(this.globalStore.connections.selectedEtcdConnection.id(), this.keyDetail.key()!, this.inplaceRenameValue!)
      .then(() => {
        patchState(this.globalStore, { keyValues: { ...this.globalStore.keyValues(), selectedKey: this.inplaceRenameValue!, renameKeySuccessAt: new Date() } });
      })
      .catch((err: any) => {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
      });
  }

  cancelRenameKey(renameKeyInplace: Inplace) {
    renameKeyInplace.deactivate();
    this.editing.update(() => false);
  }

  renameKey(renameKeyInplace: Inplace) {
    this.inplaceRenameValue = this.keyDetail.key();
    renameKeyInplace.activate();
    this.editing.update(() => true);
  }

  deleteKey(evt: any) {
    this._confirmationService.confirm({
      target: evt.target,
      message: 'Are you sure that you want to delete this key?',
      accept: () => {
        this._keyValueService.deleteKey(this.globalStore.connections.selectedEtcdConnection.id(), this.keyDetail.key()!)
          .then(() => {
            patchState(this.globalStore, { keyValues: { ...this.globalStore.keyValues(), selectedKey: '', deleteSuccessAt: new Date() } });
          })
          .catch((err: any) => {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
          });
      }
    });
  }

  changeLanguage(evt: any) {
    this.codeModel.language = evt.value;
    this.codeModel = { ...this.codeModel };
    this.editorLoaded.update(() => false);
    setTimeout(() => {
      this.editorLoaded.update(() => true);
    }, 100);
  }

  onCodeChanged(evt: any) {
    console.log('onCodeChanged', evt);
  }

  handleSelectFile(evt: any) {
    console.log('handleSelectFile', evt);
  }

  refresh() {
    if (this.globalStore.keyValues.selectedKey()) {
      this.getByKey(this.globalStore.keyValues.selectedKey());
      this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Refreshed' });
    }
  }

  export() {

  }

  import() {

  }

  viewAllVersion() {

  }

  save() {
    const key = this.isNewState() ? this.inplaceRenameValue : this.keyDetail.key();
    this._keyValueService.save(this.globalStore.connections.selectedEtcdConnection.id(), key!, this.codeModel.value)
      .then(() => {
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Saved' });
        patchState(this.globalStore, { keyValues: { ...this.globalStore.keyValues(), selectedKey: key!, newKeySuccessAt: new Date() } });
        this.isNewState.update(() => false);
      })
      .catch((err: any) => {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
      });
  }
}
