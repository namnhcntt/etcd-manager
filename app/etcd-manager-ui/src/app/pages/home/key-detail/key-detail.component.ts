import { ChangeDetectionStrategy, Component, ViewChild, effect, inject, model, signal, untracked } from '@angular/core';
import { patchState, signalState } from '@ngrx/signals';
import { CodeEditorComponent, CodeEditorModule, CodeModel } from '@ngstack/code-editor';
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
import { KeyVersionListComponent } from '../../key-version-list/key-version-list.component';
import { ExportService } from '../../service/export.service';
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
    InputTextModule, AutoFocusModule, KeyVersionListComponent],

})
export class KeyDetailComponent extends BaseComponent {
  showKeyVersion = model(false);
  loaded = signal(false);
  editorLoaded = signal(false);
  isNewState = signal(false);
  connection: any;
  keyDetail = signalState({ key: null as string | null, value: null as string | null });
  showCodeEditor = true;
  codeEditorConstant = CodeEditorConstant;
  codeModel = signal<CodeModel>(CodeEditorConstant.DEFAULT_CODE_MODEL);
  inplaceRenameValue?: string | null;
  editing = signal(false);
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

  public readonly _keyValueService = inject(KeyValueService);
  private readonly _messageService = inject(MessageService);
  private readonly _exportService = inject(ExportService);
  private readonly _confirmationService = inject(ConfirmationService);

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
    // add new key success
    this.handleOnNewKeySucceed();
  }

  private handleOnNewKeySucceed() {
    effect(() => {
      if (this.globalStore.keyValues.newKeySuccessAt()) {
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Saved' });
        untracked(() => {
          this.globalStore.setIsNewState(false);
        });
      }
    });
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
          const defaultNewKeyValue = this.globalStore.keyValues.defaultNewKey() ?? 'key1';
          patchState(this.keyDetail, { key: defaultNewKeyValue, value: null });
          this.globalStore.setSelectedKey('');
          this.codeModel.update(() => ({ ...this.codeModel(), value: '' }));
          this.isNewState.update(() => true);
          this.loaded.update(() => true);
          this.editorLoaded.update(() => true);
          // title
          this.renameKey(this.renameKeyInplace);
        });
      } else {
        untracked(() => {
          this.isNewState.update(() => false);
          this.editing.update(() => false);
          this.globalStore.setDefaultNewKey(null);
        });
      }
    });
  }

  private handleKeySelected() {
    effect(() => {
      const selectedKeyChanged = this.globalStore.keyValues.selectedKey();
      if (selectedKeyChanged) {
        untracked(async () => {
          this.globalStore.setIsNewState(false);
          await this.getByKey(selectedKeyChanged);
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
    this.codeModel.update(() => ({ ...this.codeModel(), value: keyDetail.value }));
    this.loaded.update(() => true);
    this.editorLoaded.update(() => true);
  }

  saveRenameKey() {
    this._keyValueService.renameKey(this.globalStore.connections.selectedEtcdConnection.id(), this.keyDetail.key()!, this.inplaceRenameValue!)
      .then(() => {
        this.globalStore.setSelectedKey(this.inplaceRenameValue!);
        this.globalStore.setRenameKeySuccessAt(new Date());
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
            this.globalStore.setSelectedKey('');
            this.globalStore.setDeleteSuccessAt(new Date());
          })
          .catch((err: any) => {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
          });
      }
    });
  }

  changeLanguage(evt: any) {
    this.codeModel.update(() => ({ ...this.codeModel(), language: evt.value }));

  }

  onCodeChanged(evt: any) {
    console.log('onCodeChanged', evt);
  }

  handleSelectFile(evt: any) {
    this._exportService.readDataFromFile(evt.currentFiles[0]).then(rs => {
      if (rs?.value) {
        patchState(this.keyDetail, { value: rs.value });
        this.codeModel.update(() => ({ ...this.codeModel(), value: rs.value }));
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Imported' });
      } else {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Invalid file. The file must have single node item and value must not be empty' });
      }
      this.fileControl.clear();
    }).catch(err => {
      console.error(err);
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err });
      this.fileControl.clear();
    });
  }

  refresh() {
    if (this.globalStore.keyValues.selectedKey()) {
      this.getByKey(this.globalStore.keyValues.selectedKey());
      this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Refreshed' });
    }
  }

  export() {
    this._exportService.exportJsonNode(this.keyDetail());
  }

  import() {
    if (this.fileControl.basicFileInput) {
      this.fileControl.basicFileInput.nativeElement.click();
    }
  }

  viewAllVersion() {
    this.showKeyVersion.update(() => true);
  }

  save() {
    const key = this.isNewState() ? this.inplaceRenameValue : this.keyDetail.key();
    this._keyValueService.save(this.globalStore.connections.selectedEtcdConnection.id(), key!, this.codeModel().value)
      .then(() => {
        this.globalStore.setSelectedKey(key!);
        this.globalStore.setNewKeySuccessAt(new Date());
        this.isNewState.update(() => false);
      })
      .catch((err: any) => {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
      });
  }

  useValueFromOldVersion(evt: string) {
    this.codeModel.update(() => ({ ...this.codeModel(), value: evt }));
    this.closeDialogRevisions();
  }

  closeDialogRevisions() {
    this.showKeyVersion.update(() => false);
  }
}
