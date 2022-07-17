import { Component, OnInit } from '@angular/core';
import { MessageService } from 'primeng/api';
import { AppCtxService } from 'src/app/service/app-ctx.service';
import { AppEventService } from 'src/app/service/app-event.service';
import { ComCtxService } from 'src/app/service/com-ctx.service';
import { KeyValueService } from 'src/app/service/key-value.service';

@Component({
  selector: 'app-key-list',
  templateUrl: './key-list.component.html',
  styleUrls: ['./key-list.component.scss'],
  providers: [ComCtxService]
})
export class KeyListComponent implements OnInit {
  viewMode: 'tree' | 'list' = 'list';

  selectedKey: string;
  loaded = false;
  treeDataSource = [];
  listDataSource = [];
  rootCtx: ComCtxService;

  constructor(
    private _appEventService: AppEventService,
    private _keyValueService: KeyValueService,
    private _messageService: MessageService,
    private _appCtxService: AppCtxService
  ) {
    this.rootCtx = this._appCtxService.getRootCtx();
  }

  ngOnInit() {
    this._appEventService.getSubscriptionConnection().subscribe(async rs => {
      console.log('selected connection', rs);
      if (rs) {
        const ds = await this._keyValueService.getAll(rs);
        if (ds.success) {
          console.log('all keys', ds.data);
          this.listDataSource = ds.data.map(x => {
            return { key: x };
          });
          this.loaded = true;
        } else {
          this._messageService.add({ severity: 'error', summary: 'Error', detail: ds.message });
        }
      }
    });
  }

  onChangeSelectedKey(evt) {
    console.log('onchange selected key', evt);
    this.rootCtx.dispatchEvent('changeSelectedKey', evt.value);
  }

}
