import { Component, ViewChild, effect, inject, signal } from '@angular/core';
import { CodeEditorComponent, CodeEditorModule } from '@ngstack/code-editor';
import { MessageService } from 'primeng/api';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { FileUpload, FileUploadModule } from 'primeng/fileupload';
import { Inplace, InplaceModule } from 'primeng/inplace';
import { ToolbarModule } from 'primeng/toolbar';
import { BaseComponent } from '../../../base.component';
import { commonLayoutImport } from '../../../layout/common-layout-import';
import { CodeEditorConstant } from '../../constants/code-editor.constant';
import { KeyValueService } from '../../service/key-value.service';
import { TooltipModule } from 'primeng/tooltip';

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
  imports: [...commonLayoutImport, ToolbarModule, DropdownModule, DialogModule, FileUploadModule, CodeEditorModule, InplaceModule, TooltipModule]
})
export class KeyDetailComponent extends BaseComponent {
  showKeyVersion = false;
  loaded = signal(false);
  editorLoaded = signal(false);
  connection: any;
  keyDetail: any;
  showCodeEditor = true;
  codeEditorConstant = CodeEditorConstant;
  codeModel = CodeEditorConstant.DEFAULT_CODE_MODEL;
  inplaceRenameValue?: string;
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

  @ViewChild('codeEditor') codeEditor!: CodeEditorComponent;
  @ViewChild('fileControl') fileControl!: FileUpload;
  public _keyValueService = inject(KeyValueService);
  private _messageService = inject(MessageService);

  constructor() {
    super();

    effect(() => {
      const selectedKeyChanged = this.globalStore.keyValues.selectedKey();
      if (selectedKeyChanged) {
        this.getByKey(selectedKeyChanged);
      }
    });
  }

  async getByKey(value: string) {
    try {
      const keyDetail = await this._keyValueService.getByKey(this.globalStore.connections.selectedEtcdConnection.id(), value);
      this.keyDetail = keyDetail;
      this.codeModel.value = keyDetail.value;
      this.codeModel = { ...this.codeModel };
      this.loaded.update(() => true);
      this.editorLoaded.update(() => true);
    } catch (err: any) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
    }
  }

  saveRenameKey(renameKeyInplace: Inplace) {
    this._keyValueService.renameKey(this.globalStore.connections.selectedEtcdConnection.id(), this.keyDetail.key, this.inplaceRenameValue!)
      .then(() => {
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Renamed' });
        renameKeyInplace.deactivate();
      })
      .catch((err: any) => {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
      });
  }

  cancelRenameKey(renameKeyInplace: Inplace) {
    console.log('cancelRenameKey', renameKeyInplace);
  }

  renameKey(renameKeyInplace: Inplace) {
    renameKeyInplace.activate();
  }

  deleteKey() {
    console.log('deleteKey');
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
    this._keyValueService.save(this.globalStore.connections.selectedEtcdConnection.id(), this.keyDetail.key, this.codeModel.value)
      .then(() => {
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Saved' });
      })
      .catch((err: any) => {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
      });
  }
}
