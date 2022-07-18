import { Component, OnInit } from '@angular/core';
import { CodeModel } from '@ngstack/code-editor';
import { MessageService } from 'primeng/api';
import { AppCtxService } from 'src/app/service/app-ctx.service';
import { AppEventService } from 'src/app/service/app-event.service';
import { ComCtxService } from 'src/app/service/com-ctx.service';
import { KeyValueService } from 'src/app/service/key-value.service';

@Component({
  selector: 'app-key-detail',
  templateUrl: './key-detail.component.html',
  styleUrls: ['./key-detail.component.scss'],
  providers: [ComCtxService]
})
export class KeyDetailComponent implements OnInit {
  loaded = false;
  rootCtx: ComCtxService;
  theme = 'vs-light';
  connection: any;
  keyDetail: any;
  codeModel: CodeModel = {
    language: 'yaml',
    uri: '*.yaml',
    value: ''
  };

  options = {
    contextmenu: true,
    minimap: {
      enabled: true
    },
    lineNumbers: true,
  };

  constructor(
    private _appCtxService: AppCtxService,
    private _keyValueService: KeyValueService,
    private _appEventService: AppEventService,
    private _messageService: MessageService
  ) {
    this.rootCtx = this._appCtxService.getRootCtx();
    this._appEventService.getSubscriptionConnection().subscribe(async rs => {
      this.connection = rs;
    });
  }

  ngOnInit() {
    this.rootCtx.subscribe('changeSelectedKey', async (value) => {
      this.loaded = true;
      const keyDetail = await this._keyValueService.getByKey(this.connection, value.key);
      if (keyDetail.success) {
        this.keyDetail = keyDetail.data;
        this.codeModel.value = keyDetail.data.value;
        this.codeModel = { ...this.codeModel };
      } else {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: keyDetail.message });
      }
    });
  }

  onCodeChanged(value) {
    console.log('CODE', value);
  }

}
