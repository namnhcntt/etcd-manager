import { ChangeDetectionStrategy, Component, OnInit, ViewChild, effect, inject, signal, untracked } from '@angular/core';
import { patchState } from '@ngrx/signals';
import { MenuItem, Message, MessageService, PrimeIcons, TreeNode } from 'primeng/api';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from 'primeng/dialog';
import { FileUpload, FileUploadModule } from 'primeng/fileupload';
import { Listbox, ListboxModule } from 'primeng/listbox';
import { ToolbarModule } from 'primeng/toolbar';
import { Tree, TreeModule } from 'primeng/tree';
import { BaseComponent } from '../../../base.component';
import { commonLayoutImport } from '../../../layout/common-layout-import';
import { KeyValueService } from '../../service/key-value.service';
import { LocalCacheService } from '../../service/local-cache.service';

@Component({
  selector: 'app-key-list',
  templateUrl: './key-list.component.html',
  styles: [`
  .key-list-item {
      width: 100%;

      i {
          margin-right: 10px;
      }
  }

  .button-header {
      &:hover {
          cursor: pointer;
      }

    margin-right:20px;
  }

  `],
  standalone: true,
  imports: [...commonLayoutImport, ContextMenuModule, ToolbarModule, ListboxModule, TreeModule,
    DialogModule, FileUploadModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class KeyListComponent extends BaseComponent implements OnInit {

  loaded = signal(false);
  viewMode: 'tree' | 'list' = 'tree';
  parentKeyOnNew = '';
  showNewKeyForm = false;
  showImportNodes = false;
  contextMenuSelectedKey?: string;
  treeIsExpandAll = false;
  contextMenuModel: MenuItem[] = this.getContextMenu();
  msgs: Message[] = [];
  selectedItem?: any;
  firstLoad = true;
  @ViewChild('mainTree', { static: false }) mainTree?: Tree;
  @ViewChild('fileControl') fileControl!: FileUpload;
  @ViewChild('mainList', { static: false }) mainList!: Listbox;

  private _messageService = inject(MessageService);
  private _keyValueService = inject(KeyValueService);
  private _localCacheService = inject(LocalCacheService);

  constructor() {
    super();
    this.handleOnSelectEtcdConnection();
    this.handleOnNewKeySucceeded();
    this.handleOnRenameKeySucceeded();
    this.handleOneDeleteKeySucceeded();
  }

  private handleOneDeleteKeySucceeded() {
    effect(() => {
      if (this.globalStore.keyValues.deleteSuccessAt()) {
        untracked(() => {
          this.refreshList();
        });
      }
    });
  }

  private handleOnRenameKeySucceeded() {
    effect(() => {
      if (this.globalStore.keyValues.renameKeySuccessAt()) {
        untracked(() => {
          this.refreshList();
        });
      }
    });
  }

  private handleOnNewKeySucceeded() {
    effect(() => {
      if (this.globalStore.keyValues.newKeySuccessAt()) {
        untracked(() => {
          this.refreshList();
        });
      }
    });
  }

  private handleOnSelectEtcdConnection() {
    effect(() => {
      const selectedConnection = this.globalStore.connections.selectedEtcdConnection();
      if (selectedConnection && selectedConnection.id > 0) {
        let selectedKey: string | null = null;
        if (this.firstLoad) {
          selectedKey = this._localCacheService.get('selectedKey');
          this.firstLoad = false;
        }
        this.bindDataAndSelectExistItem(selectedConnection, selectedKey);
      }
    });
  }

  private bindDataAndSelectExistItem(selectedConnection: { id: number; name: string; }, selectedKey: string | null) {
    this.bindData(selectedConnection.id, selectedKey).then(rs => {
      if (this.selectedItem != null) {
        if (this.viewMode == 'tree') {
          this.onNodeSelect({ node: this.selectedItem });
        } else {
          console.log('support listbox pre select item later');
        }
      }
    });
  }

  ngOnInit() {

  }

  getContextMenu() {
    const menu = [];

    if (this.viewMode == 'tree') {
      menu.push({
        label: 'Create child node',
        icon: 'pi pi-plus',
        command: this.createChildNode.bind(this)
      });
    }

    menu.push(...[
      {
        label: 'Delete',
        icon: 'pi pi-trash',
        command: this.menuDelete.bind(this)
      },]);

    // if (this.currentSelectRow) {
    //   menu.push(
    //     {
    //       label: 'Export current node',
    //       icon: 'pi pi-download',
    //       command: this.exportCurrentNode.bind(this)
    //     },
    //   );

    // }

    if (this.viewMode == 'tree') {
      menu.push({
        label: 'Export node and all childs',
        icon: 'pi pi-download',
        command: this.exportAllNodeAndChilds.bind(this)
      });
    }

    return menu;
  }

  menuDelete() {
    // if in tree view mode, enable delete recursive option
    // this._keyValueService.onDelete(this.contextMenuSelectedKey, this.viewMode == 'tree').then(rs => {
    //   if (rs) {
    //     this.refreshList();
    //   }
    // });
  }

  exportAllNodeAndChilds() {
    // this._keyValueService.getByKeyPrefix(this.contextMenuSelectedKey).then(rs => {
    //   if (rs.success) {
    //     this._exportService.exportNodes(rs.data);
    //   } else {
    //     this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
    //   }
    // });
  }

  exportCurrentNode() {
    // this._keyValueService.getByKey(this.currentSelectRow.key).then(rs => {
    //   if (rs.success) {
    //     this._exportService.exportJsonNode(rs.data);
    //   } else {
    //     this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
    //   }
    // }).catch(err => {
    //   this._messageService.add({ severity: 'error', summary: 'Error', detail: err.message });
    // });
  }

  createChildNode() {
    // if (this.currentSelectRow) {
    //   this.parentKeyOnNew = this.currentSelectRow.key + '/';
    // } else if (this.viewMode == 'tree' && this.treeSelectedItem) {
    //   this.parentKeyOnNew = this.treeSelectedItem.data + '/';
    // }

    // this.showNewKeyForm = true;
  }

  menuRename() {
    console.log('rename');
  }

  async bindData(selectedConnectionId: number, selectedKey?: string | null) {
    try {
      const ds = await this._keyValueService.getAllKeys(selectedConnectionId);
      const dataSource = ds.map((x: any) => {
        return { key: x };
      });

      patchState(this.globalStore, { keyValues: { ...this.globalStore.keyValues(), dataSource } });

      if (this.viewMode == 'tree') {
        this.bindDataSourceTree(dataSource, selectedKey);
      }
      this.loaded.update(() => true);
    } catch (err: any) {
      console.error(err);
      this._messageService.add({ severity: 'error', summary: 'Error', detail: err.error.error });
      this.msgs.push({ severity: 'error', summary: 'Error', detail: err.error.error });
    }
  }

  onChangeSelectedKey(evt: any) {
    // this.rootCtx.dispatchEvent('changeSelectedKey', evt.value);
  }

  refreshList(showMessage: boolean = false) {
    const id = this.globalStore.connections.selectedEtcdConnection.id();
    if (id > 0) {
      this.bindDataAndSelectExistItem(this.globalStore.connections.selectedEtcdConnection(), this.globalStore.keyValues.selectedKey());
      if (showMessage) {
        this._messageService.add({ severity: 'success', summary: 'Success', detail: 'Refresh success' });
      }
    }
  }

  switchViewMode() {
    if (this.viewMode == 'list') {
      // list
      this.bindDataSourceTree(this.globalStore.keyValues.dataSource(), null);
      this.viewMode = 'tree';
    } else {
      // tree
      this.viewMode = 'list';
    }
    console.log('switch view mode');
  }

  bindDataSourceTree(dataSource: any[], selectedKey?: string | null) {
    const paths = dataSource.map(x => x.key);
    const root: TreeNode<string> = {};
    for (let path of paths) {
      let pathParts = path.split('/');
      let currentNode = root;
      for (let part of pathParts) {
        if (part !== '') {
          if (!currentNode.children) {
            currentNode.children = [];
          }
          let childNode = currentNode.children.find(node => node.label === part);
          if (!childNode) {
            childNode = { label: part, data: pathParts.slice(0, pathParts.indexOf(part) + 1).join('/'), expanded: true, expandedIcon: PrimeIcons.FOLDER_OPEN, icon: PrimeIcons.FOLDER } as TreeNode;
            // pre-selected node
            if (selectedKey && selectedKey === childNode.data) {
              this.selectedItem = childNode;
            }
            currentNode.children.push(childNode);
          }
          currentNode = childNode;
        }
      }
    }
    const treeDataSource = root.children || [];
    patchState(this.globalStore, { keyValues: { ...this.globalStore.keyValues(), treeDataSource } });
  }

  nomarlizePathCombine(path: string) {
    if (path.startsWith('//')) {
      path = path.substring(1);
    }
    return path;
  }

  export() {
    // this._keyValueService.getAll().then(rs => {
    //   if (rs.success) {
    //     this._exportService.exportNodes(rs.data);
    //   } else {
    //     this._messageService.add({ severity: 'error', summary: 'Error', detail: rs.message });
    //   }
    // });
  }

  import() {
    if (this.fileControl.basicFileInput) {
      this.fileControl.basicFileInput.nativeElement.click();
    }
  }

  newKey() {
    patchState(this.globalStore, { keyValues: { ...this.globalStore.keyValues(), isNewState: true } });
  }

  onSaveNewKey(evt: any) {
    // this.bindData(this.rootCtx.data.connection);
    this.showNewKeyForm = false;
  }

  showContextMenuViewModeList(menu: ContextMenu, event: MouseEvent, item: any) {
    // menu.hide();
    // event.preventDefault();
    // event.stopPropagation();
    // setTimeout(() => {
    //   console.log('context menu', menu, event, item);
    //   this.currentSelectRow = item;
    //   this.contextMenuSelectedKey = item.key;
    //   menu.toggle(event);
    // }, 1);
  }

  contextMenuViewModeTreeSelect(evt: any) {
    // this.currentSelectRow = this.globalStore.keyValues.dataSource().find(x => x.key == evt.node.data);
    // this.contextMenuSelectedKey = evt.node.data;
    // this.contextMenuModel = this.getContextMenu();
  }

  preventMouseDown(event: any) {
    event.preventDefault();
    event.stopPropagation();
  }

  onNodeSelect(evt: any) {
    patchState(this.globalStore, { keyValues: { ...this.globalStore.keyValues(), selectedKey: evt.node.data } });
    // save to cache
    this._localCacheService.set('selectedKey', evt.node.data);
  }

  toggleExpandTree(isExpand: boolean) {
    this.treeIsExpandAll = isExpand;
    this.globalStore.keyValues.treeDataSource().forEach(node => {
      this.expandRecursive(node, isExpand);
    });
  }

  handleSelectFile(evt: any) {
    // this._exportService.readDataFromFile(evt.currentFiles[0]).then(rs => {
    //   if (rs) {
    //     console.log('file content', rs);
    //   }
    // });
  }

  importNodes() {
    this.showImportNodes = true;
  }

  private expandRecursive(node: TreeNode, isExpand: boolean) {
    node.expanded = isExpand;
    if (node.children) {
      node.children.forEach(childNode => {
        this.expandRecursive(childNode, isExpand);
      });
    }
  }
}
